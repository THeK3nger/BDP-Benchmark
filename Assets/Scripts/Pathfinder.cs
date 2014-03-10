// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Pathfinder.cs" company="Davide Aversa">
//   MIT License
// </copyright>
// <summary>
//   The pathfinder.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

using RoomOfRequirement.AI.Data;
using RoomOfRequirement.Search;

using UnityEngine;

/// <summary>
/// The pathfinder.
/// </summary>
public class Pathfinder : MonoBehaviour
{
    /// <summary>
    /// The agent belief.
    /// </summary>
    public IMapBelief AgentBelief;

    /// <summary>
    /// The omniscient.
    /// </summary>
    public IMapBelief Omniscient;

    /// <summary>
    /// The agent.
    /// </summary>
    public GameObject Agent;

    /// <summary>
    /// The omniscient object.
    /// </summary>
    public GameObject OmniscientObject;

    /// <summary>
    /// The agent position.
    /// </summary>
    private Vector2 agentPosition;

    // bool Executed = false;

    /// <summary>
    /// The awake.
    /// </summary>
    private void Awake()
    {
        AgentBelief = Agent.GetComponent(typeof(IMapBelief)) as IMapBelief;
        Omniscient = OmniscientObject.GetComponent(typeof(IMapBelief)) as IMapBelief;

        // AllFreeBase.Agent = AgentBelief;
        Debug.Log(AgentBelief);
    }

    /// <summary>
    /// The update.
    /// </summary>
    private void Update()
    {
    }

    /// <summary>
    /// The pg belief distance.
    /// </summary>
    /// <param name="pg1">
    /// The pg 1.
    /// </param>
    /// <param name="pg2">
    /// The pg 2.
    /// </param>
    /// <returns>
    /// The <see cref="double"/>.
    /// </returns>
    public double PGBeliefDistance(PortalGroup pg1, PortalGroup pg2)
    {
        if (pg1 == pg2)
        {
            return 0;
        }

        var d = BDPMap.Instance.PortalConnectivity.GetEdgeLabel(pg1, pg2);
        if (Math.Abs(d) > 0.0001 && BDPMap.Instance.PortalConnectivity.GetVertexLabel(pg1)
            && BDPMap.Instance.PortalConnectivity.GetVertexLabel(pg2))
        {
            return d;
        }

        return Mathf.Infinity;
    }

    /// <summary>
    /// The path find.
    /// </summary>
    /// <param name="start">
    /// The start.
    /// </param>
    /// <param name="target">
    /// The target.
    /// </param>
    /// <returns>
    /// The <see cref="Path"/>.
    /// </returns>
    public Path<MapSquare> PathFind(MapSquare start, MapSquare target)
    {
        if (start == null)
        {
            throw new ArgumentNullException("start");
        }

        if (target == null)
        {
            throw new ArgumentNullException("target");
        }

        var path = AStar.FindPath(
            start, 
            target, 
            AgentBelief.Neighbours,
            (m1, m2) =>
                {
                    List<PortalGroup> p1 = BDPMap.Instance.GetPortalGroupBySquare(m1);
                    List<PortalGroup> p2 = BDPMap.Instance.GetPortalGroupBySquare(m2);
                    if (p1 != null && p2 != null)
                    {
                        return PGBeliefDistance(p1[0], p2[0]);
                    }
                    else
                    {
                        return MapSquare.Distance(m1, m2);
                    }
                }, 
            (ms) => MapSquare.Distance(ms, target));
        return path;
    }

    /// <summary>
    /// The path find on real map.
    /// </summary>
    /// <param name="start">
    /// The start.
    /// </param>
    /// <param name="target">
    /// The target.
    /// </param>
    /// <param name="area">
    /// The area.
    /// </param>
    /// <returns>
    /// The <see cref="Path"/>.
    /// </returns>
    public Path<MapSquare> PathFindOnRealMap(MapSquare start, MapSquare target, int area)
    {
        Path<MapSquare> path = AStar.FindPath<MapSquare>(
            start, 
            target, 
            (ms) => this.Omniscient.Neighbours(ms).Where(m => BDPMap.Instance.GetArea(m) == area || m == target).ToList(), 
            MapSquare.Distance, 
            (ms) =>
                {
                    if (area == 0 || BDPMap.Instance.GetArea(ms) == area)
                    {
                        return MapSquare.Distance(ms, target);
                    }

                    return 10000000;
                });
        return path;
    }
}
