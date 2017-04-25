using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour {

	public GameObject generalPrefab;
	public GameObject arrow;
	public Country generalHome;
	public GameObject general;
	public General generalComponent;

	public ToastManager toastManager;

	public Color playerColour;
	public Color generalColour;
	public Map map;
	public TurnCounter turnCounter;

	public Queue<Turn> turnQueue;

	public bool isAi;

	public bool isAlive;

	public FollowMouse mouseFollower;

	public Country attackingCountry;
	public bool useOnlyHalfArmy;
	public bool generalSelected;

	public bool canInteract;

	public AudioSource audioSource;

	public AudioClip selectedSFX;
	public AudioClip[] newTurnSFX;
	public AudioClip attackedSFX;
	public AudioClip[] generalSelectedSFX;
	public AudioClip goodbyeSFX;

	private bool moveToNewCountry;

	private float journeyTime;
	private float journeyStartTime;
	private Vector3 generalJourneyStartPosition;
	private Quaternion generalJourneyStartRotation;
	private Quaternion generalJourneyNewRotation;
	private Vector3 generalJourneyPeakPoint;

	private Vector3 journeyDirection;
	private Vector3 avgPoint;

	private bool draggingMouse;
	private bool currentlyDraggingMouse;
	private Vector3 mouseStartPos;

	private bool sfxMute;

	// Use this for initialization
	void Start () {
		audioSource = this.GetComponent<AudioSource> ();
		Country[] countries = map.colour2country.Values.ToArray ();
		do {
			generalHome = countries [Random.Range (0, countries.Length - 1)];
		} while (generalHome.owner != null || checkPlayerIsNeighbour (generalHome));
		generalHome.SetOwner (this, map.unitStartCount);
		generalHome.unitGenerationFactor = 2;

		general = Instantiate (generalPrefab, generalHome.center, Quaternion.identity, this.transform);
		general.transform.LookAt (generalHome.center + generalHome.facing);
		general.GetComponent<MeshRenderer> ().material.color = generalColour;
		generalComponent = general.GetComponent<General> ();
		generalComponent.owner = this;
		//DisplayArrow (generalHome, generalHome.neighbours [0]);

		turnQueue = new Queue<Turn> ();
		isAlive = true;
		canInteract = true;
	}

	public void PlayGeneralSelectedSFX () {
		if (isAi || sfxMute)
			return;
		this.audioSource.clip = generalSelectedSFX [Random.Range (0, generalSelectedSFX.Length)];
		this.audioSource.Play ();
	}

	public void PlayCountrySelectedSFX () {
		if (isAi || sfxMute)
			return;
		this.audioSource.clip = selectedSFX;
		this.audioSource.Play ();
	}

	public void PlayAttackedSFX () {
		if (isAi || sfxMute)
			return;
		this.audioSource.clip = attackedSFX;
		this.audioSource.Play ();
	}

	public void PlayNewTurnSFX () {
		if (isAi || sfxMute)
			return;
		this.audioSource.clip = newTurnSFX [Random.Range (0, newTurnSFX.Length)];
		this.audioSource.Play ();
	}

	public void PlayGoodByeSFX () {
		if (isAi || sfxMute)
			return;
		this.audioSource.clip = goodbyeSFX;
		this.audioSource.Play ();
	}

	public bool checkPlayerIsNeighbour (Country country) {
		foreach (Country neighbour in country.neighbours) {
			if (neighbour.owner != null)
				return true;
		}
		return false;
	}

	public void ProcessTurn (int turnCount) {

		if (isAi) {
			GetComponent<AI> ().ProcessTurn (turnCount);
		}

		if (turnQueue.Count == 0)
			return;

		Turn turn = null;

		do {
			turn = turnQueue.Dequeue ();
		} while (turnQueue.Count > 0 && turn == null);

		if (turn == null)
			return;

		if (turn.doMoveGeneral) {
			if (generalHome.neighbours.Contains (turn.attackedCountry) && turn.attackedCountry.owner == this) {
				moveGeneral (turn.attackedCountry);
			} else if (turn.attackedCountry != null) {
				if (!isAi) {
					toastManager.DisplayToast ("General can't move to " + turn.attackedCountry.name + "!", 5);
				}
				while (turnQueue.Count > 0) {
					Turn newTurn = turnQueue.Peek ();
					if (newTurn.doMoveGeneral)
						turnQueue.Dequeue ();
					else
						break;
				}
			}
		} else {
			if (turn.attackingCountry.owner != this) {
				if (!isAi) {
					toastManager.DisplayToast ("You no longer own " + turn.attackingCountry.name + "!", 5);
				}
			} else {
				if (turn.attackedPlayer != null && !turn.attackedPlayer.isAi && turn.attackedCountry.owner != turn.attackedPlayer) {
					turn.attackedPlayer.toastManager.DisplayToast (turn.attackedCountry.name + " was attacked from " + turn.attackingCountry.name, 5);
					turn.attackedPlayer.PlayAttackedSFX ();
				} else if (turn.attackedCountry.owner != this) {
					this.PlayAttackedSFX ();
				}
				turn.attackingCountry.attack (turn.attackedCountry, turn.useOnlyHalfArmy);
			}
		}
	}
	
	void Update () {
		if (!isAlive || !canInteract)
			return;

		if (!isAi) {
			if (Input.GetKeyDown (KeyCode.M)) {
				sfxMute = !sfxMute;
			}
			selectCountryUpdate ();
		}

		if (moveToNewCountry) {

			float journeyComplete = (Time.time - journeyStartTime) / journeyTime;

			Vector3 startRelCenter = generalJourneyStartPosition - generalJourneyPeakPoint;
			Vector3 endRelCenter = generalHome.center - generalJourneyPeakPoint;

			general.transform.position = Vector3.Slerp (startRelCenter, endRelCenter, journeyComplete);
			general.transform.position += generalJourneyPeakPoint;

			general.transform.rotation = Quaternion.Lerp (generalJourneyStartRotation, generalJourneyNewRotation, journeyComplete);

			if (journeyComplete >= 1.0f) {
				// stop animation
				moveToNewCountry = false;
			}
		}

		Debug.DrawRay (map.globeCenter.position + journeyDirection * 15f, -journeyDirection * 15f, Color.green);
	}

	void moveGeneral (Country newCountry) {
		moveToNewCountry = true;
		journeyTime = turnCounter.timeBetweenTurns;
		journeyStartTime = Time.time;
		generalJourneyStartPosition = general.transform.position;
		generalJourneyStartRotation = general.transform.rotation;
		generalJourneyNewRotation = Quaternion.LookRotation ((newCountry.center + newCountry.facing) - newCountry.center);

		avgPoint = (generalHome.center + newCountry.center) / 2;
		Vector2 newPivot = findClosestPivotPoint (avgPoint, out journeyDirection);
		generalJourneyPeakPoint = map.UvTo3D (newPivot, map.GetComponent<MeshFilter> ().mesh) - journeyDirection * 2f;

		generalHome = newCountry;
	}

	List<Country> BFS (Country start, Country end) {
		HashSet<Country> S = new HashSet<Country> ();
		Queue<Country> Q = new Queue<Country> ();

		List<Country> path = new List<Country> ();

		Dictionary<Country, Country> previous = new Dictionary<Country, Country> ();

		Country current = start;
		S.Add (current);
		Q.Enqueue (current);
		while (Q.Count > 0) {
			current = Q.Dequeue ();
			if (current == end)
				break;
			
			foreach (Country neighbour in current.neighbours) {
				if (S.Contains (neighbour) || neighbour.owner != this)
					continue;
				
				S.Add (neighbour);
				previous [neighbour] = current;
				Q.Enqueue (neighbour);
			}
		}
			
		if (current != end)
			return null;

		while (previous.ContainsKey (current)) {
			if (current != start) {				
				path.Add (current);
			} else
				break;
			current = previous[current];
		}
		path.Reverse ();
			
		return path;
	}

	Turn[] findTurnPathToCountry (Country home, Country newCountry) {
		List<Turn> turns = new List<Turn> ();

		List<Country> path = BFS (home, newCountry);
		if (path == null)
			return null;

		int i = 1;
		foreach (Country moveTo in path) {
			Turn turn = new Turn (turnCounter.turn + turnQueue.Count + i, this, moveTo);
			turns.Add (turn);
			i++;
		}

		return turns.ToArray ();
	}

	void selectCountryUpdate () {

		if (Input.GetMouseButtonDown (0)) {
			draggingMouse = true;
			mouseStartPos = Input.mousePosition;
		}

		if (Input.GetMouseButton (0) && (currentlyDraggingMouse || (draggingMouse && mouseStartPos != Input.mousePosition))) {
			currentlyDraggingMouse = true;
			mouseFollower.showCursorDragging ();
		} else if (attackingCountry == null && !generalSelected) {
			mouseFollower.showCursorPointing ();
		}

		if (Input.GetKeyDown (KeyCode.Escape)) {
			generalSelected = false;

			if (attackingCountry != null) {
				foreach (Country neighbour in attackingCountry.neighbours) {
					neighbour.SetSelected (false);
				}
			}

			attackingCountry = null;
			useOnlyHalfArmy = false;
		}

		if (Input.GetMouseButtonUp (0)) {			
			mouseFollower.showCursorPointing ();
			if (currentlyDraggingMouse) {
				currentlyDraggingMouse = false;
				return;
			}
			currentlyDraggingMouse = false;
		} else if (attackingCountry == null && !generalSelected)/* if ((currentlyDraggingMouse || (draggingMouse && mouseStartPos != Input.mousePosition))) */{
			return;
		}
		
		RaycastHit hit;
		if (!Physics.Raycast (map.cam.ScreenPointToRay (Input.mousePosition), out hit))
			return;

		if (Input.GetMouseButtonUp (0) && hit.transform.tag == "general" && hit.transform.gameObject.GetComponent<General> ().owner == this) {
			generalSelected = true;
			mouseFollower.showCursorMove ();
			PlayGeneralSelectedSFX ();
			return;
		}

		Vector2 pixelUV = hit.textureCoord;
		pixelUV.x *= map.globeTexture.width;
		pixelUV.y *= map.globeTexture.height;

		Color pix = map.mapping.GetPixel ((int)pixelUV.x, (int)pixelUV.y);

		Country country = map.FindCountryByColour (pix);

		if (attackingCountry != null || generalSelected) {
			if (useOnlyHalfArmy)
				mouseFollower.showCursorAttack50 ();
			else {
				if (generalSelected || (country != null && country.owner == this)) {
					mouseFollower.showCursorMove ();
				} else {
					mouseFollower.showCursorAttack ();
				}
			}
		} else {
			mouseFollower.showCursorPointing ();
		}

		if (!Input.GetMouseButtonUp (0))
			return;

		if (attackingCountry == null || generalSelected) {
			if (country == null) {				
				toastManager.DisplayToast ("That is not a country!", 5);
			} else if (country.owner != this) {
				toastManager.DisplayToast ("You don't own that country (" + country.name + ")!", 5);
			} else {
				if (generalSelected && country == null) {
					generalSelected = false;
					return;
				}
				
				if (generalSelected) {
					generalSelected = false;
					attackingCountry = null;
					Turn[] generalTurns = findTurnPathToCountry (generalHome, country);
					PlayNewTurnSFX ();
					if (generalTurns == null) {
						toastManager.DisplayToast ("General could not find a safe path to " + country.name + "!", 5);
					} else {
						foreach (Turn turn in generalTurns) {							
							turnQueue.Enqueue (turn);
						}
						turnCounter.tqDisplayer.UpdateTurnDisplay (this);
					}
					return;
				}
				attackingCountry = country;	
				foreach (Country neighbour in attackingCountry.neighbours) {
					neighbour.SetSelected (true);
				}
				PlayCountrySelectedSFX ();
			}
		} else if (country != null) {			
			if (attackingCountry == country) {
				useOnlyHalfArmy = !useOnlyHalfArmy;
				return;
			}
			if (attackingCountry.neighbours.Contains (country)) {
				Turn newTurn = new Turn (turnCounter.turn + turnQueue.Count + 1, this, country.owner, attackingCountry, country, useOnlyHalfArmy);
				turnQueue.Enqueue (newTurn);
				PlayNewTurnSFX ();
				turnCounter.tqDisplayer.UpdateTurnDisplay (this);

				foreach (Country neighbour in attackingCountry.neighbours) {
					neighbour.SetSelected (false);
				}
				attackingCountry = null;
			} else {
				toastManager.DisplayToast ("Can't attack " + country.name + " from " + attackingCountry.name + "!", 5);
			}
			useOnlyHalfArmy = false;
		}
	}

	Vector2 findClosestPivotPoint (Vector3 avgCenter, out Vector3 dir) {
		Vector3 heading = map.globeCenter.position - avgCenter;

		float distance = heading.magnitude;

		Vector3 direction = -(heading / distance);

		dir = Vector3.zero;

		RaycastHit hit;
		if (!Physics.Raycast (map.globeCenter.position + direction * 15f, -direction * 15f, out hit))
			return Vector2.zero;

		dir = direction;

		return hit.textureCoord;
	}
}
