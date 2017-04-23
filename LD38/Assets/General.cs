using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class General : MonoBehaviour {

	public Player owner;

	public void kill () {
		// display death animation
		Destroy (this.gameObject);
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
