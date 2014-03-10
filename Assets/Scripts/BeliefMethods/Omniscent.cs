// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Omniscent.cs" company="Davide Aversa">
//   MIT License
// </copyright>
// <summary>
//   The omniscent.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// The omniscent.
/// </summary>
public class Omniscent : MonoBehaviour, IMapBelief
{
    /// <summary>
    /// Gets a value indicating whether hierarchical.
    /// </summary>
    public bool Hierarchical
    {
        get
        {
            return false;
        }
    }

    /// <summary>
    /// Gets or sets the current target.
    /// </summary>
    public MapSquare CurrentTarget { get; set; }

    /// <summary>
    /// The start.
    /// </summary>
    private void Start()
    {
    }

    /// <summary>
    /// The is free.
    /// </summary>
    /// <param name="ms">
    /// The ms.
    /// </param>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    public bool IsFree(MapSquare ms)
    {
        if (ms == null)
        {
            throw new ArgumentNullException("ms");
        }

        return BDPMap.Instance.IsFree(ms);
    }

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
    public bool IsFree(int x, int y)
    {
        return IsFree(new MapSquare(x, y));
    }

    /// <summary>
    /// Gets the square valid neighbours.
    /// </summary>
    /// <param name="ms">
    /// The ms.
    /// </param>
    /// <returns>
    /// The neighbours.
    /// </returns>
    public List<MapSquare> GetNeighbours(MapSquare ms)
    {
        if (ms == null)
        {
            throw new ArgumentNullException("ms");
        }

        var result = new List<MapSquare>();

        // Add up.
        if (IsFree(ms.Up))
        {
            result.Add(ms.Up);
        }

        // Add down.
        if (IsFree(ms.Down))
        {
            result.Add(ms.Down);
        }

        // Add left.
        if (IsFree(ms.Left))
        {
            result.Add(ms.Left);
        }

        // Add right.
        if (IsFree(ms.Right))
        {
            result.Add(ms.Right);
        }

        return result;
    }

    /// <summary>
    /// The update belief.
    /// </summary>
    /// <param name="ms">
    /// The ms.
    /// </param>
    /// <param name="state">
    /// The state.
    /// </param>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    public bool UpdateBelief(MapSquare ms, bool state)
    {
        return false;
    }

    /// <summary>
    /// The neighbours.
    /// </summary>
    /// <param name="node">
    /// The node.
    /// </param>
    /// <returns>
    /// The <see cref="IEnumerable"/>.
    /// </returns>
    public IEnumerable<MapSquare> Neighbours(MapSquare node)
    {
        return GetNeighbours(node);
    }

    /// <summary>
    /// The memory byte used.
    /// </summary>
    /// <returns>
    /// The <see cref="int"/>.
    /// </returns>
    public int MemoryByteUsed()
    {
        return BDPMap.Instance.Width * BDPMap.Instance.Height;
    }

    /// <summary>
    /// The clean believes.
    /// </summary>
    public void CleanBelieves()
    {
    }

    /// <summary>
    /// The reset believes.
    /// </summary>
    public void ResetBelieves()
    {
        // portalSquares = new Dictionary<MapSquare, bool>();
    }
}
