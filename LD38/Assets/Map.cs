using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour {

	public Texture2D mapping;
	public Dictionary<Color, Country> colour2country;
	public Color defaultColour;
	public Camera cam;
	public Transform globeCenter;

	public Renderer globeRenderer;
	public Texture2D globeTexture;

	public TurnCounter turnCounter;

	public int unitStartCount;

	public Player[] players;

	void Awake () {
		colour2country = new Dictionary<Color, Country> ();
		foreach (Country country in GetComponentsInChildren <Country> ()) {
			colour2country [country.colour] = country;
		}

		Country newCountry = null;
		Color lastColour = defaultColour;
		Dictionary<Country, SummedVector2> summedPoints = new Dictionary<Country, SummedVector2> (); 
		for (int y = 0; y < mapping.height; y++) {
			for (int x = 0; x < mapping.width; x++) {
				Color colour = mapping.GetPixel (x, y);
				if ((lastColour == colour && newCountry != null) || (newCountry = FindCountryByColour(colour)) != null) {
					lastColour = newCountry.colour;
					Vector2 p = new Vector2 (x, y);
					newCountry.pixels.Add (p);

					if (!summedPoints.ContainsKey (newCountry))
						summedPoints [newCountry] = new SummedVector2 (p);
					else
						summedPoints [newCountry].AddPoint (p);
				}
			}
		}

		foreach (Country country in summedPoints.Keys) {
			SummedVector2 sumPoints = summedPoints [country];
			country.pivot = sumPoints.getAvgPoint ();
			country.pivot.x /= mapping.width;
			country.pivot.y /= mapping.height;
			country.center = UvTo3D (country.pivot, this.GetComponent<MeshFilter> ().mesh);

			Vector3 heading = globeCenter.position - country.center;
			float distance = heading.magnitude;

			country.facing = -(heading / distance);
		}

		cam = Camera.main;
		globeRenderer = GetComponent<Renderer> ();
		globeTexture = globeRenderer.material.mainTexture as Texture2D;

		foreach (Country country in colour2country.Values) {
			foreach (Vector2 vec in country.pixels) {
				globeTexture.SetPixel ((int)vec.x, (int)vec.y, defaultColour);
			}
			globeTexture.Apply ();
		}

		players = GameObject.FindObjectsOfType <Player> ();
	}
		
	public Country FindCountryByColour (Color colour) {
		if (!colour2country.ContainsKey (colour))
			return null;
		return colour2country [colour];
	}

	public void SetColourByCountryColour (Color pix, Color newColour) {
		Country country = FindCountryByColour (pix);
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

	/* Convert uv coordinates to 3d world space
	 * See: answers.unity3d.com/answers/372156/view.html
	 */
	public Vector3 UvTo3D (Vector2 uv, Mesh mesh) {		
		int [] tris = mesh.triangles;
		Vector2 [] uvs = mesh.uv;
		Vector3 [] verts  = mesh.vertices;
		for (int i = 0; i < tris.Length; i += 3){
			Vector2 u1 = uvs[tris[i]]; // get the triangle UVs
			Vector2 u2 = uvs[tris[i+1]];
			Vector2 u3 = uvs[tris[i+2]];
		
			float a = Area(u1, u2, u3); if (a == 0) continue;
			float a1 = Area(u2, u3, uv)/a; if (a1 < 0) continue;
			float a2 = Area(u3, u1, uv)/a; if (a2 < 0) continue;
			float a3 = Area(u1, u2, uv)/a; if (a3 < 0) continue;

			Vector3 p3D = a1*verts[tris[i]]+a2*verts[tris[i+1]]+a3*verts[tris[i+2]];
			return transform.TransformPoint(p3D);
		}
		return Vector3.zero;
	}

	// calculate signed triangle area using a kind of "2D cross product":
	float Area(Vector2 p1, Vector2 p2, Vector2 p3) {
		Vector2 v1 = p1 - p3;
		Vector2 v2 = p2 - p3;
		return (v1.x * v2.y - v1.y * v2.x)/2;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

class SummedVector2 {
	
	public double x;
	public double y;
	public long length;

	public SummedVector2 (Vector2 initialSum) {
		this.x = initialSum.x;
		this.y = initialSum.y;
		this.length = 1;
	}

	public void AddPoint (Vector2 point) {
		this.length += 1;
		this.x += point.x;
		this.y += point.y;
	}

	public Vector2 getAvgPoint () {
		return new Vector2 ((float)(x / length), (float)(y / length));
	}
}
