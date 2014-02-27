using UnityEngine;
using System.Collections;

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

}
