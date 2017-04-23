using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Continent : MonoBehaviour {

	public Country[] countries;

	// Use this for initialization
	void Awake () {
		countries = this.GetComponentsInChildren <Country> ();
	}
	
	// Update is called once per frame
	void Update () {
		/* if all countries has the same owner set the unit generation factor to 2x */
	}
}
