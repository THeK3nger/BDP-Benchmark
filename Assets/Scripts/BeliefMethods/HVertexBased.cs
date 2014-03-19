// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HVertexBased.cs" company="Davide Aversa">
//   MIT License
// </copyright>
// <summary>
//   The h vertex based.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using UnityEngine;

/// <summary>
/// The h vertex based.
/// </summary>
public class HVertexBased : MonoBehaviour, IMapHierarchicalBelief
{
    /// <summary>
    /// Gets a value indicating whether hierarchical.
    /// </summary>
    public bool Hierarchical
    {
        get
        {
            return true;
        }
    }

    /// <summary>
    /// Gets or sets the current target.
    /// </summary>
    public MapSquare CurrentTarget { get; set; }

    /// <summary>
    /// The portal passability.
    /// </summary>
    private readonly Dictionary<PortalGroup, bool> portalPassability = new Dictionary<PortalGroup, bool>();

    /// <summary>
    /// The portal timestamp.
    /// </summary>
    private readonly Dictionary<PortalGroup, float> portalTimestamp = new Dictionary<PortalGroup, float>();

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

        if (!BDPMap.Instance.IsPortalSquare(ms))
        {
            return BDPMap.Instance.IsFree(ms);
        }

        var pgs = BDPMap.Instance.GetPortalGroupBySquare(ms);
        return pgs.All(pg => this.portalPassability[pg]);
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
    /// The get neighbours.
    /// </summary>
    /// <param name="ms">
    /// The ms.
    /// </param>
    /// <returns>
    /// The <see cref="List"/>.
    /// </returns>
    public List<MapSquare> GetNeighbours(MapSquare ms)
    {
        var originalMap = BDPMap.Instance;
        var result = new List<MapSquare>();
        var area = originalMap.GetArea(ms);

        var pgs = originalMap.GetPortalGroupByAreas(area);
        result.AddRange((from pg in pgs where this.portalPassability[pg] select pg.NearestPortal(ms)).Select(p => p[area, reverse: true]));

        if (area == originalMap.GetArea(CurrentTarget) && !result.Contains(CurrentTarget))
        {
            result.Add(CurrentTarget);
        }

        return result;
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
    /// The clean believes.
    /// </summary>
    public void CleanBelieves()
    {
        foreach (var pg in BDPMap.Instance.PortalConnectivity.Vertices)
        {
            if (portalPassability.ContainsKey(pg))
            {
                portalPassability[pg] = true;
                portalTimestamp[pg] = Time.time;
            }
            else
            {
                portalPassability.Add(pg, true);
                portalTimestamp.Add(pg, Time.time);
            }
        }
    }

    /// <summary>
    /// The memory byte used.
    /// </summary>
    /// <returns>
    /// The <see cref="int"/>.
    /// </returns>
    public int MemoryByteUsed()
    {
        return BDPMap.Instance.PortalsNumber;
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
        var pgs = BDPMap.Instance.GetPortalGroupBySquare(ms);
        var changed = false;
        foreach (var pg in pgs.Where(pg => this.UpdateBelief(pg, state)))
        {
            changed = true;
        }

        return changed;
    }

    /// <summary>
    /// The update belief.
    /// </summary>
    /// <param name="pg">
    /// The pg.
    /// </param>
    /// <param name="state">
    /// The state.
    /// </param>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    public bool UpdateBelief(PortalGroup pg, bool state)
    {
        var changed = false;
        if (portalPassability[pg] != state)
        {
            portalPassability[pg] = state;
            changed = true;
        }

        portalTimestamp[pg] = Time.time;
        return changed;
    }

    /// <summary>
    /// The reset believes.
    /// </summary>
    public void ResetBelieves()
    {
        CleanBelieves();
    }

    /// <summary>
    /// The open old portals.
    /// </summary>
    /// <param name="timeLimit">
    /// The time limit.
    /// </param>
    public void OpenOldPortals(float timeLimit)
    {
        foreach (var key in this.portalTimestamp.Keys.ToList().Where(key => this.portalTimestamp[key] < timeLimit))
        {
            UpdateBelief(key, false);
        }
    }
}
