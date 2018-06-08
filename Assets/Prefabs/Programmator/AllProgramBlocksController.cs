using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AllProgramBlocksController : MonoBehaviour {
	public static Programator programmatorref;//Ссылка на программатор

	public GameObject[] programElements = new GameObject[5];//Все элементы ячейки
	public InputField[] inputFields = new InputField[4];

	public int x;
	public int y;

	/// <summary>
	/// Режим отображения этой ячейки:
	/// WithoutParam,Func,If,Jmp,Label
	/// </summary>
	public ProgrElem currentProgElem = 0;//какой элемент должен отображаться в этой ячейке

	//Выбор программного элемента для отображения
	public void SetProgrElem(ProgrElem element,Sprite sprite) {


		for (int i = 0; i < 5; i++)
		{
			if (element == (ProgrElem)i) {
				programElements[i].SetActive(true);
				programElements[i].GetComponent<Image>().sprite = sprite;
			}
			else {
				programElements[i].SetActive(false);
			}
		}
	}

	public void SetProgrElem(int element, Sprite sprite)
	{
		for (int i = 0; i < 5; i++)
		{
			if (element == i)
			{
				programElements[i].SetActive(true);
				programElements[i].GetComponent<Image>().sprite = sprite;
			}
			else
			{
				programElements[i].SetActive(false);
			}
		}
	}

	//принимает сообщение про изменение данных про адрес
	public void AdrChange() {
		programmatorref.AdresIsChanged(x,y);
	}

	void Start()
	{
		//for (int i = 1; i < 5; i++) {
		//	programElements[i].SetActive(false);//выключение лишних элементов
		//}
	}
}

public enum ProgrElem
{
	WithoutParam,
	Func,
	If,
	Jmp,
	Label
}
