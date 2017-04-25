using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour {

	public Player player;

	public int aiSearchDepth;
	public bool processingTurn;

	// Use this for initialization
	void Start () {
		player = GetComponent<Player> ();
		player.isAi = true;
	}

	int miniMaxAlgorithm (WorldState node, int depth, bool maximizingPlayer, int prevValue, int turnCount, Player current, Player against) {
		int heuristic = node.evaluateWorldState (current, against) - (aiSearchDepth - depth) * 10;

		if (depth == 0 || node.isTerminalNode) {
			return prevValue + heuristic;
		}

		if (maximizingPlayer) {
			int bestValue = int.MinValue;
			foreach (WorldState state in node.getPossibleWorldStates (current, turnCount + (aiSearchDepth - depth) + 1)) {
				int v = miniMaxAlgorithm (state, depth - 1, false, heuristic, turnCount, current, against);
				bestValue = Mathf.Max (bestValue, v);
			}
			return bestValue;
		} else {
			int bestValue = int.MaxValue;
			foreach (WorldState state in node.getPossibleWorldStates (current, turnCount + (aiSearchDepth - depth) + 1)) {
				int v = miniMaxAlgorithm (state, depth - 1, true, heuristic, turnCount, current, against);
				bestValue = Mathf.Min (bestValue, v);
			}
			return bestValue;
		}
	}

	public void ProcessTurn (int turnCount) { // start coroutine
		if (player == null)
			return;
		if (processingTurn) {
			Debug.LogWarning ("AI LAGGING BEHIND!!!");
			return;
		}
		StartCoroutine(FindNextTurn(turnCount));
	}

	IEnumerator FindNextTurn (int turnCount) {
		float startTime = Time.time;
		processingTurn = true;
		int bestValue = int.MinValue;
		int bestValue2 = int.MinValue;
		Turn bestTurn = null;
		WorldState origin = new WorldState (player.map);
		//Debug.Log ("---------POSSIBLE MOVES-------");
		foreach (WorldState child in origin.getPossibleWorldStates (player, turnCount + 1)) {
			int value = miniMaxAlgorithm (child, aiSearchDepth, true, 0, turnCount, player, null);
			//printAIMove (child.turn);
		
			foreach (Player p in player.map.players) {
				if (player == p)
					continue;
				foreach (WorldState state in child.getPossibleWorldStates (p, turnCount + 1)) {
					int v = state.evaluateWorldState (p, player);
					bestValue2 = Mathf.Max (bestValue2, v);
				}

				//yield return new WaitForEndOfFrame();
			}

			if (bestValue2 > 1000) {				
				value = bestValue2;
			} else {
				int eval = child.evaluateWorldState (player, null);
				if (eval > 1000 || eval < -1000) {
					value = eval * (eval > 1000 ? 2 : 1);
				}
			}

			if ((bestValue = Mathf.Max (bestValue, value)) == value) {
				bestTurn = child.turn;
			}
			yield return new WaitForEndOfFrame();
		}
		if (bestTurn != null) {
			player.turnQueue.Enqueue (bestTurn);
			printAIMove (bestTurn);
		} else {
			Debug.Log ("AI COULD NOT FIND A MOVE");
		}
		processingTurn = false;
		Debug.Log ("it took " + (Time.time - startTime) + " seconds to calculate AI move");
		yield return null;
	}

	public void printAIMove (Turn turn) {
		if (turn.doMoveGeneral) {
			Debug.Log ("AI MOVE: move general from " + player.generalHome.name + " to " + turn.attackedCountry.name); 
			return;
		}
		if (turn.attackedCountry.owner == player) {
			if (turn.useOnlyHalfArmy) {
				Debug.Log ("AI MOVE: move 50% of army from " + turn.attackingCountry + " to " + turn.attackedCountry);
			} else {
				Debug.Log ("AI MOVE: move army from " + turn.attackingCountry + " to " + turn.attackedCountry);
			}
		} else {
			if (turn.useOnlyHalfArmy) {
				Debug.Log ("AI MOVE: attack with 50% army " + turn.attackedCountry + " from " + turn.attackingCountry);
			} else {
				Debug.Log ("AI MOVE: attack " + turn.attackedCountry + " from " + turn.attackingCountry);
			}
		}
	}

}
