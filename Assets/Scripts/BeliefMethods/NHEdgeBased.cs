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
		return result;
	}

	public bool UpdateBelief (MapSquare ms, bool state) {
		var pgs = Original.GetPortalGroupBySquare(ms);
        bool changed = false;
		foreach (PortalGroup pg in pgs) {
			if (!state) {
                changed = portalPassability[pg]; // It was true? If true, it is changed.
				portalPassability[pg] = false;
			} else {
				foreach (Portal p in pg.Portals) {
					if (p.LinkedSquares.First == ms) {
						if (Original.IsFree (p.LinkedSquares.Second)) {
                            changed = portalPassability[pg] != state;
							portalPassability [pg] = true;
							return changed;
						}
                        changed = portalPassability[pg] != state;
						portalPassability [pg] = false;
                        return changed;
					}
					if (p.LinkedSquares.Second == ms) {
						if (Original.IsFree (p.LinkedSquares.First)) {
                            changed = portalPassability[pg] != state;
							portalPassability [pg] = true;
                            return changed;
						}
                        changed = portalPassability[pg] != state;
						portalPassability [pg] = false;
                        return changed;
					}
				}
			}
		}
        return changed;
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
