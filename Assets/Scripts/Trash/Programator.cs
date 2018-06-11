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
	public Sprite[] buildingBlocksVerSprites = new Sprite[6];//проверки на строительные блоки
	public Sprite[] advansedMoveSprites = new Sprite[3];//повороты и движение относительно направления куда смотрит бот

	public int firstLine = 0;//указывает, с какой строки начинать отображение на экране (зависит от скрола)

	Vec2i coorSelectCell = new Vec2i(0,0);//Координаты выбранной ячейки на программной панели

	public bool keyboardRead = true;//если тру, то можно устанавливать программные блоки, если фолз-нельзя
	#endregion
	[Space]
	public bool progrActive = false;

	public const int pageX = 16;
	public const int pageY = 100;

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
	public Vec2i[,] ProgramAdressesAndData;
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

	//Используется для того, чтобы в случае, если выполняется процедура действия со стороны персонажа,
	//не возникало проскоков из-за отката в чарактер контроллере. Программный такт не переключится на новую ячейку пока не получит ответ
	//позволяющий продолжить работу
	public bool actionIsDone = true;

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

			{ "","","","","","","","","","","","","","","","",},//4
			{ "","","","","","","","","","","","","","","","",},
			{ "","","","","","","","","","","","","","","","",},
			{ "","","","","","","","","","","","","","","","",},

			{ "","","","","","","","","","","","","","","","",},//8
			{ "","","","","","","","","","","","","","","","",},
			{ "","","","","","","","","","","","","","","","",},
			{ "","","","","","","","","","","","","","","","",},

			{ "","","","","","","","","","","","","","","","",},//12
			{ "","","","","","","","","","","","","","","","",},
			{ "","","","","","","","","","","","","","","","",},
			{ "","","","","","","","","","","","","","","","",},
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
		if (progrActive){
			for (int n = 0; n < tactsPerFrame; n++) {
				if (TactsForCD == 0){
					ProgramTactUpdate();//выполнение следубщего шага программы
				}
				else{
					--TactsForCD;
				}
			}
		}
		else {
			//сброс всех значений для корректного старта программы
			currStep = 0;
			stackLvl = 0;
			verificationMode = false;
			actionIsDone = true;
			//logOp = LogOp.NULL;
		}
	}

	private void Update(){
		if (panelProgramator.activeSelf)
		{
			ProgramExecutionInfoUpdate();//Инфа о процессе работы программатора

			if (Time.frameCount % 6 == 0)
			{
				ProgActiveText();
			}

			if (keyboardRead)
			{
				SetProgramElementWthKey();
				ProgramScrollWithMouse();
			}
		}
	}

	void ProgramTactUpdate() {
		if (currStep == 0){//Если программа только запущена
			for (int i = 0; i < stackVolume; i++){
				stack[i] = new Vec2i(0, 0);
			}
		}

		if (stack[stackLvl].x == (pageX - 1)){//если конец строки
			stack[stackLvl].x = startCoor.x;
			stack[stackLvl].y = startCoor.y;
			Debug.Log("stack[stackLvl] = startCoor;" + startCoor.x + " " + startCoor.y);
		}
		else if(actionIsDone){
			++stack[stackLvl].x;
		}

		Debug.Log("Выполнение команды ячейки:" + stack[stackLvl].x + " " + stack[stackLvl].y);

		switch (ProgramComands[stack[stackLvl].x, stack[stackLvl].y]){
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
				if (stackLvl > 0) {
					--stackLvl;
				}
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

			case "f"://Движ вперёд
				actionIsDone = charactControl.TargetMove(charactControl.robotDir);
				; break;
			case "rl"://поворот против часовой
				switch (charactControl.robotDir) {
					case 'U':
						RotatLeft();
						; break;
					case 'D':
						RotatRight();
						; break;
					case 'L':
						RotatDown();
						; break;
					case 'R':
						RotatUp();
						; break;
				}
				verificationMode = false;
				; break;
			case "rr"://поворот за часовой
				switch (charactControl.robotDir)
				{
					case 'U':
						RotatRight();
						; break;
					case 'D':
						RotatLeft();
						; break;
					case 'L':
						RotatUp();
						; break;
					case 'R':
						RotatDown();
						; break;
				}
				verificationMode = false;
				; break;

			#region 
			case "dg"://колупать
				actionIsDone = charactControl.DiggAction();
				; break;
			case "mb"://строить основные блоки
				actionIsDone = charactControl.BuildAction();
			   ; break;
			case "ge"://геология
				
				; break;
			case "al"://Кричалка
				
				; break;
			case "ro"://строить дорогу
				
				; break;
			case "hl"://лека
				
				; break;
			case "sb"://строить вторичные блоки
				actionIsDone = charactControl.BuildAction2();
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
			case "jmp"://перемещение на указанный адрес (без переноса значения)
				Debug.Log("jmp");
				Debug.Log("stackLvl " + stackLvl);
				stack[stackLvl] = new Vec2i(ProgramAdressesAndData[stack[stackLvl].x, stack[stackLvl].y].x - 1, ProgramAdressesAndData[stack[stackLvl].x, stack[stackLvl].y].y);
				//stack[stackLvl].x = -1;
				//stack[stackLvl].y = 0;

				//Debug.Log("ProgramAdressesAndData:" ProgramAdressesAndData[0,0]);
				verificationMode = false;
				; break;
			case "fun"://вход в подпрограмму по адресу (без переноса значения)
				Debug.Log("fun");
				++stackLvl;//поднятие на уровень выше в стеке
				stack[stackLvl] = new Vec2i(ProgramAdressesAndData[stack[stackLvl-1].x, stack[stackLvl-1].y].x - 1, ProgramAdressesAndData[stack[stackLvl - 1].x, stack[stackLvl - 1].y].y);
				verificationMode = false;
				; break;
			case "ver"://Проверка
				#region проверки 
				//Активация режима проверки, в котором начнут работать все остальные условные операторы
				verificationMode = true;
				boolRegister = false;

				//координаты по которым надо делать проверку в мире (изначально присваиваются координаты робота)
				//Потом применяются все сдвиги
				verCoors = new Vec2i(transform.position.x, transform.position.y);

				Debug.Log("ver");
				Debug.Log("Кооры робота " + (int)transform.position.x + " : " + (int)transform.position.y);
				Debug.Log("Сдвиг для проверки " + ProgramAdressesAndData[stack[stackLvl].x, stack[stackLvl].y].x + "  " + ProgramAdressesAndData[stack[stackLvl].x, stack[stackLvl].y].y);
				//Применение к проверке собственного сдвига
				verCoors.x += ProgramAdressesAndData[stack[stackLvl].x, stack[stackLvl].y].x;
				verCoors.y += ProgramAdressesAndData[stack[stackLvl].x, stack[stackLvl].y].y;

				verCoorsV2 = verCoors.Vector2Set();//вывод на инспектор координат проверки

				Debug.Log("Кооры из собственным сдвигом проверки " + verCoors.x + " : " + verCoors.y);

				//Поиск операторов сдвига после оператора проверки
				//Поиск условий типа блок и т.п
				#endregion
				; break;
			default:

				string com = ProgramComands[stack[stackLvl].x, stack[stackLvl].y];
				
				#region метки блоков для логических операций (работают только если режим проверок включен)
				if (verificationMode)
				{
					result = false;//переменная для получения результата сравнения.
					bool verIsPassed = false;//указатель на то, что какая-то из проверок была выполнена
					switch (com)
					{
						#region
						case "bl":
							if (chunkLoad.ChunkMapVal(verCoors) > 5){//все объекты после ИД5 являются блоками
								result = true;

								Debug.Log("bl");
							}
							verIsPassed = true;
							Debug.Log("chunkLoad.ChunkMapVal(verCoors)   " + chunkLoad.ChunkMapVal(verCoors));
							; break;
						case "/bl":
							if (chunkLoad.ChunkMapVal(verCoors) < 6){//все объекты до ИД6 не являются блоками
								result = true;
								Debug.Log("/bl");
							}
							verIsPassed = true;
							; break;
						#region проверка на стройблоки
						case "gb":
							if (chunkLoad.ChunkMapVal(verCoors) == 36){//Зелёный блок
								result = true;
								Debug.Log("grnBl");
							}
							verIsPassed = true;
							; break;
						case "ob":
							if (chunkLoad.ChunkMapVal(verCoors) == 37){//оранжевый блок
								result = true;
								Debug.Log("ornBl");
							}
							verIsPassed = true;
							; break;
						case "rb":
							if (chunkLoad.ChunkMapVal(verCoors) == 38){//красный блок
								result = true;
								Debug.Log("redBl");
							}
							verIsPassed = true;
							; break;
						case "fb":
							if (chunkLoad.ChunkMapVal(verCoors) == 39){//опора
								result = true;
								Debug.Log("fb");
							}
							verIsPassed = true;
							; break;
						case "qb":
							if (chunkLoad.ChunkMapVal(verCoors) == 40){//квадроблок
								result = true;
								Debug.Log("qb");
							}
							verIsPassed = true;
							; break;
						case "roa":
							if ((chunkLoad.ChunkMapVal(verCoors) == 4) || (chunkLoad.ChunkMapVal(verCoors) == 5)){//квадроблок
								result = true;
								Debug.Log("ver road");
							}
							verIsPassed = true;
							; break;
						#endregion
						#endregion
						#region операторы ветвления
						case "ifT"://Переход по ссылке если регистр положительный
							if (boolRegister){
								stack[stackLvl] = new Vec2i(ProgramAdressesAndData[stack[stackLvl].x, stack[stackLvl].y].x - 1, ProgramAdressesAndData[stack[stackLvl].x, stack[stackLvl].y].y);
								verificationMode = false;
								logOp = LogOp.NULL;
								Debug.Log("ifT");
							}
							else{
								verificationMode = false;
								logOp = LogOp.NULL;
								Debug.Log("!ifT");
							}
							; break;
						case "ifF"://Переход по ссылке если регистр отрицательный
							if (!boolRegister){
								stack[stackLvl] = new Vec2i(ProgramAdressesAndData[stack[stackLvl].x, stack[stackLvl].y].x - 1, ProgramAdressesAndData[stack[stackLvl].x, stack[stackLvl].y].y);
								verificationMode = false;
								logOp = LogOp.NULL;
								Debug.Log("ifF");
							}
							else{
								verificationMode = false;
								logOp = LogOp.NULL;
								Debug.Log("ifF");
							}
							#endregion
							; break;
						#region Проверки на кристаллы
						case "gc":
							if (chunkLoad.ChunkMapVal(verCoors) == 12){//Зелёный кри
								result = true;
								Debug.Log("gc");
							}
							verIsPassed = true;
							; break;
						case "bc":
							if (chunkLoad.ChunkMapVal(verCoors) == 13){//Синий кри
								result = true;
								Debug.Log("bc");
							}
							verIsPassed = true;
							; break;
						case "rc":
							if (chunkLoad.ChunkMapVal(verCoors) == 14){//Красный кри
								result = true;
								Debug.Log("rc");
							}
							verIsPassed = true;
							; break;
						case "wc":
							if (chunkLoad.ChunkMapVal(verCoors) == 15){//Белый кри
								result = true;
								Debug.Log("wc");
							}
							verIsPassed = true;
							; break;
						case "vc":
							if (chunkLoad.ChunkMapVal(verCoors) == 16){//Фиолетовый кри
								result = true;
								Debug.Log("vc");
							}
							verIsPassed = true;
							; break;
						case "cc":
							if (chunkLoad.ChunkMapVal(verCoors) == 17){//Голубой кри
								result = true;
								Debug.Log("cc");
							}
							verIsPassed = true;
							; break;
						#endregion
						default:

								; break;
					}
				
					if (verIsPassed){
						switch (logOp){
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

		if (!verificationMode){//если режим проверок выключен, все связанные с ним переменные обнуляются
			logOp = 0;
		}

		if (actionIsDone) {
			++currStep;//инкрементация счетчика шагов
		}
	}

	#region функции движения робота
	void MoveRight() {
		actionIsDone = charactControl.TargetMove('R');
	}
	void MoveLeft(){
		actionIsDone = charactControl.TargetMove('L');
	}
	void MoveUp(){
		actionIsDone = charactControl.TargetMove('U');
	}
	void MoveDown(){
		actionIsDone = charactControl.TargetMove('D');
	}
	void RotatRight(){
		charactControl.TargetRotate('R');
	}
	void RotatLeft(){
		charactControl.TargetRotate('L');
	}
	void RotatUp(){
		charactControl.TargetRotate('U');
	}
	void RotatDown(){
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
		if ((str1.Length <= str2.Length) && (str1.Length > 0)){
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
		ProgActiveText();
	}

	void ProgActiveText() {
		if (progrActive){
			startStopButtonText.text = "STOP";
			startStopButtonText.color = new Color(1, 0, 0.05f);
		}
		else{
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
		switch (logOp){
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
		//Debug.Log("Select  " + x +" "+ y);
		coorSelectCell.x = x;
		coorSelectCell.y = y;
	}

	public void AdresIsChanged(int x, int y,int adrx, int adry)
	{
		ProgramAdressesAndData[x, y + firstLine].x = adrx;
		ProgramAdressesAndData[x, y + firstLine].y = adry;

		keyboardRead = true;
		Debug.Log("key unblock");
		Debug.Log("Changed  " + x + " " + y + "data:" + adrx + " " + adry);
	}

	//блокировка считывания клавиш для сеттера программных блоков
	public void BlockKeyboard() {
		keyboardRead = false;
		Debug.Log("key block");
	}

	//включение или выключение панели программатора. Если включение, панель обновляется
	public void ProgramatorPanelActive(bool mode) {
		panelProgramator.SetActive(mode);
		charactControl.actionsIsAlloved = !mode;
		if (mode) {
			ProgrPanelUpdate();
		}
	}

	//Обновление всей панели программатора
	void ProgrPanelUpdate() {

		for (int y = 0; y < 12; y++)
			for (int x = 0; x < 16; x++){
				ProgElementUpdate(x,y);
			}
	}

	//Обновление одного блока программатора
	void ProgElementUpdate(int x, int y) {
		switch (ProgramComands[x, y + firstLine])
		{
			#region
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

			case "f":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, advansedMoveSprites[0]);
				; break;
			case "rl":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, advansedMoveSprites[1]);
				; break;
			case "rr":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, advansedMoveSprites[2]);
				; break;

			//Проверки на кристаллы
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

				//Логические операции
			case "or":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, orAndSprites[0]);
				; break;
			case "and":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, orAndSprites[1]);
				; break;

				//проверка на имение блока
			case "bl":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, blockOrNoSprites[0]);
				; break;
			case "/bl":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, blockOrNoSprites[1]);
				; break;

				//выполнение определенных действий со стороны бота(не движения)
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

				//проверки на строительные блоки
			case "gb":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, buildingBlocksVerSprites[0]);
				; break;
			case "ob":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, buildingBlocksVerSprites[1]);
				; break;
			case "rb":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, buildingBlocksVerSprites[2]);
				; break;
			case "fb":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, buildingBlocksVerSprites[3]);
				; break;
			case "qb":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, buildingBlocksVerSprites[4]);
				; break;
			case "roa":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, buildingBlocksVerSprites[5]);
				; break;

				//входы в функции
			case "fun":
				programBlocks[x, y].SetProgrElem(ProgrElem.Func, funkSprites[0]);
				; break;
			case "+fun":
				programBlocks[x, y].SetProgrElem(ProgrElem.Func, funkSprites[1]);
				; break;

				//вклчение режима проверки
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
								programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, verifSprites[5]);
								; break;
							case -1:
								programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, verifSprites[8]);
								; break;
						}
						; break;
					case 0:
						switch (ProgramAdressesAndData[x, y + firstLine].y)
						{
							case 1:
								programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, verifSprites[1]);
								; break;
							case 0:
								programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, verifSprites[4]);
								; break;
							case -1:
								programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, verifSprites[7]);
								; break;
						}
						; break;
					case -1:
						switch (ProgramAdressesAndData[x, y + firstLine].y)
						{
							case 1:
								programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, verifSprites[0]);
								; break;
							case 0:
								programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, verifSprites[3]);
								; break;
							case -1:
								programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, verifSprites[6]);
								; break;
						}
						; break;
				}
				#endregion
				; break;

				//ветвление
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
			case "+ret":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, returnSprites[1]);
				; break;
			case "jmp":
				programBlocks[x, y].SetProgrElem(ProgrElem.Jmp, jmpSprite);
				; break;
			case "next":
				programBlocks[x, y].SetProgrElem(ProgrElem.WithoutParam, noneNextSprites[1]);
				; break;
				#endregion
		}
	}

	string[] moveCodes = new string[] {
		"U","D","L","R",
	};
	string[] rotatCodes = new string[] {
		"u","d","l","r",
	};
	string[] ifCodes = new string[] {
		"ifT","ifF",
	};
	string[] funcCodes = new string[] {
		"fun","+fun",
	};
	string[] retCodes = new string[] {
		"ret","+ret",
	};
	string[] startendCodes = new string[] {
		"S","end",
	};
	string[] cristallsCodes = new string[] {
		"gc","bc","rc","wc","vc","cc",
	};
	string[] rockFituresCodes = new string[] {//Особенности породы (и ее отсутствие)
		"bl","/bl",
	};
	string[] buildingBlocksVerCodes = new string[] {//проверки на строительные блоки (fb - опоры, qb - квадро-блоки, roa - дорога)
		"gb","ob","rb","fb","qb","roa",
	};
	string[] actionCodes = new string[] {
		"mb","sb","ge","ro","hl","al",
	};
	string[] advMoveCodes = new string[] {
		"f","rl","rr",
	};
	string nextLineCode = "next";
	string jmpCode = "jmp";

	string cashCode = "";//нужен для копирования и вставки програмного блока

	//установка программного элемента кнопками
	void SetProgramElementWthKey() {
		//ЗАЖАТЫЙ ШИФТ
		if (Input.GetKey(KeyCode.LeftShift)) {
			if (Input.GetKeyDown(KeyCode.W))
			{
				ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = "ver";
				if (ProgramAdressesAndData[coorSelectCell.x, coorSelectCell.y + firstLine].y < 1)
				{
					++ProgramAdressesAndData[coorSelectCell.x, coorSelectCell.y + firstLine].y;
				}
				ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
			}
			else if (Input.GetKeyDown(KeyCode.S))
			{
				ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = "ver";
				if (ProgramAdressesAndData[coorSelectCell.x, coorSelectCell.y + firstLine].y > -1)
				{
					--ProgramAdressesAndData[coorSelectCell.x, coorSelectCell.y + firstLine].y;
				}
				ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
			}
			else if (Input.GetKeyDown(KeyCode.A))
			{
				ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = "ver";
				if (ProgramAdressesAndData[coorSelectCell.x, coorSelectCell.y + firstLine].x > -1)
				{
					--ProgramAdressesAndData[coorSelectCell.x, coorSelectCell.y + firstLine].x;
				}
				ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
			}
			else if (Input.GetKeyDown(KeyCode.D))
			{
				ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = "ver";
				if (ProgramAdressesAndData[coorSelectCell.x, coorSelectCell.y + firstLine].x < 1)
				{
					++ProgramAdressesAndData[coorSelectCell.x, coorSelectCell.y + firstLine].x;
				}
				ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
			}
			else if (Input.GetKeyDown(KeyCode.C)){//выбор свойств породы
				string comand = ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine];
				for (int i = 0; i < 2; i++)
				{
					if (comand == rockFituresCodes[0]){
						ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = rockFituresCodes[(i + 1) % 2];
						break;
					}
					else if (i == 1){
						ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = rockFituresCodes[0];
					}
				}
				ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
			}
			else if (Input.GetKeyDown(KeyCode.Z))
			{
				string comand = ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine];
				for (int i = 0; i < 6; i++)
				{
					if (comand == actionCodes[i])
					{
						ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = actionCodes[(i + 1) % 6];
						break;
					}
					else if (i == 5)
					{
						ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = actionCodes[0];
					}
				}
				ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
			}
		}
		//ЗАЖАТЫЙ КОНТРОЛЛ
		else if (Input.GetKey(KeyCode.LeftControl))
		{
			if (Input.GetKeyDown(KeyCode.C))
			{
				cashCode = ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine];
			}
			else if (Input.GetKey(KeyCode.V))
			{
				ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = cashCode;
				ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
			}
		}
		else if(Input.GetKeyDown(KeyCode.Z))
		{
			ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = "dg";
			ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
		}
		else if (Input.GetKeyDown(KeyCode.X)){// движение по направлению смотра, поворот против и за часовой стрелкой
			string comand = ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine];
			for (int i = 0; i < 3; i++)
			{
				if (comand == advMoveCodes[i])
				{
					ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = advMoveCodes[(i + 1) % 3];
					break;
				}
				else if (i == 2)
				{
					ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = advMoveCodes[0];
				}
			}
			ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
		}
		else if (Input.GetKeyDown(KeyCode.B)){//выбор проверки на строительные блоки 
			string comand = ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine];
			for (int i = 0; i < 6; i++){
				if (comand == buildingBlocksVerCodes[i]){
					ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = buildingBlocksVerCodes[(i + 1) % 6];
					break;
				}
				else if (i == 5){
					ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = buildingBlocksVerCodes[0];
				}
			}
			ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
		}
		else if (Input.GetKeyDown(KeyCode.C)){//выбор кристалла 
			string comand = ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine];
			for (int i = 0; i < 6; i ++) {
				if (comand == cristallsCodes[i]) {
					ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = cristallsCodes[(i + 1) % 6];
					break;
				}
				else if (i == 5) {
					ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = cristallsCodes[0];
				}
			}
			ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
		}
		else if (Input.GetKeyDown(KeyCode.Q)){//кнопка установки начала и конца проги
			if (ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] == startendCodes[0]){
				ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = startendCodes[1];
			}
			else{
				ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = startendCodes[0];
			}
			ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
		}
		else if (Input.GetKeyDown(KeyCode.I)){//кнопка выбора ветвления
			if (ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] == ifCodes[0]){
				ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = ifCodes[1];
			}
			else{
				ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = ifCodes[0];
			}
			ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
		}
		else if (Input.GetKeyDown(KeyCode.F)){//кнопка выбора входа в функцию
			if (ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] == funcCodes[0]){
				ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = funcCodes[1];
			}
			else{
				ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = funcCodes[0];
			}
			ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
		}
		else if (Input.GetKeyDown(KeyCode.E)){//возвраты
			if (ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] == retCodes[0]){
				ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = retCodes[1];
			}
			else{
				ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = retCodes[0];
			}
			ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
		}
		else if (Input.GetKeyDown(KeyCode.Backspace)){
			ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = nextLineCode;
			ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
		}
		else if (Input.GetKeyDown(KeyCode.W)){
			if (ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] != "U")
			{
				ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = moveCodes[0];
				ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
			}
			else {
				ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = rotatCodes[0];
				ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
			}
		}
		else if (Input.GetKeyDown(KeyCode.S)){
			if (ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] != "D")
			{
				ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = moveCodes[1];
				ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
			}
			else
			{
				ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = rotatCodes[1];
				ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
			}
		}
		else if (Input.GetKeyDown(KeyCode.A)){
			if (ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] != "L")
			{
				ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = moveCodes[2];
				ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
			}
			else
			{
				ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = rotatCodes[2];
				ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
			}
		}
		else if (Input.GetKeyDown(KeyCode.D)){
			if (ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] != "R")
			{
				ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = moveCodes[3];
				ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
			}
			else
			{
				ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = rotatCodes[3];
				ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
			}
		}
		else if (Input.GetKey(KeyCode.Delete)){
			ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = "";
			ProgramAdressesAndData[coorSelectCell.x, coorSelectCell.y + firstLine] = new Vec2i(0, 0);
			ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
		}
		else if (Input.GetKeyDown(KeyCode.G)){
			ProgramComands[coorSelectCell.x, coorSelectCell.y + firstLine] = jmpCode;
			ProgElementUpdate(coorSelectCell.x, coorSelectCell.y);
		}
	}

	//управление скроллом через мышь
	void ProgramScrollWithMouse()
	{
		//при нажатии на скролл блокировка спадает, но во избежание пролетов сделана проверка
		if (keyboardRead) {
			float Zoom = Input.GetAxis("Mouse ScrollWheel");
			if (Zoom > 0)
			{
				float maxSLine = pageY - 12;

				if (firstLine > 0)
				{
					--firstLine;
				}
				scrollbar.value = firstLine / maxSLine;
			}
			else if (Zoom < 0)
			{
				float maxSLine = pageY - 12;

				if (firstLine < maxSLine)
				{
					++firstLine;
				}
				scrollbar.value = firstLine / maxSLine;
			}
		}
	}

	//Скролл программы через скроллбар
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