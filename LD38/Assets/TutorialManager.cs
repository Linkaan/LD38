using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class TutorialManager : MonoBehaviour {

	public Text toastText;
	public GameObject toast;

	public GameObject tqDisplayer;
	public GameObject generalButton;
	public GameObject countryInfo;
	public GameObject leaderboard;
	public GameObject mouseIcon;

	public GameObject continueButton;

	public GlobeRotater globeRotater;

	public Player player;

	public Player ai1;
	public Player ai2;

	private float startTime;
	private float duration;

	private int state;

	private bool handledState;
	private bool doNotIncrementState;

	private bool generalButtonWasPressed;
	private bool continueButtonWasPressed;

	private bool delayedIncrementState;
	private float startDelayedIncrementState;

	private bool isWaiting;

	private Country newCountry;

	void Start () {
		ai1.generalHome.units = 0;
		ai2.generalHome.units = 0;
		player.canInteract = false;
		globeRotater.canInteract = false;

		state = 0;
		isWaiting = true;
		DisplayToast ("Welcome to planet Nabelon", 4);
	}

	void Update () {
		if (!isWaiting && (Time.time - startTime) > duration) {
			isWaiting = true;
			if (!doNotIncrementState) {
				continueButton.SetActive (true);
			} else
				doNotIncrementState = false;
		}

		if (delayedIncrementState) {
			if ((Time.time - startDelayedIncrementState) > 1f) {
				delayedIncrementState = false;
				state += 1;
				handledState = false;
			}
		}

		if (continueButtonWasPressed) {
			continueButtonWasPressed = false;
			toast.SetActive (false);

			state += 1;
			handledState = false;
			continueButton.SetActive (false);
			isWaiting = true;
		}

		if (!handledState) {
			switch (state) {
			case 1:
				DisplayToast ("You are a general who wish to conquer the whole planet", 0.5f);
				handledState = true;
				break;
			case 2:
				DisplayToast ("However two other generals have the exact same intention", 0.5f);
				handledState = true;
				break;
			case 3:
				DisplayToast ("To win you must take out all other generals", 0.5f);
				handledState = true;
				break;
			case 4:
				ActivateNavigation (true);
				DisplayToast ("To navigate drag with your mouse or use ASWD", 0.5f);
				handledState = true;
				break;
			case 5:
				ActivateNavigation (false);
				DisplayToast ("The green tiles are countries that can be either unoccupied (green) ...", 0.5f);
				handledState = true;
				break;
			case 6:
				DisplayToast ("... or occupied by another player", 0.5f);
				handledState = true;
				break;
			case 7:
				DisplayToast ("The current owner of the country and the amount of units ...", 0.5f);
				handledState = true;
				break;
			case 8:
				DisplayToast ("... will be displayed in the upper left corner when you hover over a country", 0.5f);
				handledState = true;
				break;
			case 9:				
				countryInfo.SetActive (true);
				doNotIncrementState = true;
				ActivateNavigation (true);
				DisplayToast ("Try hovering over a country with your mouse", 0.5f);
				state += 1;
				break;
			case 10:
				if (countryInfo.GetComponent<CountryInfoDisplayer> ().isDisplaying) {
					doNotIncrementState = false;
					delayedIncrementState = true;
					startDelayedIncrementState = Time.time;
					handledState = true;
				}
				break;
			case 11:
				ActivateNavigation (false);
				DisplayToast ("Great! Now let's explain how units work", 0.5f);
				handledState = true;
				break;
			case 12:
				DisplayToast ("The units in each country is used to attack other countries", 0.5f);
				handledState = true;
				break;
			case 13:
				DisplayToast ("Country A must have atleast one more unit than country B to conquer that country", 0.5f);
				handledState = true;
				break;
			case 14:
				DisplayToast ("Every fifth turn each country that you own will receive units based on the ...", 0.5f);
				handledState = true;
				break;
			case 15:
				DisplayToast ("... unit generation factor that is displayed when you hover over a country", 0.5f);
				handledState = true;
				break;
			case 16:
				DisplayToast ("This unit generation factor is normally 1 but will be 2 if the general occupies that country", 0.5f);
				handledState = true;
				break;
			case 17:				
				DisplayToast ("To take out a general you must conqeur the country ...", 0.5f);
				handledState = true;
				break;
			case 18:
				DisplayToast ("... that the general is occupying", 0.5f);
				handledState = true;
				break;
			case 19:
				DisplayToast ("But make sure that your own general is safe!", 0.5f);
				handledState = true;
				break;
			case 20:
				DisplayToast ("If your general is in danger you can click on the general ...", 0.5f);
				handledState = true;
				break;
			case 21:
				DisplayToast ("... and then on another country that you own", 0.5f);
				handledState = true;
				break;
			case 22:
				DisplayToast ("The general can only move to that country if there is a safe path", 0.5f);
				handledState = true;
				break;
			case 23:
				DisplayToast ("That means the home country of the general must be connected to ...", 0.5f);
				handledState = true;
				break;
			case 24:
				DisplayToast ("... the country that you clicked on", 0.5f);
				handledState = true;
				break;
			case 25:
				DisplayToast ("You can only do ONE action per turn", 0.5f);
				handledState = true;
				break;
			case 26:
				DisplayToast ("The current turn queue is displayed in the bottom left corner", 0.5f);
				tqDisplayer.SetActive (true);
				handledState = true;
				break;
			case 27:
				DisplayToast ("The actions available are: move, attack, move/attack 50%", 0.5f);
				handledState = true;
				break;
			case 28:
				DisplayToast ("The move action will move every unit except 1 from A to B", 0.5f);
				handledState = true;
				break;
			case 29:
				DisplayToast ("B must be adjacent to A and you must own both countries", 0.5f);
				handledState = true;
				break;
			case 30:
				DisplayToast ("The attack move works like the move action but B must be ...", 0.5f);
				handledState = true;
				break;
			case 31:
				DisplayToast ("... an enemy country or unoccupied country (green)", 0.5f);
				handledState = true;
				break;
			case 32:
				DisplayToast ("The move/attack 50% action will do the same but with only 50% of units", 0.5f);
				handledState = true;
				break;
			case 33:
				ActivateNavigation (true);
				generalButton.SetActive (true);
				doNotIncrementState = true;
				DisplayToast ("To find the position of your general: click on the button in the bottom center", 0.5f);
				state += 1;
				generalButtonWasPressed = false;
				break;
			case 34:
				if (generalButtonWasPressed) {
					doNotIncrementState = false;
					delayedIncrementState = true;
					startDelayedIncrementState = Time.time;
					handledState = true;
				}
				break;
			case 35:
				ActivateNavigation (false);
				DisplayToast ("Good! To attack a country first click on the country that you want to attack with", 0.5f);
				handledState = true;
				break;
			case 36:
				SetCanInteract (true);
				doNotIncrementState = true;
				DisplayToast ("Try clicking on " + player.generalHome.name + " that your general stands on (the red country)", 0.5f);
				state += 1;
				break;
			case 37:
				if (player.attackingCountry != null) {
					doNotIncrementState = false;
					state += 1;
				}
				break;
			case 38:
				doNotIncrementState = true;
				DisplayToast ("Now click on one of the highlighted adjacent countries to attack!", 0.5f);
				state += 1;
				break;
			case 39:
				if (player.turnQueue.Count > 0 && player.turnQueue.Peek ().attackedPlayer != player) {
					newCountry = player.turnQueue.Peek ().attackedCountry;
					doNotIncrementState = false;
					state += 1;
				}
				break;
			case 40:
				doNotIncrementState = true;
				DisplayToast ("Your move was queued to the turn queue (wait for the turn to end)", 0.5f);
				state += 1;
				break;
			case 41:
				if (player.turnQueue.Count == 0) {
					doNotIncrementState = false;
					state += 1;
				}
				break;
			case 42:
				SetCanInteract (false);
				DisplayToast ("Great, you now own " + newCountry.name, 0.5f);
				handledState = true;
				break;
			case 43:				
				DisplayToast ("To move/attack a country with 50% of units click on the attacking country twice", 0.5f);
				handledState = true;
				break;
			case 44:
				SetCanInteract (true);
				doNotIncrementState = true;
				DisplayToast ("Try clicking on " + newCountry.name + " twice", 0.5f);
				state += 1;
				break;
			case 45:
				if (player.attackingCountry != null && player.useOnlyHalfArmy) {
					doNotIncrementState = false;
					state += 1;
				}
				break;
			case 46:
				doNotIncrementState = true;
				DisplayToast ("Now click on one of the highlighted adjacent countries to attack/move with 50% units!", 0.5f);
				state += 1;
				break;
			case 47:
				if (player.turnQueue.Count > 0 && player.turnQueue.Peek ().useOnlyHalfArmy) {
					doNotIncrementState = false;
					state += 1;
				}
				break;
			case 48:
				doNotIncrementState = true;
				DisplayToast ("Your move was queued to the turn queue (wait for the turn to end)", 0.5f);
				state += 1;
				break;
			case 49:
				if (player.turnQueue.Count == 0) {
					doNotIncrementState = false;
					state += 1;
				}
				break;
			case 50:
				SetCanInteract (false);
				leaderboard.SetActive (true);
				DisplayToast ("To deselect a country or cancel your current action press ESC", 0.5f);
				handledState = true;
				break;
			case 51:
				DisplayToast ("The current amount of units and countries for each player is displayed ...", 0.5f);
				handledState = true;
				break;
			case 52:
				DisplayToast ("... in the leaderboard in the upper right corner", 0.5f);
				handledState = true;
				break;
			case 53:
				DisplayToast ("Now to finish the tutorial you must take out one other general", 0.5f);
				handledState = true;
				break;
			case 54:
				DisplayToast ("To do this take over the country that the general occupies", 0.5f);
				handledState = true;
				break;
			case 55:
				DisplayToast ("Press the back to main menu button if you want to play a real game", 0.5f);
				handledState = true;
				break;
			case 56:
				SetCanInteract (true);
				doNotIncrementState = true;
				DisplayToast ("Objective: take out one other general by taking the country that the general occupies", 0.5f);
				state += 1;
				break;
			case 57:
				if (!ai1.isAlive || !ai2.isAlive) {
					doNotIncrementState = false;
					state += 1;
				}
				break;
			case 58:
				DisplayToast ("Excellent, now try playing against the AI", 0.5f);
				handledState = true;
				break;
			case 59:
				SceneManager.LoadScene (0);
				handledState = true;
				break;
			}
		}
	}

	void ActivateNavigation (bool activate) {
		mouseIcon.SetActive (activate);
		globeRotater.canInteract = activate;
	}

	void SetCanInteract (bool can) {
		ActivateNavigation (can);
		player.canInteract = can;
	}

	public void ContinueButtonPressed () {
		continueButtonWasPressed = true;
	}

	public void GeneralButtonPressed () {
		generalButtonWasPressed = true;
	}

	public void DisplayToast (string text, float duration) {
		this.duration = duration;
		this.startTime = Time.time;
		isWaiting = false;
		toast.SetActive (true);
		toastText.text = text;
	}
}
