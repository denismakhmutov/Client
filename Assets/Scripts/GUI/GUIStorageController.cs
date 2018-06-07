using UnityEngine;
using UnityEngine.UI;

public class GUIStorageController : MonoBehaviour {

	public GameObject GUIStorage;//ссылка на основу гуишки
	public CharactControl charactControlScript;//Ссылка на контроллер персонажа
	public Transform charTransf;

	CharacterDataScript characterDataScript;//ссылка на данные персонажа
	public ChankLoad chankLoadscript;//ссылка на чанклоадер


	// НОМЕРА 6 И 7 Это Деньги и Кредиты
	public InputField[] inFieldCharStor = new InputField[8];//ссылки на инпуты Песронажа
	public InputField[] inFieldStor = new InputField[8];//ссылки на инпуты склада

	ulong[] summResources = new ulong[8];//сколько ресов в сумме на боте и на складе(нужно для обновления данных в инпутах если сумма изменится)
	ulong[] cashResources = new ulong[8];//сколько ресов на стороне бота взято(нужно для того, чтобы инпуты не обновлялись до изменения данных на сервере)

	public Slider[] slider = new Slider[8];//ссылки на слайдеры склада

	bool frstFrame = true;

	string[] strResIDs = new string[] {
		"G","B","R","W","V","C",
		"$","CR",
	};

	// Use this for initialization
	void Start () {
		characterDataScript = gameObject.GetComponent<CharacterDataScript>();
	}

