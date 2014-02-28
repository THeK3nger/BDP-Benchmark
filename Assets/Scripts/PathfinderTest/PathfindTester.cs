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
    [Range(1, 1000)]
    public int ScrambleRate;

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
    public IPortalsRandomStrategy RandomStrategy { get; private set; }

    /// <summary>
    /// Return the name of he map.
    /// </summary>
    public string CurrentMapName { get { return allMaps[CurrentMapIndex].name; } }

    bool ExecutionError = false;

    SingleRunData srd;

    MapSquare lastSquare;

    public AgentPositioning AgentIndicator;
    public AgentPositioning TargetIndicator;

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
    void Start() {
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
            allMaps.Add((TextAsset)o);
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
            BDPMap.Instance.MapFile = txa;
            BDPMap.Instance.ComputeMap();
            ThePathfinder.AgentBelief.ResetBelieves();
#if PATHTESTER_DEBUG_LOG
            Debug.Log(String.Format("Starting Iteration on map {0}",txa.name));
#endif
            /* ************************************* */
            BenchmarkData bd = new BenchmarkData(this);
            CurrentMapIteration = 0;
            RandomStrategy.RandomizeWorldPortals();
            while (CurrentMapIteration < NumberOfRuns) {
#if PATHTESTER_DEBUG_LOG
                Debug.Log(String.Format("Computing Path {0} of {1}",CurrentMapIteration+1,NumberOfRuns));
#endif
                //Debug.Log("Iteration " + counter);
                if (CurrentMapIteration % ScrambleRate == 0) {
#if PATHTESTER_DEBUG_LOG
                    Debug.Log("Randomizing Portals");
#endif
                    RandomStrategy.RandomizeWorldPortals();
                }
                MapSquare currentPos = RandomFreePosition();
                lastSquare = currentPos;
                MapSquare targetPos = RandomFreePosition();
                Debug.Log(String.Format("From {0} to {1}", currentPos, targetPos));
                TargetIndicator.GridPosition = new MapSquare(targetPos.x, targetPos.y);
                // Avoid Same Area Paths.
                if (IsSameAreaPath(currentPos, targetPos)) {
                    // Ignore Path. Repick.
                    continue;
                }
                /* BENCHMARK */
                InitializeSRD(currentPos, targetPos);
                /* ********* */
                UpdateAllPortalInArea(currentPos);
                while (lastSquare != targetPos) {
                ExecutionError = false;
                //while (ExecutionError) {
#if PATHTESTER_DEBUG_LOG
                    Debug.Log(String.Format("Execution Step from {0} to {1}",lastSquare,targetPos));
#endif
                    currentPos = lastSquare;
                    if (ThePathfinder.AgentBelief.Hierarchical) ThePathfinder.AgentBelief.CurrentTarget = targetPos;
                    Path<MapSquare> path = ThePathfinder.PathFind(currentPos, targetPos);
                    /* BENCHMARK */
                    UpdateSRD();
                    pathfindingCall++;
                    if (path == null) {
                        if (ThePathfinder.AgentBelief.Hierarchical) {
                            path = BeliefRevisionLoop(currentPos, targetPos, path);
                        }
                        if (path == null) {
                            srd.PathFound = false;
#if PATHTESTER_DEBUG_LOG
                            Debug.Log("No path found!");
#endif
                            break;
                        }
                    }
                    srd.PathFound = true;
                    /* ********* */
                    List<MapSquare> pathList = new List<MapSquare>(path);
                    pathList.Reverse();
                    if (ThePathfinder.AgentBelief.Hierarchical) {
                        yield return StartCoroutine(ExecuteHierarchicalPath(pathList));
                    } else {
                        yield return StartCoroutine(ExecutePath(pathList));
                    }
                    //Debug.Log(lastSquare.ToString() + " " + targetPos.ToString());
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

    private void InitializeSRD(MapSquare currentPos, MapSquare targetPos) {
        srd = new SingleRunData();
        srd.StartingPoint = currentPos.ToString();
        srd.TargetPoint = targetPos.ToString();
    }

    private void UpdateSRD() {
        srd.PathfindingTicks += AStar.ElapsedTime;
        srd.ExploredNodes += AStar.ExpandedNodes;
        srd.MaxMemoryUsage = Mathf.Max(srd.MaxMemoryUsage, AStar.MaxMemoryQueue);
        srd.NumberOfAttempts++;
    }

    private bool IsSameAreaPath(MapSquare currentPos, MapSquare targetPos) {
        return BDPMap.Instance.GetArea(currentPos) == BDPMap.Instance.GetArea(targetPos) && AvoidSameAreaPath;
    }

    private Path<MapSquare> BeliefRevisionLoop(MapSquare currentPos, MapSquare targetPos, Path<MapSquare> path) {
#if PATHTESTER_DEBUG_LOG
        Debug.Log("Portal belief revision!");
#endif
        IMapHierarchicalBelief belief = ThePathfinder.AgentBelief as IMapHierarchicalBelief;
        float T = 10.0f;
        float Tmin = 1.0f;
        while (path == null && T > Tmin) {
            belief.OpenOldPortals(T);
            path = ThePathfinder.PathFind(currentPos, targetPos);
            if (path == null) {
                T--;
            }
        }
        return path;
    }

    public IEnumerator ExecutePath(IList<MapSquare> pathList) {
#if PATHTESTER_DEBUG_LOG
        Debug.Log("Starting Path Execution");
#endif
        bool mapInconsistency = false;
        bool pathCompleted = false;
        int stepIndex = 1;
        MapSquare currentPos = pathList[0];
        MapSquare targetPos = pathList[pathList.Count - 1];
        MapSquare nextPos;
        for (int i = 0; i < pathList.Count - 1; ++i) {
            var start = MapRenderer.Instance.Grid2Cartesian(pathList[i]);
            var end = MapRenderer.Instance.Grid2Cartesian(pathList[i + 1]);
            Debug.DrawLine(new Vector3(start.x, start.y, 0), new Vector3(end.x, end.y, 0), Color.yellow, 2.0f);
            Debug.Log(pathList[i] + " " + pathList[i + 1]);
        }
        while (!pathCompleted && !mapInconsistency) {
            nextPos = pathList[stepIndex];
            if (!BDPMap.Instance.IsFree(nextPos)) {
                bool changed;
                srd.UpdateTicks += MethodProfiler.ProfileMethod(ThePathfinder.AgentBelief.UpdateBelief, nextPos, false, out changed);
                ExecutionError = true;
                lastSquare = currentPos;
                break;
            }
            AgentIndicator.GridPosition = new MapSquare(nextPos.x, nextPos.y);
            int nextPosArea = BDPMap.Instance.GetArea(nextPos);
            // If enter a new area, update all the portals in the area.
            if (BDPMap.Instance.GetArea(currentPos) != nextPosArea) {
                bool changed = UpdateAllPortalInArea(nextPos);
                if (changed) {
#if PATHTESTER_DEBUG_LOG
                    Debug.Log("Discrepancy Found!");
#endif
                    ExecutionError = true;
                    lastSquare = currentPos;
                    break;
                }
            }
            currentPos = nextPos;
            srd.PathLenght++;
            if (currentPos == targetPos) {
                //pathCompleted = true;
                ExecutionError = false;
                lastSquare = currentPos;
                break;
            }
            stepIndex++;
            //yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(0.5f);
        }
    }

    public IEnumerator ExecuteHierarchicalPath(IList<MapSquare> pathList) {
#if PATHTESTER_DEBUG_LOG
        Debug.Log("Starting Hierarchical Path Execution");
#endif
        bool mapInconsistency = false;
        bool pathCompleted = false;
        int stepIndex = 1;
        MapSquare currentHighLevelPos = pathList[0];
        MapSquare targetSquare = pathList[pathList.Count - 1];
        MapSquare nextHighLevelPos;
        for (int i = 0; i < pathList.Count-1; ++i) {
            var start = MapRenderer.Instance.Grid2Cartesian(pathList[i]);
            var end = MapRenderer.Instance.Grid2Cartesian(pathList[i+1]);
            Debug.DrawLine(new Vector3(start.x,start.y,0),new Vector3(end.x,end.y,0),Color.red,2.0f);
            Debug.Log(pathList[i] + " " + pathList[i + 1]);
        }
        while (!pathCompleted && !mapInconsistency) {
            nextHighLevelPos = pathList[stepIndex];
            // Expand the first step.
            Path<MapSquare> path = ThePathfinder.PathFindOnRealMap(currentHighLevelPos, nextHighLevelPos, BDPMap.Instance.GetArea(nextHighLevelPos));
            if (path == null) {
                ExecutionError = true;
                //UpdateAllPortalInArea(currentHighLevelPos);
                //UpdateAllPortalInArea(nextHighLevelPos);
                break;
            }
            List<MapSquare> subPathList = new List<MapSquare>(path);
            subPathList.Reverse();
            // --
            ExecutionError = false;
            yield return StartCoroutine(ExecutePath(subPathList));
            if (ExecutionError) {
                Debug.Log("Error on ExecutePath.");
                mapInconsistency = true;
                break;
            }
            stepIndex++;
            currentHighLevelPos = nextHighLevelPos;
            if (currentHighLevelPos == targetSquare) break;
            nextHighLevelPos = pathList[stepIndex];
        }
    }

    MapSquare RandomFreePosition() {
        MapSquare chosenPos = null;
        int count = 1;
        for (int x = 0; x < BDPMap.Instance.Width; x++) {
            for (int y = 0; y < BDPMap.Instance.Height; y++) {
                var tryPos = new MapSquare(x, y);
                if (BDPMap.Instance.IsFree(tryPos) && !BDPMap.Instance.IsPortalSquare(tryPos)) {
                    if (r.Next(0, count) == 0) {
                        chosenPos = tryPos;
                    }
                    count++;
                }
            }
        }
        return chosenPos;
    }

    bool UpdateAllPortalInArea(MapSquare ms) {
        int msArea = BDPMap.Instance.GetArea(ms);
        var pgs = BDPMap.Instance.GetPortalGroupByAreas(msArea);
        bool changed = false;
        foreach (PortalGroup pg in pgs) {
            Portal p = pg.NearestPortal(ms);
            MapSquare updateSquare = p[msArea];
            if (updateSquare == null) { Debug.Log("ERROR"); break; }
            bool tmpChange;
            srd.UpdateTicks += MethodProfiler.ProfileMethod(ThePathfinder.AgentBelief.UpdateBelief,
                                                            updateSquare,
                                                            BDPMap.Instance.IsFree(updateSquare),
                                                            out tmpChange);
            if (tmpChange) changed = true;
        }
        return changed;
    }
}
