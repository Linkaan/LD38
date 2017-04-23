using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Country : MonoBehaviour {

	public Color colour;
	public List<Vector2> pixels;
	public Vector2 pivot;
	public Vector3 center;
	public Vector3 facing;

	public float minDistanceDisplayText;

	public int units;
	public int unitGenerationFactor;
	public Player owner;
	public Camera cam;
	public Map map;

	public Country[] neighbours;

	public GameObject textPrefab;

	public bool textVisible;

	private TextMesh text;

	void Start () {
		cam = Camera.main;
		this.transform.position = center;
		text = Instantiate (textPrefab, center, Quaternion.identity, this.transform).GetComponent<TextMesh> ();
		text.transform.LookAt (center + facing);
		text.text = this.gameObject.name;
		text.transform.localPosition = new Vector3 (0, 0.05f, 0);

		unitGenerationFactor = 1;
	}
		
	public void SetOwner (Player newOwner, int units) {
		this.units = units;
		this.owner = newOwner;
		this.owner.map.SetCountryPlayerColour (this, newOwner.playerColour);
	}

	public void SetTextVisible (bool visible) {
		if (visible == this.textVisible)
			return;
		this.textVisible = visible;
		this.text.gameObject.SetActive (visible);
	}

	public void attack (Country other, bool useOnlyHalfArmy) {
		if (units < 2)
			return;
		int unitsToAttackWith = useOnlyHalfArmy ? Mathf.CeilToInt (units / 2) : units - 1;

		this.units -= unitsToAttackWith;

		// move the units
		if (other.owner == owner) {
			other.units += unitsToAttackWith;
			return;
		}

		int remainder = unitsToAttackWith - other.units;
		other.units -= unitsToAttackWith;

		// we won this war
		if (other.units <= 0) {
			other.SetOwner (owner, remainder);
		}
	}
		
	void FixedUpdate () {		
		if (this.textVisible)
			fixTextFacing ();

		Vector3 direction;
		checkTextVisibility (out direction);

		float dist = Vector3.Distance (-direction, facing);
		Debug.DrawRay (center, -direction, Color.blue);
		Debug.DrawRay (center, facing, Color.red);
		if (dist < minDistanceDisplayText) {
			Color new_colour = Color.white;
			new_colour.a = Mathf.Max (0, 1 - (dist - 0.4f) * (1f - 0f) / (0.4f - minDistanceDisplayText));
			text.GetComponent <Renderer> ().material.color = new_colour;
		} else {
			text.GetComponent <Renderer> ().material.color = Color.white;
		}
	}

	void fixTextFacing () {
		this.text.transform.LookAt (text.transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);
	}

	void checkTextVisibility (out Vector3 dir) {
		RaycastHit hit;

		Vector3 heading = center - cam.transform.position;

		float distance = heading.magnitude;

		Vector3 direction = heading / distance;

		dir = direction;

		if (!Physics.Raycast (cam.transform.position, direction * 10.0f, out hit)) {
			SetTextVisible (false);
			return;
		}

		Vector2 pixelUV = hit.textureCoord;
		pixelUV.x *= map.globeTexture.width;
		pixelUV.y *= map.globeTexture.height;

		SetTextVisible (map.mapping.GetPixel ((int)pixelUV.x, (int)pixelUV.y) == colour);
	}
}
