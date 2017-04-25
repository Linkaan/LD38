using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class WinManager : MonoBehaviour {

	public GameObject winObject;
	public GameObject toastObject;
	public Image winColour;

	public void SetWinner (Player player) {
		toastObject.SetActive (false);
		winObject.SetActive (true);
		winColour.color = player.playerColour;
	}
}
