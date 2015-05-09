﻿using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	public GameObject player;
	public GameObject hand;
	public float cameraSpeed;
	public float zoomSpeed = 1.0f;
	[Range(0,1)] public float handFocusAmount;

	private bool zoom;
	private float zoomAmount;
	
	private bool useBounds = true;
	private Bounds bounds = new Bounds (Vector3.zero, Vector3.one * Mathf.Infinity);
	private float zPosition;
	private float startZoom;

	void Start () {
		// Fixed Z position
		zPosition = transform.position.z;

		// Starting zoom
		startZoom = GetComponent<Camera> ().orthographicSize;
	}

	void LateUpdate () {

		Vector3 pos = player.transform.position;

		Zoom ();

		if (hand.GetComponent<HandController> ().GetState () != HandController.States.idle)
			FocusHand (ref pos);

		if (useBounds)
			InsideBounds (ref pos);

		// Fixed Z position. Don't vary.
		pos.z = zPosition;

		// Lerp
		transform.position = Vector3.Lerp (transform.position, pos, Time.deltaTime * cameraSpeed);

	}

	#region Change camera

	void Zoom() {
		Camera cam = GetComponent<Camera> ();

		// Lerp the size/zoom amount
		cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, zoom ? zoomAmount : startZoom, Time.deltaTime * zoomSpeed);
	}

	void FocusHand(ref Vector3 pos) {
		// Lerp
		pos = Vector3.Lerp (pos, hand.transform.position, handFocusAmount);
	}

	void InsideBounds(ref Vector3 pos) {
		// The camera viewport bounds
		Camera cam = GetComponent<Camera> ();
		Vector3 camSize = new Vector3 (cam.orthographicSize * cam.aspect * 2, cam.orthographicSize * 2);
		Bounds viewport = new Bounds (pos, camSize);
		
		// Keep the camera inside the bounds
		Vector3 center = viewport.center;
		Vector3 size = viewport.size / 2;
		center.x = Mathf.Clamp (center.x, bounds.min.x + size.x, bounds.max.x - size.x);
		center.y = Mathf.Clamp (center.y, bounds.min.y + size.y, bounds.max.y - size.y);

		// Bound is too small
		if (bounds.size.x < viewport.size.x) 
			center.x = bounds.center.x;
		if (bounds.size.y < viewport.size.y)
			center.y = bounds.center.y;
		
		pos = center;
	}

	#endregion

	void OnDrawGizmos() {
		if (useBounds) {
			// Draw bounds
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube (bounds.center, bounds.size);
		}

		// Draw camera
		Camera cam = GetComponent<Camera> ();
		Vector3 camSize = new Vector3 (cam.orthographicSize * cam.aspect * 2, cam.orthographicSize * 2);
		Bounds viewport = new Bounds (transform.position, camSize);

		Gizmos.color = Color.green;
		Gizmos.DrawWireCube (viewport.center, viewport.size);
		Gizmos.DrawRay (viewport.min, viewport.size);
	}

	#region Public methods

	public void SetBounds(Bounds newBounds) {
		bounds = newBounds;
		useBounds = true;
	}

	public void DisableBounds() {
		useBounds = false;
	}

	public void SetZoom(float newZoomAmount) {
		zoom = true;
		zoomAmount = newZoomAmount;
	}

	public void DisableZoom() {
		zoom = false;
	}

	#endregion

}
