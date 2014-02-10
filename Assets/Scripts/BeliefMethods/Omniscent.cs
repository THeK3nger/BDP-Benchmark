﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Omniscent : MonoBehaviour, IMapBelief  {
	
	public Map2D OriginalMap;
	
	public Map2D Original { get { return OriginalMap; } }
	public bool Hierarchical {get { return false; } }
	public MapSquare CurrentTarget { get; set; }
	
	void Start() {
	}
	
	public bool IsFree (MapSquare ms) {
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

	}
	
	public IEnumerable<MapSquare> Neighbours (MapSquare node) {
		return GetNeighbours(node);
	}

	public int MemoryByteUsed () {
		return Original.Width*Original.Height;
	}

	public void CleanBelieves () {

	}

	public void ResetBelieves() {
		//portalSquares = new Dictionary<MapSquare, bool>();
	}
}
