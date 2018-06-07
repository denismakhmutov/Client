using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIRespawnController : MonoBehaviour {

	public GameObject GUIRespawn;//ссылка на основу гуишки
	public CharactControl charactControlScript;//Ссылка на контроллер персонажа

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (GUIRespawn.activeSelf) {
			KeyControls();
		}
	}

	public void GUISetActive(bool activeMode)
	{
		GUIRespawn.SetActive(activeMode);
		charactControlScript.QuitBuilding("R2");
	}

	//контрольные кнопки этого интерфейса
	void KeyControls()
	{
		if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Escape))
		{
			GUISetActive(false);
		}
		else if(Input.GetKeyDown(KeyCode.Return))
		{
			Debug.Log("EnterActive");
		}

	}
}