	int chunkX;//кооры бота в чанках
	int chunkY;
	int buildIndex = -1;
	// Update is called once per frame
	void Update () {
		if (GUIStorage.activeSelf) {
			KeyControls();

			chunkX = (int)charTransf.position.x / 32;//кооры бота в чанках
			chunkY = (int)charTransf.position.y / 32;
			buildIndex = -1;
			//Обновление индекса здания(чтобы не случилось слета при установке паков)
			for (int i = 0; i < chankLoadscript.chankBuildingsDataArray[chunkX, chunkY].Count; i++)
			{
				//поиск по координатам
				if (chankLoadscript.chankBuildingsDataArray[chunkX, chunkY][i].x == (int)charTransf.position.x && chankLoadscript.chankBuildingsDataArray[chunkX, chunkY][i].y == (int)charTransf.position.y)
				{
					buildIndex = i;
				}
			}

			//установка положения ползунка в зависимости от содержимого(далее содержимое зависит от ползунка)
			if (frstFrame)
			{
				//Содержимое персонажа
				for (int i = 0; i < 6; i++)
				{
					//заполнение массива максимумов, для будущих проверок на изменение баланса
					summResources[i] = characterDataScript.charactCrystalls[i] + chankLoadscript.chankBuildingsDataArray[chunkX, chunkY][buildIndex].paramIArray[i];
					cashResources[i] = characterDataScript.charactCrystalls[i];

					inFieldCharStor[i].text = "" + characterDataScript.charactCrystalls[i];
				}
				summResources[6] = characterDataScript.moneyValue + chankLoadscript.chankBuildingsDataArray[chunkX, chunkY][buildIndex].paramLArray[0];
				cashResources[6] = characterDataScript.moneyValue;

				summResources[7] = characterDataScript.creditsValue + chankLoadscript.chankBuildingsDataArray[chunkX, chunkY][buildIndex].paramLArray[1];
				cashResources[7] = characterDataScript.creditsValue;

				//содержимое склада
				for (int i = 0; i < 6; i++)
				{
					inFieldStor[i].text = "" + chankLoadscript.chankBuildingsDataArray[chunkX, chunkY][buildIndex].paramIArray[i];
				}
				inFieldStor[6].text = "" + chankLoadscript.chankBuildingsDataArray[chunkX, chunkY][buildIndex].paramLArray[0];
				inFieldStor[7].text = "" + chankLoadscript.chankBuildingsDataArray[chunkX, chunkY][buildIndex].paramLArray[1];

				inFieldCharStor[6].text = "" + characterDataScript.moneyValue;
				inFieldCharStor[7].text = "" + characterDataScript.creditsValue;
			}
			else {
				//Обновление содержимого персонажа если сумма кристаллов была изменена
				//Если был изменен максимум, идет обновление фиелдов со сбросом изменений
				//если было изменено содержимое фиелда персонажа, идут отправлка сообщения на сервер с запросом на изменение
				for (int i = 0; i < 6; i++)
				{
					//если сумма кристаллов была изменена
					if (summResources[i] != (characterDataScript.charactCrystalls[i] + chankLoadscript.chankBuildingsDataArray[chunkX, chunkY][buildIndex].paramIArray[i])) {
						inFieldCharStor[i].text = "" + characterDataScript.charactCrystalls[i];
						inFieldStor[i].text = "" + chankLoadscript.chankBuildingsDataArray[chunkX, chunkY][buildIndex].paramIArray[i];
						summResources[i] = (characterDataScript.charactCrystalls[i] + chankLoadscript.chankBuildingsDataArray[chunkX, chunkY][buildIndex].paramIArray[i]);
						cashResources[i] = characterDataScript.charactCrystalls[i];
						Debug.Log("Change Sum Crystalls [" + i + "]");
					}
				}
				if (summResources[6] != characterDataScript.moneyValue + chankLoadscript.chankBuildingsDataArray[chunkX, chunkY][buildIndex].paramLArray[0]) {
					inFieldCharStor[6].text = "" + characterDataScript.moneyValue;
					inFieldStor[6].text = "" + chankLoadscript.chankBuildingsDataArray[chunkX, chunkY][buildIndex].paramLArray[0];
					summResources[6] = characterDataScript.moneyValue + chankLoadscript.chankBuildingsDataArray[chunkX, chunkY][buildIndex].paramLArray[0];
					cashResources[6] = characterDataScript.moneyValue;
					Debug.Log("Change Sum Money");
				}
				if (summResources[7] != characterDataScript.creditsValue + chankLoadscript.chankBuildingsDataArray[chunkX, chunkY][buildIndex].paramLArray[1])
				{
					inFieldCharStor[7].text = "" + characterDataScript.creditsValue;
					inFieldStor[7].text = "" + chankLoadscript.chankBuildingsDataArray[chunkX, chunkY][buildIndex].paramLArray[1];
					summResources[7] = characterDataScript.creditsValue + chankLoadscript.chankBuildingsDataArray[chunkX, chunkY][buildIndex].paramLArray[1];
					cashResources[7] = characterDataScript.creditsValue;
					Debug.Log("Change Sum Credits");
				}

				//отправка на сервер запроса на изменение, если таково есть
				if (Time.frameCount % 15 == 0) {
					for (int i = 0; i < 8; i++)
					{
						ulong cashRes = 0;
						if (inFieldCharStor[i].text.Length > 0) {
							cashRes = ulong.Parse(inFieldCharStor[i].text);
						}
						if (cashResources[i] != cashRes) {
							ChangeValuesOnServer(chankLoadscript.chankBuildingsDataArray[chunkX, chunkY][buildIndex].buildingID, strResIDs[i], cashRes);
						}
					}
				}
			}
			frstFrame = false;
		}
	}

	//событие изменения поля кристаллов(сторона персонажа)
	public void CharactFieldChanged(int ID) {
		if (!frstFrame)
		{
			//кол-во кристаллов на стороне персонажа
			uint cryVal = 0;
			//сумма кри персонажа и склада
			uint cryMaxVal = characterDataScript.charactCrystalls[ID] + chankLoadscript.chankBuildingsDataArray[chunkX, chunkY][buildIndex].paramIArray[ID];

			if (inFieldCharStor[ID].text.Length > 0)
			{
				cryVal = uint.Parse(inFieldCharStor[ID].text);//кол-во кристаллов,выбранное персонажем
			}

			if (cryVal <= cryMaxVal)
			{
				inFieldStor[ID].text = "" + (cryMaxVal - cryVal);
				//Debug.Log(inFieldStor[ID].text);
			}
			else {
				inFieldStor[ID].text = "" + 0;
				inFieldCharStor[ID].text = "" + cryMaxVal;
				//Debug.Log(inFieldStor[ID].text);
			}
		}
	}

