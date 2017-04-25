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

	public WinManager winManager;

	public bool hasWon;

	private float lastTurnTime;

	private bool initalizedLeaderboard;
	
	// Update is called once per frame
	void Update () {
		if (hasWon)
			return;

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

		foreach (Country country in map.colour2country.Values) {
			if (country.owner != null) {
				if (country.owner.generalHome == country) {
					country.unitGenerationFactor = 2;
				} else {
					country.unitGenerationFactor = 1;
				}
			}
		}

		if (turn % unitSpawnRate == 0) {
			foreach (Country country in map.colour2country.Values) {
				if (country.owner != null)
					country.units += country.unitGenerationFactor;
			}
		}

		foreach (Player player in GameObject.FindObjectsOfType<Player> ()) {
			if (player.isAlive)
				player.ProcessTurn (turn);
		}

		int generalCount = 0;
		foreach (General general in GameObject.FindObjectsOfType<General> ()) {			
			if (general.owner.generalHome.owner != general.owner) {
				general.owner.isAlive = false;
				foreach (Country country in general.owner.map.colour2country.Values) {
					if (country.owner == general.owner)
						country.SetOwner (general.owner.generalHome.owner, country.units);
				}
				general.kill ();
				continue;
			}
			generalCount += 1;
		}

		leaderboard.UpdateLeaderboard (map);
		tqDisplayer.UpdateTurnDisplay (map.localPlayer);

		if (generalCount == 1) {
			foreach (Player player in GameObject.FindObjectsOfType<Player> ()) {
				if (player.isAlive) {
					winManager.SetWinner (player);
					hasWon = true;
					break;
				}
			}
		}
	}
}
