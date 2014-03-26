using UnityEngine;
using System.Collections;

public class SwitchRandom : IPortalsRandomStrategy {

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
    void Start()
    {
        // Only one component of type IPortalRandomStrategy can exist in one object!
        if (GetComponent<IPortalsRandomStrategy>() != null)
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
            if (!(ratioToss < this.StartingClosedAmount))
            {
                continue;
            }

            var currentState = BDPMap.Instance.GetPortalGroupState(pg, pg.LinkedAreas.First);
            BDPMap.Instance.SetPortalGroup(pg, !currentState, pg.LinkedAreas.First);
            BDPMap.Instance.SetPortalGroup(pg, !currentState, pg.LinkedAreas.Second);
        }
    }

    public override float GetRandomAmount() {
        return StartingClosedAmount;
    }

    public override void SetRandomness(float value)
    {
        StartingClosedAmount = value;
    }

    public override void SetScrambleAmount(float value) {

    }
}
