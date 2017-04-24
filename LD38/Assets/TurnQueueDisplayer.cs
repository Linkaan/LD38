using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnQueueDisplayer : MonoBehaviour {

	public Text[] attackingCountry;
	public Image[] action;
	public Text[] attackedCountry;

	public Sprite moveAction;
	public Sprite attackAction;
	public Sprite attack50Action;

	public GameObject[] turnPlaceholders;

	public void UpdateTurnDisplay (Player player) {
		Queue<Turn> queue = new Queue<Turn> (player.turnQueue);
		foreach (GameObject go in turnPlaceholders) {
			go.SetActive (false);
		}
		for (int i = 0; i < action.Length; i++) {
			if (queue.Count == 0)
				break;
			turnPlaceholders [i].SetActive (true);
			Turn turn = queue.Dequeue ();
			if (turn.doMoveGeneral) {
				attackingCountry [i].text = "General";
				action [i].overrideSprite = moveAction;
				attackedCountry [i].text = turn.attackedCountry.name;
			} else {
				attackingCountry [i].text = turn.attackingCountry.name;
				attackedCountry [i].text = turn.attackedCountry.name;

				if (turn.useOnlyHalfArmy) {
					action [i].overrideSprite = attack50Action;
				} else {
					action [i].overrideSprite = attackAction;
				}
			}
		}
	}
}
