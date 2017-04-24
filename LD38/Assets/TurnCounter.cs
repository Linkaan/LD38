using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnCounter : MonoBehaviour {

	public int turn;
	public int unitSpawnRate;
	public float timeBetweenTurns;

	public Map map;
	public Leaderboard leaderboard;
	public TurnQueueDisplayer tqDisplayer;

	private float lastTurnTime;

	private bool initalizedLeaderboard;
	
	// Update is called once per frame
	void Update () {
		if (!initalizedLeaderboard) {
			initalizedLeaderboard = true;
			leaderboard.UpdateLeaderboard (map);
			tqDisplayer.UpdateTurnDisplay (map.localPlayer);
		}

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

		leaderboard.UpdateLeaderboard (map);
		tqDisplayer.UpdateTurnDisplay (map.localPlayer);
	}
}
