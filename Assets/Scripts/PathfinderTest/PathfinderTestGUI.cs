using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// This component write on two Unity Label GUI the global and local progress
/// of the benchmark.
/// </summary>
[RequireComponent(typeof(PathfindTester))]
public class PathfinderTestGUI : MonoBehaviour {

    /// <summary>
    /// A reference to the LocalProg label.
    /// </summary>
    public GUIText LocalProg;

    /// <summary>
    /// A reference to the LocalProg label.
    /// </summary>
    public GUIText GlobalProg;

    PathfindTester PathTester;

	// Use this for initialization
	void Start () {
        PathTester = GetComponent<PathfindTester>();
        if (PathTester == null) {
            Debug.LogError("Something gone really wrong! No PathfinderTester" +
               " found by PathfinderTesterGUI!");
        }
	}
	
	// Update is called once per frame
	void Update () {
        GlobalProg.text = String.Format("Global Progress: {0:0.0}", 
            (float) PathTester.CurrentMapIndex  / (float) PathTester.MapNumber);
		LocalProg.text = String.Format("Local Progress: {0:0.0}",
            (float) PathTester.CurrentMapIteration/ (float) PathTester.NumberOfRuns);
	}
}
