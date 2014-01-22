using UnityEngine;
using System.Collections;

public class SimpleController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.RightArrow))
			gameObject.transform.Translate(0.2f,0f,0f);
		if (Input.GetKey(KeyCode.LeftArrow))
			gameObject.transform.Translate(-0.2f,0f,0f);
		if (Input.GetKey(KeyCode.UpArrow))
			gameObject.transform.Translate(0.0f,0.2f,0f);
		if (Input.GetKey(KeyCode.DownArrow))
			gameObject.transform.Translate(0.0f,-0.2f,0f);
	}
}
