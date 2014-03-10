// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMapBelief.cs" company="Davide Aversa">
//   MIT License
// </copyright>
// <summary>
//   The MapBelief interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System.Collections;

using RoomOfRequirement;

using UnityEngine;

/// <summary>
/// The MapBelief interface.
/// </summary>
public interface IMapBelief : IHasNeighbours<MapSquare>
{
    /// <summary>
    /// Specify if the agent is hierarchical or not.
    /// </summary>
    bool Hierarchical { get; }

    /// <summary>
    /// Gets or sets the current target.
    /// </summary>
    MapSquare CurrentTarget { get; set; }

    /// <summary>
    /// The is free.
    /// </summary>
    /// <param name="ms">
    /// The ms.
    /// </param>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    bool IsFree(MapSquare ms);

    /// <summary>
    /// The is free.
    /// </summary>
    /// <param name="x">
    /// The x.
    /// </param>
    /// <param name="y">
    /// The y.
    /// </param>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    bool IsFree(int x, int y);

    /// <summary>
    /// Updates the belief on the given portal/portal group.
    /// </summary>
    /// <param name="ms">
    /// The ms.
    /// </param>
    /// <param name="state">
    /// Set the portal to <c>true</c> or <c>flase</c> state.
    /// </param>
    /// <returns>
    /// <c>true</c> if and only if someting is changed in the agent belief.
    /// </returns>
    bool UpdateBelief(MapSquare ms, bool state);

    /// <summary>
    /// The memory byte used.
    /// </summary>
    /// <returns>
    /// The <see cref="int"/>.
    /// </returns>
    int MemoryByteUsed();

    /// <summary>
    /// The clean believes.
    /// </summary>
    void CleanBelieves();

    /// <summary>
    /// The reset believes.
    /// </summary>
    void ResetBelieves();
}
