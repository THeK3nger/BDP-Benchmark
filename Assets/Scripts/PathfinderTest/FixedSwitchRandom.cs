// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FixedSwitchRandom.cs" company="">
//   
// </copyright>
// <summary>
//   The fixed switch random.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;

using UnityEngine;

using Random = System.Random;

/// <summary>
/// The fixed switch random.
/// </summary>
public class FixedSwitchRandom : IPortalsRandomStrategy
{
    /// <summary>
    /// Seed for the RNG.
    /// </summary>
    public int Seed;

    /// <summary>
    /// An instance of the RNG.
    /// </summary>
    private Random r;

    /// <summary>
    /// The ratio of initial closed portals in the map.
    /// </summary>
    [Range(0.0f, 1.0f)]
    public float StartingClosedAmount;

    /// <summary>
    /// The amount of scrambled portals.
    /// </summary>
    public float ScrambleAmount = 2;

    /// <summary>
    /// The start.
    /// </summary>
    private void Start()
    {
        // Only one component of type IPortalRandomStrategy can exist in one object!
        if (GetComponent<IPortalsRandomStrategy>() != this)
        {
            Destroy(this);
        }

        this.r = new Random(Seed);
    }

    public override void Init()
    {
        foreach (var pg in BDPMap.Instance.PortalConnectivity.Vertices) 
        {
            var ratioToss = r.NextDouble();
            if (ratioToss < StartingClosedAmount)
            {
                var sideToss = r.NextDouble();
                BDPMap.Instance.SetPortalGroup(pg, false, sideToss < 0.5 ? pg.LinkedAreas.First : pg.LinkedAreas.Second);
            } 
            else 
            {
                BDPMap.Instance.SetPortalGroup(pg, true, pg.LinkedAreas.First);
                BDPMap.Instance.SetPortalGroup(pg, true, pg.LinkedAreas.Second);
            }
        }
    }

    /// <summary>
    /// The randomize world portals.
    /// </summary>
    public override void RandomizeWorldPortals()
    {
        // Select a random set of portals.
        var scrambleNumber = (int)Mathf.Floor(BDPMap.Instance.PortalConnectivity.Vertices.Count() * ScrambleAmount);
        Debug.LogWarning(scrambleNumber);
        Debug.LogWarning(ScrambleAmount);
        var pp =
            BDPMap.Instance.PortalConnectivity.Vertices.OrderBy(x => r.Next())
                .Take(scrambleNumber);

        // Switch that set.
        foreach (var pg in pp)
        {
            var currentState = BDPMap.Instance.GetPortalGroupState(pg, pg.LinkedAreas.First);
            BDPMap.Instance.SetPortalGroup(pg, !currentState, pg.LinkedAreas.First);
            BDPMap.Instance.SetPortalGroup(pg, !currentState, pg.LinkedAreas.Second);
        }
    }

    /// <summary>
    /// The get random amount.
    /// </summary>
    /// <returns>
    /// The <see cref="float"/>.
    /// </returns>
    public override float GetRandomAmount()
    {
        return this.ScrambleAmount;
    }

    public override void SetRandomness(float value)
    {
        StartingClosedAmount = value;
    }

    public override void SetScrambleAmount(float value)
    {
        ScrambleAmount = value;
    }
}
