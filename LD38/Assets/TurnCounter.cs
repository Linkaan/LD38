using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnCounter : MonoBehaviour {

	public int turn;
	public int unitSpawnRate;
	public float timeBetweenTurns;

	private float lastTurnTime;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if ((Time.time - lastTurnTime) >= timeBetweenTurns) {
			lastTurnTime = Time.time;
			doTurn ();
		}
	}

	void doTurn () {
		turn++;

		if (turn % unitSpawnRate == 0) {
			foreach (Country country in GameObject.FindObjectsOfType<Country> ()) {
				if (country.owner != null)
					country.units += country.unitGenerationFactor;
			}
		}

		foreach (Player player in GameObject.FindObjectsOfType<Player> ()) {
			if (player.isAlive)
				player.ProcessTurn (turn);
		}

		foreach (General general in GameObject.FindObjectsOfType<General> ()) {
			if (general.owner.generalHome.owner != general.owner) {
				general.owner.isAlive = false;
				foreach (Country country in general.owner.map.colour2country.Values) {
					if (country.owner == general.owner)
						country.SetOwner (general.owner.generalHome.owner, country.units);
				}
				general.kill ();
			}
		}
	}
}
