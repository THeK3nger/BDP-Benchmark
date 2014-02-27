﻿using UnityEngine;
using System.Collections;
using RoomOfRequirement;

public interface IMapBelief : IHasNeighbours<MapSquare> {

    /// <summary>
    /// Specify if the agent is hierarchical or not.
    /// </summary>
	bool Hierarchical {get; }
	MapSquare CurrentTarget {get; set; }

	bool IsFree(MapSquare ms);
	bool IsFree(int x, int y);

	/// <summary>
	/// Updates the belief on the given portal/portal group.
	/// </summary>
	/// <param name="pg">The updated portals.</param>
	/// <param name="area">The curren agent area.</param>
	/// <param name="state">Set the portal to <c>true</c> or <c>flase</c> state.</param>
    /// <returns><c>true</c> if and only if someting is changed in the agent belief.</returns>
	bool UpdateBelief(MapSquare ms, bool state);

	int MemoryByteUsed();

	void CleanBelieves();

	void ResetBelieves();
}
