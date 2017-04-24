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

	public Color playerColour;
	public Color generalColour;
	public Map map;
	public TurnCounter turnCounter;

	public Queue<Turn> turnQueue;

	public bool isAi;

	public bool isAlive;

	private Country attackingCountry;
	private bool useOnlyHalfArmy;
	private bool moveToNewCountry;
	private bool generalSelected;

	private float journeyTime;
	private float journeyStartTime;
	private Vector3 generalJourneyStartPosition;
	private Quaternion generalJourneyStartRotation;
	private Quaternion generalJourneyNewRotation;
	private Vector3 generalJourneyPeakPoint;

	private Vector3 journeyDirection;
	private Vector3 avgPoint;

	// Use this for initialization
	void Start () {
		Country[] countries = map.colour2country.Values.ToArray ();
		do {
			generalHome = countries [Random.Range (0, countries.Length - 1)];
		} while (generalHome.owner != null || checkPlayerIsNeighbour (generalHome));
		generalHome.SetOwner (this, map.unitStartCount);

		general = Instantiate (generalPrefab, generalHome.center, Quaternion.identity, this.transform);
		general.transform.LookAt (generalHome.center + generalHome.facing);
		general.GetComponent<MeshRenderer> ().material.color = generalColour;
		generalComponent = general.GetComponent<General> ();
		generalComponent.owner = this;
		//DisplayArrow (generalHome, generalHome.neighbours [0]);

		turnQueue = new Queue<Turn> ();
		isAlive = true;
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

		Turn turn = turnQueue.Dequeue ();

		Debug.Log ("prediction correct: " + (turn.turnCount == turnCount).ToString());

		if (turn.doMoveGeneral) {
			if (generalHome.neighbours.Contains (turn.attackedCountry)) {
				moveGeneral (turn.attackedCountry);
			} else {
				Debug.LogError ("PATH NO LONGER AVAILABLE");
				while (turnQueue.Count > 0) {
					Turn newTurn = turnQueue.Peek ();
					if (newTurn.doMoveGeneral)
						turnQueue.Dequeue ();
					else
						break;
				}
			}
		} else {
			turn.attackingCountry.attack (turn.attackedCountry, turn.useOnlyHalfArmy);
		}
	}
	
	void Update () {
		if (!isAlive)
			return;

		if (!isAi)
			selectCountryUpdate ();

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
		if (!Input.GetMouseButtonDown (1))
			return;
		
		RaycastHit hit;
		if (!Physics.Raycast (map.cam.ScreenPointToRay (Input.mousePosition), out hit))
			return;

		if (hit.transform.tag == "general" && hit.transform.gameObject.GetComponent<General> ().owner == this) {
			generalSelected = true;
			Debug.Log ("selected general");
			return;
		}

		Vector2 pixelUV = hit.textureCoord;
		pixelUV.x *= map.globeTexture.width;
		pixelUV.y *= map.globeTexture.height;

		Color pix = map.mapping.GetPixel ((int)pixelUV.x, (int)pixelUV.y);

		Country country = map.FindCountryByColour (pix);
		if (attackingCountry == null || generalSelected) {
			if (country == null || country.owner != this) {				
				Debug.LogError ("YOU MUST FIRST SELECT THE ATTACKING COUNTRY!");
			} else {
				if (generalSelected && attackingCountry == null)
					generalSelected = false;
				
				if (generalSelected) {
					generalSelected = false;
					attackingCountry = null;
					Turn[] generalTurns = findTurnPathToCountry (generalHome, country);
					if (generalTurns == null) {
						Debug.LogError ("NO SAFE PATH TO COUNTRY FOUND!");
					} else {
						foreach (Turn turn in generalTurns) {							
							turnQueue.Enqueue (turn);
						}
						turnCounter.tqDisplayer.UpdateTurnDisplay (this);
					}
					return;
				}
				attackingCountry = country;	
			}
		} else if (country != null) {			
			if (attackingCountry == country) {
				useOnlyHalfArmy = true;
				Debug.Log ("using 50% of army");
				return;
			}
			if (attackingCountry.neighbours.Contains (country)) {
				Turn newTurn = new Turn (turnCounter.turn + turnQueue.Count + 1, this, country.owner, attackingCountry, country, useOnlyHalfArmy);
				turnQueue.Enqueue (newTurn);
				turnCounter.tqDisplayer.UpdateTurnDisplay (this);

				attackingCountry = null;
			} else {
				Debug.LogError ("YOU CAN ONLY ATTACK YOUR NEIGHBOURS!");
			}
			useOnlyHalfArmy = false;
		}
	}

	void DisplayArrow (Country A, Country B) {
		if (A == null || B == null) {
			arrow.SetActive (false);
			return;
		}
		arrow.SetActive (true);

		Vector3 avgCenter = (A.center + B.center) / 2.5f;
		Vector3 direction;
		Vector2 avgPoint = findClosestPivotPoint (avgCenter, out direction);

		if (avgPoint == Vector2.zero) {
			Debug.LogError ("could not find closest pivot point!");
			return;
		}

		Vector3 origin = map.UvTo3D (avgPoint, map.GetComponent<MeshFilter> ().mesh) + direction * 0.1f;
		Vector3 heading = B.center - origin;

		float distance = heading.magnitude;

		arrow.transform.position = origin;
		arrow.transform.LookAt (heading / distance);
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
