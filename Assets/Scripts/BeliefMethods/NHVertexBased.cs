using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class NHVertexBased : MonoBehaviour, IMapBelief  {

    public BDPMap OriginalMap;

    public BDPMap Original { get { return OriginalMap; } }
	public bool Hierarchical {get { return false; } }
	public MapSquare CurrentTarget { get; set; }

	Dictionary<MapSquare,bool> portalSquares =  new Dictionary<MapSquare, bool>();

	void Awake() {
		CleanBelieves();
	}

	public bool IsFree (MapSquare ms) {
		if (portalSquares.ContainsKey(ms)) {
			return portalSquares[ms];
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
		if (portalSquares.ContainsKey(ms)) {
			var pgs = Original.GetPortalGroupBySquare(ms);
			if (pgs == null) {
				Debug.LogError(String.Format("No Portal Group for square {0}",ms));
				//Debug.Log(Original.PortalSquares.ContainsKey(ms));
				Debug.Log(portalSquares.ContainsKey(ms));
				Debug.Log(state);
				Debug.Log(ms);
				Original.PrintStateAround(ms,4);
			}
			int area = BDPMap.Instance.GetArea(ms);
			if (pgs.Count > 1) {
                bool changed = portalSquares[ms] != state;
				portalSquares[ms] = state;
				return changed;
			}
			foreach(Portal p in pgs[0].Portals) {
				MapSquare pms = p[area];
				if (null == pms) { 
					Debug.LogError(String.Format("No map square in {0} for the area {1}",pms,area));
					Debug.Log(p);
					Debug.Log(area);
					Debug.Log(state);
					Debug.Log(ms);
					Original.PrintStateAround(p.LinkedSquares.First,4);
					Debug.Log("----------------------");
					continue; 
				}
				portalSquares[pms] = state;
			}
		}
        return false;
	}

	public IEnumerable<MapSquare> Neighbours (MapSquare node) {
		return GetNeighbours(node);
	}

	public int MemoryByteUsed () {
		return BDPMap.Instance.PortalsNumber;
	}

	public void CleanBelieves () {
		// Initialize Internal Representation
		foreach (MapSquare ms in BDPMap.Instance.PortalSquares) {
			if (portalSquares.ContainsKey(ms)) {
				portalSquares[ms] = true;
			} else {
				portalSquares.Add(ms,true);
			}
		}
	}

	public void ResetBelieves() {
		portalSquares = new Dictionary<MapSquare, bool>();
	}
}
