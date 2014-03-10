// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PathfindTester.cs" company="Davide Aversa">
//   MIT License
// </copyright>
// <summary>
//   The pathfind tester.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

#define PATHTESTER_DEBUG_LOG

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
    /// Gets a reference to a portal rondomizer algorithm.
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

#if PATHTESTER_DEBUG_LOG

    /// <summary>
    /// The my logger.
    /// </summary>
    private readonly BasicLogger myLogger = new BasicLogger("PATHTEST");
#endif

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
        // Set the seed to allow multiple test.
        r = new System.Random(Seed);
        RandomStrategy = GetComponent<IPortalsRandomStrategy>();
        AStar.CollectProfiling = true;
        LoadAllMaps();
        StartCoroutine(MainNHTestLoop());
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
            CurrentMapIndex++;

            /* Update the map and recompute the map. */
            BDPMap.Instance.MapFile = txa;
            BDPMap.Instance.ComputeMap();
            ThePathfinder.AgentBelief.ResetBelieves();
#if PATHTESTER_DEBUG_LOG
            Debug.Log(string.Format("Starting Iteration on map {0}", txa.name));
#endif

            /* ************************************* */
            var bd = new BenchmarkData(this);
            CurrentMapIteration = 0;
            RandomStrategy.RandomizeWorldPortals();
            while (CurrentMapIteration < NumberOfRuns)
            {
#if PATHTESTER_DEBUG_LOG
                Debug.Log(string.Format("Computing Path {0} of {1}", CurrentMapIteration + 1, NumberOfRuns));
#endif

                // Debug.Log("Iteration " + counter);
                if (CurrentMapIteration % ScrambleRate == 0)
                {
#if PATHTESTER_DEBUG_LOG
                    Debug.Log("Randomizing Portals");
#endif
                    RandomStrategy.RandomizeWorldPortals();
                }

                var currentPos = RandomFreePosition();
                lastSquare = currentPos;
                var targetPos = RandomFreePosition();
                Debug.Log(string.Format("From {0} to {1}", currentPos, targetPos));
                AgentIndicator.GridPosition = new MapSquare(currentPos.x, currentPos.y);
                TargetIndicator.GridPosition = new MapSquare(targetPos.x, targetPos.y);

                // Avoid Same Area Paths.
                if (IsSameAreaPath(currentPos, targetPos))
                {
                    // Ignore Path. Repick.
                    continue;
                }

                /* BENCHMARK */
                InitializeSRD(currentPos, targetPos);

                /* ********* */
                UpdateAllPortalInArea(currentPos);
                while (lastSquare != targetPos)
                {
                    this.executionError = false;

                    // while (ExecutionError) {
#if PATHTESTER_DEBUG_LOG
                    Debug.Log(string.Format("Execution Step from {0} to {1}", lastSquare, targetPos));
#endif
                    currentPos = lastSquare;
                    if (ThePathfinder.AgentBelief.Hierarchical)
                    {
                        ThePathfinder.AgentBelief.CurrentTarget = targetPos;
                    }

                    Debug.Log("======= COMPUTE PATH ========");
                    var path = ThePathfinder.PathFind(currentPos, targetPos);

                    /* BENCHMARK */
                    UpdateSRD();
                    if (path == null)
                    {
                        if (ThePathfinder.AgentBelief.Hierarchical)
                        {
                            path = BeliefRevisionLoop(currentPos, targetPos, path);
                        }

                        if (path == null)
                        {
                            srd.PathFound = false;
#if PATHTESTER_DEBUG_LOG
                            Debug.Log("No path found!");
#endif
                            break;
                        }
                    }

                    srd.PathFound = true;

                    /* ********* */
                    var pathList = new List<MapSquare>(path);
                    pathList.Reverse();
                    if (ThePathfinder.AgentBelief.Hierarchical)
                    {
                        yield return StartCoroutine(ExecuteHierarchicalPath(pathList));
                    }
                    else
                    {
                        yield return StartCoroutine(ExecutePath(pathList));
                    }

                    // Debug.Log(lastSquare.ToString() + " " + targetPos.ToString());
                }

                CurrentMapIteration++;
                bd.RunsData.Add(srd);
            }

            bd.PrintToFile();
        }

#if PATHTESTER_DEBUG_LOG
        myLogger.Log("TEST COMPLETED");
