using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class countrySelector : MonoBehaviour {

	public Camera cam;
	public Texture2D mapping;
	public Map map;

	private Renderer globeRenderer;
	private Texture2D globeTexture;

	void Start () {
		cam = GetComponent<Camera> ();
		globeRenderer = map.GetComponent<Renderer> ();
		globeTexture = globeRenderer.material.mainTexture as Texture2D;

		foreach (Country country in map.colour2country.Values) {
			foreach (Vector2 vec in country.pixels) {
				globeTexture.SetPixel ((int)vec.x, (int)vec.y, map.defaultColour);
			}
			globeTexture.Apply ();
		}
	}

	void Update () {
		if (!Input.GetMouseButton (1))
			return;

		RaycastHit hit;
		if (!Physics.Raycast (cam.ScreenPointToRay (Input.mousePosition), out hit))
			return;

		Vector2 pixelUV = hit.textureCoord;
		pixelUV.x *= globeTexture.width;
		pixelUV.y *= globeTexture.height;

		Color pix = mapping.GetPixel ((int)pixelUV.x, (int)pixelUV.y);

		SetColourByCountryColour (pix, Color.blue);
	}

	void SetColourByCountryColour (Color pix, Color newColour) {
		Country country = map.FindCountryByColour (pix);
		if (country == null)
			return;
		
		foreach (Vector2 vec in country.pixels) {
			globeTexture.SetPixel ((int)vec.x, (int)vec.y, newColour);
		}
		globeTexture.Apply ();
	}

	public void SetCountryPlayerColour (Country country, Color playerColour) {
		foreach (Vector2 vec in country.pixels) {
			globeTexture.SetPixel ((int)vec.x, (int)vec.y, playerColour);
		}
		globeTexture.Apply ();
	}
}
