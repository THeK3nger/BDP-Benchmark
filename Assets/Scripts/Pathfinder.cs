using System;
using System.Collections;
using UnityEngine;

public class Pathfinder : MonoBehaviour {

	public Map2D GameMap;
	public IMapBelief AgentBelief;
	public AllFree AllFreeBase;
	public GameObject Agent;

	Vector2 agentPosition;

	bool Executed = false;

	//public enum BPFMethod { PORTALSQUARES, PORTALSTATE, HIERARCHICAL };
	//public BPFMethod UsedMethod;


	// Use this for initialization
	void Awake () {
		AgentBelief = Agent.GetComponent(typeof(IMapBelief)) as IMapBelief;
		AllFreeBase.Agent = AgentBelief;
		Debug.Log(AgentBelief);
	}
	
	// Update is called once per frame
	void Update () {
//		if (Time.time > 2 && !Executed) {
//			AStar.CollectProfiling = true;
//			// Set all pg to open.
//			foreach (PortalGroup pg in GameMap.PortalConnectivity.Vertices) {
//					GameMap.PortalConnectivity.SetVertexLabel(pg,true);
//			}
//			// TODO: Fix this mess! I want to close the 3-2 portal!
//			foreach (PortalGroup pg in GameMap.PortalConnectivity.Vertices) {
//				if ((pg.Connect(3,2))){
//					GameMap.PortalConnectivity.SetVertexLabel(pg,false);
//					AgentBelief.UpdateBelief(pg.,false);
//					break;
//				}
//			}
//			Path<MapSquare> path = AStar.FindPath<MapSquare>(
//				AgentBelief,
//				new MapSquare(1,1),
//				new MapSquare(13,9),
//				MapSquare.Distance,
//				(ms) => { return MapSquare.Distance(ms,new MapSquare(13,9)); }
//			);
//			string pathString = "";
//			foreach (MapSquare ms in path) {
//				pathString = "(" + ms.x + " " + ms.y + ") ->" + pathString;
//			}
//			Debug.Log(pathString);
//			Debug.Log(AStar.LastRunProfilingInfo["Expanded Nodes"]);
//			Debug.Log(AStar.LastRunProfilingInfo["Elapsed Time"]);
//			/*
//			pathString = "";
//			// Starting and Ending Points.
//			Vector2 start = new Vector2(1,1);
//			Vector2 end = new Vector2(13,9);
//			// Get starting/end Area
//			int areaStart = GameMap.Areas[(int) start.x, (int) start.y];
//			int areaEnd = GameMap.Areas[(int) end.x, (int) end.y];
//			if (areaEnd == areaStart) {
//				// TODO: Same area. No high level navigation required.
//			}
//			// Retireve Nearest Portal Group to Start;
//			PortalGroup PGStart = null;
//			PortalGroup PGEnd = null;
//			double minDist = Mathf.Infinity;
//			double tmp;
//			foreach (PortalGroup pg in GameMap.PortalConnectivity.Vertices) {
//				Tuple<int,int> areas = pg.LinkedAreas;
//				if (areas.First == areaStart || areas.Second == areaStart) {
//					pg.NearestPortal(start, out tmp);
//					if (tmp<minDist) {minDist = tmp; PGStart = pg; }
//				}
//			}
//			// Retireve Nearest Portal Group to End;
//			minDist = Mathf.Infinity;
//			foreach (PortalGroup pg in GameMap.PortalConnectivity.Vertices) {
//				Tuple<int,int> areas = pg.LinkedAreas;
//				if (areas.First == areaEnd || areas.Second == areaEnd) {
//					pg.NearestPortal(end, out tmp);
//					if (tmp<minDist) {minDist = tmp; PGEnd = pg; }
//				}
//			}
//
//			Path<PortalGroup> pathPG = AStar.FindPath<PortalGroup>(
//				GameMap.PortalConnectivity,
//				PGStart,
//				PGEnd,
//				PGBeliefDistance,
//				(pg1) => { return PortalGroup.Distance(pg1,end); }
//			);
//			foreach (PortalGroup pg in pathPG) {
//				pathString = "(" + pg + ") ->" + pathString;
//			}
//			Debug.Log(pathString);
//			Debug.Log(pathPG.TotalCost);
//			*/
//			Executed = true;
//		}
	}

	public double PGBeliefDistance(PortalGroup pg1, PortalGroup pg2) {
		if (pg1 == pg2) return 0;
		double d = GameMap.PortalConnectivity.GetEdgeLabel(pg1,pg2);
		if (d != 0 &&
		    GameMap.PortalConnectivity.GetVertexLabel(pg1) &&
		    GameMap.PortalConnectivity.GetVertexLabel(pg2)) {
			return d;
		}
		return Mathf.Infinity;
	}

	public Path<MapSquare> PathFind(MapSquare start, MapSquare target) {
		return AStar.FindPath<MapSquare>(
			AgentBelief,
			start,
			target,
			MapSquare.Distance,
			(ms) => { return MapSquare.Distance(ms,target); }
		);
	}

	public Path<MapSquare> PathFindOnRealMap(MapSquare start, MapSquare target) {
		AllFreeBase.CurrentTarget = target;
		AllFreeBase.Area = GameMap.Areas[start.x,start.y];
		return AStar.FindPath<MapSquare>(
			AllFreeBase,
			start,
			target,
			MapSquare.Distance,
			(ms) => { return MapSquare.Distance(ms,target); }
		);
	}
}
