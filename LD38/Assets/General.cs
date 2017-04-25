using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class General : MonoBehaviour {

	public Player owner;

	private bool startRotating;

	public void kill () {
		// display death animation
		if (owner.isAi) {
			Destroy (this.gameObject);
			return;
		}
		Camera.main.GetComponent<GlobeRotater> ().LookAtGeneral (true);
		owner.PlayGoodByeSFX ();
		startRotating = true;
		Destroy (this.gameObject, 2);
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (startRotating)
			transform.Rotate (transform.up, 5);
	}
}
