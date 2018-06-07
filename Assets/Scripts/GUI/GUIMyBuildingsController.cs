using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Models;

public class GUIMyBuildingsController : MonoBehaviour {

	enum BuildFilterMode : int {
		OllBuild,
		Guns,
		ProfitBuilds,
		Coords
	}

	GUIItemsController scriptGUIItemsController;//Ссылка на скрипт с массивом спрайтов

	public GameObject panelGUIMyBuildings;//ссылка на объект GUI
	[Space]
	public GameObject[] panelMyBuildingInfo = new GameObject[10];//ссылка на объект-основу панельки информации о здании
	[Space]
	public Image[] imageMyBuilding = new Image[10];//изображение иконки со зданием
	[Space]
	public Sprite[] spriteCrystalls = new Sprite[6];//спрайты кристаллов
	[Space]
	public Image[] imageMyBuildingReserveType = new Image[10];//изображение типа кри , используемого паком
	[Space]
	public Text[] textMyBuildingCoord = new Text[10];//координата здания
	[Space]
	public Text[] textMyBuildingReserveStatus = new Text[10];//количество кри/максимум кри
	[Space]
	public Text[] textMyBuildingProfit = new Text[10];//текст с прибылью от пака
	[Space]
	public Button[] buttonTakeProfits = new Button[10];//кнопка ЗАБРАТЬ ПРИБЫЛЬ
	[Space]
	public GameObject[] myBuildingReserveStatusBar = new GameObject[10];//Статусбар (нужно только для возможности скрыть статус бар с панели)
	[Space]
	public Image[] myBuildingReserveStatusLine = new Image[10];//полоска статус-бара
	[Space]
	public Scrollbar scrollbarMyBuildings;//ссылка на скролбар
	[Space]
	public GameObject[] canvasMyBuildingProfit = new GameObject[10];//ссылки на канвасы для возможности выключать панельки с прибылью зданий

	List<ConstructData>[] myBuildingsList;//массив списков структур зданий, поставленных игроком

	int filterMode = (int)BuildFilterMode.OllBuild;//режим фильтра
	int buildingsCount;//кол-во установленных зданий

	// Use this for initialization
	void Start() {

		scriptGUIItemsController = gameObject.GetComponent<GUIItemsController>();

		myBuildingsList = new List<ConstructData>[scriptGUIItemsController.packsTypesCount];

		for (byte i = 0; i < scriptGUIItemsController.packsTypesCount; i++)//симуляция внесения списка из сервера
		{
			//Нулевая и Первая ячейки массива структуры это координаты здания
			//Вторая - ёмкость кристаллов
			//Третья - Запас на данный момент
			//Прибыль здания
			//Пятая - Есть ли модификатор у здания(на пример,пухи)

			myBuildingsList[i] = new List<ConstructData>();

			if(i != 24 && i != 25 && i != 20)
			for (byte j = 0; j < Random.Range(0, 2); j++)
			{
				myBuildingsList[i].Add(new ConstructData());
					myBuildingsList[i][j].paramArray = new short[6] { (short)Random.Range(0,2048), (short)Random.Range(0, 2048), 1000, (short)Random.Range(20, 1000),0,0 };
			}
		}
	}

	// Update is called once per frame
	void Update() {

		if (panelGUIMyBuildings.activeSelf) {
			KeyControls();
		}
	}

