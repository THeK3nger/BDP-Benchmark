using UnityEngine;
using System.Collections;
using RoomOfRequirement;

public interface IMapBelief : IHasNeighbours<MapSquare> {

    /// <summary>
    /// A reference to the original map.
    /// </summary>
    BDPMap Original { get; }

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
	void UpdateBelief(MapSquare ms, bool state);

	int MemoryByteUsed();

	void CleanBelieves();

	void ResetBelieves();
}
