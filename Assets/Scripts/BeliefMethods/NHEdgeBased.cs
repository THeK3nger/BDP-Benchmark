using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NHEdgeBased : MonoBehaviour, IMapBelief {

    public BDPMap OriginalMap;
	public bool Hierarchical {get { return false; } }
	public MapSquare CurrentTarget { get; set; }

	Dictionary<PortalGroup,bool> portalPassability = new Dictionary<PortalGroup, bool>();

	// Use this for initialization
	void Start () {
		//CleanBelieves();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

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

	/// <summary>
	/// Gets the square valid neighbours.
	/// </summary>
	/// <returns>The neighbours.</returns>
	public List<MapSquare> GetNeighbours(MapSquare ms) {
		var result = new List<MapSquare>();
		// Add up.
		if (IsFree(ms.Up)) result.Add(ms.Up);
		// Add down.
		if (IsFree(ms.Down)) result.Add(ms.Down);
		// Add left.
		if (IsFree(ms.Left)) result.Add(ms.Left);
		// Add right.
		if (IsFree(ms.Right)) result.Add(ms.Right);
//		Grid<int> areas = OriginalMap.Areas;
//		if (areas[ms.x,ms.y] != areas[ms.Up.x,ms.Up.y] ||
//		    areas[ms.x,ms.y] != areas[ms.Down.x,ms.Down.y] ||
//		    areas[ms.x,ms.y] != areas[ms.Left.x,ms.Left.y] ||
//		    areas[ms.x,ms.y] != areas[ms.Right.x,ms.Right.y]) 
//		    {
//			foreach (PortalGroup pg in portalPassability.Keys) {
//				if (pg.Contains(ms) && !portalPassability[pg]) {
//					if (pg.Contains(ms.Up)) result.Remove(ms.Up);
//					if (pg.Contains(ms.Down)) result.Remove(ms.Down);
//					if (pg.Contains(ms.Left)) result.Remove(ms.Left);
//					if (pg.Contains(ms.Right)) result.Remove(ms.Right);
//				}
//			}
//		}
		return result;
	}

	public void UpdateBelief (MapSquare ms, bool state) {
		var pgs = Original.GetPortalGroupBySquare(ms);
		foreach (PortalGroup pg in pgs) {
			if (!state) {
				portalPassability[pg] = false;
			} else {
				foreach (Portal p in pg.Portals) {
					if (p.LinkedSquares.First == ms) {
						if (Original.IsFree (p.LinkedSquares.Second)) {
							portalPassability [pg] = true;
							return;
						}
						portalPassability [pg] = false;
						return;
					}
					if (p.LinkedSquares.Second == ms) {
						if (Original.IsFree (p.LinkedSquares.First)) {
							portalPassability [pg] = true;
							return;
						}
						portalPassability [pg] = false;
						return;
					}
				}
			}
		}
	}

	public IEnumerable<MapSquare> Neighbours (MapSquare node) {
		return GetNeighbours(node);
	}

    public BDPMap Original {
		get {
			return OriginalMap;
		}
	}

	public int MemoryByteUsed () {
		return portalPassability.Count;
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

	public void ResetBelieves() {
		//portalSquares = new Dictionary<MapSquare, bool>();
	}

}