	public void UpdateGUI() {

		
		buildingsCount = 0;
		for (int i = 0; i < scriptGUIItemsController.packsTypesCount; i++) {
			buildingsCount += myBuildingsList[i].Count;
		}
		//Debug.Log("buildingsCount: " + buildingsCount);

		int startPosShowMyBuilds;//с какого пака начинать отображение
		if (buildingsCount <= 10)
		{
			startPosShowMyBuilds = 0;

			for (int i = 0; i < 10; i++)
			{
				if (i < buildingsCount)
				{
					panelMyBuildingInfo[i].SetActive(true);
				}
				else {
					panelMyBuildingInfo[i].SetActive(false);
				}
			}
		}
		else
		{
			startPosShowMyBuilds = (int)((float)(buildingsCount - 10) * scrollbarMyBuildings.value);
			//Debug.Log("startPosShowMyBuilds: " + startPosShowMyBuilds);
		}


		int iterator = 0;//номер текущей ячейки с данными про здание

		if (filterMode == (int)BuildFilterMode.OllBuild)
		{

			for (int i = 0; i < scriptGUIItemsController.packsTypesCount; i++)
			{
				//Debug.Log(myBuildingsList[i].Count);
				for (int j = 0; j < myBuildingsList[i].Count; j++)
				{
					if (iterator < 10)
					{
						if (startPosShowMyBuilds <= 0)
						{
							imageMyBuilding[iterator].sprite = scriptGUIItemsController.spriteItems[i];
							textMyBuildingCoord[iterator].text = (int)myBuildingsList[i][j].paramArray[0] + ":" + (int)myBuildingsList[i][j].paramArray[1];
							switch (i)
							{
								case 0:
									imageMyBuildingReserveType[iterator].sprite = spriteCrystalls[2];
									imageMyBuildingReserveType[iterator].color = new Color(1, 1, 1, 1);
									textMyBuildingReserveStatus[iterator].text = (int)myBuildingsList[i][j].paramArray[3] + "/" + (int)myBuildingsList[i][j].paramArray[2];
									textMyBuildingReserveStatus[iterator].color = new Color(1, 1, 1, 1);
									myBuildingReserveStatusLine[iterator].fillAmount = (float)myBuildingsList[i][j].paramArray[3] / (float)myBuildingsList[i][j].paramArray[2];
									myBuildingReserveStatusBar[iterator].SetActive(true);
									break;
								case 1:
									imageMyBuildingReserveType[iterator].sprite = spriteCrystalls[1];
									imageMyBuildingReserveType[iterator].color = new Color(1, 1, 1, 1);
									textMyBuildingReserveStatus[iterator].text = (int)myBuildingsList[i][j].paramArray[3] + "/" + (int)myBuildingsList[i][j].paramArray[2];
									textMyBuildingReserveStatus[iterator].color = new Color(1, 1, 1, 1);
									myBuildingReserveStatusLine[iterator].fillAmount = (float)myBuildingsList[i][j].paramArray[3] / (float)myBuildingsList[i][j].paramArray[2];
									myBuildingReserveStatusBar[iterator].SetActive(true);
									break;
								case 5:
									imageMyBuildingReserveType[iterator].sprite = spriteCrystalls[5];
									imageMyBuildingReserveType[iterator].color = new Color(1, 1, 1, 1);
									textMyBuildingReserveStatus[iterator].text = (int)myBuildingsList[i][j].paramArray[3] + "/" + (int)myBuildingsList[i][j].paramArray[2];
									textMyBuildingReserveStatus[iterator].color = new Color(1, 1, 1, 1);
									myBuildingReserveStatusLine[iterator].fillAmount = (float)myBuildingsList[i][j].paramArray[3] / (float)myBuildingsList[i][j].paramArray[2];
									myBuildingReserveStatusBar[iterator].SetActive(true);
									break;
								case 6:
									imageMyBuildingReserveType[iterator].sprite = spriteCrystalls[4];
									imageMyBuildingReserveType[iterator].color = new Color(1, 1, 1, 1);
									textMyBuildingReserveStatus[iterator].text = (int)myBuildingsList[i][j].paramArray[3] + "/" + (int)myBuildingsList[i][j].paramArray[2];
									textMyBuildingReserveStatus[iterator].color = new Color(1, 1, 1, 1);
									myBuildingReserveStatusLine[iterator].fillAmount = (float)myBuildingsList[i][j].paramArray[3] / (float)myBuildingsList[i][j].paramArray[2];
									myBuildingReserveStatusBar[iterator].SetActive(true);
									break;
								default:
									imageMyBuildingReserveType[iterator].color = new Color(1, 1, 1, 0);
									textMyBuildingReserveStatus[iterator].color = new Color(1, 1, 1, 0);
									myBuildingReserveStatusBar[iterator].SetActive(false);
									break;
							}

							if (i >= 0 && i <= 3 || i >= 6 && i <= 7)
							{
								canvasMyBuildingProfit[iterator].SetActive(true);
							}
							else
							{
								canvasMyBuildingProfit[iterator].SetActive(false);
							}

							iterator++;
						}
						else {
							startPosShowMyBuilds--;
						}
					}

					else
					{
						break;
					}
				}
			}
		}
	}

	public void ButtonTakeProfits (int buttonID){

	}

	public void ButtonGUIMyBuildings(bool openOrQuit)
	{
		panelGUIMyBuildings.SetActive(openOrQuit);
		if (!openOrQuit) {
			scrollbarMyBuildings.value = 0f;
		}
	}

	//контрольные кнопки этого интерфейса
	void KeyControls()
	{
		if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Escape))
		{
			ButtonGUIMyBuildings(false);
		}
	}

}
