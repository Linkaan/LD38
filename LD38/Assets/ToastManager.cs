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

	private float delay;
	private bool showDelayed;

	void Update () {
		if (!showForever && (isDisplaying && (Time.time - startTime) > duration)) {
			isDisplaying = false;
			toast.SetActive (false);
		}

		if (showDelayed && (Time.time - startTime) > delay) {
			showDelayed = false;
			startTime = Time.time;
			isDisplaying = true;
			toast.SetActive (true);
		}
	}

	public void DisplayToast (string text, float duration) {
		this.showDelayed = false;
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

	public void DisplayToastDelayed (string text, float duration, float delay) {
		this.delay = delay;
		this.showDelayed = true;
		this.startTime = Time.time;

		if (duration < 0) {
			showForever = true;
		} else {
			showForever = false;
		}

		toastText.text = text;
	}
}
