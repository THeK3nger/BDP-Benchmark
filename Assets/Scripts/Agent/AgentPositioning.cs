using UnityEngine;
using System.Collections;

public class AgentPositioning : MonoBehaviour {

    /// <summary>
    /// The agent position expressed in grid coordinates.
    /// </summary>
    public Vector2 GridPosition;

    /// <summary>
    /// The size of a grid square;
    /// </summary>
    public float CellSize;

    /// <summary>
    /// The origin of the grid;
    /// </summary>
    public Vector2 Origin;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        float x = Origin.x + (1.0f + GridPosition.x) * 0.5f * CellSize;
        float y = Origin.y + (1.0f + GridPosition.y) * 0.5f * CellSize;
        transform.position = new Vector3(x, y, 0);
	}
}
