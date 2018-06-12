using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIUPController : MonoBehaviour {

	public GameObject GUIUPObj;//ссылка на основу гуишки
	public CharactControl charactControlScript;//Ссылка на контроллер персонажа
	public GameObject canvasCurrentUpObj;//Ссылка на оббъект панели с текущим скиллом
	public Text textCharactLVl;//общий уровень персонажа
	public WarningGUI warningGUI;//Окно предупреждения
	[Space]
	public Image imageCurrentSkill;//ссылка на спрайт изображения текущего скилла
	[Space]
	public GameObject[] buttonUPSkillObj = new GameObject[20];//Ссылки на объект кнопки,для их отключения
	public Text textCurrentSkillNameAndLVL;//Имя и Уровень Текущего скилла
	public Text textCurrentSkillHint;//Инфа про функцию скилла
	public Text textCurrentSkillEffect;//Эффект, который дает текущий скилл
	public Image UPSkillStatusLine;//Статуслайн опыта для апа скилла
	public Text textCurrentUPSkillStatusLine;//соотношение полученного опыта и нужного для апа скилла
	[Space]
	public GameObject[] panelForTextOfButtonUPSkill = new GameObject[20];//ссылки на панельки с текстом уровня скиллов на основной скилл-панели
	[Space]
	public Text textCurrentSkillNextLVLPrice;//цена следующего уровня
	[Space]
	public GameObject buttonUPLVObj;//ссылка на кнопку апгрейда уровня(для скрытия её если надо)
	[Space]
	public Sprite skillEmptyFrame;
	[Space]
	public Sprite[] skillIcons = new Sprite[47];//Иконки скиллов
	[Space]
	public Button[] buttonUPSkill = new Button[20];//кнопка с иконкой скилла
	[Space]
	public Text[] textUPSkill = new Text[20];//уровень скилла

	public bool offline;
	/// <summary>
	///Масcив имеющихся у игрока скиллов
	///по иксу выбор скилла по номеру ячейки, по игреку выбор :
	///0-айди типа скилла,			1-текущий уровень скилла
	///2-сила скилла				3-текущий набранный опыт,
	///4-Опыт для нового уровня		5-цена следующего	уровня,
	/// </summary>
	public int[,] skillsInfoValues = new int[20,6]{
	{ 0, 1, 10, 10, 1000, 1000 },//передвиж
	{ 1, 1, 10, 20, 1000, 2000 },//передвиж по дорогам
	{ 3, 1, 10, 20, 1000, 5000 },//улучшенное охлаждение
	{ 4, 1, 10, 60, 1000, 5000 },//Ремонт
	{ 6, 1, 10, 660, 1000, 5000 },//Защита
	{ 7, 1, 10, 50, 1000, 1000 },//Защита от слизи
	{ 9, 1, 10, 30, 1000, 2000 },//Копание
	{ 13, 1, 10,60, 1000, 5000 },//Кристаллография
	{ 18, 1, 10, 220, 1000, 5000 },//Добыча
	{ 19, 1, 10, 50, 1000, 5000 },//Извлечение
	{ 20, 1, 10, 1000, 1000, 1000 },//Смежное извлечение
	{ 23, 1, 10, 1000, 1000, 5000 },//Сортировка
	{ 24, 1, 10, 1000, 1000, 5000 },//Дополнительные зеленые
	{ 28, 1, 10, 1000, 2000, 2000 },//Дополнительные фиолетовые
	{ 33, 1, 10, 0, 1000, 5000 },//Нано-упаковка
	{ 34, 1, 10, 0, 1000, 5000 },//Вместимость зелёных
	{ 38, 1, 10, 0, 1000, 2000 },//Вместимость фиолетовых
	{ 40, 1, 10, 0, 1000, 5000 },//Стройка опор
	{ 43, 1, 10, 0, 1000, 2000 },//Стройка зелёных блоков
	{ 46, 1, 10, 1000, 1000, 5000 }//Стройка дороги
	};


	/// <summary>
	///по иксу выбор скилла по иду, по игреку выбор : 0-Название,1-Описание,2-эффект,3-единица измерения.
	/// </summary>
	public string[,] skillsInfoText = new string[,] {
	/* 0 */	{ "Передвижение", "Позволяет боту двигаться быстрее.", "Скорость:"," км/ч"},
	/* 1 */	{ "Передвижение по дорогам", "Ускоряет перемещение по дорожным покрытиям.", "Множитель скорости:"," К скорости"},
	/* 2 */	{ "Охлаждение", "Повышает максимальную рабочую температуру для бота (+ 10 за ур.).", "Макс. Температура:"," градусов"},
	/* 3 */	{ "Улучшенное охлаждение", "Повышает максимальную рабочую температуру для бота (+ 30 за ур.).", "Макс. Температура:"," градусов"},
	/* 4 */	{ "Ремонт", "Позволяет боту восстанавливать HP", "HP за операцию:",""},

	/* 5 */	{ "Геология", "Дает возможность забирать в инвентарь некоторые породы", "Макс. кол-во:"," единиц"},
	/* 6 */	{ "Защита", "Повышает прочность корпуса бота", "Уровень защиты:","%"},
	/* 7 */	{ "Защита от слизи", "Понижает урон от слызи", "Уровень защиты:","%"},
	/* 8 */	{ "Защита от пушек", "Понижает урон от пушек", "Уровень защиты:","%"},
	/* 9 */	{ "Копание", "Позволяет разрушать большинство пород и блоков", "Вероятность разрушения за удар:","%"},

	/* 10 */{ "Аннигиляция", "Ускоряет разрушение песка", "Вероятность разрушения за удар:","%"},
	/* 11 */{ "Анти-блок", "Ускоряет разрушение квадроблоков", "Вероятность повреждения за удар:","%"},
	/* 12 */{ "Дробление", "Ускоряет разрушение булыжника", "Вероятность разрушения за удар:","%"},
	/* 13 */{ "Кристаллография", "Позволяет добывать кристаллы более эффективно", "Эффективность:","%"},
	/* 14 */{ "Деактивация", "Ускоряет разрушение слизи", "Вероятность разрушения за удар:","%"},

	/* 15 */{ "Деконструкция", "Ускоряет разрушение блоков", "Вероятность разрушения за удар:","%"},
	/* 16 */{ "Размагничивание", "Ускоряет разрушение железного песка", "Вероятность разрушения за удар:","%"},
	/* 17 */{ "Разрушение", "Ускоряет разрушение булыжников и пустых пород", "Вероятность разрушения за удар:","%"},
	/* 18 */{ "Добыча", "Увеличивает добычу кристаллов", "Макс. кристаллов:",""},
	/* 19 */{ "Извлечение", "Увеличивает добычу синих и зелёных", "Макс. кристаллов:",""},

	/* 20 */{ "Смежное извлечение", "Позволяет получать до. кристаллы (син->зел,зел->син)", "Макс. кристаллов:",""},
	/* 21 */{ "Промывание", "Позволяет добывать кристаллы из пустых и сыпучих пород", "Макс. кристаллов:",""},
	/* 22 */{ "Обнаружение", "позволяет получать все виды кристаллов из пустых пород", "Макс. кристаллов:",""},
	/* 23 */{ "Сортировка", "Позволяет получать доп кри другого цвета(гол->бел,фио->гол,белл->крас,крас->фио)", "Макс. кристаллов:",""},
	/* 24 */{ "Дополнительные зелёные", "Повышает добычу зелёных", "Прирост к добыче:",""},

	/* 25 */{ "Дополнительные синие", "Повышает добычу синих", "Прирост к добыче:",""},
	/* 26 */{ "Дополнительные крассные", "Повышает добычу крассных", "Прирост к добыче:",""},
	/* 27 */{ "Дополнительные белые", "Повышает добычу белых", "Прирост к добыче:",""},
	/* 28 */{ "Дополнительные фиолетовые", "Повышает добычу фиолетовых", "Прирост к добыче:",""},
	/* 29 */{ "Дополнительные голубые", "Повышает добычу голубых", "Прирост к добыче:",""},

	/* 30 */{ "Упаковка", "Повышает максимальный груз (+10 за ур.)", "Прирост к грузу:","Кг"},
	/* 31 */{ "Улучшенная упаковка", "Повышает максимальный груз (+50 за ур.)", "Прирост к грузу:","Кг"},
	/* 32 */{ "Гипер-упаковка", "Повышает максимальный груз (+100 за ур.)", "Прирост к грузу:","Кг"},
	/* 33 */{ "Нано-упаковка", "Повышает максимальный груз (+500 за ур.)", "Прирост к грузу:","Кг"},
	/* 34 */{ "Повышенная вместимость зелёных", "Увеличивает вместимость зелёных", "Прирост к грузу:","%"},

	/* 35 */{ "Повышенная вместимость синих", "Увеличивает вместимость синих", "Прирост к грузу:","%"},
	/* 36 */{ "Повышенная вместимость крассных", "Увеличивает вместимость крассных", "Прирост к грузу:","%"},
	/* 37 */{ "Повышенная вместимость белых", "Увеличивает вместимость белых", "Прирост к грузу:","%"},
	/* 38 */{ "Повышенная вместимость фиолетовых", "Увеличивает вместимость фиолетовых", "Прирост к грузу:","%"},
	/* 39 */{ "Повышенная вместимость голубых", "Увеличивает вместимость голубых", "Прирост к грузу:","%"},

	/* 40 */{ "Стройка опор", "Позволяет строить опоры", "цена постройки:","<color=#0f0> зелёных</color>"},
	/*  */	{ "Стройка квадро-блоков", "Позволяет строить квадро-блоки", "цена постройки:","<color=#fff> белых</color>"},
	/*  */	{ "Табличка", "Позволяет строить таблички", "цена постройки:","<color=#fff> белых</color>"},
	/*  */	{ "Стройка зелёных блоков", "Позволяет строить зелёные блоки", "цена постройки:","<color=#0f0> зелёных</color>"},
	/* 44 */{ "Стройка жёлтых блоков", "Позволяет строить жёлтые блоки", "цена постройки:","<color=#fff> белых</color>"},

	/* 45 */{ "Стройка крассных блоков", "Позволяет строить крассные блоки", "цена постройки:","<color=#f00> красных</color>"},
	/* 46 */{ "Стройка дорожного покрытия", "Позволяет строить дорожное покрытие", "цена постройки:","<color=#0f0> зелёных</color>"},

	};

	int currentSkillIndexInArray = -1;//переменная для хранения текущего открытого в панели скилла. Число равно индексу данных в массиве имеющихся скиллов

	CharacterDataScript characterDataScript;

	int characterLVL = 0;//уровень персонажа

	// Use this for initialization
	void Start () {

		characterDataScript = gameObject.GetComponent<CharacterDataScript>();

		canvasCurrentUpObj.SetActive(false);
		if (offline) {
			#region дебаговая херь для стат бота
			//for (int i = 0; i < 20; i++)//Симуляция получения данных с сервера
			//{
			//	skillsInfoValues[i, 0] = Random.Range(-3, 46 + 1);//айди скилла
			//	skillsInfoValues[i, 1] = Random.Range(1, 522);//Уровень скилла

			//	skillsInfoValues[i, 2] = Random.Range(1, 100);//Эффект скилла

			//	skillsInfoValues[i, 3] = Random.Range(0, 1000000);//Текущий набранный опыт
			//	if (i % 2 == 0)
			//	{
			//		skillsInfoValues[i, 4] = Random.Range(skillsInfoValues[i, 3], skillsInfoValues[i, 3] * 5);//Опыт для нового уровня
			//	}
			//	else
			//	{
			//		skillsInfoValues[i, 4] = skillsInfoValues[i, 3];//Опыт для нового уровня
			//	}
			//	skillsInfoValues[i, 5] = Random.Range(10000, 1000000000);//цена следующего уровня
			//}
			#endregion
		}
	}
	void Update () {

		CharacterLVLUpdate();

		if (GUIUPObj.activeSelf)
		{
			//Debug.Log("+");
			if (warningGUI.isActive)
			{
				if (warningGUI.buttonOKPressed) {
					ServerMessageDeleteSkill(0);
					skillsInfoValues[currentSkillIndexInArray, 0] = -1;
					warningGUI.GUISetActive(false);
				}
				else if (warningGUI.buttonUndoPressed)
				{
					warningGUI.GUISetActive(false);
				}
			}

			KeyControls();
			SkillsIconsUpdate();
		}
	}

	void SkillsIconsUpdate() {
		for (int i = 0; i < 20; i++)
		{
			if (skillsInfoValues[i, 0] >= 0)
			{
				buttonUPSkill[i].image.sprite = skillIcons[skillsInfoValues[i, 0]];
				textUPSkill[i].text = "" + skillsInfoValues[i, 1];
				panelForTextOfButtonUPSkill[i].SetActive(true);
			}
			else
			{
				buttonUPSkill[i].image.sprite = skillEmptyFrame;
				textUPSkill[i].text = "";
				panelForTextOfButtonUPSkill[i].SetActive(false);
			}
		}
	}

	void CharacterLVLUpdate() {
		characterLVL = 0;
		for (int i = 0; i < 20; i++)
		{
			if (skillsInfoValues[i, 0] >= 0)
			{
				characterLVL += skillsInfoValues[i, 1];
			}
		}
		textCharactLVl.text = characterLVL + " ур.";
	}

	//отобразить в панели инфо о текущем скилле
	public void ShowSkillInfo(int ButtonID) {

		if(skillsInfoValues[ButtonID, 0] >= 0)
		{
			canvasCurrentUpObj.SetActive(true);
			currentSkillIndexInArray = ButtonID;

			imageCurrentSkill.sprite = skillIcons[skillsInfoValues[ButtonID, 0]];
			textCurrentSkillNameAndLVL.text = skillsInfoText[skillsInfoValues[ButtonID, 0], 0] + ". Уровень: " + skillsInfoValues[ButtonID, 1];
			textCurrentSkillHint.text = skillsInfoText[skillsInfoValues[ButtonID, 0], 1];

			//инфо о эффекте скилла
			textCurrentSkillEffect.text = skillsInfoText[skillsInfoValues[ButtonID, 0], 2] + skillsInfoValues[ButtonID, 2] + skillsInfoText[skillsInfoValues[ButtonID, 0], 3];

			UPSkillStatusLine.fillAmount = (float)skillsInfoValues[ButtonID, 3] / (float)skillsInfoValues[ButtonID, 4];
			textCurrentUPSkillStatusLine.text = skillsInfoValues[ButtonID, 3] + "/" + skillsInfoValues[ButtonID, 4];

			//вывод цены и соответствующий цвет текста при возможности\или нет - покупки
			textCurrentSkillNextLVLPrice.text = "" + skillsInfoValues[ButtonID, 5];
			if (characterDataScript.moneyValue >= (ulong)skillsInfoValues[ButtonID, 5])
			{
				textCurrentSkillNextLVLPrice.color = new Color(0, 1, 0.03f);
			}
			else {
				textCurrentSkillNextLVLPrice.color = new Color(0.9f, 0.3f, 0);
			}

			if (skillsInfoValues[ButtonID, 3] == skillsInfoValues[ButtonID, 4])
			{
				buttonUPLVObj.SetActive(true);
			}
			else
			{
				buttonUPLVObj.SetActive(false);
			}
		}
	}

	public void GUISetActive(bool activeMode)
	{
		charactControlScript.QuitBuilding("D2");
		GUIUPObj.SetActive(activeMode);
		if (!activeMode) {
			warningGUI.GUISetActive(false);
			canvasCurrentUpObj.SetActive(false);
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

	public void UPGRADESKILL()
	{

	}

	//функция вызова окна предупреждения, если игрок хочет удалить умение
	public void DELETESKILL()
	{
		warningGUI.content.text = "Если вы удалите умение, его проресс и уровень будут УТЕРЯНЫ. Вы уверены, что хотите это сделать?";
		warningGUI.GUISetActive(true);
	}

	//сообщение серверу о удалении скилла на боте
	//отправляется номер ячейки, в которой был скилл
	void ServerMessageDeleteSkill(int IDSkillCell) {
		canvasCurrentUpObj.SetActive(false);

	}

}
