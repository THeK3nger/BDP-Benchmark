// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PathfinderTestGUI.cs" company="Davide Aversa">
//   MIT License
// </copyright>
// <summary>
//   This component write on two Unity Label GUI the global and local progress
//   of the benchmark.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;

/// <summary>
/// This component write on two Unity Label GUI the global and local progress
/// of the benchmark.
/// </summary>
[RequireComponent(typeof(PathfindTester))]
public class PathfinderTestGui : MonoBehaviour
{
    /// <summary>
    /// A reference to the LocalProg label.
    /// </summary>
    public GUIText LocalProg;

    /// <summary>
    /// A reference to the LocalProg label.
    /// </summary>
    public GUIText GlobalProg;

    /// <summary>
    /// The path tester.
    /// </summary>
    private PathfindTester pathTester;

    /// <summary>
    /// The start.
    /// </summary>
    private void Start()
    {
        this.pathTester = GetComponent<PathfindTester>();
        if (this.pathTester == null)
        {
            Debug.LogError("Something gone really wrong! No PathfinderTester" + " found by PathfinderTesterGUI!");
        }
    }

    /// <summary>
    /// The update.
    /// </summary>
    private void Update()
    {
        GlobalProg.text = string.Format(
            "Global Progress: {0:0.0}", 
            (float)this.pathTester.CurrentMapIndex / (float)this.pathTester.MapNumber);
        LocalProg.text = string.Format(
            "Local Progress: {0:0.0}", 
            (float)this.pathTester.CurrentMapIteration / (float)this.pathTester.NumberOfRuns);
    }
}
