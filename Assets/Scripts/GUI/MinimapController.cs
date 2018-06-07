using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour {

	public GameObject PanelMap;//ссылка на объект панели карты

	public Transform charactTransform;//координаты персонажа
	public ChankLoad ChankLoadScript;//ссылка на скрипт с хранящимся в нем массивом чанков
	public Image miniMapImage;//Ссылка на миникарту
	public Image mapImage;//Ссылка на карту
	public Texture2D texture2MiniMapBlocks;//ссылка на набор обозначений для миникарты

	Texture2D texture2MiniMap;//текстура для построения миникарты. Присваивается к текстуре Миникарты
	Texture2D texture2Map;//текстура для построения карты. Присваивается к текстуре карты
	Color[][] arrayMiniMapBlocks = new Color[44][];//Массив хранения цветов пикселей всех блоков

	byte[,] mapBuffer;//буфер карты. На карте будут обновляться тольео те части, где происходит изменение

	byte mapMode = 1;//0 - ни мини, ни просто карта. 1 - мини-карта. 2-карта

	int mapW;
	int mapH;

	// Use this for initialization
	void Start () {
		for (int i = 0; i < 44; i++)//Добавление всех блоков в отдельные массивы блоков
		{
			arrayMiniMapBlocks[i] = texture2MiniMapBlocks.GetPixels(i * 2,1,2,2);
		}

		texture2MiniMap = (Texture2D)miniMapImage.mainTexture;
		texture2Map = (Texture2D)mapImage.mainTexture;

		mapBuffer = new byte[texture2Map.width / 2, texture2Map.height / 2];
		for (int y = 0; y < texture2Map.height / 2; y++)
			for (int x = 0; x < texture2Map.width / 2; x++)
				mapBuffer[x, y] = 255;

				mapW = ChankLoadScript.chanks_X * 32;
		mapH = ChankLoadScript.chanks_Y * 32;
	}

	Vector3 mouseCurrentPos;//Позиция мышки в данный момент
	Vector3 mouseAnchor;//Позиция мышки в момент её нажатия
	Vector3 distance;//расстояние между точкой нажатия мышки и точкой, где она в данный момент
	Vector3 anchor;//Якорь, относительно которого идет сдвиг

	// Update is called once per frame
	void Update () {
		
		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			mouseAnchor = Input.mousePosition;
			//print(mouseAnchor);
		}
		if (Input.GetKey(KeyCode.Mouse0))
		{
			mouseCurrentPos = Input.mousePosition;
			distance = new Vector3(mouseCurrentPos.x - mouseAnchor.x, mouseCurrentPos.y - mouseAnchor.y, 0);
			//print(distance);
		}
		if (Input.GetKeyUp(KeyCode.Mouse0))
		{
			distance = new Vector3(0,0,0);
			anchor = (mouseCurrentPos - mouseAnchor) + anchor;
			//print("anchor = " + anchor);
		}
		if (Input.GetKeyUp(KeyCode.M)) {
			if (mapMode != 2)
			{
				mapMode = 2;
				anchor = new Vector3(0,0,0);
				PanelMap.SetActive(true);
				MapUpdate(ref texture2Map);
			}
			else {
				mapMode = 1;
				PanelMap.SetActive(false);
			}
		}
		if (mapMode == 1) {
			if (Time.frameCount % 2 == 0)
			{
				MapUpdate(ref texture2MiniMap);
			}
		}
		else if (mapMode == 2) {
			if (Time.frameCount % 10 == 0) {
				MapUpdate(ref texture2Map);
			}
		}
	}

	void MapUpdate(ref Texture2D mapOrMinimapTexture) {
		Vector3 charPos = charactTransform.position;
		if (mapMode == 2) {
			charPos = new Vector3(charactTransform.position.x - anchor.x - distance.x, charactTransform.position.y - anchor.y - distance.y, 0);
		}

		for (int y = 0; y < mapOrMinimapTexture.height / 2; y++)
			for (int x = 0; x < mapOrMinimapTexture.width / 2; x++)
			{
				int x2 = x - mapOrMinimapTexture.width / 4;
				int y2 = y - mapOrMinimapTexture.height / 4;

				int x3 = (int)charPos.x + x2;
				int y3 = (int)charPos.y + y2;

				if (x3 >= 0 && y3 >= 0 && x3 < mapW && y3 < mapH && ChankLoadScript.chankMap[x3 / 32, y3 / 32] != null)
				{
					byte block = ChankLoadScript.chankMap[x3 / 32, y3 / 32][x3 % 32, y3 % 32];
					//if (mapBuffer[x,y] != block) {
						mapOrMinimapTexture.SetPixels(x * 2, y * 2, 2, 2, arrayMiniMapBlocks[block]);
					//	mapBuffer[x, y] = block;
					//}
				}
				else
				{
					mapOrMinimapTexture.SetPixels(x * 2, y * 2, 2, 2, arrayMiniMapBlocks[0]);
					//mapBuffer[x, y] = 0;
				}
			}
		mapOrMinimapTexture.Apply();
	}

	public void Map(bool onOff)
	{
		if (onOff) {
			mapMode = 2;
			MapUpdate(ref texture2Map);
		}
		else {
			mapMode = 1;
		}
		PanelMap.SetActive(onOff);
	}
}
