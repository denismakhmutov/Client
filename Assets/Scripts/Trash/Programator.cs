using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Programator : MonoBehaviour {
	#region ссылки на графическую часть программатора
	public GameObject panelProgramator;//Ссылка на панель программатора
	public CharactControl charactControl;//Ссылка на контрол игрока

	public GameObject[] programLines = new GameObject[12];
	public Text[] ProgramExecutionInfo = new Text[9];//Информация о состоянии программного блока

	public Text startStopButtonText;//текст кнопки включения и выключения программы
	public Button debugButton;//кнопка дебага(для отключения или включения в зависимости от чего-то)
	public Slider SliderBackground;//прозрачность задника программатора
	public Scrollbar scrollbar;//скролл для проги

	//Префабы, нужные для построения сетки программатора
	public GameObject LinePrefab;//префаб строки программатора
	public GameObject alFuncBlocksPrefab;//все программные блоки и скрипт для обращения к ним

	AllProgramBlocksController[,] programBlocks;//Массив программных блоков
	GameObject[,] programCells;//Массив программных ячеек
	Text[] lineNum;//Массив номеров ячеек

	public Sprite[] verifSprites = new Sprite[9];//страйты проверок в разные стороны
	public Sprite[] ifSprites = new Sprite[2];//
	public Sprite[] orAndSprites = new Sprite[2];//
	public Sprite jmpSprite;
	public Sprite[] funkSprites = new Sprite[2];//
	public Sprite[] returnSprites = new Sprite[2];//
	public Sprite[] noneNextSprites = new Sprite[2];//
	public Sprite[] moveAndRotateSprites = new Sprite[8];//
	public Sprite[] CristallsSprites = new Sprite[8];//
	public Sprite[] startEndSprites = new Sprite[2];//
	public Sprite[] blockOrNoSprites = new Sprite[2];//
	public Sprite[] actionsSprites = new Sprite[7];//

	public int firstLine = 0;//указывает, с какой строки начинать отображение на экране (зависит от скрола)
	#endregion
	[Space]
	public bool progrActive = false;

	public const int pageX = 16;
	public const int pageY = 64;

	public int tactsPerFrame = 1;//тактов на один кадр
	public int TactsForCD = 0;//такты бездействия. разные функции дают разное количество единиц бездействия

	public Vec2i startCoor = new Vec2i(0, 0);//координаты стартовой метки программы
	public int currStep = 0;//текущий шаг в прогремме
	public int stackVolume = 1024;//Объём стека
	Vec2i[] stack;//типа стек для работы подфункций, чтобы не юзать рекурсию

	public int stackLvl = 0;//на каком уровне в стеке в данный момент нужно считывать значения

	ChankLoad chunkLoad;//ссылка на чанклоадер для работы с картой

	//массив команд программы
	string[,] ProgramComands;
	//Ьфссив адресов и данных (в основном будет использоваться для адресов)
	Vec2i[,] ProgramAdressesAndData;
	/// <summary>
	///если был определен опратор "условия" то режим проверки включается
	///и все операторы сдвига принимаются для перемещения точки слежки
	///любой оператор перемещения робота или другое нарушение цепочки:
	///Проверить-[операторы сдвига]-[условия(если более 1, то надо указать оператор И/ИЛИ)][оператор ветвления]
	///Приведет к сбросу режима проверки и регистр состояния сбросится до false
	///все операторы ветвления и условия будут игнорироваться если не был включен режим проверки
	///</summary>
	public bool verificationMode = false;
	/// <summary>Регистр состояния логического блока. от его значения зависит направление ветвления в программе</summary>
	public bool boolRegister = false;
	public bool result = false;//Переменная, применяемая в логическом блоке для получения текущего результата
							   /// <summary>Координаты, в которых надо брать пробу с мира</summary>
	Vec2i verCoors = new Vec2i(0, 0);
	public Vector2 verCoorsV2;//те же кооры но с выводом в юнити редактор
	//переменная, указывающая, какой логический оператор применять если идет несколько условий подряд.
	//Если идет несколько условий но ни разу не встречается 
	public LogOp logOp = 0;

	// Use this for initialization
	void Start() {
		stack = new Vec2i[stackVolume];
		chunkLoad = GameObject.Find("ChunkLoader").GetComponent<ChankLoad>();
		scrollbar.size = 12 / (float)pageY;

		programBlocks = new AllProgramBlocksController[16, 12];//Массив всех программных блоков на поле программатора(для упрощенного доступа)
		programCells = new GameObject[16,12];//Массив всех программных ячеек на поле программатора(для упрощенного доступа)
		lineNum = new Text[12];//таблица с номерами строк (при скроле переключение номеров)

		for (int i = 0;i < 12; i++) {//прохождение по строкам и создание для них ячеек на основе префаба с ячейками
			GameObject line = Instantiate(LinePrefab, programLines[i].transform);//создание строки ячеек

			GameObject[] cells = line.GetComponent<LineCells>().Cells;//изъятие из скрипта на строке ссылок на его ячейки

			lineNum[i] = line.GetComponent<LineCells>().text;
			lineNum[i].text = "" + i;

			for (int ii = 0; ii < 16; ii++)
			{
				programCells[ii, i] = cells[ii];//добавление в массив ячеек ссылок на ячейки строки

				programBlocks[ii,i] = Instantiate(alFuncBlocksPrefab, cells[ii].transform, false).GetComponent<AllProgramBlocksController>();//составление таблицы программных блоков
				//чтобы каждый из программных блоков кричал где его меняют
				programBlocks[ii, i].x = ii;
				programBlocks[ii, i].y = i;

				cells[ii].GetComponent<EnterSensor>().x = ii;
				cells[ii].GetComponent<EnterSensor>().y = i;
			}
		}
		//раздача ссылки на программатор скрипт чтобы скрипты возвращали события
		EnterSensor.programator = this;
		AllProgramBlocksController.programmatorref = this;

		string[,] program0 = new string[16, 16] {
			{ "S","","","","","","","","","","","","","","","",},//0
			{ "","","","","","","","","","","","","","","","",},
			{ "","","","","","","","","","","","","","","","",},
			{ "","","","","","","","","","","","","","","","",},

			{ "","S","","","","","","","","","","","","","","",},//4
			{ "","","","","","","","","","","","","","","","",},
			{ "","","","","","","","","","","","","","","","",},
			{ "","","","","","","","","","","","","","","","",},

			{ "","","S","","","","","","","","","","","","","",},//8
			{ "","","","","","","","","","","","","","","","",},
			{ "","","","","","","","","","","","","","","","",},
			{ "","","","","","","","","","","","","","","","",},

			{ "","","","S","","","","","","","","","","","","",},//12
			{ "","","","","","","","","","","","","","","","",},
			{ "","","","","","","","","","","","","","","","",},
			{ "","","","","","","","","","","","","","","","end",},
		};

		ProgramComands = new string[pageX, pageY];
		ProgramAdressesAndData = new Vec2i[pageX, pageY];
		for (int y = 0; y < pageY; y++)
			for (int x = 0; x < pageX; x++) {
				ProgramAdressesAndData[x, y] = new Vec2i(0,0);
				ProgramComands[x, y] = "";
			}

		for (int y = 0; y < 16; y++)
			for (int x = 0; x < 16; x++)
			{
				ProgramComands[x, y] = program0[y, x];
			}
		

		//Мой гениальный способ удалять баг с отображенрием меню при первом открытии
		//БЕЗ ЭТОЙ ФИЧИ ВСЕ СЛЕТАЕТ
		panelProgramator.SetActive(true);
		panelProgramator.SetActive(false);
	}

	// Update is called once per frame
	void FixedUpdate() {
		for (int n = 0; n < tactsPerFrame; n++)
			if (progrActive)
			{
				if (TactsForCD == 0)
				{
					ProgramTactUpdate();//выполнение следубщего шага программы
				}
				else {
					--TactsForCD;
				}
			}
			else {
				//сброс всех значений для корректного старта программы
				currStep = 0;
				stackLvl = 0;
				verificationMode = false;
				//logOp = LogOp.NULL;
			}
		if(panelProgramator.activeSelf){
			ProgramExecutionInfoUpdate();//Инфа о процессе работы программатора
		}
	}

	void ProgramTactUpdate() {
		if (currStep == 0)//Если программа только запущена
		{
			for (int i = 0; i < stackVolume; i++)
			{
				stack[i] = new Vec2i(0, 0);
			}
		}

		if (stack[stackLvl].x == (pageX - 1))//если конец строки
		{
			stack[stackLvl].x = startCoor.x;
			stack[stackLvl].y = startCoor.y;
			Debug.Log("stack[stackLvl] = startCoor;" + startCoor.x + " " + startCoor.y);
		}
		else
		{
			++stack[stackLvl].x;
		}

		Debug.Log(stack[stackLvl].x + " " + stack[stackLvl].y);

		switch (ProgramComands[stack[stackLvl].x, stack[stackLvl].y])
		{
			#region примитивные метки
			case ""://
				; break;
			case "S"://стартовая точка
				startCoor.x = stack[stackLvl].x;
				startCoor.y = stack[stackLvl].y;
				; break;
			case "next"://следующая строка
				stack[stackLvl].x = 0;
				++stack[stackLvl].y;
				Debug.Log("End");
				; break;
			case "end"://Конец проги
				Debug.Log("End");
				currStep = 0;
				progrActive = false;
				verificationMode = false;
				; break;
			case "ret"://завершение подпрограммы и возврат на предыдущий уровень стека
				--stackLvl;
				verificationMode = false;
				; break;
			case "+ret"://завершение подпрограммы с возвратом булевого значения и возврат на предыдущий уровень стека

				; break;
			#endregion
			#region перемещение
			case "R":
				MoveRight();
				verificationMode = false;
				; break;
			case "L":
				MoveLeft();
				verificationMode = false;
				; break;
			case "U":
				MoveUp();
				verificationMode = false;
				; break;
			case "D":
				MoveDown();
				verificationMode = false;
				; break;
			case "M"://Двигаться в направлении куда смотрит робот
					 //РЕАЛИЗОВАТЬ КОГДА БУДЕТ РАБОТА УЖЕ С РОБОТОМ А НЕ ТЕСТОВЫМ СТЕНДОМ
				verificationMode = false;
				; break;
			#endregion
			#region Поворот
			case "r":
				RotatRight();
				verificationMode = false;
				; break;
			case "l":
				RotatLeft();
				verificationMode = false;
				; break;
			case "u":
				RotatUp();
				verificationMode = false;
				; break;
			case "d":
				RotatDown();
				verificationMode = false;
				; break;
			#endregion
			#region указатели логических операций
			case "or":
				logOp = LogOp.OR;
				; break;
			case "and":
				logOp = LogOp.AND;
				; break;
			#endregion
			#region команды с параметром
			default:
				string com = ProgramComands[stack[stackLvl].x, stack[stackLvl].y];//ПОМЕНЯТЬ В БУДУЩЕМ ИКС И ИГРЕК МЕСТАМИ. СЕЙЧАС ТАК ДЛЯ ДЕБАГА

				if (Ident("jmp", com))
				{//перемещение на указанный адрес (без переноса значения)
					Debug.Log("jmp");
					stack[stackLvl].x = int.Parse(com.Substring(3, 2)) - 1;
					stack[stackLvl].y = int.Parse(com.Substring(5, 2));
					verificationMode = false;
				}
				else if (Ident("fun", com))
				{//вход в подпрограмму по адресу (без переноса значения)
					Debug.Log("fun");
					++stackLvl;//поднятие на уровень выше в стеке
					stack[stackLvl].x = int.Parse(com.Substring(3, 2)) - 1;
					stack[stackLvl].y = int.Parse(com.Substring(5, 2));
					verificationMode = false;
				}
				#region проверки 
				else if (Ident("ver", com))//проверка 
				{
					//Активация режима проверки, в котором начнут работать все остальные условные операторы
					verificationMode = true;
					boolRegister = false;

					//координаты по которым надо делать проверку в мире (изначально присваиваются координаты робота)
					//Потом применяются все сдвиги
					verCoors = new Vec2i(transform.position.x, transform.position.y);

					Debug.Log("ver");
					Debug.Log("Кооры робота " + (int)transform.position.x + " : " + (int)transform.position.y);
					Debug.Log("Сдвиг для проверки " + int.Parse(com.Substring(3, 2)) + "  " + int.Parse(com.Substring(5, 2)));
					//Применение к проверке собственного сдвига
					verCoors.x += int.Parse(com.Substring(3, 2));
					verCoors.y += int.Parse(com.Substring(5, 2));

					verCoorsV2 = verCoors.Vector2Set();//вывод на инспектор координат проверки

					Debug.Log("Кооры из собственным сдвигом проверки " + verCoors.x + " : " + verCoors.y);

					//Поиск операторов сдвига после оператора проверки
					//Поиск условий типа блок и т.п
				}
				#endregion
				#region метки блоков для логических операций (работают только если режим проверок включен)
				else if (verificationMode)
				{
					result = false;//переменная для получения результата сравнения.
					bool verIsPassed = false;//указатель на то, что какая-то из проверок была выполнена
					switch (com)
					{
						case "blc":
							if (chunkLoad.ChunkMapVal(verCoors) > 5)
							{//все объекты после ИД5 являются блоками
								result = true;
								verIsPassed = true;
								Debug.Log("blc");
							}
							else
							{
								verIsPassed = true;
							}
							; break;
						case "/blc":
							if (chunkLoad.ChunkMapVal(verCoors) < 6)
							{//все объекты до ИД6 не являются блоками
								result = true;
								verIsPassed = true;
								Debug.Log("/blc");
							}
							else
							{
								verIsPassed = true;
							}
							; break;
						case "gb":
							if (chunkLoad.ChunkMapVal(verCoors) == 36)
							{//Зелёный блок
								result = true;
								verIsPassed = true;
								Debug.Log("grnBl");
							}
							else
							{
								verIsPassed = true;
							}
							; break;
						case "ob":
							if (chunkLoad.ChunkMapVal(verCoors) == 37)
							{//оранжевый блок
								result = true;
								verIsPassed = true;
								Debug.Log("ornBl");
							}
							else
							{
								verIsPassed = true;
							}
							; break;
						case "rb":
							if (chunkLoad.ChunkMapVal(verCoors) == 38)
							{//красный блок
								result = true;
								verIsPassed = true;
								Debug.Log("redBl");
							}
							else
							{
								verIsPassed = true;
							}
							; break;
						default:
							#region операторы ветвления
							if (Ident("ifT", com))
							{//Переход по ссылке если регистр положительный
								if (boolRegister)
								{
									stack[stackLvl].x = int.Parse(com.Substring(3, 2)) - 1;
									stack[stackLvl].y = int.Parse(com.Substring(5, 2));
									verificationMode = false;
									logOp = LogOp.NULL;
									Debug.Log("ifT");
								}
								else
								{
									verificationMode = false;
									logOp = LogOp.NULL;
									Debug.Log("!ifT");
								}
							}
							else if (Ident("ifF", com))//Переход по ссылке если регистр отрицательный
							{
								if (!boolRegister)
								{
									stack[stackLvl].x = int.Parse(com.Substring(3, 2)) - 1;
									stack[stackLvl].y = int.Parse(com.Substring(5, 2));
									verificationMode = false;
									logOp = LogOp.NULL;
									Debug.Log("ifF");
								}
								else
								{
									verificationMode = false;
									logOp = LogOp.NULL;
									Debug.Log("ifF");
								}
							}
							#endregion
								; break;

					}

					if (verIsPassed)
					{
						switch (logOp)
						{
							case LogOp.NULL://если еще не было меток блоков
								boolRegister = result;
								logOp = LogOp.None;
								; break;
							case LogOp.None://если метка блока уже была, но условие так и не указано
								boolRegister = result;
								; break;
							case LogOp.OR://Если условие всё-таки указано, происходит логическое ИЛИ для регистра программатора
								boolRegister = result || boolRegister;
								; break;
							case LogOp.AND://Если условие всё-таки указано, происходит логическое И для регистра программатора
								boolRegister = (result && boolRegister);
								Debug.Log("boolRegister = " + boolRegister);
								; break;
						}
					}
				}
				#endregion
					; break;
				#endregion
		}

		if (!verificationMode)
		{//если режим проверок выключен, все связанные с ним переменные обнуляются
			logOp = 0;
		}

		++currStep;//инкрементация счетчика шагов
	}

	#region функции движения робота
	void MoveRight() {
		//transform.position = new Vector3(transform.position.x + 1f, transform.position.y, transform.position.z);
		charactControl.TargetMove('R');
	}
	void MoveLeft()
	{
		//transform.position = new Vector3(transform.position.x - 1f, transform.position.y, transform.position.z);
		charactControl.TargetMove('L');
	}
	void MoveUp()
	{
		//transform.position = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
		charactControl.TargetMove('U');
	}
	void MoveDown()
	{
		//transform.position = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);
		charactControl.TargetMove('D');
	}
	void RotatRight()
	{
		charactControl.TargetRotate('R');
	}
	void RotatLeft()
	{
		charactControl.TargetRotate('L');
	}
	void RotatUp()
	{
		charactControl.TargetRotate('U');
	}
	void RotatDown()
	{
		charactControl.TargetRotate('D');
	}
	#endregion
	#region воздействие робота на мир
	void Digg() {

	}
	void MainBlocs()
	{

	}
	void SecondBlocks()
	{

	}
	void Road()
	{

	}
	void Geology()
	{

	}
	#endregion
	#region воздействие робота на самого себя
	void Heal()
	{

	}
	#endregion
	#region прерывания
	#endregion
	//сравнивает две стринг-переменные. Возвращает истинну если во второй строке начало совпадает с первой строкой
	bool Ident(string str1, string str2) {
		if ((str1.Length <= str2.Length) && (str1.Length > 0))
		{
			for (int i = 0; i < str1.Length; ++i) {
				if (str1[i] != str2[i]) {
					return false;
				}
			}
			return true;
		}
		else {
			return false;
		}
	}

	#region графическая часть программатора
	//Включение и выключение программы
	public void ProgramActive() {
		progrActive = !progrActive;
		if (progrActive)
		{
			startStopButtonText.text = "STOP";
			startStopButtonText.color = new Color(1, 0, 0.05f);
		}
		else {
			startStopButtonText.text = "START";
			startStopButtonText.color = new Color(0, 1, 0.05f);
		}
	}

	//смена прозрачности заднего фона программатора для улучшения видимости происходящего (Полезно для дебага)
	public void Background()
	{
		Color color = panelProgramator.GetComponent<Image>().color;
		panelProgramator.GetComponent<Image>().color = new Color(color.r, color.g, color.b, SliderBackground.value);
	}

	//обновление данных в панели информации программатора
	void ProgramExecutionInfoUpdate() {
		ProgramExecutionInfo[0].text = (progrActive ? "Вкл": "Выкл");
		ProgramExecutionInfo[1].text = "120";
		ProgramExecutionInfo[2].text = "" + currStep;
		ProgramExecutionInfo[3].text = "" + stackVolume;
		ProgramExecutionInfo[4].text = "" + stackLvl;
		ProgramExecutionInfo[5].text = (verificationMode ? "Вкл" : "Выкл");
		ProgramExecutionInfo[6].text = (boolRegister ? "Истина" : "Ложь");
		ProgramExecutionInfo[7].text = (result ? "Истина" : "Ложь"); ;
		switch (logOp)
		{
			case LogOp.NULL://если еще не было меток блоков
				ProgramExecutionInfo[8].text = "Отключ";
				; break;
			case LogOp.None://если метка блока уже была, но условие так и не указано
				ProgramExecutionInfo[8].text = "Ожидание";
				; break;
			case LogOp.OR://Если условие всё-таки указано, происходит логическое ИЛИ для регистра программатора
				ProgramExecutionInfo[8].text = "И";
				; break;
			case LogOp.AND://Если условие всё-таки указано, происходит логическое И для регистра программатора
				ProgramExecutionInfo[8].text = "ИЛИ";
				; break;
		}
	}

	//вызывается клеткой, над которой сейчас курсор
	public void CellIsSelect(int x,int y) {
		Debug.Log("Select  " + x +" "+ y);
	}

	public void AdresIsChanged(int x, int y)
	{
		Debug.Log("Changed  " + x + " " + y);
	}

	//включение панели программатора
	public void ProgramatorPanelActive() {
		panelProgramator.SetActive(true);
		ProgrPanelUpdate();
	}

	//Обновление всей панели программатора
	void ProgrPanelUpdate() {

		for (int y = 0; y < 12; y++)
			for (int x = 0; x < 16; x++)
			{
				ProgElementUpdate(x,y);
			}
	}

	//Обновление одного блока программатора
	void ProgElementUpdate(int x, int y) {
		switch (ProgramComands[x, y + firstLine])
		{
			case "":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, noneNextSprites[0]);
				; break;
			case "U":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, moveAndRotateSprites[0]);
				; break;
			case "D":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, moveAndRotateSprites[2]);
				; break;
			case "L":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, moveAndRotateSprites[1]);
				; break;
			case "R":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, moveAndRotateSprites[3]);
				; break;
			case "u":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, moveAndRotateSprites[4]);
				; break;
			case "d":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, moveAndRotateSprites[6]);
				; break;
			case "l":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, moveAndRotateSprites[5]);
				; break;
			case "r":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, moveAndRotateSprites[7]);
				; break;
			case "S":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, startEndSprites[0]);
				; break;

			case "gc":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, CristallsSprites[0]);
				; break;
			case "bc":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, CristallsSprites[1]);
				; break;
			case "rc":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, CristallsSprites[2]);
				; break;
			case "wc":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, CristallsSprites[3]);
				; break;
			case "vc":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, CristallsSprites[4]);
				; break;
			case "cc":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, CristallsSprites[5]);
				; break;

			case "or":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, orAndSprites[0]);
				; break;
			case "and":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, orAndSprites[1]);
				; break;

			case "blc":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, blockOrNoSprites[0]);
				; break;
			case "/blc":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, blockOrNoSprites[1]);
				; break;

			case "dg":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, actionsSprites[0]);
				; break;
			case "mb":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, actionsSprites[1]);
				; break;
			case "ge":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, actionsSprites[2]);
				; break;
			case "al":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, actionsSprites[3]);
				; break;
			case "ro":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, actionsSprites[4]);
				; break;
			case "hl":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, actionsSprites[5]);
				; break;
			case "sb":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, actionsSprites[6]);
				; break;

			case "fun":
				programBlocks[x, y].SetProgrElem(ProgrElem.Func, funkSprites[0]);
				; break;
			case "+fun":
				programBlocks[x, y].SetProgrElem(ProgrElem.Func, funkSprites[1]);
				; break;

			case "ver":
				#region определение направления проверки
				switch (ProgramAdressesAndData[x, y + firstLine].x)
				{
					case 1:
						switch (ProgramAdressesAndData[x, y + firstLine].y)
						{
							case 1:
								programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, verifSprites[2]);
								; break;
							case 0:
								programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, verifSprites[1]);
								; break;
							case -1:
								programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, verifSprites[0]);
								; break;
						}
						; break;
					case 0:
						switch (ProgramAdressesAndData[x, y + firstLine].y)
						{
							case 1:
								programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, verifSprites[5]);
								; break;
							case 0:
								programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, verifSprites[4]);
								; break;
							case -1:
								programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, verifSprites[3]);
								; break;
						}
						; break;
					case -1:
						switch (ProgramAdressesAndData[x, y + firstLine].y)
						{
							case 1:
								programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, verifSprites[6]);
								; break;
							case 0:
								programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, verifSprites[7]);
								; break;
							case -1:
								programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, verifSprites[8]);
								; break;
						}
						; break;
				}
				#endregion
						; break;
			case "ifT":
				programBlocks[x, y].SetProgrElem(ProgrElem.If, ifSprites[0]);
				; break;
			case "ifF":

				programBlocks[x, y].SetProgrElem(ProgrElem.If, ifSprites[1]);
				; break;
			case "end":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, startEndSprites[1]);
				; break;
			case "ret":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, returnSprites[0]);
				; break;
			case "jmp":
				programBlocks[x, y].SetProgrElem(ProgrElem.Jmp, jmpSprite);
				; break;
			case "+ret":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, returnSprites[1]);
				; break;
			case "next":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, noneNextSprites[1]);
				; break;
		}
	}

	public void ProgramScroll() {
		firstLine = (int)(scrollbar.value * (pageY - 12));
		for (int i = 0;i < 12; i++) {
			lineNum[i].text = "" + (i + firstLine);
		}
		ProgrPanelUpdate();
	}

	#endregion
}

