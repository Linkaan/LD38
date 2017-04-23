using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountryInfoDisplayer : MonoBehaviour {

	public Text countryName;
	public Text unitCount;
	public Text unitGenerationFactor;
	public Image countryOwner;
	public GameObject countryDisplayPanel;

	public Camera cam;
	public Map map;

	private Country country;
	
	// Update is called once per frame
	void FixedUpdate () {
		if (map.colour2country == null)
			return;

		RaycastHit hit;
		if (!Physics.Raycast (cam.ScreenPointToRay (Input.mousePosition), out hit))
			return;

		if (hit.transform.tag == "general") {
			countryDisplayPanel.SetActive (true);
			General general = hit.transform.GetComponent<General> ();
			countryName.text = "General";
			unitCount.text = "";
			unitGenerationFactor.text = "";
			countryOwner.color = general.owner.playerColour;
			return;
		}

		Vector2 pixelUV = hit.textureCoord;
		pixelUV.x *= map.globeTexture.width;
		pixelUV.y *= map.globeTexture.height;

		Color pix = map.mapping.GetPixel ((int)pixelUV.x, (int)pixelUV.y);

		country = map.FindCountryByColour (pix);
		countryDisplayPanel.SetActive (country != null);

		if (country == null) {
			return;
		}

		countryName.text = country.name;
		unitCount.text = "Unit count: " + country.units.ToString ();
		unitGenerationFactor.text = "Unit generation factor: " + country.unitGenerationFactor.ToString ();
		countryOwner.color = country.owner != null ? country.owner.playerColour : Color.white;
	}
}