	//событие изменения поля денег(0 - деньги, 1-креды)(сторона персонажа)
	public void CharactMoneyFieldChanged(int ID)
	{
		if (!frstFrame)
		{
			//кол-во денег(кредов или долларов)на стороне персонажа
			ulong credMoneyVal = 0;
			//сумма денег персонажа и склада
			ulong credMoneyMaxVal;

			if (ID == 0){
				credMoneyMaxVal = characterDataScript.moneyValue + chankLoadscript.chankBuildingsDataArray[chunkX, chunkY][buildIndex].paramLArray[ID];
			}
			else {
				credMoneyMaxVal = characterDataScript.creditsValue + chankLoadscript.chankBuildingsDataArray[chunkX, chunkY][buildIndex].paramLArray[ID];
			}

			if (inFieldCharStor[ID + 6].text.Length > 0)// + 6 т.к. первые 6 это кристаллы(логично хм)
			{
				if (ID == 0)
				{
					credMoneyVal = ulong.Parse(inFieldCharStor[ID + 6].text);//кол-во денег,выбранное персонажем
				}
				else
				{
					credMoneyVal = ulong.Parse(inFieldCharStor[ID + 6].text);//кол-во крудов,выбранное персонажем
				}
				
			}

			if (credMoneyVal <= credMoneyMaxVal)
			{
				inFieldStor[ID + 6].text = "" + (credMoneyMaxVal - credMoneyVal);
				//Debug.Log(inFieldStor[ID].text);
			}
			else
			{
				inFieldStor[ID + 6].text = "" + 0;
				inFieldCharStor[ID + 6].text = "" + credMoneyMaxVal;
				//Debug.Log(inFieldStor[ID].text);
			}
		}
	}

	//событие изменения поля кристаллов(сторона склада)
	public void StorageFieldChanged(int ID)
	{
		//кол-во кристаллов на стороне склада
		uint cryVal = 0;
		//сумма кри персонажа и склада
		uint cryMaxVal = characterDataScript.charactCrystalls[ID] + chankLoadscript.chankBuildingsDataArray[chunkX, chunkY][buildIndex].paramIArray[ID];

		if (inFieldStor[ID].text.Length > 0)
		{
			cryVal = uint.Parse(inFieldStor[ID].text);//кол-во кристаллов,выбранное персонажем на стороне склада
		}

		if (cryVal <= cryMaxVal)
		{
			inFieldCharStor[ID].text = "" + (cryMaxVal - cryVal);
			//Debug.Log(inFieldStor[ID].text);
		}
		else
		{
			inFieldCharStor[ID].text = "" + 0;
			inFieldStor[ID].text = "" + cryMaxVal;
			//Debug.Log(inFieldStor[ID].text);
		}
	}

	//событие изменения поля денег(0 - деньги, 1-креды)(сторона склада)
	public void StorageMoneyFieldChanged(int ID)
	{
		if (!frstFrame)
		{
			//кол-во денег(кредов или долларов)на стороне склада
			ulong credMoneyVal = 0;
			//сумма денег персонажа и склада
			ulong credMoneyMaxVal;

			if (ID == 0)
			{
				credMoneyMaxVal = characterDataScript.moneyValue + chankLoadscript.chankBuildingsDataArray[chunkX, chunkY][buildIndex].paramLArray[ID];
			}
			else
			{
				credMoneyMaxVal = characterDataScript.creditsValue + chankLoadscript.chankBuildingsDataArray[chunkX, chunkY][buildIndex].paramLArray[ID];
			}

			if (inFieldStor[ID + 6].text.Length > 0)// + 6 т.к. первые 6 это кристаллы(логично хм)
			{
				if (ID == 0)
				{
					credMoneyVal = ulong.Parse(inFieldStor[ID + 6].text);//кол-во денег,выбранное персонажем на складе
				}
				else
				{
					credMoneyVal = ulong.Parse(inFieldStor[ID + 6].text);//кол-во крудов,выбранное персонажем на складе
				}

			}

			if (credMoneyVal <= credMoneyMaxVal)
			{
				inFieldCharStor[ID + 6].text = "" + (credMoneyMaxVal - credMoneyVal);
				//Debug.Log(inFieldStor[ID].text);
			}
			else
			{
				inFieldCharStor[ID + 6].text = "" + 0;
				inFieldStor[ID + 6].text = "" + credMoneyMaxVal;
				//Debug.Log(inFieldStor[ID].text);
			}
		}
	}

