using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIMarketController : MonoBehaviour {

	enum MarkMode {
		Sale,//продажа игроком
		Purc//покупка игроком
	};

	public GameObject GUIMarket;//Ссылка на основу гуихи

	public CharactControl charactControlScript;//Ссылка на контроллер персонажа
	CharacterDataScript characterDataScript;//ссылка на данные персонажа

	public Text[] cristallsOnMarketValue = new Text[6];//кол-во кристаллов в маркете или в инвентаре
	public Text[] cristallsPriceForOne = new Text[6];//цена кристаллов за штуку
	public InputField[] inputField = new InputField[6];
	public Slider[] slider = new Slider[6];
	public Text marketModeText;
	public Text moneyCalculation;//ссылка на поле с расчетом общей суммы и комиссии

	public Text[] textMarcketSellBuy = new Text[6];//ссылки на текст в кнопках (продать/купить)

	int[] arrayCristallsPurchasePriceForOne = new int[6];//цена покупки кри(персонажем)
	int[] arrayCristallsSellingPriceForOne = new int[6];//цена продажи(персонажем)
	int[] arrayCristallsInMarket = new int[6];//кристаллов в маркете

	bool itsFieldEdit = false;

	MarkMode marketMode = MarkMode.Sale;//режим маркета (относительно персонажа(продажа-игроком,покупка-игроком))
	// Use this for initialization
	void Start () {
		characterDataScript = gameObject.GetComponent<CharacterDataScript>();
		for (int i = 0; i < 6; i++)
		{
			arrayCristallsInMarket[i] = Random.Range(50000, 2500000);
			arrayCristallsPurchasePriceForOne[i] = Random.Range(50,250);
			arrayCristallsSellingPriceForOne[i] = Random.Range(2, arrayCristallsPurchasePriceForOne[i] / 2);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (GUIMarket.activeSelf) {
			MarketUpdate();
			KeyControls();
		}
		
	}

	//Эта ФУНКЦИЯ ДОЛЖНА БРАТЬ ИНФУ О МАРКЕТЕ ИЗ СЕРВЕРА(цены на все кристаллы, кол-во кристаллов в маркете, комиссия)
	void GetServerDataAboutMarket() {

	}

	void MarketUpdate() {
		if (marketMode == MarkMode.Sale)//Продажа кри
		{
			marketModeText.text = "Режим:<color=#00FF00>Продажа</color>";

			for (int i = 0; i < 6; i++)
			{
				slider[i].maxValue = characterDataScript.charactCrystalls[i];

				cristallsOnMarketValue[i].text = "" + characterDataScript.charactCrystalls[i];

				cristallsPriceForOne[i].text = "" + arrayCristallsSellingPriceForOne[i];

				if (!inputField[i].isFocused)
				{
					inputField[i].text = "" + slider[i].value;
				}
			}
		}
		else //покупка кри
		{
			marketModeText.text = "Режим:<color=#FF1100>Покупка</color>";

			for (int i = 0; i < 6; i++)
			{
				slider[i].maxValue = arrayCristallsInMarket[i];

				cristallsOnMarketValue[i].text = "" + arrayCristallsInMarket[i];//Кристаллов в маркете

				cristallsPriceForOne[i].text = "" + arrayCristallsPurchasePriceForOne[i];//цена кристаллов при покупке

				if (!inputField[i].isFocused)
				{
					inputField[i].text = "" + slider[i].value;
				}
			}
		}
	}

	//событие изменения значения в слайдере
	public void SliderValueChanged(int ID) {

	}

	//событие изменения числа в поле ввода
	public void FieldEdit(int ID)
	{
		if (inputField[ID].text.Length > 0)
		{

			int valueIF = int.Parse(inputField[ID].text);//получение числа из инпутфиелда

			if (marketMode == MarkMode.Sale)
			{//При продаже в маркет
				if (valueIF <= characterDataScript.charactCrystalls[ID])
				{
					slider[ID].value = valueIF;
				}
				else
				{
					inputField[ID].text = "" + characterDataScript.charactCrystalls[ID];
					slider[ID].value = slider[ID].maxValue;
				}
			}
			else
			{//при покупке с маркета
				if (valueIF <= arrayCristallsInMarket[ID])
				{
					slider[ID].value = valueIF;
				}
				else
				{
					inputField[ID].text = "" + arrayCristallsInMarket[ID];
					slider[ID].value = slider[ID].maxValue;
				}
			}

		}
	}

	//событие нажатия на кнопку продажи/покупки кристаллов
	public void Sale(int ID)
	{
		if (marketMode == MarkMode.Sale) {
			//Продать
		}
		else {
			//купить
		}
	}

	//Продать все кристаллы в инвентаре
	public void SaleALL()
	{

	}

	public void MarketMode()
	{
		for (int i = 0; i < 6; i++)
		{
			textMarcketSellBuy[i].text = (marketMode == MarkMode.Sale) ? ("Купить") : ("Продать");

			inputField[i].text = "" + 0;
			slider[i].value = 0;
		}
		marketMode = (marketMode == MarkMode.Sale) ? (MarkMode.Purc) : (MarkMode.Sale);
	}

	public void GUISetActive(bool activeMode)
	{
		GUIMarket.SetActive(activeMode);
		charactControlScript.QuitBuilding("D3");
		for (int i = 0; i < 6; i++)
		{
			inputField[i].text = "" + 0;
			slider[i].value = 0;
		}
	}

	//контрольные кнопки этого интерфейса
	void KeyControls()
	{
		if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Escape))
		{
			GUISetActive(false);
		}
	}
}
