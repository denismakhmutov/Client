using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterDataScript : MonoBehaviour {

	public Transform charactTransf;
	[Space]
	public Text textMoney;
	public Text textCredits;
	[Space]
	public Text textGreen;
	public Text textBlue;
	public Text textRed;
	public Text textWhite;
	public Text textViolet;
	public Text textCyan;
	[Space]
	public Text textCoordinates;
	[Space]
	public Text CurrentNickname;
	[Space]
	public Text textRobotsInMines;
	[Space]
	public Image imageHPStatus;
	public Image imageCargoStatus;

	public ulong moneyValue;
	public ulong creditsValue;

	public int characterMaxHP;
	public int characterCurrentHP;

	int i = 0;

	public uint[] charactCrystalls;

	// Use this for initialization
	void Start () {
		characterMaxHP = Random.Range(15,100);
		characterCurrentHP = Random.Range(25, characterMaxHP + 1);
		charactCrystalls = new uint[6];



		moneyValue = 3500;
		creditsValue = 2;

		CurrentNickname.text = "<color=#00ffffff> RoboMiner_01 </color>" ;
	}

	// Update is called once per frame
	void Update () {

	}

	void LateUpdate() {
		float fill = (float)characterCurrentHP / (float)characterMaxHP;
		imageHPStatus.fillAmount = fill;
		imageHPStatus.color = new Color((1f - fill) * 0.65f, fill * 0.65f,0);


		textMoney.text = "" + moneyValue;
		textCredits.text = "" + creditsValue;


		textCoordinates.text = (int)charactTransf.position.x + ":" + (int)charactTransf.position.y;

		textGreen.text = charactCrystalls[0].ToString();
		textBlue.text = charactCrystalls[1].ToString();
		textRed.text = charactCrystalls[2].ToString();
		textWhite.text = charactCrystalls[3].ToString();
		textViolet.text = charactCrystalls[4].ToString();
		textCyan.text = charactCrystalls[5].ToString();
	}
}