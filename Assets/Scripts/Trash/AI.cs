using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour {

	public ChankLoad chankLoad;//ссылка на чанклоадер

	// Use this for initialization
	void Start () {
		chankLoad = GameObject.Find("ChunkLoader").GetComponent<ChankLoad>();//ссылка на чанклоадер
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		for (int i = 0;i < 10000; i++) {
			transform.position = new Vector3(((int)transform.position.x) + 0.5f, ((int)transform.position.y) + 0.5f, -5);

			int x = (int)transform.position.x;
			int y = (int)transform.position.y;
			byte block;

			switch (Random.Range(0, 4))
			{
				case 0:
					block = (byte)ChankMapVal(x, y + 1);
					//Debug.Log(0);
					if (block < 6 || (block != 6 && block != 7 && block != 8 && block != 28 && block != 41 && block != 42))
					{
						ChankMapValChange(x, y + 1, 1);
						transform.position = new Vector3((int)transform.position.x, (int)transform.position.y + 1, -5);
					}
					; break;//верх
				case 1:
					block = (byte)ChankMapVal(x, y - 1);
					//Debug.Log(1);
					if (block < 6 || (block != 6 && block != 7 && block != 8 && block != 28 && block != 41 && block != 42))
					{
						ChankMapValChange(x, y - 1, 1);
						transform.position = new Vector3((int)transform.position.x, (int)transform.position.y - 1, -5);
					}
					; break;//низ
				case 2:
					block = (byte)ChankMapVal(x + 1, y);
					//Debug.Log(2);
					if (block < 6 || (block != 6 && block != 7 && block != 8 && block != 28 && block != 41 && block != 42))
					{
						ChankMapValChange(x + 1, y, 1);
						transform.position = new Vector3((int)transform.position.x + 1, (int)transform.position.y, -5);
					}
					; break;//право
				case 3:
					block = (byte)ChankMapVal(x - 1, y);
					//Debug.Log(3);
					if (block < 6 || (block != 6 && block != 7 && block != 8 && block != 28 && block != 41 && block != 42))
					{
						ChankMapValChange(x - 1, y, 1);
						transform.position = new Vector3((int)transform.position.x - 1, (int)transform.position.y, -5);
					}
					; break;//лево
			}

			//Debug.Log(ChankMapVal(x,y));
		}
	}

	//функции для упрощения обращения в 4-мерный массив чанков
	//получение значения из массива
	int ChankMapVal(int x, int y)
	{
		return chankLoad.chankMap[x / 32, y / 32][x % 32, y % 32];
	}
	void ChankMapValChange(int x, int y, int Value)
	{
		chankLoad.chankMap[x / 32, y / 32][x % 32, y % 32] = (byte)Value;
	}
}
