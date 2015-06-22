// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PathfindTester.cs" company="Davide Aversa">
//   MIT License
// </copyright>
// <summary>
//   The pathfind tester.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

//#define PATHTESTER_DEBUG_LOG

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

using RoomOfRequirement.Logger;
using RoomOfRequirement.Search;

using UnityEngine;

using Object = UnityEngine.Object;
using Random = System.Random;

/// <summary>
/// The pathfind tester.
/// </summary>
[RequireComponent(typeof(Pathfinder))]
public class PathfindTester : MonoBehaviour
{

    public bool ShowPathAnimation = false;

    public float PathStepInterval = 0.1f;

    public int UpdateDepth = 1;

    public long Windows = 2;
	public long MinWindows = 0;
	public bool Caching = true;

    /// <summary>
    /// Seed for the RNG.
    /// </summary>
    public int Seed;

    /// <summary>
    /// The number of path tried in the test.
    /// </summary>
    public int NumberOfRuns;

    /// <summary>
    /// If true the pathfinder avoids to search for paths who starts and ends 
    /// in the same area.
    /// </summary>
    public bool AvoidSameAreaPath;

    /// <summary>
    /// The frequency of portals scrambling.
    /// </summary>
    [Range(1, 1000)]
    public int ScrambleRate;

    /// <summary>
    /// Gets a reference to the main pathfinder object.
    /// </summary>
    public Pathfinder ThePathfinder { get; private set; }

    /// <summary>
    /// An instance of the RNG.
    /// </summary>
    private Random r;

    /// <summary>
    /// Store the list of maps.
    /// </summary>
    private readonly List<TextAsset> allMaps = new List<TextAsset>();

    /// <summary>
    /// Reference to the agent.
    /// </summary>
    private AgentEntity agent;

    /// <summary>
    /// Gets a reference to a portal randomizer algorithm.
    /// </summary>
    public IPortalsRandomStrategy RandomStrategy { get; private set; }

    /// <summary>
    /// Return the name of he map.
    /// </summary>
    public string CurrentMapName
    {
        get
        {
            return allMaps[CurrentMapIndex].name;
        }
    }

    /// <summary>
    /// The execution error.
    /// </summary>
    private bool executionError;

    /// <summary>
    /// The srd.
    /// </summary>
    private SingleRunData srd;

    /// <summary>
    /// The last square.
    /// </summary>
    private MapSquare lastSquare;

    /// <summary>
    /// The agent indicator.
    /// </summary>
    public AgentPositioning AgentIndicator;

    /// <summary>
    /// The target indicator.
    /// </summary>
    public AgentPositioning TargetIndicator;

	public string CurrentParam;

    /// <summary>
    /// Gets the map number.
    /// </summary>
    public int MapNumber
    {
        get
        {
            return allMaps.Count;
        }
    }

    /// <summary>
    /// Gets the current map index.
    /// </summary>
    public int CurrentMapIndex { get; private set; }

    /// <summary>
    /// Gets the current map iteration.
    /// </summary>
    public int CurrentMapIteration { get; private set; }

    /// <summary>
    /// The awake.
    /// </summary>
    private void Awake()
    {
        ThePathfinder = gameObject.GetComponent<Pathfinder>();
    }

    /// <summary>
    /// The start.
    /// </summary>
    private void Start()
    {
        agent = new AgentEntity(ThePathfinder.AgentBelief as HVertexBased);
        RandomStrategy = GetComponent<IPortalsRandomStrategy>();
        StartCoroutine(MetaLoop());
    }

    public IEnumerator MetaLoop()
    {
        var parameters = Resources.LoadAll("Parameters");
        foreach (var paramFile in parameters) {
            LoadParameters((TextAsset)paramFile);
            // Set the seed to allow multiple test.
            r = new System.Random(Seed);
            AStar.CollectProfiling = true;
            LoadAllMaps();
            yield return StartCoroutine(MainNHTestLoop());
        }
    }

