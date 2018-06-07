using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Models;
using Newtonsoft.Json;

public class ChankLoad : MonoBehaviour {
    public NetHelper nethelper;
	//количество чанков на X и Y карты. Должны принимать значения с сервера
	public static int chanksX = 64;
	public static int chanksY = 64;
    
	[System.NonSerialized]
	public int chanks_X = chanksX;
	[System.NonSerialized]
	public int chanks_Y = chanksY;
    public string chunk;//УДАЛИТЬ
	[Space]
	public bool offline = false;
	[Space]
	public Texture2D mapMainTexture;//Карта мира, пока загрузка со спрайта

	//Массив чанков 32:32. Используется для более экономного использования памяти, чем если бы использовался массив с заранее определенными ячейками,
	//т.к. многие всё равно не будут использованы.
	public byte[,][,] chankMap = new byte[chanksX, chanksY][,];

	//Массив списков зданий, установленных в чанки с соответствующими координатами
	public List<BuildingData>[,] chankBuildingsDataArray = new List<BuildingData>[chanksX, chanksY];
	

	void Awake() {
		//MapRandomizer();
		if (offline) {
			MapLoadFromFile();
		}
		else {
			//в этом случае всем занимается нэт хелпер
		}
	}

	// Use this for initialization
	void Start () {
		chankBuildingsDataArray[9, 9] = new List<BuildingData>();
		chankBuildingsDataArray[9, 9].Add(new BuildingData() { buildingID = 0, x = 301, y = 313, packType = 0 });//лека
		chankBuildingsDataArray[9, 9].Add(new BuildingData() { buildingID = 1, x = 306, y = 313, packType = 1 });//респ

		chankBuildingsDataArray[9, 9].Add(new BuildingData() { buildingID = 5, x = 316, y = 314, packType = 4,
			paramIArray = new uint[] {5000,50000,5000,50000,50,5}/*кристаллы*/,paramLArray = new ulong[] {10000000000, 100} });//склад

		chankBuildingsDataArray[10, 9] = new List<BuildingData>();
		chankBuildingsDataArray[10, 9].Add(new BuildingData() { buildingID = 2, x = 342, y = 311, packType = 2 });//маркет
		chankBuildingsDataArray[10, 9].Add(new BuildingData() { buildingID = 3, x = 325, y = 311, packType = 3 });//Ап
		chankBuildingsDataArray[10, 9].Add(new BuildingData() { buildingID = 4, x = 320, y = 314, packType = 4,
			paramIArray = new uint[] {2500000,255,2555,25555,23121,4321}/*кристаллы*/,paramLArray = new ulong[] {10000000, 100} });//склад
		chankBuildingsDataArray[10, 9].Add(new BuildingData() { buildingID = 6, x = 320, y = 317, packType = 4,
			paramIArray = new uint[] {0,0,0,0,0,0}/*кристаллы*/,paramLArray = new ulong[] {0, 0} });//склад
	}

	// Update is called once per frame
	void Update () {
		//MapRandomizer();
	}
    public void bag()
    {
        //Debug.Log(pos.X);
        Debug.Log(JsonConvert.SerializeObject(chankMap[10, 9]));
    }

	/// <summary>Возвращает ID блока в указанных координатах. Если чанк не прогружен, возвращается -1</summary>
	public int ChunkMapVal(int x, int y)
	{
		if (chankMap[x / 32, y / 32] != null) {
			return chankMap[x / 32, y / 32][x % 32, y % 32];
		}
		else {
			return -1;
		}
	}
	/// <summary>Возвращает ID блока в указанных координатах. Если чанк не прогружен, возвращается -1</summary>
	public int ChunkMapVal(Vec2i vector)
	{
		if (chankMap[vector.x / 32, vector.y / 32] != null)
		{
			return chankMap[vector.x / 32, vector.y / 32][vector.x % 32, vector.y % 32];
		}
		else
		{
			return -1;
		}
	}
	/// <summary>Возвращает ID блока в указанных координатах. Если чанк не прогружен, возвращается -1</summary>
	public int ChunkMapVal(Vector3 vector)
	{
		if (chankMap[(int)vector.x / 32, (int)vector.y / 32] != null)
		{
			return chankMap[(int)vector.x / 32, (int)vector.y / 32][(int)vector.x % 32, (int)vector.y % 32];
		}
		else
		{
			return -1;
		}
	}

	//отладочная функция для видимости обновления чанка
	void MapRandomizer() {
		for (int y = 0; y < chanksY; y++)
			for (int x = 0; x < chanksX; x++)
			{
				chankMap[x, y] = new byte[32, 32];
				for (int y2 = 0; y2 < 32; y2++)
					for (int x2 = 0; x2 < 32; x2++)
						chankMap[x, y][x2, y2] = (byte)Random.Range(1, 5 + 1);
			}
	}

	void MapLoadFromFile() {

		Color[,] colorIDs = new Color[chanksX * 32, chanksY * 32];

		for (int y = 0; y < (chanksY * 32); y++)
			for (int x = 0; x < (chanksX * 32); x++)
				colorIDs[x, y] = mapMainTexture.GetPixel(x, y);

		for (int y = 0; y < chanksY; y++)
			for (int x = 0; x < chanksX; x++)
			{
				chankMap[x, y] = new byte[32, 32];
				for (int y2 = 0; y2 < 32; y2++)
					for (int x2 = 0; x2 < 32; x2++)
						chankMap[x, y][x2, y2] = (byte)(colorIDs[x * 32 + x2,y * 32 + y2].b * 256);
			}
	}
}
