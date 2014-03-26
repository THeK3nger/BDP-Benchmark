using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PathfindTester))]
public sealed class FixedTotalRandom : IPortalsRandomStrategy {

    /// <summary>
    /// Seed for the RNG.
    /// </summary>
    public int Seed;

    /// <summary>
    /// An instance of the RNG.
    /// </summary>
    System.Random r;

    /// <summary>
    /// The ratio of initial closed portals in the map.
    /// </summary>
    [Range(0.0f, 1.0f)]
    public float StartingClosedAmount;

	// Use this for initialization
    void Awake () 
    {
        // Only one component of type IPortalRandomStrategy can exist in one object!
        if (GetComponent<IPortalsRandomStrategy>() != this)
        {
            Destroy(this);
        }

        r = new System.Random(Seed);
    }

    public override void Init()
    {
        this.RandomizeWorldPortals();
    }

    /// <summary>
    /// The randomize world portals.
    /// </summary>
    public override void RandomizeWorldPortals()
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
    /// The get random amount.
    /// </summary>
    /// <returns>
    /// The <see cref="float"/>.
    /// </returns>
    public override float GetRandomAmount()
    {
        return StartingClosedAmount;
    }

    /// <summary>
    /// The set randomness.
    /// </summary>
    /// <param name="value">
    /// The value.
    /// </param>
    public override void SetRandomness(float value)
    {
        StartingClosedAmount = value;
    }

    /// <summary>
    /// The set scramble amount.
    /// </summary>
    /// <param name="value">
    /// The value.
    /// </param>
    public override void SetScrambleAmount(float value)
    {
        return;
    }
}