    private void LoadParameters(TextAsset paramFile)
    {
        var parameters = ParseParameters.ParseFile(paramFile);
		CurrentParam = paramFile.name;
        foreach (var par in parameters.Keys)
        {
            switch (par)
            {
                case "update_radius":
                    UpdateDepth = (int)parameters[par];
                    break;
                case "seed":
                    Seed = (int)parameters[par];
                    break;
                case "number_of_runs":
                    NumberOfRuns = (int)parameters[par];
                    break;
                case "scramble_rate":
                    ScrambleRate = (int)parameters[par];
                    break;
                case "initial_randomness":
                    RandomStrategy.SetRandomness(parameters[par]);
                    break;
                case "scramble_amount":
                    RandomStrategy.SetScrambleAmount(parameters[par]);
                    break;
                case "valid_revision_limit":
                    Windows = (long)parameters[par];
                    break;
				case "minimal_windows":
					MinWindows = (long)parameters[par];
					break;
				case "cache" :
					Caching = (parameters[par] > 0.0f);
					break;
            }
        }
    }

    /// <summary>
    /// Loads all maps into memory.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "Reviewed. Suppression is OK here.")]
    private void LoadAllMaps()
    {
        Object[] allMapObj = Resources.LoadAll("Maps");
        if (allMapObj.Length == 0)
        {
#if PATHTESTER_DEBUG_LOG
            myLogger.LogError("No Map Loaded in PathfinderTest");
#endif
        }

        foreach (var o in allMapObj)
        {
            allMaps.Add((TextAsset)o);
        }

#if PATHTESTER_DEBUG_LOG
        myLogger.Log(string.Format("{0} maps loaded!", allMaps.Count));
#endif
    }

    /// <summary>
    /// Main test loop. (Coroutine)
    /// </summary>
    /// <returns>
    /// The <see cref="IEnumerator"/>.
    /// </returns>
    public IEnumerator MainNHTestLoop()
    {
        CurrentMapIndex = 0;
        foreach (var txa in allMaps)
        {
            /* Update the map and recompute the map. */
            BDPMap.Instance.MapFile = txa;
            BDPMap.Instance.ComputeMap();

            var bd = new BenchmarkData(this) { TMin = Windows };

            while (!BDPMap.Instance.MapIsLoaded)
            {
                yield return new WaitForSeconds(0.5f);
            }

            agent.CleanBelief();

#if PATHTESTER_DEBUG_LOG
            Debug.Log(string.Format("[MAINLOOP] Starting Iteration on map {0}", txa.name));
#endif
            CurrentMapIteration = 0;
            agent.CurrentPosition = this.RandomFreePosition();
            RandomStrategy.Init();
            while (CurrentMapIteration < NumberOfRuns)
            {
#if PATHTESTER_DEBUG_LOG
                Debug.Log("--------------------------------------------------");
                Debug.Log(string.Format("[MAINLOOP] Computing Path {0} of {1}", CurrentMapIteration + 1, NumberOfRuns));
#endif

                // Debug.Log("Iteration " + counter);
                if (CurrentMapIteration % ScrambleRate == 0)
                {
#if PATHTESTER_DEBUG_LOG
                    Debug.Log("[MAINLOOP] Randomizing Portals");
#endif
                    StepCounter.Increase();
                    RandomStrategy.RandomizeWorldPortals();
                }

                var targetPos = this.DraftRandomTargetPos();

                /* BENCHMARK */
                InitializeSRD(agent.CurrentPosition, targetPos);

                /* ********* */
                UpdateAllPortalInArea(agent.CurrentPosition);

                var startTime = 0;
                var valid = true;

                while (agent.CurrentPosition != targetPos)
                {
                    startTime++;
                    this.executionError = false;
                    if (startTime > 30) 
                    {
                        srd.PathFound = false;
                        Debug.Log("[MAINLOOP] Time out! Go to next pick.");
                        valid = false;
                        break;
                    }

#if PATHTESTER_DEBUG_LOG
                    Debug.Log(string.Format("[MAINLOOP] Execution Step from {0} to {1}", agent.CurrentPosition, targetPos));
#endif

                    if (ThePathfinder.AgentBelief.Hierarchical)
                    {
                        ThePathfinder.AgentBelief.CurrentTarget = targetPos;
                    }

					if (!Caching) {
						agent.ReviewBeliefOlderThan(Windows);
					}
                    
					var path = ThePathfinder.PathFind(agent.CurrentPosition, targetPos);

                    /* BENCHMARK */
                    UpdateSRD();

                    if (path == null)
                    {

                        Debug.Log("[MAINLOOP] No path found! Try to perform belief revision.");
                        if (ThePathfinder.AgentBelief.Hierarchical)
                        {
                            Debug.Log("[MAINLOOP] Starting Belief Revision Loop");
                            path = BeliefRevisionLoop(agent.CurrentPosition, targetPos);
                        }

                        if (path == null)
                        {
                            srd.PathFound = false;
#if PATHTESTER_DEBUG_LOG
                            Debug.Log("[MAINLOOP] No path found! Go to next pick.");
#endif
                            break;
                        }
                    }

                    srd.PathFound = true;

                    /* ********* */
                    var pathList = Path2MapSquareList(path);

                    //Debug.Log("[MAINLOOP] Path Found. Start Execution.");
                    if (ThePathfinder.AgentBelief.Hierarchical)
                    {
                        yield return StartCoroutine(ExecuteHierarchicalPath(pathList));
                    }
                    else
                    {
                        yield return StartCoroutine(ExecutePath(pathList));
                    }
                }

                if (true)
                {
                    CurrentMapIteration++;
                    bd.RunsData.Add(srd);
                }

                yield return new WaitForSeconds(0.1f);
            }

            bd.PrintToFile();
            CurrentMapIndex++;
        }

#if PATHTESTER_DEBUG_LOG
        myLogger.Log("[MAINLOOP] TEST COMPLETED");
#endif
    }

