using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharactControl : MonoBehaviour {

	public ChankLoad chankLoadScript;//ссылка на скрипт чанклоадера
	public Transform cameraTransform;//трансформ камеры
	public Transform characterMoveTarget;//трансформ таргета персонажа
	public GameObject characterMoveTargetbj;//Ссылка на таргет
	[Space]
	public GameObject upGUI;//ссылка на GUI Апа
	public GameObject storageGUI;//ссылка на GUI склада
	public GameObject respGUI;//ссылка на GUI респа
	public GameObject marketGUI;//ссылка на GUI маркета
	public GameObject repairGUI;//ссылка на GUI леки
    public NetHelper netHelper;
	public GameObject buttonAutoDig;//ссылка на кнопку автокопы

	DiggingAnimationController diggingAnimationController;//контроллер анимации копания
	HealAnimationController healAnimationController;//контроллер анимации лечения

	Transform characTransform;//трансформ персонажа(для сокращения ссылки к позиции)

	Vector3 moveTarget;//цель для движения персонажа
	Quaternion quaternionTarget;//цель для поворота персонажа

	bool godMode = false;//доступно ли пролетание через блоки или другие функции админа

	float Z;//используется как сокращение трансформа персонажа по Z

	bool inBuilding = false;//Находится ли бот в здании
	[Space]
	public bool offline = false;
	[Space]
	float lastStepTime;//время на момент прошлого движения
	readonly float timeDelay = 1f / 50f;//задержка между шагами передвижения
	float accelerationCoef;//коэффициент, делающий более плавным старт движения бота.
	int moveSeries = 1;//кол-во движений без остановки 

	bool autoDig = true;//Автокопа

	char robotDir = 'U';

	//координаты, к которым двигается камера игрока
	float XX;
	float YY;

	/// <summary> накопитель времени задержки перед тем, как действие будет доступно.
	///			  разные действия могут давать разную задержку, поэтому лучше иметь такую переменную </summary>
	public float cooldown = 0f;
	/// <summary> КД для передвижения, берется на основе скиллов скорости</summary>
	public float moveCD = 0.06f;
	public float movePlusCD = 0.03f;
	public float diggCD = 0.4f;
	public float healCD = 0.5f;
	float lastActionTime = 0;

	// Use this for initialization
	void Start () {
		VarInit();
	}


	void VarInit()
	{
		diggingAnimationController = characterMoveTargetbj.GetComponent<DiggingAnimationController>();
		healAnimationController = GetComponent<HealAnimationController>();

		accelerationCoef = timeDelay * 10f;

		characTransform = gameObject.transform;
		lastStepTime = Time.unscaledTime;

		XX = characTransform.position.x;
		YY = characTransform.position.y;

		moveTarget = characTransform.position;
		quaternionTarget = new Quaternion(0, 0, 0, 0);
		Z = characTransform.position.z;
	}
	readonly float moveSpeed = 13f;//Скорость реакции бота на таргет
	void FixedUpdate()
	{
		if (!inBuilding)//если бот не в здании
		{
			//сокращение накопленного кулдауна
			cooldown -= (Time.time - lastActionTime);
			if (cooldown < 0f) {
				cooldown = 0f;
			}
			lastActionTime = Time.time;

			BuildingLocator();//поиск зданий и активация нужного GUI при совпадении координат
			if (Input.GetKeyDown(KeyCode.G))//режим отключения проверки на столкновения (прохождение через блоки)
			{
				godMode = !godMode;
			}
		}
		if (Input.GetKeyUp(KeyCode.E))//автокопа
		{
			autoDig = !autoDig;
			buttonAutoDig.SetActive(!autoDig);
		}
		if (Input.GetKeyUp(KeyCode.V))//Хилка
		{
			healAnimationController.AnimActive(characTransform);
		}
		if (Input.GetKey(KeyCode.Z))//копание на кнопку
		{
				MessageToServer('D', robotDir);
		}

		characterMoveTarget.position = moveTarget;
		characTransform.position = Vector3.Lerp(characTransform.position, moveTarget, Time.deltaTime * moveSpeed);
		characTransform.rotation = Quaternion.LerpUnclamped(characTransform.rotation, quaternionTarget, 0.3f);

		if (/*Time.frameCount % 4 == 0*/ true)
		{
			XX = characTransform.position.x;
			YY = characTransform.position.y;
		}

		//проверка на нажатие кнопок движения
		if ((Time.unscaledTime - lastStepTime) > timeDelay * 50)
		{
			moveSeries = 1;
		}
		if ((Time.unscaledTime - lastStepTime) >= (timeDelay + accelerationCoef / (moveSeries * 3)) && !inBuilding)
		{
			if (CharactControlWhithKeyboard())
			{
				lastStepTime = Time.unscaledTime;
				moveSeries++;
				//Debug.Log(lastStepTime);
				//Debug.Log(moveSeries);
			}
		}

		cameraTransform.position = Vector3.Lerp(cameraTransform.position, new Vector3(XX, YY, cameraTransform.position.z), Time.deltaTime * 4);
	}

	//проверка чанка, в котором находится игрок, на наличие зданий. если есть здания-сопоставляет координаты.
	//Если координаты сходятся-открывает соответствующее GUI
	void BuildingLocator()
	{
		int mtx = (int)moveTarget.x / 32;
		int mty = (int)moveTarget.y / 32;
		if (chankLoadScript.chankBuildingsDataArray[mtx, mty] != null)
		{
			for (int i = 0; i < chankLoadScript.chankBuildingsDataArray[mtx, mty].Count; i++)
				if (chankLoadScript.chankBuildingsDataArray[mtx, mty][i].x == (int)moveTarget.x && chankLoadScript.chankBuildingsDataArray[mtx, mty][i].y == (int)moveTarget.y)
				{
					inBuilding = true;
					switch (chankLoadScript.chankBuildingsDataArray[mtx, mty][i].packType)
					{
						case 0: repairGUI.SetActive(true); break;
						case 1: respGUI.SetActive(true); break;
						case 2: marketGUI.SetActive(true); break;
						case 3: upGUI.SetActive(true); break;
						case 4: storageGUI.SetActive(true); break;
					}
				}
		}
	}

	//движение таргета
	public void TargetMove(char direction)
	{
		if (cooldown < 0.002f)
		{
			float block = ChankMapVal((int)moveTarget.x, (int)moveTarget.y);
			if ((block == 4) || (block == 5)) {
				cooldown += movePlusCD;
			}
			else {
				cooldown += moveCD - (block / 200);
			}
			switch (direction)
			{
				case 'U':
					moveTarget = new Vector3((int)moveTarget.x + 0.5f, (int)moveTarget.y + 1f + 0.5f, Z);
					TargetRotate(direction);
					MessageToServer('m', 'U');
					break;
				case 'D':
					moveTarget = new Vector3((int)moveTarget.x + 0.5f, (int)moveTarget.y - 1f + 0.5f, Z);
					TargetRotate(direction);
					MessageToServer('m', 'D');
					break;
				case 'R':
					moveTarget = new Vector3((int)moveTarget.x + 1 + 0.5f, (int)moveTarget.y + 0.5f, Z);
					TargetRotate(direction);
					break;
				case 'L':
					moveTarget = new Vector3((int)moveTarget.x - 1 + 0.5f, (int)moveTarget.y + 0.5f, Z);
					TargetRotate(direction);
					break;
			}
		}
	}
	//поворот таргета
	public void TargetRotate(char direction)
	{
		switch (direction)
		{
			case 'U':
				robotDir = 'U';
				quaternionTarget.eulerAngles = new Vector3(0, 0, 0);
				characterMoveTarget.rotation = quaternionTarget;
				MessageToServer('r', 'U');
				break;
			case 'D':
				robotDir = 'D';
				quaternionTarget.eulerAngles = new Vector3(0, 0, 180);
				characterMoveTarget.rotation = quaternionTarget;
				MessageToServer('r', 'D');
				break;
			case 'R':
				robotDir = 'R';
				quaternionTarget.eulerAngles = new Vector3(0, 0, -90);
				MessageToServer('r', 'R');
				break;
			case 'L':
				robotDir = 'L';
				quaternionTarget.eulerAngles = new Vector3(0, 0, 90);
				MessageToServer('r', 'L');
				break;
		}
	}

	//выброс таргета из здания
	public void QuitBuilding(string direction)
	{
		int distance = ((int)direction[1] - 48);
		MessageToServer('o', ' ');
		switch (direction[0])
		{
			case 'U': moveTarget.y += distance; break;
			case 'D': moveTarget.y -= distance; break;
			case 'R': moveTarget.x += distance; break;
			case 'L': moveTarget.x -= distance; break;
		}
		inBuilding = false;
	}

	//функция возврата на респаун
	public void ReturnToRespawn(Vector3 Pos)
	{
		moveTarget = new Vector3(Pos.x + 0.5f, Pos.y + 0.5f, -5f);
	}

	//управление перемещением персонажа клавиатурой(перемещение таргета в новое положение, если можно)
	bool CharactControlWhithKeyboard()
	{
		int X = (int)characTransform.position.x;
		int Y = (int)characTransform.position.y;

		bool keyPressed = false;//была ли нажата какая-нибудь кнопка

		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
		{
			int upperDir = ChankMapVal((int)moveTarget.x, (int)moveTarget.y + 1);

			if (!Input.GetKey(KeyCode.LeftShift))
			{
				if (upperDir < 5 || godMode)
				{
					keyPressed = true;
					TargetMove('U');
				}
				else if (upperDir != 6 && upperDir != 7 && upperDir != 8 && upperDir != 42)
				{
					robotDir = 'U';
					MessageToServer('d', robotDir);
					TargetRotate('U');
				}
				else
				{
					TargetRotate('U');
				}
			}
			else if (upperDir > 8 && upperDir != 42)
			{
				robotDir = 'U';
				MessageToServer('d', robotDir);
				TargetRotate('U');
			}
			else
			{
				TargetRotate('U');
			}
			characterMoveTarget.position = moveTarget;
		}
		else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
		{
			int lowerDir = ChankMapVal((int)moveTarget.x, (int)moveTarget.y - 1);
			if (!Input.GetKey(KeyCode.LeftShift))
			{
				if (lowerDir < 5 || godMode)
				{
					keyPressed = true;
					TargetMove('D');
				}
				else if (lowerDir != 6 && lowerDir != 7 && lowerDir != 8 && lowerDir != 42)
				{
					robotDir = 'D';
					MessageToServer('d', robotDir);
					TargetRotate('D');
				}
				else
				{
					TargetRotate('D');
				}
			}
			else if (lowerDir > 8 && lowerDir != 42)
			{
				robotDir = 'D';
				MessageToServer('d', robotDir);
				TargetRotate('D');
			}
			else
			{
				TargetRotate('D');
			}
			characterMoveTarget.position = moveTarget;
		}
		else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
		{
			int rightDir = ChankMapVal((int)moveTarget.x + 1, (int)moveTarget.y);
			if (!Input.GetKey(KeyCode.LeftShift))
			{
				if (rightDir < 5 || godMode)
				{
					keyPressed = true;
					TargetMove('R');
				}
				else if (rightDir != 6 && rightDir != 7 && rightDir != 8 && rightDir != 42)
				{
					robotDir = 'R';
					MessageToServer('d', robotDir);
					TargetRotate('R');
				}
				else
				{
					TargetRotate('R');
				}
			}
			else if (rightDir > 8 && rightDir != 42)
			{
				robotDir = 'R';
				MessageToServer('d', robotDir);
				TargetRotate('R');
			}
			else {
				TargetRotate('R');
			}
			characterMoveTarget.position = moveTarget;
		}
		else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
		{
			int leftDir = ChankMapVal((int)moveTarget.x - 1, (int)moveTarget.y);
			if (!Input.GetKey(KeyCode.LeftShift))
			{
				if (leftDir < 5 || godMode)
				{
					keyPressed = true;
					TargetMove('L');
				}
				else if (leftDir > 8 && leftDir != 42)
				{
					robotDir = 'L';
					MessageToServer('d', robotDir);
					TargetRotate('L');
				}
				else
				{
					TargetRotate('L');
				}
			}
			else if (leftDir > 8 && leftDir != 42)
			{
				robotDir = 'L';
				MessageToServer('d', robotDir);
				TargetRotate('L');
			}
			else
			{
				TargetRotate('L');
			}
			characterMoveTarget.position = moveTarget;
		}
		else if (Input.GetKey(KeyCode.Q))
		{
			MessageToServer('g', robotDir);
		}
		else if (Input.GetKey(KeyCode.F))
		{
			MessageToServer('b', robotDir);
		}

		return keyPressed;
	}

	public void AutoDigOn()
	{
		autoDig = true;
		buttonAutoDig.SetActive(false);
	}

	#region взаимодействие с картой
	//функции для упрощения обращения в 4-мерный массив чанков
	//получение значения из массива
	int ChankMapVal(int x, int y)
	{
		return chankLoadScript.chankMap[x / 32, y / 32][x % 32, y % 32];
	}
	//установка значения в массив(должен быть запрос на сервер)

	void MessageToServer(char param, char dir)
	{
		if (param == 'd')//копание при движении
		{
			if (autoDig)
			{
				if (cooldown == 0f)
				{
					cooldown += diggCD;
					if (offline)
					{
						ChangeMap(dir, 1);
					}
					else
					{
						netHelper.ChangeMap('d', dir);
					}
					diggingAnimationController.AnimActive(characterMoveTarget.position, robotDir);
				}
			}
		}
		else if (param == 'D')//Копание при нажатии на кнопку копания
		{
			if (cooldown == 0f)
			{
				cooldown += diggCD;
				if (offline)
				{
					ChangeMap(dir, 1);
				}
				else
				{
					netHelper.ChangeMap('d', dir);
				}
				diggingAnimationController.AnimActive(characterMoveTarget.position, robotDir);
			}
		}
		else if (param == 'g')
		{//Геология
			if (offline)
			{

			}
			else
			{
				netHelper.ChangeMap('g', dir);
			}
			//Добавить анимацию геологии
		}
		else if (param == 'b')//Стройка
		{
			if (offline)
			{
				ChangeMap(dir, 36);
			}
			else
			{
				netHelper.ChangeMap(param, dir);
			}
			//Добавить анимацию геологии
		}
		else if (param == 'm')//Передвижение
		{
			if (offline)
			{

			}
			else
			{
				netHelper.ChangePos('m', dir);
			}
		}
		else if (param == 'r')//Поворот персонажа
		{
			if (offline)
			{

			}
			else
			{
				netHelper.ChangePos('r', dir);
			}
		}
		else if (param == 'o')//Выход из здания
		{
			if (offline)
			{

			}
			else
			{
				netHelper.ChangePos('o', dir);
			}
		}
	}

	void ChangeMap(char direction, int val) {
		int x = (int)characterMoveTarget.position.x;
		int y = (int)characterMoveTarget.position.y;

		switch (direction)
		{
			case 'O': ; break;
			case 'U': y++ ; break;
			case 'D': y-- ; break;
			case 'R': x++ ; break;
			case 'L': x-- ; break;
		}
		chankLoadScript.chankMap[x / 32, y / 32][x % 32, y % 32] = (byte)val;
	}
	#endregion


}
