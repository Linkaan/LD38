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

		player.map.localPlayer.toastManager.GetComponent<RectTransform> ().localPosition = new Vector3 (0, -55, 0);
		player.map.localPlayer.toastManager.DisplayToastDelayed ("Press the button in the bottom left corner", -1, 2);
	}
}