    /// <summary>
    /// Transform a Path into a list.
    /// </summary>
    /// <param name="path">The input path.</param>
    /// <returns>A list representation of the path.</returns>
    public static List<MapSquare> Path2MapSquareList(Path<MapSquare> path)
    {
        //TODO: Move to Path.
        var pathList = new List<MapSquare>(path);
        pathList.Reverse();
        return pathList;
    }

    /// <summary>
    /// Draft a random multi-area target and starting location.
    /// </summary>
    /// <returns></returns>
    private MapSquare DraftRandomTargetPos()
    {
        var targetPos = this.RandomFreePosition();
        //Debug.Log(string.Format("From {0} to {1}", this.lastSquare, targetPos));
        this.AgentIndicator.GridPosition = new MapSquare(this.agent.CurrentPosition.x, this.agent.CurrentPosition.y);
        this.TargetIndicator.GridPosition = new MapSquare(targetPos.x, targetPos.y);

        // Avoid Same Area Paths.
        while (this.IsSameAreaPath(this.agent.CurrentPosition, targetPos))
        {
            // Ignore Path. Repick.
            targetPos = this.RandomFreePosition();
            this.TargetIndicator.GridPosition = new MapSquare(targetPos.x, targetPos.y);
        }

        return targetPos;
    }

    /// <summary>
    /// The initialize srd.
    /// </summary>
    /// <param name="currentPos">
    /// The current pos.
    /// </param>
    /// <param name="targetPos">
    /// The target pos.
    /// </param>
    private void InitializeSRD(MapSquare currentPos, MapSquare targetPos)
    {
        var p = ThePathfinder.TheMightyOmniscientAndPerfectPathfinder(currentPos, targetPos);
		var popo = SimpleHOmniscient.FindPath(currentPos,targetPos);
		srd = new SingleRunData { StartingPoint = currentPos.ToString(), TargetPoint = targetPos.ToString(), RealPathExists = p!=null , OmniscientEffort = popo};
    }

    /// <summary>
    /// The update srd.
    /// </summary>
    private void UpdateSRD()
    {
        srd.PathfindingTicks += AStar.ElapsedTime;
        srd.ExploredNodes += AStar.ExpandedNodes;
        srd.MaxMemoryUsage = Mathf.Max(srd.MaxMemoryUsage, AStar.MaxMemoryQueue);
        srd.NumberOfAttempts++;
    }

    /// <summary>
    /// The is same area path.
    /// </summary>
    /// <param name="currentPos">
    /// The current pos.
    /// </param>
    /// <param name="targetPos">
    /// The target pos.
    /// </param>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    private bool IsSameAreaPath(MapSquare currentPos, MapSquare targetPos)
    {
        return BDPMap.Instance.GetArea(currentPos) == BDPMap.Instance.GetArea(targetPos) && AvoidSameAreaPath;
    }

