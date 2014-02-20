#define PATHTESTER_DEBUG_LOG

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using RoomOfRequirement.Search;
using RoomOfRequirement.Logger;
using System.IO;
using System.Linq;

[RequireComponent(typeof(Pathfinder))]
public class PathfindTester : MonoBehaviour {
	
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
	[Range(1,1000)]
	public int ScrambleRate;

    /// <summary>
    /// The frequency of belief reset for the agent.
    /// </summary>
	[Range(1,1000)]
	public int BeliefResetRate;

    /// <summary>
    /// A reference to the main pathfinder object.
    /// </summary>
    public Pathfinder ThePathfinder { get; private set; }

    /// <summary>
    /// An instance of the RNG.
    /// </summary>
	System.Random r;

	/// <summary>
	/// Store the number of calls for the pathfinding.
	/// </summary>
	int pathfindingCall = 0;

    /// <summary>
    /// Store the list of maps.
    /// </summary>
	List<TextAsset> allMaps = new List<TextAsset>();

    /// <summary>
    /// A reference to a portal rondomizer algorithm.
    /// </summary>
    public IPortalsRandomStrategy RandomStrategy {get; private set;}

    /// <summary>
    /// Return the name of he map.
    /// </summary>
    public string CurrentMapName { get { return allMaps[CurrentMapIndex].name; } }

    bool ExecutionError = false;

    SingleRunData srd;

    MapSquare lastSquare;

    public AgentPositioning AgentIndicator;

#if PATHTESTER_DEBUG_LOG
    BasicLogger myLogger = new BasicLogger("PATHTEST");
#endif

    public int MapNumber {
        get { return allMaps.Count; }
    }

    public int CurrentMapIndex { get; private set; }
    public int CurrentMapIteration { get; private set; }

	void Awake() {

		ThePathfinder = gameObject.GetComponent<Pathfinder>();
	}

