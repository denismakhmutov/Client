﻿using Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIItemsController : MonoBehaviour {

	//В ЛИБУ/////////////////////////////////////////////////////////////////////////////
	public class Vec2 {
		float x;
		float y;

		public Vec2(float x, float y)
		{
			this.x = x;
			this.y = y;
		}
	}
	public class Vec2i
	{
		float x;
		float y;

		public Vec2i(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
	}
	public struct PersonInfo {

		public int[][] skillsInfoArray;//массив скиллов персонажа
		//координаты игрока в мире
		public Vec2i Coord;

		public ulong Money;
		public ulong Credits;

		public ushort HP;

		public int[] Cristalls;
		public int corp;

		public Vec2i respCoords;
		public int respID;

		public float[] skillsProperties;

		public short[] skinsArray;
		public bool modsArray;
		public bool[] charactSettings;
		public bool GodMode;
		public bool ModerMode;
		public uint bannedFromChat;
		public uint bannedFromGame;

		public List<Vec2i>[] charactBuildingsList;//список координат зданий игрока для упрощения поиска их по миру 
		public List<ConstructData>[] characterInventoryList;//массив предметов игрока

	}
	public class FramePattern
	{
		public byte[,] pattern;
		public Vec2i angle;

		public FramePattern(byte[,] pattern, Vec2i angle)
		{
			this.pattern = pattern;
			this.angle = angle;
		}
	}
	FramePattern[] framePattern = new FramePattern[5];
	void FramePatternFilling() {
		//0-не трогать,1 - фрейм здания, 2 - запретная зона

		//УЧИТЫВАТЬ, ЧТО В МАССИВАХ ОТОБРАЖЕНЫ СХЕМЫ, РАЗВЕРНУТЫЕ НА 90 ГРАДУСОВ ПО ЧАСОВОЙ СТРЕЛКЕ

		//лека, респаун,магазин бумов и диззов
		framePattern[0].pattern = new byte[4, 3] {
			{1,1,1},
			{1,2,1},
			{1,2,1},
			{0,2,0},
		};
		framePattern[0].angle = new Vec2i(-1,-1);
		
		//склад
		framePattern[1].pattern = new byte[3, 3] {
			{0,1,1},
			{2,2,1},
			{0,1,1},
		};
		framePattern[1].angle = new Vec2i(-1, -1);

		//Маркет, Пакс, Репакс, Экспрес
		framePattern[2].pattern = new byte[7, 7] {
			{0,0,0,2,0,0,0},
			{0,0,1,2,1,0,0},
			{0,1,1,2,1,1,0},
			{2,2,2,2,2,2,2},
			{0,1,1,2,1,1,0},
			{0,0,1,2,1,0,0},
			{0,0,0,2,0,0,0},
		};
		framePattern[2].angle = new Vec2i(-3, -3);

		//Ворота положение 1
		framePattern[3].pattern = new byte[3, 3] {
			{1,0,1},
			{1,1,1},
			{1,0,1},
		};
		framePattern[3].angle = new Vec2i(-1, -1);

		//Ворота положение 2
		framePattern[4].pattern = new byte[3, 3] {
			{1,1,1},
			{0,1,0},
			{1,1,1},
		};
		framePattern[4].angle = new Vec2i(-1, -1);

		//Пушка
		framePattern[5].pattern = new byte[5, 5] {
			{0,0,2,0,0},
			{0,1,2,1,0},
			{2,2,2,2,2},
			{0,2,1,2,0},
			{0,0,2,0,0},
		};
		framePattern[5].angle = new Vec2i(-2, -2);

		//АП, МОДС
		framePattern[6].pattern = new byte[3, 5] {
			{0,1,1,1,1},
			{2,2,2,1,1},
			{0,1,1,1,1},
		};
		framePattern[6].angle = new Vec2i(-1, -2);

		//Скинс
		framePattern[7].pattern = new byte[3, 4] {
			{0,1,1,1},
			{2,2,2,1},
			{0,1,1,1},
		};
		framePattern[7].angle = new Vec2i(-1, -2);

		//Башня клана
		framePattern[8].pattern = new byte[7, 7] {
			{0,0,0,2,0,0,0},
			{0,1,1,2,1,1,0},
			{0,1,1,2,1,1,0},
			{2,2,2,2,2,2,2},
			{0,1,1,2,1,1,0},
			{0,1,1,2,1,1,0},
			{0,0,0,2,0,0,0},
		};
		framePattern[8].angle = new Vec2i(-3, -3);

		//Фед здание
		framePattern[9].pattern = new byte[9,5] {
			{1,1,1,0,0},
			{1,1,1,1,0},
			{1,1,1,1,1},
			{2,2,1,1,1},
			{2,2,2,2,1},
			{2,2,1,1,1},
			{1,1,1,1,1},
			{1,1,1,1,0},
			{1,1,1,0,0},
		};
		framePattern[9].angle = new Vec2i(-3, -4);

	}
	/////////////////////////////////////////////////////////////////////////////

	public Text[] textItemValues = new Text[9];
	public Button[] buttonItems = new Button[9];
	public GameObject[] buttonItemsObj = new GameObject[9];
	[Space]

	public Texture[] textureItems = new Texture[25];//Убрать, если не понадобится. Мне лень будет заново писать
	[Space]

	public Sprite[] spriteItems = new Sprite[26];//спрайты всех итемов в игре для вывода в иконку кнопки
	[Space]
	public Button buttonNext;
	public Button buttonPrev;
	public Text textPage;
	[Space]
	public GameObject panelGUICharacterItemInfo;//панель, на которой отображается инфо о выбранном предмете
	public Text textNameSelectObj;
	public Image imageSelectObj;
	public Text textSelectObjType;
	public Text textSelectObjIsReusable;
	public Text textSelectObjInform;
	public Text textSelectObjInstallationHint;
	[Space]

	int currentPage = 1;//текущая страница, открытая в панели
	int pagesCount = 1;//кол-во страниц в списке паков игрока

	int packsTypesOnCharacterCount = 0;//кол-во имеющихся у игрока предметов
	[System.NonSerialized]
	public int packsTypesCount = 26;//кол-во существующих в игре паков и предметов

	List<ConstructData> [] characterInventoryList;//массив списков структур имеющихся паков

	byte[] typesAvailable;//массив типов паков, которые присутствуют у игрока. Костыль для обхода многоцикловой проверки на ноль в списках

	// Use this for initialization
	void Start () {

		characterInventoryList = new List<ConstructData>[packsTypesCount];
		typesAvailable = new byte[packsTypesCount];

		for (byte i = 0; i < packsTypesCount; i++)//симуляция внесения списка из сервера
		{
			characterInventoryList[i] = new List<ConstructData>();

			for (byte j = 0; j < Random.Range(0,3); j++) {
				characterInventoryList[i].Add(new ConstructData());
			}

			//Debug.Log("index in array:" + i);
			//Debug.Log("objects in current list:" + characterInventoryList[i].Count);
		}
	}
	
	// Update is called once per frame
	void Update () {
		UpdateGUI();
	}

	void UpdateGUI() {
		//Обновление списка существующих паков
		packsTypesOnCharacterCount = 0;
		for (int i = 0; i < packsTypesCount; i++)
		{
			if (characterInventoryList[i].Count > 0)
			{
				typesAvailable[packsTypesOnCharacterCount] = (byte)i;
				packsTypesOnCharacterCount++;
			}
		}

		//вывод списка в игровое поле
		for (int i = (currentPage - 1) * 9; (i < ((currentPage - 1) * 9 + 9)); i++)
		{
			if (i < packsTypesOnCharacterCount)
			{
				buttonItems[i % 9].image.sprite = spriteItems[typesAvailable[i]];

				textItemValues[i % 9].text = characterInventoryList[typesAvailable[i]].Count.ToString();

				buttonItemsObj[i % 9].SetActive(true);
			}
			else
			{
				buttonItemsObj[i % 9].SetActive(false);
			}
		}

		textPage.text = "Стр:" + currentPage + "/" + ((int)((float)packsTypesOnCharacterCount / 9f + 0.9f));
	}
	
	//событие нажатия кнопки ВПЕРЁД
	public void PageNext() {
		if( currentPage < (int)((float)packsTypesOnCharacterCount / 9f + 0.9f)) currentPage++;
	}
	//событие нажатия кнопки НАЗАД
	public void PagePrev()
	{
		if (currentPage > 1) currentPage--;
	}

	//информация про все паки и предметы инвентаря
	string[,] informationAboutPacks = new string[,] {
		{"Ремонтная станция"    ,"Восстанавливает корпус робота потребляя красные кри"     ,"hp/R:"},
		{"Респаун"              ,"Бот появится на нем после его уничтожения"                ,"B/Resp"},
		{"Рынок"                ,"В нем можно продать или купить кристаллы"                 ,"% комиссии:"},
		{"Магазин улучшений"    ,"Здание, где можно прокачать твои умения за деньги"        ,"Код умений:"},
		{"Склад"                ,"В нём можно хранить кристаллы, породы и строй-пакеты"     ,"Слотов под предметы:"},
		{"Пушка"                ,"Наносит урон врагам корпорации"                           ,"C/h:"},
		{"Телепорт"             ,"Телепортирует бота к площадке, к которой привязан"        ,"V/tp"},
		{"Фед.ТП"               ,"ты не должен видеть этот текст"                           ,""},
		{"Площадка"             ,"Используется как точка привязки для телепортов"           ,"макс.Глубина:"},
		{"Магазин скинов"       ,"В нём можно купить скины на вашего бота за кредиты"       ,""},
		{"RePack"               ,"репак"                                                    ,""},
		{"RatingPack"           ,"рейтинг пак"                                              ,""},
		{"Магазин паков"        ,"В нем можно покупать здания"                              ,""},
		{"PacksExpress"         ,""                                                         ,""},
		{"NickNamePack"         ,""                                                         ,""},
		{"Ворота"               ,"Пропускают только ботов своей корпорации"                 ,""},
		{"FIN"                  ,""                                                         ,""},
		{"FedPack"              ,""                                                         ,""},
		{"ExchangeFloor"        ,""                                                         ,""},
		{"Магазин диззов"       ,"В нём можно купить диззасемблер"                          ,""},
		{"Диззасемблер"         ,"Им можно собрать своё здание в пак"                       ,""},
		{"Башня корпорации"     ,"С его помощью можно создать свою корпорацию"              ,""},
		{"Обнаружитель боксов"  ,"мда"                                                      ,"Радиус:"},
		{"Магазин бомб"         ,"В нём можно купить бомбы за кредиты"                      ,"Бомб в сутки:"},
		{"Бойген"               ,"Повышает урон пушки"										,"Коэф.Умножения:"},
		{"Бомба"                ,"Позволяет взрывать скалу"                                 ,"Радиус:"},
		};

	//событие нажатия одной из кнопок панели паков
	public void PackSelection(int IDKey)
	{
		panelGUICharacterItemInfo.SetActive(!panelGUICharacterItemInfo.activeSelf);

		imageSelectObj.sprite = spriteItems[typesAvailable[(currentPage - 1) * 9 + IDKey]];
		textNameSelectObj.text = informationAboutPacks[typesAvailable[(currentPage - 1) * 9 + IDKey], 0];
		textSelectObjInform.text = informationAboutPacks[typesAvailable[(currentPage - 1) * 9 + IDKey], 1];
		textSelectObjIsReusable.text = informationAboutPacks[typesAvailable[(currentPage - 1) * 9 + IDKey], 2];

	}
}

/*
ПАРАМЕТРЫ ПЕРСОНАЖА(расчитанные на основе его скиллов)
Массив будет расширяться по мере нужні в новіх параметрах
//0//Скорость перемещения на бездорожии (учет скилла перемещение)
//1//Скорость перемещения на дороге (скилл перемещение + перемещение на дорогах)
//2//
//3//Минимальная возможная добыча с кристалла(скилл кристалка повышает минимальное значение в скиле добыча)

//4//Добыча ЗЕЛЕНЫХ из ЗЕЛЕНЫХ целое значение ((инт)добыча + (инт)извлечение + (инт)добыча ЗЕЛЕНЫХ)
//5//Добыча ЗЕЛЕНЫХ из ЗЕЛЕНЫХ нецелое значение ( (мантисса)добыча + (мантисса)извлечение + (мантисса)добыча ЗЕЛЕНЫХ)
//6//Добыча ЗЕЛЕНЫХ из СИНИХ целое значение ((инт)извлечение)
//7//Добыча ЗЕЛЕНЫХ из СИНИХ нецелое значение ((мантисса)извлечение)

//8//Добыча СИНИХ из СИНИХ целое значение ((инт)добыча + (инт)извлечение + (инт)добыча СИНИХ)
//9//Добыча СИНИХ из СИНИХ нецелое значение ((мантисса)добыча + (мантисса)извлечение + (мантисса)добыча СИНИХ)
//10//Добыча СИНИХ из ЗЕЛЕНЫХ целое значение ((инт)извлечение)
//11//Добыча СИНИХ из ЗЕЛЕНЫХ нецелое значение ((мантисса)извлечение)

//12//Добыча КРАСНЫХ из КРАСНЫХ целое значение ((инт)добыча + (инт)добыча КРАСНЫХ)
//13//Добыча КРАСНЫХ из КРАСНЫХ нецелое значение ((мантисса)добыча + (мантисса)добыча КРАСНЫХ)
//14//Добыча КРАСНЫХ из БЕЛЫХ целое значение ((инт)сортировка)
//15//Добыча КРАСНЫХ из БЕЛЫХ нецелое значение ((мантисса)сортировка)

//16//Добыча БЕЛЫХ из БЕЛЫХ целое значение ((инт)добыча + (инт)добыча БЕЛЫХ)
//17//Добыча БЕЛЫХ из БЕЛЫХ нецелое значение ((мантисса)добыча + (мантисса)добыча БЕЛЫХ)
//18//Добыча БЕЛЫХ из ГОЛУБЫХ целое значение ((инт)сортировка)
//19//Добыча БЕЛЫХ из ГОЛУБЫХ нецелое значение ((мантисса)сортировка)

//20//Добыча БЕЛЫХ из БЕЛЫХ целое значение ((инт)добыча + (инт)добыча БЕЛЫХ)
//21//Добыча БЕЛЫХ из БЕЛЫХ нецелое значение ((мантисса)добыча + (мантисса)добыча БЕЛЫХ)
//22//Добыча БЕЛЫХ из ГОЛУБЫХ целое значение ((инт)сортировка)
//23//Добыча БЕЛЫХ из ГОЛУБЫХ нецелое значение ((мантисса)сортировка)

//24//Добыча ФИОЛЕТОВЫХ из ФИОЛЕТОВЫХ целое значение ((инт)добыча + (инт)добыча ФИОЛЕТОВЫХ)
//25//Добыча ФИОЛЕТОВЫХ из ФИОЛЕТОВЫХ нецелое значение ((мантисса)добыча + (мантисса)добыча ФИОЛЕТОВЫХ)
//26//Добыча ФИОЛЕТОВЫХ из КРАСНЫХ целое значение ((инт)сортировка)
//27//Добыча ФИОЛЕТОВЫХ из КРАСНЫХ нецелое значение ((мантисса)сортировка)

//28//Добыча ГОЛУБЫХ из ГОЛУБЫХ целое значение ((инт)добыча + (инт)добыча ГОЛУБЫХ)
//29//Добыча ГОЛУБЫХ из ГОЛУБЫХ нецелое значение ((мантисса)добыча + (мантисса)добыча ГОЛУБЫХ)
//30//Добыча ГОЛУБЫХ из ФИОЛЕТОВЫХ целое значение ((инт)сортировка)
//31//Добыча ГОЛУБЫХ из ФИОЛЕТОВЫХ нецелое значение ((мантисса)сортировка)

//32//Добыча ОРАНЖЕВЫХ кристаллов из ОРАНЖЕВЫХ булыжников
//33//
//34//
//35//

//36//Добыча ЖЕЛТЫХ кристаллов из ЖЕЛТЫХ булыжников
//37//
//38//
//39//

//40//Добыча ЗЕЛЕНЫХ или СИНИХ кристаллов с БУЛЫЖНИКА целая часть((инт)обнаружение)
//41//Добыча ЗЕЛЕНЫХ или СИНИХ кристаллов с БУЛЫЖНИКА нецелая часть((мант)обнаружение)
//42//Добыча ЗЕЛЕНЫХ или СИНИХ кристаллов с ПЕСКОВ целая часть((инт)обнаружение) - для песков мантисса не учитывается
//43//
//44//
//45//
//46//
//47//
//48//
//49//
//50//Максимальный урон от золотой,пустой скалы (пока что просто брать значение по умолчанию)
//51//Максимальный урон от булыжника (пока что просто брать значение по умолчанию)
//52//Максимальный урон от обычного песка (пока что просто брать значение по умолчанию)
//53//Максимальный урон от синего песка (пока что просто брать значение по умолчанию)
//54//Максимальный урон от слизи (значение по умолчанию деленное на защиту от слизи)
//55//Максимальный урон от пушки (значение по умолчанию деленное на защиту от пушек)
//56//Максимальный урон от кристалла или живки (пока что просто брать значение по умолчанию)
//57//
//58//
//59//
//если прочность блоков равняется нулю, значит они не доступны игроку
//на счет изменения цен на блоки я пока не решил. лучше пусть всегда стоят одинаково.Деф должен быть дорогим
//60//Прочность ЗЕЛЕНЫХ блоков
//61//Прочность ЖЕЛТЫХ блоков
//62//Прочность КРАСНЫХ блоков
//63//Прочность Опор
//64//Прочность Квадро-блоков
//65//Прочность дорожного покрытия или его цена, пока хз
//66//
//67//
//68//
//69//
//70//Эффективность разрушения золотой, пустой скалы (расчет по всем скиллам разрушения и копе)
//71//Эффективность разрушения живых кристаллов (зависит от копы)
//72//Эффективность разрушения кристаллов (пока не надо. Промахи на кристаллах не нужны, как по мне, и так жестко с эффективностью)
//73//Эффективность разрушения зеленых блоков и выше
//74//Эффективность разрушения опор и выше
//75//Эффективность разрушения дорожного покрытия (зависит от копы и мода Разрушение покрытий)
//76//Эффективность разрушения булыжников
//77//
//78//
//79//
//80//Грузоподъёмность (сумма всех скиллов, дающих груз + 200 изначального груза)
//Груз кристаллов перемножается на процент прироста кристаллов в скиллах груза на отдельные кристаллы
//81//Грузоподъёмность ЗЕЛЕНЫХ
//82//Грузоподъёмность СИНИХ
//83//Грузоподъёмность КРАСНЫХ
//84//Грузоподъёмность БЕЛЫХ
//85//Грузоподъёмность ФИОЛЕТОВЫХ
//86//Грузоподъёмность ГОЛУБЫХ
//87//Грузоподъёмность ОРАНЖЕВЫХ
//88//Грузоподъёмность ЖЁЛТЫХ
////
////
*/