    void OnDrawGizmos()
    {
        if (!BDPMap.Instance.MapIsLoaded) 
        {
            return;
        }

        for (var x = 0; x < BDPMap.Instance.Width; ++x)
        {
            for (var y = 0; y < BDPMap.Instance.Height; ++y) 
            {
                if (!BDPMap.Instance.IsPortalSquare(new MapSquare(x, y))) 
                {
                    continue;
                }

                Gizmos.color = this.ThePathfinder.AgentBelief.IsFree(new MapSquare(x, y)) ? Color.green : Color.red;
                Gizmos.DrawSphere(MapRenderer.Instance.Grid2Cartesian(new MapSquare(x, y)), 0.3f);
            }
        }
    }

    /// <summary>
    /// The belief revision loop.
    /// </summary>
    /// <param name="currentPos">
    /// The current pos.
    /// </param>
    /// <param name="targetPos">
    /// The target pos.
    /// </param>
    /// <param name="path">
    /// The path.
    /// </param>
    /// <returns>
    /// The <see cref="Path"/>.
    /// </returns>
    private Path<MapSquare> BeliefRevisionLoop(MapSquare currentPos, MapSquare targetPos)
    {
#if PATHTESTER_DEBUG_LOG
        Debug.Log(string.Format("[REVISION] Portal belief revision for {0} to {1}", currentPos, targetPos));
#endif
        Path<MapSquare> path = null;

		long currentWindows = Windows;

		while (currentWindows >= MinWindows && path == null) {
			Debug.Log (string.Format ("[REVISION] Open portals older than {0}", currentWindows));
			agent.ReviewBeliefOlderThan (currentWindows);
			path = ThePathfinder.PathFind (currentPos, targetPos);
			currentWindows--;
		}
        return path;
    }

    /// <summary>
    /// The execute path.
    /// </summary>
    /// <param name="pathList">
    /// The path list.
    /// </param>
    /// <returns>
    /// The <see cref="IEnumerator"/>.
    /// </returns>
    public IEnumerator ExecutePath(IList<MapSquare> pathList)
    {
#if PATHTESTER_DEBUG_LOG
        Debug.Log("[LOW-LEVEL-PATH] Starting Path Execution");
        LogPath("[LOW-LEVEL-PATH] ", pathList);
#endif
        // var mapInconsistency = false;
        // var pathCompleted = false;
        var stepIndex = 1;
        var currentPos = pathList[0];
        var targetPos = pathList[pathList.Count - 1];
        DrawDebugPath(pathList, Color.yellow);
        while (true)
        {
            var nextPos = pathList[stepIndex];
            var currentArea = BDPMap.Instance.GetArea(agent.CurrentPosition);
            var nextPosArea = BDPMap.Instance.GetArea(nextPos);
            if (!agent.MoveTo(nextPos))
            {
                bool changed;
                srd.UpdateTicks += MethodProfiler.ProfileMethod(
                    agent.UpdateBelief,
                    nextPos,
                    false,
                    out changed);
                this.executionError = true;
                agent.CurrentPosition = currentPos;
                break;
            }

            AgentIndicator.GridPosition = agent.CurrentPosition;

            // If enter a new area, update all the portals in the area.
            if (currentArea != nextPosArea)
            {
                var changed = UpdateAllPortalInArea(nextPos);
                if (changed)
                {
#if PATHTESTER_DEBUG_LOG
                    Debug.Log("[LOW-LEVEL-PATH] Discrepancy Found!");
#endif
                    this.executionError = true;
                    break;
                }
            }

            srd.PathLenght++;
            if (agent.CurrentPosition == targetPos)
            {
                // pathCompleted = true;
                this.executionError = false;
                break;
            }

            stepIndex++;

            if (ShowPathAnimation)
            {
                yield return new WaitForSeconds(PathStepInterval);
            }
        }
    }

    private static void LogPath(string tag, IList<MapSquare> pathList)
    {
        var pathstring = pathList.Aggregate(
            tag,
            (current, mapSquare) => current + (mapSquare.ToString() + " -> "));
        Debug.Log(pathstring);
    }

    /// <summary>
    /// The draw debug path.
    /// </summary>
    /// <param name="pathList">
    /// The path list.
    /// </param>
    /// <param name="color">
    /// The color.
    /// </param>
    private static void DrawDebugPath(IList<MapSquare> pathList, Color color)
    {
        for (var i = 0; i < pathList.Count - 1; ++i)
        {
            var start = MapRenderer.Instance.Grid2Cartesian(pathList[i]);
            var end = MapRenderer.Instance.Grid2Cartesian(pathList[i + 1]);
            Debug.DrawLine(new Vector3(start.x, start.y, 0), new Vector3(end.x, end.y, 0), color, 2.0f);
            //Debug.Log(pathList[i] + " " + pathList[i + 1]);
        }
    }

