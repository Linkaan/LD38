using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobeRotater : MonoBehaviour {

	public float maximumMomentum;
	public float momentumInertia;
	public float momentumDrag;
	public float momentumAxisChangeFactor;

	public float maxZoom;
	public float zoomSpeed;

	public Transform target;
	public Player player;
	public Camera cam;

	private float momentum;

	private float startX;
	private float startY;

	private float newX;
	private float newY;

	private float slerpedYchange;
	private float slerpedXchange;

	private bool lookAtGoal;
	private Quaternion startRotation;
	private Quaternion lookAtGoalRotation;
	private float lookAtJourneyLength;
	private float lookAtStartTime;
	private float lookAtJourneyTime;

	private float zoomAmount;

	private Vector3 startPos;
	private Vector3 endPos;

	void Start () {
		cam = GetComponent<Camera> ();
		Vector3 mousePos = Input.mousePosition;
		startPos = cam.transform.localPosition;
		endPos = startPos - Vector3.forward * maxZoom;

	}
	
	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			startX = Input.GetAxis ("Mouse X");
			startY = Input.GetAxis ("Mouse Y");
		} else if (Input.GetMouseButton (0)) {
			newX = startX - Input.GetAxis ("Mouse X");
			newY = startY - Input.GetAxis ("Mouse Y");
			momentum = maximumMomentum;
		}
			
		float Xchange = Input.GetAxis ("Horizontal") * momentumAxisChangeFactor;
		float Ychange = Input.GetAxis ("Vertical") * momentumAxisChangeFactor;
		if (Xchange != 0 || Ychange != 0) {
			if (!Input.GetMouseButton (0)) {
				newX = 0;
				newY = 0;
			}
			momentum = maximumMomentum;
		}

		zoomAmount += Input.GetAxis ("Mouse ScrollWheel") * zoomSpeed * Time.deltaTime;
		if (zoomAmount > 1)
			zoomAmount = 1;
		else if (zoomAmount <= 0)
			zoomAmount = 0;

		slerpedYchange = Mathf.Lerp (slerpedYchange, (newY + Ychange), momentumInertia * Time.deltaTime);
		slerpedXchange = Mathf.Lerp (slerpedXchange, (newX + Xchange), momentumInertia * Time.deltaTime);

		cam.transform.localPosition = Vector3.Lerp (startPos, endPos, zoomAmount);

		if (lookAtGoal) {
			slerpedYchange = 0;
			slerpedXchange = 0;
			newY = 0;
			newX = 0;
			Ychange = 0;
			Xchange = 0;

			float fracComplete = (Time.time - lookAtStartTime) / lookAtJourneyTime;

			target.rotation = Quaternion.Slerp (target.rotation, lookAtGoalRotation, fracComplete);
			if (fracComplete > 1.0f)
				lookAtGoal = false;
		} else {
			target.Rotate (transform.right, slerpedYchange * momentum * Time.deltaTime, Space.World);
			target.Rotate (-transform.up, slerpedXchange * momentum * Time.deltaTime, Space.World);
		}
	}

	public void LookAtGeneral () {
		startRotation = target.rotation;
		lookAtStartTime = Time.time;
		lookAtGoalRotation = Quaternion.LookRotation (player.general.transform.position - target.position);
		lookAtJourneyLength = Quaternion.Angle (startRotation, lookAtGoalRotation);
		lookAtJourneyTime = Mathf.Min (lookAtJourneyLength / 180.0f, 1.0f);
		lookAtGoal = true;
	}

	void FixedUpdate () {
		if (momentum != 0) {
			momentum -= momentum > 0 ? momentumDrag : -momentumDrag;
			if (Mathf.Abs (momentum - momentumDrag) < momentumDrag)
				momentum = 0;
		}
	}
}
