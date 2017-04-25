using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobeRotating : MonoBehaviour {

	public float rotationX;
	public float rotationY;
	public float rotationZ;
	
	// Update is called once per frame
	void FixedUpdate () {
		transform.Rotate (new Vector3 (rotationX, rotationY, rotationZ));
	}
}