	// Use this for initialization
	void Start () {
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
	void LoadAllMaps() {
		UnityEngine.Object[] allMapObj = Resources.LoadAll("Maps");
		if (allMapObj.Length == 0) {
#if PATHTESTER_DEBUG_LOG
			myLogger.LogError("No Map Loaded in PathfinderTest");
#endif
		}
		foreach (UnityEngine.Object o in allMapObj) {
			allMaps.Add((TextAsset) o);
		}
#if PATHTESTER_DEBUG_LOG
        myLogger.Log(String.Format("{0} maps loaded!", allMaps.Count));
#endif
	}

	/// <summary>
	/// Main test loop. (Coroutine)
	/// </summary>
	public IEnumerator MainNHTestLoop() {
        CurrentMapIndex = 0;
		foreach (TextAsset txa in allMaps) {
            CurrentMapIndex++;
			/* Update the map and recompute the map. */
			ThePathfinder.GameMap.MapFile = txa;
			ThePathfinder.GameMap.ComputeMap();
            ThePathfinder.AgentBelief.ResetBelieves();
			/* ************************************* */
			BenchmarkData bd = new BenchmarkData(this);
            CurrentMapIteration = 0;
            RandomStrategy.RandomizeWorldPortals(ThePathfinder.GameMap);
            while (CurrentMapIteration < NumberOfRuns) {
			//Debug.Log("Iteration " + counter);
                if (CurrentMapIteration % ScrambleRate == 0) RandomStrategy.RandomizeWorldPortals(ThePathfinder.GameMap);
                if (CurrentMapIteration % BeliefResetRate == 0) ThePathfinder.AgentBelief.CleanBelieves();
			MapSquare currentPos = RandomFreePosition();
            MapSquare targetPos = RandomFreePosition();
			// Avoid Same Area Paths.
			if (ThePathfinder.GameMap.Areas[currentPos.x,currentPos.y] == ThePathfinder.GameMap.Areas[targetPos.x,targetPos.y]
			    && AvoidSameAreaPath) {
				continue;
			}
			/* BENCHMARK */
			srd = new SingleRunData();
			srd.StartingPoint = currentPos.ToString();
			srd.TargetPoint = targetPos.ToString();
			/* ********* */
			//UpdateAllPortalInArea(currentPos,srd);
            while (lastSquare != targetPos) {
				if (ThePathfinder.AgentBelief.Hierarchical) ThePathfinder.AgentBelief.CurrentTarget = targetPos;
				Path<MapSquare> path = ThePathfinder.PathFind(currentPos,targetPos);
				/* BENCHMARK */
				srd.PathfindingTicks += AStar.ElapsedTime;
				srd.ExploredNodes += AStar.ExpandedNodes;
				srd.MaxMemoryUsage = Mathf.Max( srd.MaxMemoryUsage, AStar.MaxMemoryQueue);
				pathfindingCall++;
				srd.NumberOfAttempts++;
				if (path == null) {
                    if (ThePathfinder.AgentBelief.Hierarchical) {
                        IMapHierarchicalBelief belief = ThePathfinder.AgentBelief as IMapHierarchicalBelief;
                        float T = 10.0f;
                        float Tmin = 1.0f;
                        while (path == null || T > Tmin) {
                            Debug.Log("Portal belief revision!");
                            belief.OpenOldPortals(T);
                            path = ThePathfinder.PathFind(currentPos, targetPos);
                            if (path == null) {
                                T--;
                            }
                        }
                    }
                    srd.PathFound = false;
#if PATHTESTER_DEBUG_LOG
					Debug.Log("No path found!");
#endif
					break;
				}
				srd.PathFound = true;
				/* ********* */
				List<MapSquare> pathList = new List<MapSquare>(path);
				pathList.Reverse();
                ExecutionError = false;
                if (ThePathfinder.AgentBelief.Hierarchical) {
                    yield return StartCoroutine(ExecuteHierarchicalPath(pathList));
                } else {
                    yield return StartCoroutine(ExecutePath(pathList));
                }
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

	public IEnumerator ExecutePath(IList<MapSquare> pathList) {
		bool mapInconsistency = false;
		bool pathCompleted = false;
		int stepIndex = 1;
        MapSquare currentPos = pathList[0];
        MapSquare targetPos = pathList[pathList.Count - 1];
		MapSquare nextPos;
		while (!pathCompleted && !mapInconsistency) {
			nextPos = pathList[stepIndex];
            if (!ThePathfinder.GameMap.IsFree(nextPos)) {
                //mapInconsistency = true;
                //if (ThePathfinder.AgentBelief.Original.PortalSquares.ContainsKey(nextPos)) {
                srd.UpdateTicks += MethodProfiler.ProfileMethod(ThePathfinder.AgentBelief.UpdateBelief, nextPos, false);
                //}
                ExecutionError = true;
                lastSquare = currentPos;
                break;
            }
            AgentIndicator.GridPosition = new Vector2(nextPos.x, (ThePathfinder.GameMap.Height) - nextPos.y);
			int nextPosArea = ThePathfinder.GameMap.Areas[nextPos.x,nextPos.y];
			// If enter a new area, update all the portals in the area.
			if (ThePathfinder.GameMap.Areas[currentPos.x,currentPos.y] != 
			    nextPosArea) {
				UpdateAllPortalInArea(nextPos);
			}
			currentPos = nextPos;
			srd.PathLenght++;
			if (currentPos == targetPos) { 
				//pathCompleted = true;
                ExecutionError = false;
                lastSquare = targetPos;
				break; 
			}
			stepIndex++;
			//yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(0.1f);
		}
	}

    public IEnumerator ExecuteHierarchicalPath(IList<MapSquare> pathList) {
        bool mapInconsistency = false;
        bool pathCompleted = false;
        int stepIndex = 1;
        MapSquare currentHighLevelPos = pathList[0];
        MapSquare targetSquare = pathList[pathList.Count - 1];
        MapSquare nextHighLevelPos;
        while (!pathCompleted && !mapInconsistency) {
            nextHighLevelPos = pathList[stepIndex];
            // Expand the first step.
            Path<MapSquare> path = ThePathfinder.PathFindOnRealMap(currentHighLevelPos, nextHighLevelPos);
            if (path == null) {
                ExecutionError = true;
                UpdateAllPortalInArea(currentHighLevelPos);
                UpdateAllPortalInArea(nextHighLevelPos);
                break;
            }
            List<MapSquare> subPathList = new List<MapSquare>(path);
            subPathList.Reverse();
            // --
            ExecutionError = false;
            yield return StartCoroutine(ExecutePath(subPathList));
            if (ExecutionError) break;
            stepIndex++;
            currentHighLevelPos = nextHighLevelPos;
            if (currentHighLevelPos == targetSquare) break;
            nextHighLevelPos = pathList[stepIndex];
        }
    }

	MapSquare RandomPosition() {
		int x = r.Next(0,ThePathfinder.GameMap.Width);
		int y = r.Next(0,ThePathfinder.GameMap.Height);
		return new MapSquare(x,y);
	}

	MapSquare RandomFreePosition() {
		int chosenX = -1;
		int chosenY = -1;
		int count = 1;
		for (int x=0;x<ThePathfinder.GameMap.Width;x++) {
			for (int y=0;y<ThePathfinder.GameMap.Height;y++) {
				{
					if (ThePathfinder.GameMap.IsFree(x,y) && !ThePathfinder.GameMap.PortalSquares.ContainsKey(new MapSquare(x,y)))
					{
						if (r.Next(0,count) == 0)
						{
							chosenX = x;
							chosenY = y;
						}
						count++;
					}
				}
			}
		}
		return new MapSquare(chosenX,chosenY);
	}

	void UpdateAllPortalInArea(MapSquare ms) {
		int msArea = ThePathfinder.GameMap.Areas[ms.x,ms.y];
		var pgs = ThePathfinder.GameMap.GetPortalGroupByAreas(msArea);
		foreach (PortalGroup pg in pgs) {
			Portal p = pg.NearestPortal(ms);
			MapSquare updateSquare = p[msArea];
			if (updateSquare == null) { Debug.Log("ERROR"); break; }
			srd.UpdateTicks += MethodProfiler.ProfileMethod(ThePathfinder.AgentBelief.UpdateBelief,
			                                                updateSquare,
			                                                ThePathfinder.GameMap.IsFree(updateSquare));
		}
	}
}