#endif
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
        srd = new SingleRunData { StartingPoint = currentPos.ToString(), TargetPoint = targetPos.ToString() };
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
    private Path<MapSquare> BeliefRevisionLoop(MapSquare currentPos, MapSquare targetPos, Path<MapSquare> path)
    {
#if PATHTESTER_DEBUG_LOG
        Debug.Log("Portal belief revision!");
#endif
        var belief = ThePathfinder.AgentBelief as IMapHierarchicalBelief;
        if (belief == null)
        {
            Debug.LogError("BELIEF IS NULL!");
            throw new Exception("BELIEF IS NULL!");
        }

        var t = 10.0f;
        const float Tmin = 1.0f;
        while (path == null && t > Tmin)
        {
            belief.OpenOldPortals(t);
            path = ThePathfinder.PathFind(currentPos, targetPos);
            if (path == null)
            {
                t--;
            }
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
        Debug.Log("Starting Path Execution");
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
            if (!BDPMap.Instance.IsFree(nextPos))
            {
                bool changed;
                srd.UpdateTicks += MethodProfiler.ProfileMethod(
                    ThePathfinder.AgentBelief.UpdateBelief, 
                    nextPos, 
                    false, 
                    out changed);
                this.executionError = true;
                lastSquare = currentPos;
                break;
            }

            AgentIndicator.GridPosition = new MapSquare(nextPos.x, nextPos.y);
            var nextPosArea = BDPMap.Instance.GetArea(nextPos);

            // If enter a new area, update all the portals in the area.
            if (BDPMap.Instance.GetArea(currentPos) != nextPosArea)
            {
                var changed = UpdateAllPortalInArea(nextPos);
                if (changed)
                {
#if PATHTESTER_DEBUG_LOG
                    Debug.Log("Discrepancy Found!");
#endif
                    this.executionError = true;
                    lastSquare = currentPos;
                    break;
                }
            }

            currentPos = nextPos;
            srd.PathLenght++;
            if (currentPos == targetPos)
            {
                // pathCompleted = true;
                this.executionError = false;
                lastSquare = currentPos;
                break;
            }

            stepIndex++;

            // yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(0.5f);
        }
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
            Debug.Log(pathList[i] + " " + pathList[i + 1]);
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
        Debug.Log("Starting Hierarchical Path Execution");
#endif
        var mapInconsistency = false;
        // var pathCompleted = false;
        var stepIndex = 1;
        var currentHighLevelPos = pathList[0];
        var targetSquare = pathList[pathList.Count - 1];
        DrawDebugPath(pathList, Color.red);
        while (!mapInconsistency)
        {
            var nextHighLevelPos = pathList[stepIndex];

            // Expand the first step.
            var path = ThePathfinder.PathFindOnRealMap(
                currentHighLevelPos, 
                nextHighLevelPos, 
                BDPMap.Instance.GetArea(currentHighLevelPos));
            if (path == null)
            {
                Debug.Log("No low level path.");
                UpdateAllPortalInArea(currentHighLevelPos);
                this.executionError = true;

                // UpdateAllPortalInArea(currentHighLevelPos);
                // UpdateAllPortalInArea(nextHighLevelPos);
                break;
            }

            var subPathList = new List<MapSquare>(path) { currentHighLevelPos };
            subPathList.Reverse();

            // --
            this.executionError = false;
            yield return StartCoroutine(ExecutePath(subPathList));
            if (this.executionError)
            {
                Debug.Log("Error on ExecutePath.");
                mapInconsistency = true;
                break;
            }

            stepIndex++;
            currentHighLevelPos = nextHighLevelPos;
            if (currentHighLevelPos == targetSquare)
            {
                break;
            }

            nextHighLevelPos = pathList[stepIndex];
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
        for (var x = 0; x < BDPMap.Instance.Width; x++)
        {
            for (var y = 0; y < BDPMap.Instance.Height; y++)
            {
                var tryPos = new MapSquare(x, y);
                if (!BDPMap.Instance.IsFree(tryPos) || BDPMap.Instance.IsPortalSquare(tryPos))
                {
                    continue;
                }

                if (this.r.Next(0, count) == 0)
                {
                    chosenPos = tryPos;
                }

                count++;
            }
        }

        return chosenPos;
    }

    /// <summary>
    /// The update all portal in area.
    /// </summary>
    /// <param name="ms">
    /// The ms.
    /// </param>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    private bool UpdateAllPortalInArea(MapSquare ms)
    {
        var mapSquareArea = BDPMap.Instance.GetArea(ms);
        var pgs = BDPMap.Instance.GetPortalGroupByAreas(mapSquareArea);
        var changed = false;
        foreach (var updateSquare in pgs.Select(pg => pg.NearestPortal(ms)).Select(p => p[mapSquareArea]))
        {
            if (updateSquare == null)
            {
                Debug.Log("ERROR");
                break;
            }

            bool tmpChange;
            this.srd.UpdateTicks += MethodProfiler.ProfileMethod(
                this.ThePathfinder.AgentBelief.UpdateBelief, 
                updateSquare, 
                BDPMap.Instance.IsFree(updateSquare), 
                out tmpChange);
            if (tmpChange)
            {
                changed = true;
            }
        }

        return changed;
    }
}
