/// <summary>
/// This class represents an AgentEntity in the benchmark.
/// </summary>
public class AgentEntity
{

	/// <summary>
	/// Gets or sets the current position.
	/// </summary>
	/// <value>The current position.</value>
    public MapSquare CurrentPosition { get; set; }

	/// <summary>
	/// A reference to an agentBelief representation.
	/// </summary>
    private HVertexBased agentBelief;

	/// <summary>
	/// Initializes a new instance of the <see cref="AgentEntity"/> class.
	/// </summary>
	/// <param name="agentBelief">Agent belief.</param>
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

	/// <summary>
	/// Cleans the belief.
	/// </summary>
    public void CleanBelief()
    {
        this.agentBelief.ResetBelieves();
    }

	/// <summary>
	/// Reviews the belief older than.
	/// </summary>
	/// <param name="stepWindow">Step window.</param>
    public void ReviewBeliefOlderThan(long stepWindow)
    {
        this.agentBelief.OpenOldPortals(stepWindow);
    }

	/// <summary>
	/// Updates the belief.
	/// </summary>
	/// <returns><c>true</c>, if belief was updated, <c>false</c> otherwise.</returns>
	/// <param name="ms">Ms.</param>
	/// <param name="state">If set to <c>true</c> state.</param>
    public bool UpdateBelief(MapSquare ms, bool state)
    {
        return this.agentBelief.UpdateBelief(ms, state);
    }

}
