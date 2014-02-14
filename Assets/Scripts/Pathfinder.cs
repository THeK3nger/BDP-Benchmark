using RoomOfRequirement.Search;
using UnityEngine;

public class Pathfinder : MonoBehaviour {

    public BDPMap GameMap;
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
		double d = GameMap.PortalConnectivity.GetEdgeLabel(pg1,pg2);
		if (d != 0 &&
		    GameMap.PortalConnectivity.GetVertexLabel(pg1) &&
		    GameMap.PortalConnectivity.GetVertexLabel(pg2)) {
			return d;
		}
		return Mathf.Infinity;
	}

	public Path<MapSquare> PathFind(MapSquare start, MapSquare target) {
		Path<MapSquare> path = AStar.FindPath<MapSquare>(
			AgentBelief,
			start,
			target,
			MapSquare.Distance,
			(ms) => { return MapSquare.Distance(ms,target); }
		);
        //if (AgentBelief.Hierarchical) {
//            IMapHierarchicalBelief hAgent = (IMapHierarchicalBelief)AgentBelief;
//            return hAgent.ExpandPath(path);
//        }
        return path;
	}

	public Path<MapSquare> PathFindOnRealMap(MapSquare start, MapSquare target) {
        Path<MapSquare> path = AStar.FindPath<MapSquare>(
            Omniscient,
            start,
            target,
            MapSquare.Distance,
            (ms) => { return MapSquare.Distance(ms, target); }
        );
        return path;
	}
}
