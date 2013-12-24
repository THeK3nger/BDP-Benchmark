using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Provide static functions for random manipulation of standard collections. 
/// </summary>
public class RandomCollection {

	/// <summary>
	/// Create a shiffled version of the specified list.
	/// </summary>
	/// <param name="list">List.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static List<T> ShuffleCopy<T>(List<T> list) {
		var resultList = new List<T>(list);
		Shuffle(resultList);
		return resultList;
	}

	/// <summary>
	/// Shuffles the specified list.
	/// </summary>
	/// <param name="list">List.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static void Shuffle<T>(List<T> list) {
		int n = list.Count;
		while (n>1) {
			int k = Random.Range(0,n-1);
			n--;
			T tmp = list[n];
			list[n] = list[k];
			list[k] = tmp;
		}
	}

	/// <summary>
	/// Pich a random element of the list.
	/// </summary>
	/// <returns>The pick.</returns>
	/// <param name="list">List.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T RandomPick<T>(IList<T> list) {
		int n = list.Count;
		int k = Random.Range(0,n-1);
		return list[k];
	}

	/// <summary>
	/// Extrract a random element of the list according given weights.
	/// </summary>
	/// <returns>The extraction.</returns>
	/// <param name="list">List.</param>
	/// <param name="weights">Weights.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T RouletteExtraction<T>(IList<T> list, float[] weights) {
		if (list.Count != weights.Length) {
			return default(T);
		}
		// Sum the weights.
		float sum = 0;
		for (int i=0; i< weights.Length; i++) {
			sum += weights[i];
		}
		float spin = Random.Range(0,sum);
		int selectedIdx = -1;
		while (spin>0) {
			selectedIdx++;
			spin -= weights[selectedIdx];
		}
		return list[selectedIdx-1];
	}
}