public enum LogOp {
	NULL,//режим обозначающий что условий еще не было
	None,//условие уже было, но операторы не попадались (результат прошлого условия не учитывается)
	OR,//текущее условие надо сравнить с регистром состояния оператором ||
	AND//текущее условие надо сравнить с регистром состояния оператором &&
}

public class Vec2i
{
	public int x;
	public int y;

	public Vec2i(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public Vec2i(float x, float y)
	{
		this.x = (int)x;
		this.y = (int)y;
	}

	/// <summary>
	/// сброс значений в 0
	/// </summary>
	public void Reset() {
		x = 0;
		y = 0;
	}

	/// <summary>
	/// сброс значений в v
	/// </summary>
	public void Reset(int v)
	{
		x = v;
		y = v;
	}

	public Vector2 Vector2Set()
	{
		return new Vector2(this.x, this.y);
	}
}

//string[,] program0 = new string[pageY, pageX] {
//			{ "S","jmp0001","","","","","","","","","","","","","","end",},//0
//			{ "ver0001","blck","ifT0002","U","jmp0001","","","","","","","","","","","",},
//			{ "ver0100","blck","ifT0009","","jmp0005","","","","","","","","","","","",},
//			{ "","","","","","","","","","","","","","","","",},

//			{ "","","","","","","","","","","","","","","","",},//4
//			{ "ver0100","blck","ifT0006","R","jmp0005","","","","","","","","","","","",},
//			{ "ver00-1","blck","ifT0001","","jmp0013","","","","","","","","","","","",},
//			{ "","","","","","","","","","","","","","","","",},

//			{ "","","","","","","","","","","","","","","","",},//8
//			{ "ver-100","blck","ifT0010","L","jmp0009","","","","","","","","","","","",},
//			{ "ver0001","blck","ifT0013","","jmp0001","","","","","","","","","","","",},
//			{ "","","","","","","","","","","","","","","","",},

//			{ "","","","","","","","","","","","","","","","",},//12
//			{ "ver00-1","blck","ifT0014","D","jmp0013","","","","","","","","","","","",},
//			{ "ver-100","blck","ifT0005","","jmp0009","","","","","","","","","","","",},
//			{ "","","","","","","","","","","","","","","","",},
//		};