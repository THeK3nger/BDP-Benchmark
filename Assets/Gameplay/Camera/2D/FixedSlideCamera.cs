using UnityEngine;
using System.Collections;

/// <summary>
/// Implement a basic fixed-size 2D Zelda-like camera system. The camera slide only when
/// the tracked object reach the boundaries of the block.
/// </summary>
public class FixedSlideCamera : MonoBehaviour {

	/// <summary>
	/// Set the starting block of the camera.
	/// </summary>
	public Vector2 StartingBlock;
	/// <summary>
	/// The block size is the viewport size?
	/// </summary>
	public bool FullViewport;
	/// <summary>
	/// The size of the block.
	/// </summary>
	public Vector2 BlockSize;
	/// <summary>
	/// The tracked object.
	/// </summary>
	public GameObject TrackedObject;
	/// <summary>
	/// The unit per pixel convertion value.
	/// </summary>
	public float UnitPerPixel = 0.01f;
	/// <summary>
	/// The transition time.
	/// </summary>
	public float TransitionTime = 1.0f;

	Vector2 currentBlock;
	Vector2 screenSize;
	int runningItweens = 0;
	
	void Start () {
		currentBlock = StartingBlock;
		Camera.main.orthographicSize = (Screen.height / 2f) * UnitPerPixel;
		screenSize = new Vector2(Screen.width*UnitPerPixel,Screen.height*UnitPerPixel);
		if (FullViewport) BlockSize = screenSize;
		gameObject.transform.position = new Vector3(screenSize.x/2,screenSize.y/2,-10);
	}
	
	// Update is called once per frame
	void Update () {
		if (!FullViewport) {
			// TODO: Track inside the block.
		}
		if (TrackedObject.transform.position.x > (currentBlock.x+1)*BlockSize.x) {
			TranslateCamera("right");
		}
		if (TrackedObject.transform.position.x < (currentBlock.x)*BlockSize.x) {
			TranslateCamera("left");
		}
		if (TrackedObject.transform.position.y > (currentBlock.y+1)*BlockSize.y) {
			TranslateCamera("up");
		}
		if (TrackedObject.transform.position.y < (currentBlock.y)*BlockSize.y) {
			TranslateCamera("down");
		}
	}

	/// <summary>
	/// Performs the camera translation effect.
	/// </summary>
	/// <param name="direction">Direction.</param>
	void TranslateCamera(string direction) {
		string coord = null;
		float amount = 0.0f;
		if (direction == "right") {
			coord = "x";
			amount = screenSize.x;
			currentBlock.x++;
		}
		if (direction == "left") {
			coord = "x";
			amount = -screenSize.x;
			currentBlock.x--;
		}
		if (direction == "down") {
			coord = "y";
			amount = -screenSize.y;
			currentBlock.y--;
		}
		if (direction == "up") {
			coord = "y";
			amount = screenSize.y;
			currentBlock.y++;
		}
		iTween.MoveBy(gameObject,iTween.Hash(
			coord,amount,
			"time",TransitionTime,
			"delay",TransitionTime*runningItweens+0.1,
			"oncomplete","DecreaseCounter"));
		runningItweens++;
	}

	/// <summary>
	/// Decreases the counter o queued iTween instances.
	/// </summary>
	void DecreaseCounter() {
		runningItweens--;
	}

}
