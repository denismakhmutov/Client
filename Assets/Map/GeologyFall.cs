using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeologyFall : MonoBehaviour {

	public ChankLoad chankLoad;//ссылка на массив
	public Transform charactTransform;//
	Vector3 charactPos;
	public bool offline;
	public int time;

	//размер карты в блоках
	int mWidth;
	int mHeight;
	//размер карты в чанках
	int chunkWidth;
	int chunkHeight;

	// Use this for initialization
	void Start () {
		int chunkWidth = chankLoad.chanks_X;
		int chunkHeight = chankLoad.chanks_Y;
		mWidth = chunkWidth * 32;
		mHeight = chunkHeight * 32;
	}

	int fixUpCount = 0;
	//
	void FixedUpdate() {
		if (offline)
		{
			if (fixUpCount % time == 0)
			{
				charactPos = charactTransform.position;

				//чанк, в котором находится бот
				int xChunk = (int)charactPos.x / 32;
				int yChunk = (int)charactPos.y / 32;

				for (int y = yChunk - 1; y < yChunk + 2; ++y)
					for (int x = xChunk - 1; x < xChunk + 2; ++x)
					{
						ChunkFalling(x * 32, y * 32);
					}
			}
			++fixUpCount;
		}
	}

	//падение сыпучки в пределах одного чанка
	void ChunkFalling(int startX,int startY) {
		int endX = startX + 32;
		int endY = startY + 32;

		for (int y = startY; y < endY; ++y)
			for (int x = startX; x < endX; ++x) {

				int block = chankLoad.chankMap[x / 32, y / 32][x % 32, y % 32];//ид блока,чтобы реже использовать это обращение к массиву
				//18-26 - сыпучие породы
				//1-5 - земля и покрытия
				if (block > 17 && block < 27 && Random.Range(0,4)!=0) {

					//чтобы не расчитывать координату лярд раз
					int blockBelow = ChankMapVal(x, y - 1);//блок ниже

					if (blockBelow == 1)
					{//если блок ниже это пустота
						ChankMapValChange(x, y, 1);
						ChankMapValChange(x, y - 1, block);
					}
					else if (blockBelow < 18 || blockBelow > 26)
					{//если блок ниже не сыпучка
					 //забить хер
					}
					//если по сторонам есть куда сыпаться
					else if (ChankMapVal(x - 1, y - 1) == 1)
					{//если блок левей пустует, идет проверка и на правый, чтобы выбрать, куда падать
						if (ChankMapVal(x + 1, y - 1) == 1)
						{//если и правый равен пустоте

							if (Random.Range(0, 2) == 0)//выбор куда падатб
							{
								ChankMapValChange(x, y, 1);
								ChankMapValChange(x - 1, y - 1, block);
							}
							else
							{
								ChankMapValChange(x, y, 1);
								ChankMapValChange(x + 1, y - 1, block);
							}
						}
						else
						{//если нехуя не равен
							ChankMapValChange(x, y, 1);
							ChankMapValChange(x - 1, y - 1, block);
						}
					}
					else if(ChankMapVal(x + 1, y - 1) == 1)
					{//если левый не пустует
						ChankMapValChange(x, y, 1);
						ChankMapValChange(x + 1, y - 1, block);
					}
				}
			}
	}

	//ChankMapVal(,)
	//получение значения из массива
	int ChankMapVal(int x, int y)
	{
		return chankLoad.chankMap[x / 32, y / 32][x % 32, y % 32];
	}
	//установка значения в массив(должен быть запрос на сервер)
	void ChankMapValChange(int x, int y, int Value)
	{
		chankLoad.chankMap[x / 32, y / 32][x % 32, y % 32] = (byte)Value;//(byte)Value;
	}

}
