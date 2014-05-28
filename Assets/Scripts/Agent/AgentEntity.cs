using System.ComponentModel;
using System.Linq;

using UnityEngine;
using System.Collections;

public class AgentEntity
{

    public MapSquare CurrentPosition { get; set; }

    private HVertexBased agentBelief;

    public AgentEntity(HVertexBased agentBelief)
    {
        this.agentBelief = agentBelief;
        this.Init();
    }

    /// <summary>
    /// Initialize the Agent.
    /// </summary>
    public void Init()
    {
        CurrentPosition = new MapSquare(0,0);
    }

    /// <summary>
    /// MoveTo the given MapSquare.
    /// </summary>
    /// <param name="ms"></param>
    /// <returns></returns>
    public bool MoveTo(MapSquare ms)
    {
        if (!BDPMap.Instance.IsFree(ms))
        {
            return false;
        }

        CurrentPosition = ms;
        return true;
    }

    public void CleanBelief()
    {
        this.agentBelief.ResetBelieves();
    }

    public void ReviewBeliefOlderThan(long stepWindow)
    {
        this.agentBelief.OpenOldPortals(stepWindow);
    }

    public bool UpdateBelief(MapSquare ms, bool state)
    {
        return this.agentBelief.UpdateBelief(ms, state);
    }

}