    /// <summary>
    /// The execute hierarchical path.
    /// </summary>
    /// <param name="pathList">
    /// The path list.
    /// </param>
    /// <returns>
    /// The <see cref="IEnumerator"/>.
    /// </returns>
    public IEnumerator ExecuteHierarchicalPath(IList<MapSquare> pathList)
    {
#if PATHTESTER_DEBUG_LOG
        Debug.Log("[HIGH-LEVEL-PATH] Starting Hierarchical Path Execution");
        LogPath("[HIGH-LEVEL-PATH] ", pathList);
#endif
        // var mapInconsistency = false;
        var stepIndex = 1;
        var currentHighLevelPos = pathList[0];
        var targetSquare = pathList[pathList.Count - 1];
        DrawDebugPath(pathList, Color.red);
        while (true)
        {
            var nextHighLevelPos = pathList[stepIndex];

            // Expand the first step.
            var path = ThePathfinder.PathFindOnRealMap(
                currentHighLevelPos, 
                nextHighLevelPos, 
                BDPMap.Instance.GetArea(currentHighLevelPos));
			srd.ExploredNodes += AStar.ExpandedNodes;
            if (path == null)
            {
                Debug.Log("[HIGH-LEVEL-PATH] No low level path.");
                //UpdateAllPortalInArea(currentHighLevelPos);
                agent.UpdateBelief(nextHighLevelPos, false);
                this.executionError = true;
                break;
            }

            var subPathList = Path2MapSquareList(path);

            // --
            this.executionError = false;
            yield return StartCoroutine(ExecutePath(subPathList));
            if (this.executionError)
            {
                Debug.Log("[HIGH-LEVEL-PATH] Error on ExecutePath.");
                Debug.Log(string.Format("Update {0}",nextHighLevelPos));
                agent.UpdateBelief(nextHighLevelPos, false);
                break;
            }

            stepIndex++;
            currentHighLevelPos = nextHighLevelPos;
            if (currentHighLevelPos == targetSquare)
            {
                break;
            }
        }
    }

    /// <summary>
    /// The random free position.
    /// </summary>
    /// <returns>
    /// The <see cref="MapSquare"/>.
    /// </returns>
    private MapSquare RandomFreePosition()
    {
        MapSquare chosenPos = null;
        var count = 1;
        foreach (var ms in BDPMap.Instance.MapSquares().Where(ms => BDPMap.Instance.IsFree(ms) && !BDPMap.Instance.IsPortalSquare(ms)))
        {
            if (this.r.Next(0, count) == 0)
            {
                chosenPos = ms;
            }

            count++;
        }

        return chosenPos;
    }

    private bool UpdateAllPortalInArea(MapSquare ms, int depth = 1) {
        var mapSquareArea = BDPMap.Instance.GetArea(ms);
        var pgs = BDPMap.Instance.GetPortalGroupByAreas(mapSquareArea);
        var changed = false;
        foreach (var pg in pgs.Select(pg => pg.NearestPortal(ms))) 
        {
            var updateSquareIn = pg[mapSquareArea];
            var updateSquareOut = pg[mapSquareArea, reverse: true];
            if (updateSquareIn == null)
            {
                Debug.Log("ERROR");
                break;
            }

            bool tmpChange;
            this.srd.UpdateTicks += MethodProfiler.ProfileMethod(
                agent.UpdateBelief,
                updateSquareIn,
                BDPMap.Instance.IsFree(updateSquareIn),
                out tmpChange);
            changed = tmpChange;

            if (!BDPMap.Instance.IsFree(updateSquareIn)) 
            {
                continue;
            }

            this.srd.UpdateTicks += MethodProfiler.ProfileMethod(
                agent.UpdateBelief,
                updateSquareOut,
                BDPMap.Instance.IsFree(updateSquareOut),
                out tmpChange);
            changed = changed || tmpChange;

            if (depth < UpdateDepth) 
            {
                tmpChange = UpdateAllPortalInArea(updateSquareOut, depth + 1);
                changed = changed || tmpChange;
            }
            
        }

        return changed;
    }
}
