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
/// This class represents a hierarchical vertex-based BDP.
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
    /// State of the portal group on a given area.
    /// </summary>
    private readonly Dictionary<Tuple<PortalGroup,int>,bool> portalGroupState = new Dictionary<Tuple<PortalGroup, int>, bool>();

    /// <summary>
    /// The portal timestamp.
    /// </summary>
    private readonly Dictionary<PortalGroup, long> portalTimestamp = new Dictionary<PortalGroup, long>();

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
        var area = BDPMap.Instance.GetArea(ms);
        return pgs.All(pg => this.portalGroupState[PortalGroupKey(pg, area)]);
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
        if (pgs == null)
        {
            Debug.LogError("BOOM");
        }

        var candidates = (from pg in pgs where this.IsPassable(pg) select MidPortal(pg)).Select(p => p[area, reverse: true]);

        result.AddRange(candidates);

        if (area == originalMap.GetArea(CurrentTarget) && !result.Contains(CurrentTarget))
        {
            result.Add(CurrentTarget);
        }

        return result;
    }

    /// <summary>
    /// Return the nearest portal to a given 2D point in the current instance.
    /// </summary>
    /// <returns>
    /// The portal.
    /// </returns>
    /// <param name="ms">
    /// The ms.
    /// </param>
    /// <param name="minDist">
    /// Minimum dist.
    /// </param>
    public Portal NearestPassablePortal(MapSquare ms, PortalGroup pg) {
        Portal min = null;
        var minDist = Mathf.Infinity;
        foreach (var p in pg.Portals.Where(IsPassable))
        {
            var newDist = Portal.Distance(p, ms);
            if (!(newDist < minDist))
            {
                continue;
            }

            minDist = newDist;
            min = p;
        }

        return min;
    }

    /// <summary>
    /// Return the nearest portal to a given 2D point in the current instance.
    /// </summary>
    /// <returns>
    /// The portal.
    /// </returns>
    /// <param name="ms">
    /// The ms.
    /// </param>
    /// <param name="minDist">
    /// Minimum dist.
    /// </param>
    public Portal MidPortal(PortalGroup pg) {
        return pg.Portals.ToList()[Mathf.FloorToInt(pg.Portals.Count() / 2.0f)];
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
            if (portalTimestamp.ContainsKey(pg))
            {
                this.FullPortalGroupStateUpdate(pg, true);
                portalTimestamp[pg] = StepCounter.Steps;
            }
            else
            {
                portalGroupState.Add(PortalGroupKey(pg, pg.LinkedAreas.First), true);
                portalGroupState.Add(PortalGroupKey(pg, pg.LinkedAreas.Second), true);
                portalTimestamp.Add(pg, StepCounter.Steps);
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
        //TODO: This is an hack.
        if (pgs == null) return false;
        // end
        if (pgs.Count == 0)
        {
            Debug.LogWarning("Updating a non portalsquare!");
            return false;
        }
        var changed = false;
        foreach (var pg in pgs.Where(pg => this.UpdateBelief(pg, BDPMap.Instance.GetArea(ms), state)))
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
    /// <param name="area">
    /// The square area.
    /// </param>
    /// <param name="state">
    /// The state.
    /// </param>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    public bool UpdateBelief(PortalGroup pg, int area, bool state)
    {
        var changed = false;

        if (portalGroupState[PortalGroupKey(pg, area)] != state)
        {
            var valid = true;
            //foreach (var p in pg.Portals.Where(p => BDPMap.Instance.IsFree(p[area]) != state))
            //{
            //    valid = false;
            //}

            if (valid)
            {
                portalGroupState[PortalGroupKey(pg, area)] = state;
                changed = true;
            }
        }

        portalTimestamp[pg] = StepCounter.Steps;
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
    public void OpenOldPortals(long stepWindow)
    {
        var stepHorizon = StepCounter.Steps - stepWindow;
        foreach (var key in this.portalTimestamp.Keys.ToList().Where(key => this.portalTimestamp[key] < stepHorizon))
        {
            this.FullPortalGroupStateUpdate(key, true);
        }
    }

    /// <summary>
    /// Shorthand to get a key tuple for the portalGroupState data structure.
    /// </summary>
    /// <param name="pg">The portal group.</param>
    /// <param name="area">The area.</param>
    /// <returns>A key for the dictionary portalGroupState</returns>
    private static Tuple<PortalGroup, int> PortalGroupKey(PortalGroup pg, int area)
    {
        return new Tuple<PortalGroup, int>(pg, area);
    }

    /// <summary>
    /// Check if a given PortalGroup is traversable or not.
    /// </summary>
    /// <param name="pg">The portal group.</param>
    /// <returns>true if and only if the portal group is traversable.</returns>
    private bool IsPassable(PortalGroup pg)
    {
        return portalGroupState[PortalGroupKey(pg, pg.LinkedAreas.First)]
               && portalGroupState[PortalGroupKey(pg, pg.LinkedAreas.Second)];
    }

    private bool IsPassable(Portal p)
    {
        return this.IsFree(p.LinkedSquares.First) && IsFree(p.LinkedSquares.Second);
    }

    /// <summary>
    /// Set the given state to both side of the given portal group.
    /// </summary>
    /// <param name="pg">The given portal group.</param>
    /// <param name="state">The desired state.</param>
    private void FullPortalGroupStateUpdate(PortalGroup pg, bool state) {
        this.portalGroupState[PortalGroupKey(pg, pg.LinkedAreas.First)] = state;
        this.portalGroupState[PortalGroupKey(pg, pg.LinkedAreas.Second)] = state;
    }
}
