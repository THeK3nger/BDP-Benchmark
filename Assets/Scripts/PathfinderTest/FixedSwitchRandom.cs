using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class FixedSwitchRandom : IPortalsRandomStrategy {

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

    /// <summary>
    /// The amount of scrambled portals.
    /// </summary>
    public int ScrambleAmount = 2;

    // Use this for initialization
    void Start() {
        // Only one component of type IPortalRandomStrategy can exist in one object!
        if (GetComponent<IPortalsRandomStrategy>() != null) Destroy(this);
        r = new System.Random(Seed);
    }

    public override void RandomizeWorldPortals(BDPMap gameMap) {
        // Select a random set of portals.
        IEnumerable<PortalGroup> pp = gameMap.PortalConnectivity.Vertices.OrderBy(x => r.Next()).Take(ScrambleAmount);
        // Switch that set.
        foreach (PortalGroup pg in pp) {
            bool currentState = gameMap.GetPortalGroupState(pg, pg.LinkedAreas.First);
            gameMap.SetPortalGroup(pg, !currentState, pg.LinkedAreas.First);
            gameMap.SetPortalGroup(pg, !currentState, pg.LinkedAreas.Second);
        }
    }

    public override float GetRandomAmount() {
        return ScrambleAmount;
    }
}
