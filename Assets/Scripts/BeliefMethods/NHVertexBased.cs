using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NHVertexBased : MonoBehaviour, IMapBelief  {

	public Map2D OriginalMap;

	public Map2D Original { get { return OriginalMap; } }
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
	
	public void UpdateBelief (MapSquare ms, bool state) {
		if (portalSquares.ContainsKey(ms)) {
			var pgs = Original.GetPortalGroupBySquare(ms);
			int area = Original.Areas[ms.x,ms.y];
			if (pgs.Count > 1) {
				portalSquares[ms] = state;
				return;
			}
			foreach(Portal p in pgs[0].Portals) {
				MapSquare pms = p[area];
				// Debug.Log("AREA: " + area + " " + p);
				if (null == pms) { Debug.Log("WHAT?"); continue; }
				portalSquares[pms] = state;
			}
		}
	}

	public IEnumerable<MapSquare> Neighbours (MapSquare node) {
		return GetNeighbours(node);
	}

	public int MemoryByteUsed () {
		return portalSquares.Count;
	}

	public void CleanBelieves () {
		// Initialize Internal Representation
		foreach (MapSquare ms in Original.PortalSquares.Keys) {
			if (portalSquares.ContainsKey(ms)) {
				portalSquares[ms] = true;
			} else {
				portalSquares.Add(ms,true);
			}
		}
	}
}
