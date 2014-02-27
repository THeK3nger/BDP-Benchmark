using UnityEngine;
using System.Collections;

public class AgentPositioning : MonoBehaviour {

    /// <summary>
    /// The agent position expressed in grid coordinates.
    /// </summary>
    public MapSquare GridPosition;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        var coord = MapRenderer.Instance.Grid2Cartesian(GridPosition);
        transform.position = new Vector3(coord.x, coord.y, 0);
	}
}
