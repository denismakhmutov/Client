using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIRepairController : MonoBehaviour {

	public GameObject GUIRepair;//ссылка на основу гуишки
	public CharactControl charactControlScript;//Ссылка на контроллер персонажа

	public Text textRepairPrice;//цена за единицу и общая цена за хил

	CharacterDataScript characterDataScript;//ссылка на скрипт с данными про здоровье персонажа

	int priceForOne = 6;
	// Use this for initialization
	void Start () {
		characterDataScript = gameObject.GetComponent<CharacterDataScript>();
	}
	
	// Update is called once per frame
	void Update () {
		if (GUIRepair.activeSelf) {

			KeyControls();

			textRepairPrice.text = "Цена за ремонт:<color=#0a0>" + (priceForOne * (characterDataScript.characterMaxHP - characterDataScript.characterCurrentHP)) + "</color> (<color=#0a0>" + priceForOne + "$</color>/HP)";
		}
	}

	public void GUISetActive(bool activeMode)
	{
		GUIRepair.SetActive(activeMode);
		charactControlScript.QuitBuilding("R2");
	}

	public void BotHeal()
	{
		characterDataScript.moneyValue -= (ulong)(priceForOne * (characterDataScript.characterMaxHP - characterDataScript.characterCurrentHP));
		characterDataScript.characterCurrentHP = characterDataScript.characterMaxHP;
	}

	//контрольные кнопки этого интерфейса
	void KeyControls()
	{
		if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Escape))
		{
			GUISetActive(false);
		}
		else if (Input.GetKeyDown(KeyCode.Return))
		{
			Debug.Log("EnterActive");
		}
	}
}
