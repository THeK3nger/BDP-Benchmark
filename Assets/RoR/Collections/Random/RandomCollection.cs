// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RandomCollection.cs" company="Davide Aversa">
//   MIT License
// </copyright>
// <summary>
//   Provide static functions for random manipulation of standard collections.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

/// <summary>
/// Provide static functions for random manipulation of standard collections. 
/// </summary>
public class RandomCollection
{
    /// <summary>
    /// Create a shuffled version of the specified list.
    /// </summary>
    /// <param name="list">
    /// List.
    /// </param>
    /// <typeparam name="T">
    /// The 1st type parameter.
    /// </typeparam>
    /// <returns>
    /// The <see cref="List"/>.
    /// </returns>
    public static List<T> ShuffleCopy<T>(List<T> list)
    {
        var resultList = new List<T>(list);
        Shuffle(resultList);
        return resultList;
    }

    /// <summary>
    /// Shuffles the specified list.
    /// </summary>
    /// <param name="list">
    /// List.
    /// </param>
    /// <typeparam name="T">
    /// The 1st type parameter.
    /// </typeparam>
    public static void Shuffle<T>(List<T> list)
    {
        var n = list.Count;
        while (n > 1)
        {
            var k = Random.Range(0, n - 1);
            n--;
            var tmp = list[n];
            list[n] = list[k];
            list[k] = tmp;
        }
    }

    /// <summary>
    /// Pich a random element of the list.
    /// </summary>
    /// <returns>
    /// The pick.
    /// </returns>
    /// <param name="list">
    /// List.
    /// </param>
    /// <typeparam name="T">
    /// The 1st type parameter.
    /// </typeparam>
    public static T RandomPick<T>(IList<T> list)
    {
        var n = list.Count;
        var k = Random.Range(0, n - 1);
        return list[k];
    }

    /// <summary>
    /// Extrract a random element of the list according given weights.
    /// </summary>
    /// <returns>
    /// The extraction.
    /// </returns>
    /// <param name="list">
    /// List.
    /// </param>
    /// <param name="weights">
    /// Weights.
    /// </param>
    /// <typeparam name="T">
    /// The 1st type parameter.
    /// </typeparam>
    public static T RouletteExtraction<T>(IList<T> list, float[] weights)
    {
        if (list.Count != weights.Length)
        {
            return default(T);
        }

        // Sum the weights.
        var sum = weights.Sum();

        var spin = Random.Range(0, sum);
        var selectedIdx = -1;

        while (spin > 0)
        {
            selectedIdx++;
            spin -= weights[selectedIdx];
        }

        return list[selectedIdx - 1];
    }
}
