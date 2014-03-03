using RoomOfRequirement.Search;
using UnityEngine;
using System.Collections.Generic;

public class Pathfinder : MonoBehaviour {

	public IMapBelief AgentBelief;
	public IMapBelief Omniscient;
	public GameObject Agent;
    public GameObject OmniscientObject;

	Vector2 agentPosition;

	//bool Executed = false;

	// Use this for initialization
	void Awake () {
		AgentBelief = Agent.GetComponent(typeof(IMapBelief)) as IMapBelief;
        Omniscient = OmniscientObject.GetComponent(typeof(IMapBelief)) as IMapBelief;
		//AllFreeBase.Agent = AgentBelief;
		Debug.Log(AgentBelief);
	}
	
	// Update is called once per frame
	void Update () {

	}

	public double PGBeliefDistance(PortalGroup pg1, PortalGroup pg2) {
		if (pg1 == pg2) return 0;
        double d = BDPMap.Instance.PortalConnectivity.GetEdgeLabel(pg1, pg2);
		if (d != 0 &&
            BDPMap.Instance.PortalConnectivity.GetVertexLabel(pg1) &&
            BDPMap.Instance.PortalConnectivity.GetVertexLabel(pg2)) {
			return d;
		}
		return Mathf.Infinity;
	}

	public Path<MapSquare> PathFind(MapSquare start, MapSquare target) {
		Path<MapSquare> path = AStar.FindPath<MapSquare>(
			start,
			target,
            AgentBelief.Neighbours,
			MapSquare.Distance,
			(ms) => { return MapSquare.Distance(ms,target); }
		);
        return path;
	}

	public Path<MapSquare> PathFindOnRealMap(MapSquare start, MapSquare target, int area) {
        Path<MapSquare> path = AStar.FindPath<MapSquare>(
            start,
            target,
            (ms) => {
                List<MapSquare> SameAreaNeighbours = new List<MapSquare>();
                foreach (MapSquare m in Omniscient.Neighbours(ms)) {
                    if (BDPMap.Instance.GetArea(m) == area || m == target) {
                        SameAreaNeighbours.Add(m);
                    }
                }
                return SameAreaNeighbours;
            },
            MapSquare.Distance,
            (ms) => { if (area == 0 || BDPMap.Instance.GetArea(ms) == area) return MapSquare.Distance(ms, target); else return 10000000; }
        );
        return path;
	}
}
