using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class BackToMainMenu : MonoBehaviour {

	public void GoBackToMainMenu () {
		SceneManager.LoadScene (0);
	}
}
