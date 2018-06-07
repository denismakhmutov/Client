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

	//Префабы, нужные для построения сетки программатора
	public GameObject LinePrefab;//префаб строки программатора
	public GameObject WithoutParametersPrefab;//програмный элемент без вписывемых в него параметров
	public GameObject FuncPrefab;//элемент "сылка на функцию"
	public GameObject IfPrefab;//оператор ветвления
	public GameObject JmpPrefab;//перенос по адресу
	public GameObject LabelPrefab;//именованная ячейка

	#endregion
	[Space]
	public bool progrActive = false;

	public const int pageX = 16;
	public const int pageY = 16;

	public int tactsPerFrame = 1;//тактов на один кадр

	public Vec2i startCoor = new Vec2i(0, 0);//координаты стартовой метки программы
	public int currStep = 0;//текущий шаг в прогремме
	public int stackVolume = 1024;//Объём стека
	Vec2i[] stack;//типа стек для работы подфункций, чтобы не юзать рекурсию

	public int stackLvl = 0;//на каком уровне в стеке в данный момент нужно считывать значения

	ChankLoad chunkLoad;//ссылка на чанклоадер для работы с картой

	//массив программы (разбиваться на страницы не будет. 
	//Программатор будет иметь скролбар и вся прога будет размещаться на одном большом поле)
	string[,] program;
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

		for (int i = 0;i < 12; i++) {
			GameObject line = Instantiate(LinePrefab, programLines[i].transform);
			//GameObject[] cells = line.GetComponentsInChildren<GameObject>();
			//for (int ii = 0; ii < 16; ii++)
			//{
			//	Instantiate(FuncPrefab, cells[ii].transform, false);
			//}
		}

		program = new string[pageX, pageY] {
			{ "S","jmp0001","","","","","","","","","","","","","","end",},//0
			{ "ver0001","blck","ifT0002","U","jmp0001","","","","","","","","","","","",},
			{ "ver0100","blck","ifT0009","","jmp0005","","","","","","","","","","","",},
			{ "","","","","","","","","","","","","","","","",},

			{ "","","","","","","","","","","","","","","","",},//4
			{ "ver0100","blck","ifT0006","R","jmp0005","","","","","","","","","","","",},
			{ "ver00-1","blck","ifT0001","","jmp0013","","","","","","","","","","","",},
			{ "","","","","","","","","","","","","","","","",},

			{ "","","","","","","","","","","","","","","","",},//8
			{ "ver-100","blck","ifT0010","L","jmp0009","","","","","","","","","","","",},
			{ "ver0001","blck","ifT0013","","jmp0001","","","","","","","","","","","",},
			{ "","","","","","","","","","","","","","","","",},

			{ "","","","","","","","","","","","","","","","",},//12
			{ "ver00-1","blck","ifT0014","D","jmp0013","","","","","","","","","","","",},
			{ "ver-100","blck","ifT0005","","jmp0009","","","","","","","","","","","",},
			{ "","","","","","","","","","","","","","","","",},
		};

	}

	// Update is called once per frame
	void FixedUpdate() {
		for (int n = 0; n < tactsPerFrame; n++)
			if (progrActive)
			{
				ProgramTactUpdate();//выполнение следубщего шага программы
			}
			else {
				//сброс всех значений для корректного старта программы
				currStep = 0;
				stackLvl = 0;
				verificationMode = false;
				//logOp = LogOp.NULL;
			}
		ProgramExecutionInfoUpdate();
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

		switch (program[stack[stackLvl].y, stack[stackLvl].x])//ПОМЕНЯТЬ В БУДУЩЕМ ИКС И ИГРЕК МЕСТАМИ. СЕЙЧАС ТАК ДЛЯ ДЕБАГА
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
				verificationMode = false;
				; break;
			case "l":
				verificationMode = false;
				; break;
			case "u":
				verificationMode = false;
				; break;
			case "d":
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
				string com = program[stack[stackLvl].y, stack[stackLvl].x];//ПОМЕНЯТЬ В БУДУЩЕМ ИКС И ИГРЕК МЕСТАМИ. СЕЙЧАС ТАК ДЛЯ ДЕБАГА

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
						case "blck":
							if (chunkLoad.ChunkMapVal(verCoors) > 5)
							{//все объекты после ИД5 являются блоками
								result = true;
								verIsPassed = true;
								Debug.Log("blck");
							}
							else
							{
								verIsPassed = true;
							}
							; break;
						case "noblck":
							if (chunkLoad.ChunkMapVal(verCoors) < 6)
							{//все объекты до ИД6 не являются блоками
								result = true;
								verIsPassed = true;
								Debug.Log("noblck");
							}
							else
							{
								verIsPassed = true;
							}
							; break;
						case "grnBl":
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
						case "ornBl":
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
						case "redBl":
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

//{ "S","jmp0001","","","","","","","","","","","","","","end",},//0
//	{ "ver0001","blck","ifT0002","U","jmp0001","","","","","","","","","","","",},
//	{ "ver0100","blck","ifT0009","","jmp0005","","","","","","","","","","","",},
//	{ "","","","","","","","","","","","","","","","",},
	
//	{ "","","","","","","","","","","","","","","","",},//4
//	{ "ver0100","blck","ifT0006","R","jmp0005","","","","","","","","","","","",},
//	{ "ver00-1","blck","ifT0001","","jmp0013","","","","","","","","","","","",},
//	{ "","","","","","","","","","","","","","","","",},

//	{ "","","","","","","","","","","","","","","","",},//8
//	{ "ver-100","blck","ifT0010","L","jmp0009","","","","","","","","","","","",},
//	{ "ver0001","blck","ifT0013","","jmp0001","","","","","","","","","","","",},
//	{ "","","","","","","","","","","","","","","","",},
	
//	{ "","","","","","","","","","","","","","","","",},//12
//	{ "ver00-1","blck","ifT0014","D","jmp0013","","","","","","","","","","","",},
//	{ "ver-100","blck","ifT0005","","jmp0009","","","","","","","","","","","",},
//	{ "","","","","","","","","","","","","","","","",},