using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HVertexBased : MonoBehaviour, IMapHierarchicalBelief {

	public bool Hierarchical {get { return true; } }
	public MapSquare CurrentTarget { get; set; }

    Dictionary<PortalGroup, bool> portalPassability = new Dictionary<PortalGroup, bool>();
    Dictionary<PortalGroup, float> portalTimestamp = new Dictionary<PortalGroup, float>();

	// Use this for initialization
	void Start () {
		//CleanBelieves();
		//Debug.Log("MEMORY USED: " + MemoryByteUsed());
	}

	public bool IsFree (MapSquare ms) {
        if (BDPMap.Instance.IsPortalSquare(ms)) {
            var pgs = BDPMap.Instance.GetPortalGroupBySquare(ms);
			foreach (PortalGroup pg in pgs) {
				if (portalPassability[pg]) {
					return true;
				}
			}
		}
        return BDPMap.Instance.IsFree(ms);
	}
	
	public bool IsFree (int x, int y) {
		return IsFree(new MapSquare(x,y));
	}

	public List<MapSquare> GetNeighbours(MapSquare ms) {
		List<MapSquare> result = new List<MapSquare>();
        int area = BDPMap.Instance.GetArea(ms);
        var pgs = BDPMap.Instance.GetPortalGroupByAreas(area);
		foreach (PortalGroup pg in pgs) {
			if (portalPassability[pg]) {
				Portal p = pg.NearestPortal(ms);
				if (p.LinkedAreas.First == area) result.Add(p.LinkedSquares.Second);
				if (p.LinkedAreas.Second == area) result.Add(p.LinkedSquares.First);
			}
		}
        if (area == BDPMap.Instance.GetArea(CurrentTarget)) {
			result.Add(CurrentTarget);
		}
		return result;
	}

	public IEnumerable<MapSquare> Neighbours (MapSquare node) {
		return GetNeighbours(node);
	}

	public void CleanBelieves () {
        foreach (PortalGroup pg in BDPMap.Instance.PortalConnectivity.Vertices) {
			if (portalPassability.ContainsKey(pg)) {
				portalPassability[pg] = true;
                portalTimestamp[pg] = Time.time;
			} else {
				portalPassability.Add(pg,true);
                portalTimestamp.Add(pg, Time.time);
			}
		}
	}

	public int MemoryByteUsed () {
        return BDPMap.Instance.PortalsNumber;
	}

	public bool UpdateBelief (MapSquare ms, bool state) {
        var pgs = BDPMap.Instance.GetPortalGroupBySquare(ms);
        bool changed = false;
        foreach (PortalGroup pg in pgs) {
            if (UpdateBelief(pg, state)) changed = true;
		}
        return changed;
	}

    public bool UpdateBelief(PortalGroup pg, bool state) {
        bool changed = false;
        if (portalPassability[pg] != state) {
            portalPassability[pg] = state;
            changed = true;
        }
        portalTimestamp[pg] = Time.time;
        return changed;
    }

	public void ResetBelieves() {
        CleanBelieves();
	}

    public void OpenOldPortals(float timeLimit) {
        foreach (PortalGroup pg in portalPassability.Keys) {
            if (portalTimestamp[pg] < timeLimit) {
                UpdateBelief(pg,true);
            }
        }
    }
}