	//Отдать всё
	public void GiveAll(int ID)
	{
		ulong charVal = 0;
		if (inFieldCharStor[ID].text.Length > 0)
		{
			charVal = ulong.Parse(inFieldCharStor[ID].text);
		}
		ulong storVal = 0;
		if (inFieldStor[ID].text.Length > 0)
		{
			storVal = ulong.Parse(inFieldStor[ID].text);
		}

		inFieldStor[ID].text = "" + (charVal + storVal);
	}

	//Отдать половину
	public void GiveHalf(int ID)
	{
		ulong charVal = 0;
		if (inFieldCharStor[ID].text.Length > 0) {
			charVal = ulong.Parse(inFieldCharStor[ID].text);
		}
		ulong storVal = 0;
		if (inFieldStor[ID].text.Length > 0)
		{
			storVal = ulong.Parse(inFieldStor[ID].text);
		}
		ulong maxVal = charVal + storVal;

		ulong halfVal = charVal / 2;


		inFieldCharStor[ID].text = "" + halfVal;

		inFieldStor[ID].text = "" + (maxVal - halfVal);
	}

	//Получить всё
	public void GetAll(int ID)
	{
		ulong charVal = 0;
		if (inFieldCharStor[ID].text.Length > 0)
		{
			charVal = ulong.Parse(inFieldCharStor[ID].text);
		}
		ulong storVal = 0;
		if (inFieldStor[ID].text.Length > 0)
		{
			storVal = ulong.Parse(inFieldStor[ID].text);
		}

		inFieldCharStor[ID].text = "" + (charVal + storVal);
	}

	//Получить половину
	public void GetHalf(int ID)
	{
		ulong charVal = 0;
		if (inFieldCharStor[ID].text.Length > 0)
		{
			charVal = ulong.Parse(inFieldCharStor[ID].text);
		}

		ulong storVal = 0;
		if (inFieldStor[ID].text.Length > 0)
		{
			storVal = ulong.Parse(inFieldStor[ID].text);
		}

		ulong maxVal = charVal + storVal;
		ulong halfVal = storVal / 2;

		inFieldStor[ID].text = "" + halfVal;
		inFieldCharStor[ID].text = "" + (maxVal - halfVal);
	}
	
	//Взять ровно под груз
	public void ToСargo(int ID)
	{

	}

	public void SliderCristallsChanged(int ID)
	{

	}

	public void SliderMoneyChanged(int ID)
	{

	}

	void KeyControls() {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			GUISetActive(false);
		}
	}

	//запрос на изменение числа ресурсов IDResurs персонажа на число CharactValue
	//после чего сервер определит валидность данной операции и выполнит при положительном результате
	//работает только на ресурсы. на геологию и предметы другая функция
	//айдишники:G,B,R,W,V,C,CR,$
	void ChangeValuesOnServer(int IDStorage,string IDResurs,ulong CharactValue) {
		Debug.Log("ChangeValuesOnServer: IDB: "+ IDStorage + " IDR " + IDResurs + " value= " + CharactValue);
	}

	//называется сетактивом, но использовать можно только для выхода из здания
	public void GUISetActive(bool activeMode)
	{
		charactControlScript.QuitBuilding("D1");
		GUIStorage.SetActive(activeMode);
		frstFrame = true;
	}
}
