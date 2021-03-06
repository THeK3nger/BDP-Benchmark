using UnityEngine;
using System.Collections;

/// <summary>
/// This class represent a portal random strategy.
/// </summary>
/// A random portal strategy is the algorithm used when the BDP tester
/// ask for a randomization of the world portal.
public abstract class IPortalsRandomStrategy : MonoBehaviour {

    /// <summary>
    /// Implement a strategy for world portal shuffling.
    /// </summary>
    /// <param name="gameMap">The GameMap to shuffle.</param>
    public abstract void RandomizeWorldPortals();

    /// <summary>
    /// Return a randomness amount value for the given strategy.
    /// </summary>
    /// <returns></returns>
    public abstract float GetRandomAmount();

    public abstract void SetRandomness(float value);

    public abstract void SetScrambleAmount(float value);

    public abstract void Init();

}
