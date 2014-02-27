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
	void Awake () {
        // Only one component of type IPortalRandomStrategy can exist in one object!
        if (GetComponent<IPortalsRandomStrategy>() != null) Destroy(this);
        r = new System.Random(Seed);
	}

    public override void RandomizeWorldPortals() {
        foreach (PortalGroup pg in BDPMap.Instance.PortalConnectivity.Vertices) {
            double ratioToss = r.NextDouble();
            if (ratioToss < StartingClosedAmount) {
                double sideToss = r.NextDouble();
                if (sideToss < 0.5) {
                    BDPMap.Instance.SetPortalGroup(pg, false, pg.LinkedAreas.First);
                } else {
                    BDPMap.Instance.SetPortalGroup(pg, false, pg.LinkedAreas.Second);
                }
            } else {
                BDPMap.Instance.SetPortalGroup(pg, true, pg.LinkedAreas.First);
                BDPMap.Instance.SetPortalGroup(pg, true, pg.LinkedAreas.Second);
            }
        }
    }

    public override float GetRandomAmount() {
        return StartingClosedAmount;
    }
}
