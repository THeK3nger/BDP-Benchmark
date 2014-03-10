// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AgentPositioning.cs" company="Davide Aversa">
//   MIT
// </copyright>
// <summary>
//   Defines the AgentPositioning component.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;

/// <summary>
/// The AgentPositioning component move the attaching object to the grid-position specified in public properties
/// GridPosition.
/// </summary>
public class AgentPositioning : MonoBehaviour
{
    /// <summary>
    /// Gets or sets the grid position.
    /// </summary>
    public MapSquare GridPosition { get; set; }

    /// <summary>
    /// The start.
    /// </summary>
    public void Start()
    {
        GridPosition = new MapSquare(0, 0);
    }

    /// <summary>
    /// The update.
    /// </summary>
    public void Update()
    {
        var coord = MapRenderer.Instance.Grid2Cartesian(GridPosition);
        transform.position = new Vector3(coord.x, coord.y, 0);
    }
}
