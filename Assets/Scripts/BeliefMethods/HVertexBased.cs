using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HVertexBased : MonoBehaviour, IMapBelief {

	public Map2D OriginalMap;
	
	public Map2D Original { get { return OriginalMap; } }
	public bool Hierarchical {get { return true; } }
	public MapSquare CurrentTarget { get; set; }

	Dictionary<PortalGroup,bool> portalPassability = new Dictionary<PortalGroup, bool>();

	// Use this for initialization
	void Awake () {
		CleanBelieves();
		Debug.Log("MEMORY USED: " + MemoryByteUsed());
	}

//	public bool IsFree (MapSquare ms) {
//		if (Original.PortalSquares.ContainsKey(ms)) {
//			var pgs = Original.GetPortalGroupBySquare(ms);
//			foreach (PortalGroup pg in pgs) {
//				if (!portalPassability[pg]) return false;
//			}
//			return true;
//		}
//		return Original.IsFree(ms);
//	}

	public bool IsFree (MapSquare ms) {
		if (Original.PortalSquares.ContainsKey(ms)) {
			var pgs = Original.GetPortalGroupBySquare(ms);
			foreach (PortalGroup pg in pgs) {
				if (portalPassability[pg]) {
					return true;
				}
			}
		}
		return Original.IsFree(ms);
	}
	
	public bool IsFree (int x, int y) {
		return IsFree(new MapSquare(x,y));
	}

	public List<MapSquare> GetNeighbours(MapSquare ms) {
		List<MapSquare> result = new List<MapSquare>();
//		if (Original.PortalSquares.ContainsKey(ms)) {
//
//		} else {
		int area = Original.Areas[ms.x,ms.y];
		var pgs = Original.GetPortalGroupByAreas(area);
		foreach (PortalGroup pg in pgs) {
			if (portalPassability[pg]) {
				Portal p = pg.NearestPortal(ms);
				if (p.LinkedAreas.First == area) result.Add(p.LinkedSquares.Second);
				if (p.LinkedAreas.Second == area) result.Add(p.LinkedSquares.First);
			}
		}
		if (area == Original.Areas[CurrentTarget.x,CurrentTarget.y]) {
			result.Add(CurrentTarget);
		}
//		}
		return result;
	}

	public IEnumerable<MapSquare> Neighbours (MapSquare node) {
		return GetNeighbours(node);
	}

	public void CleanBelieves () {
		foreach (PortalGroup pg in Original.PortalConnectivity.Vertices) {
			if (portalPassability.ContainsKey(pg)) {
				portalPassability[pg] = true;
			} else {
				portalPassability.Add(pg,true);
			}
		}
	}

	public int MemoryByteUsed () {
		return portalPassability.Count;
	}

	public void UpdateBelief (MapSquare ms, bool state) {
		var pgs = Original.GetPortalGroupBySquare(ms);
		foreach (PortalGroup pg in pgs) {
			portalPassability[pg] = state;
		}
	}
}
