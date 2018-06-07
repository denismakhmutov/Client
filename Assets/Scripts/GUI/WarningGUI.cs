using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarningGUI : MonoBehaviour {

	public Text warningText;
	public Text content;
	public Button buttonOK;
	public Button buttonUndo;

	public bool isActive = false;
	public bool buttonOKPressed;
	public bool buttonUndoPressed;


	//// Use this for initialization
	//void Start () {
		
	//}
	
	// Update is called once per frame
	void Update () {
		if (gameObject.activeSelf) {
			
		}
	}

	public void ButtonOKPressed() {
		buttonOKPressed = true;
	}

	public void ButtonUndoPressed(){
		buttonUndoPressed = true;
	}

	public void GUISetActive(bool activeMode)
	{
		isActive = activeMode;
		gameObject.SetActive(activeMode);

		buttonOKPressed = false;
		buttonUndoPressed = false;
	}
}
