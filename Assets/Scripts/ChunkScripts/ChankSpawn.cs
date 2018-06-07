using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChankSpawn : MonoBehaviour
{

	public GameObject chank;

	GameObject ChankLoadobj;
	public Transform characterTransform;//нужен для получения координат персонажа на карте

	public bool[,] chankIsLoaded;//массив состояний чанков (для предотвращение дублирования чанков на одно и то же поле по несколько раз)
	
	//количество тайлмепов, используемых для построения видимой зоны
	int cacheTMValue = 12;

	//массив тайлмепов
	GameObject[] tileMaps;

	// Use this for initialization
	void Start()
	{
		ChanksTMInit();
	}

	// Update is called once per frame
	void Update()
	{
		ChankSet();
	}

	void ChanksTMInit() {
		tileMaps = new GameObject[cacheTMValue];
		for (int i = 0; i < cacheTMValue; i++)
		{
			tileMaps[i] = Instantiate(chank, new Vector3(0, 0, -20), Quaternion.identity);
		}

		ChankLoadobj = GameObject.Find("ChunkLoader");//ссылка на чанклоадер

		//размер карты в чанках
		int chanksX = ChankLoadobj.GetComponent<ChankLoad>().chanks_X;
		int chanksY = ChankLoadobj.GetComponent<ChankLoad>().chanks_Y;

		chankIsLoaded = new bool[chanksX, chanksY];

		for (int y = 0; y < chanksY; y++)
			for (int x = 0; x < chanksX; x++)
			{
				chankIsLoaded[x, y] = false;
			}
	}

	void ChankSet()
	{
		//размер карты в чанках
		int chanksX = ChankLoadobj.GetComponent<ChankLoad>().chanks_X;
		int chanksY = ChankLoadobj.GetComponent<ChankLoad>().chanks_Y;

		//координаты персонажа в чанках
		int cTPX = (int)characterTransform.position.x / 32;
		int cTPY = (int)characterTransform.position.y / 32;

		for (int x = cTPX - 1; x <= cTPX + 1; x++)
			for (int y = cTPY - 1; y <= cTPY + 1; y++)
			{
				if (x >= 0 && x < chanksX && y >= 0 && y < chanksY && !chankIsLoaded[x, y])
				{
					for (int i = 0; i < cacheTMValue; i++)
					{
						if (tileMaps[i].GetComponent<TileMap>().isActiveChank == false)
						{
							tileMaps[i].transform.position = new Vector3(x * 32 + 16, y * 32 + 16, 0);
							tileMaps[i].GetComponent<TileMap>().isActiveChank = true;
							tileMaps[i].GetComponent<TileMap>().UpdateTexture();
							chankIsLoaded[x, y] = true;
							break;
						}
					}
				}
			}
	}
}

