using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class FollowMouse : MonoBehaviour {

	public Canvas canvas;
	public Image image;

	public Sprite pointing;
	public Sprite dragging;
	public Sprite attack;
	public Sprite move;
	public Sprite attack50;

	void Start () {
		//Cursor.visible = false;
		Update ();
	}

	void Update () {
		Vector2 pos;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, canvas.worldCamera, out pos);
		Vector3 position = canvas.transform.TransformPoint (pos);
		position.x -= 25;
		position.y -= 25;
		transform.position = position;
	}

	public void showCursorPointing () {
		image.overrideSprite = pointing;
	}

	public void showCursorDragging () {
		image.overrideSprite = dragging;
	}

	public void showCursorAttack () {
		image.overrideSprite = attack;
	}

	public void showCursorMove () {
		image.overrideSprite = move;
	}

	public void showCursorAttack50 () {
		image.overrideSprite = attack50;
	}
}
