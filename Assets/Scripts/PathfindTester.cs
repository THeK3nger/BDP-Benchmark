﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PathfindTester : MonoBehaviour {

	public int Seed;
	public Pathfinder ThePathfinder;
	[Range(0.0f,1.0f)]
	public float InconsistencyRate;
	public int NumberOfRuns;
	public bool AvoidSameAreaPath;


	[Range(1,1000)]
	public int ScrambleRate;

	[Range(1,1000)]
	public int BeliefResetRate;

	MapSquare currentPos;
	MapSquare targetPos;

	System.Random r;

	//int runs = 0;
	int pathfindingCall = 0;

	// Use this for initialization
	void Start () {
		// Set the seed to allow multiple test.
		r = new System.Random(Seed);
		AStar.CollectProfiling = true;
		StartCoroutine(MainNHTestLoop());
	}
	
	// Update is called once per frame
	void Update () {
	}

	public IEnumerator MainNHTestLoop() {
		BenchmarkData bd = new BenchmarkData();
		bd.AgentType = ThePathfinder.AgentBelief.ToString();
		bd.BeliefMemoryUsed = ThePathfinder.AgentBelief.MemoryByteUsed();
		bd.ScrambleRate = ScrambleRate;
		bd.IncosistencyRate = InconsistencyRate;
		int counter = 0;
		while (counter < NumberOfRuns) {
			Debug.Log("Iteration " + counter);
			if (counter % ScrambleRate == 0) RandomizeWorldPortals();
			if (counter % BeliefResetRate == 0) ThePathfinder.AgentBelief.CleanBelieves();
			SingleRunData srd = new SingleRunData();
			currentPos = RandomFreePosition();
			targetPos = RandomFreePosition();
			if (ThePathfinder.GameMap.Areas[currentPos.x,currentPos.y] == ThePathfinder.GameMap.Areas[targetPos.x,targetPos.y]
			    && AvoidSameAreaPath) {
				continue;
			}
//			Debug.Log("FROM: " + currentPos + " TO: " + targetPos);
			srd.StartingPoint = currentPos.ToString();
			srd.TargetPoint = targetPos.ToString();
			UpdateAllPortalInArea(currentPos,srd);
			while (currentPos != targetPos) {
				if (ThePathfinder.AgentBelief.Hierarchical) ThePathfinder.AgentBelief.CurrentTarget = targetPos;
				Path<MapSquare> path = ThePathfinder.PathFind(currentPos,targetPos);
				srd.PathfindingTicks += AStar.ElapsedTime;
				srd.ExploredNodes += AStar.ExpandedNodes;
				srd.MaxMemoryUsage = Mathf.Max( srd.MaxMemoryUsage, AStar.MaxMemoryQueue);
				pathfindingCall++;
				srd.NumberOfAttempts++;
				if (path == null) {
					//Debug.Log("No path found!");
					srd.PathFound = false;
					break;
				}
				srd.PathFound = true;
				List<MapSquare> pathList = new List<MapSquare>(path);
				pathList.Reverse();
				if (ThePathfinder.AgentBelief.Hierarchical) {
					pathList = ExpandHierarchicalPath(pathList,srd);
					if (pathList==null) {
						//Debug.Log("No path found!");
						srd.PathFound = false;
						break;
					}
				}
				yield return StartCoroutine(ExecutePath(pathList,srd));
			}
			counter++;
			bd.RunsData.Add(srd);
			ThePathfinder.GameMap.DrawAreaMap();
			ThePathfinder.GameMap.DrawMap();
		}
		bd.PrintToFile();
		Debug.Log("TEST COMPLETED");
	}

	public List<MapSquare> ExpandHierarchicalPath(List<MapSquare> old, SingleRunData srd) {
		var result = new List<MapSquare>();
		result.Add(old[0]);
		string log = "" + old[0];
		for (int i=0;i<old.Count-1;i++) {
			log += " : " + old[i+1];
			Path<MapSquare> p = ThePathfinder.PathFindOnRealMap(old[i],old[i+1]);
			srd.PathfindingTicks += AStar.ElapsedTime;
			srd.ExploredNodes += AStar.ExpandedNodes;
			srd.MaxMemoryUsage = Mathf.Max( srd.MaxMemoryUsage, AStar.MaxMemoryQueue);
			if (p==null) return null;
			List<MapSquare> tmp = new List<MapSquare>(p);
			tmp.Reverse();
			for (int j=1;j<tmp.Count;j++) {
				result.Add(tmp[j]);
			}
		}
		//Debug.Log(log);
		return result;
	}

	public IEnumerator ExecutePath(List<MapSquare> pathList, SingleRunData srd) {
		srd.PathLenght = 0;
		bool mapInconsistency = false;
		bool pathCompleted = false;
		int stepIndex = 1;
		MapSquare nextPos;
		while (!pathCompleted && !mapInconsistency) {
			nextPos = pathList[stepIndex];
			if (!ThePathfinder.GameMap.IsFree(nextPos)) {
//				Debug.Log("Path Inconsistency");
//				Debug.Log(currentPos + " " + nextPos);
				mapInconsistency = true;
				srd.UpdateTicks += MethodProfiler.ProfileMethod(ThePathfinder.AgentBelief.UpdateBelief,nextPos,false);
				break;
			}
			int nextPosArea = ThePathfinder.GameMap.Areas[nextPos.x,nextPos.y];
			// If enter a new area, update all the portals in the area.
			if (ThePathfinder.GameMap.Areas[currentPos.x,currentPos.y] != 
			    nextPosArea) {
				UpdateAllPortalInArea(nextPos,srd);
			}
			currentPos = nextPos;
			srd.PathLenght++;
			if (currentPos == targetPos) { 
				pathCompleted = true; 
				break; 
			}
			stepIndex++;
			yield return new WaitForEndOfFrame();
		}
	}

	public void RandomizeWorldPortals() {
		foreach (PortalGroup pg in ThePathfinder.GameMap.PortalConnectivity.Vertices) {
			double ratioToss = r.NextDouble();
			if (ratioToss < InconsistencyRate) {
				double sideToss = r.NextDouble();
				if (sideToss < 0.5) {
				ThePathfinder.GameMap.SetPortalGroup(pg,false,pg.LinkedAreas.First);
				} else {
				ThePathfinder.GameMap.SetPortalGroup(pg,false,pg.LinkedAreas.Second);
				}
			} else {
				ThePathfinder.GameMap.SetPortalGroup(pg,true,pg.LinkedAreas.First);
				ThePathfinder.GameMap.SetPortalGroup(pg,true,pg.LinkedAreas.Second);
			}
		}
	}

	public void RandomizeWorldPortals2() {
		foreach (PortalGroup pg in ThePathfinder.GameMap.PortalConnectivity.Vertices) {
			double ratioToss = r.NextDouble();
			if (ratioToss < InconsistencyRate) {
				bool currentState = ThePathfinder.GameMap.GetPortalGroupState (pg, pg.LinkedAreas.First);
				ThePathfinder.GameMap.SetPortalGroup(pg,!currentState,pg.LinkedAreas.First);
				ThePathfinder.GameMap.SetPortalGroup(pg,!currentState,pg.LinkedAreas.Second);
			}
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

	void UpdateAllPortalInArea(MapSquare ms, SingleRunData srd) {
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