using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ToastManager : MonoBehaviour {

	public Text toastText;
	public GameObject toast;

	private bool isDisplaying;

	private float startTime;
	private float duration;

	private bool showForever;

	void Update () {
		if (!showForever && (isDisplaying && (Time.time - startTime) > duration)) {
			isDisplaying = false;
			toast.SetActive (false);
		}
	}

	public void DisplayToast (string text, float duration) {
		this.duration = duration;
		this.startTime = Time.time;

		if (duration < 0) {
			showForever = true;
		} else {
			showForever = false;
		}

		isDisplaying = true;
		toast.SetActive (true);
		toastText.text = text;
	}
}
