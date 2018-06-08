using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllProgramBlocksController : MonoBehaviour {

	public GameObject[] programElements = new GameObject[5];//Все элементы ячейки

	/// <summary>
	/// Режим отображения этой ячейки:
	/// WithoutParam,Func,If,Jmp,Label
	/// </summary>
	public ProgrElem currentProgElem = 0;//какой элемент должен отображаться в этой ячейке

	public void SetProgrElem(ProgrElem element) {
		currentProgElem = element;

		for (int i = 0; i < 5; i++)
		{
			if (currentProgElem == (ProgrElem)i) {
				programElements[i].SetActive(true);
			}
			else {
				programElements[i].SetActive(false);
			}
		}
	}

	void Start()
	{
		for (int i = 1; i < 5; i++) {
			programElements[i].SetActive(false);//выключение лишних элементов
		}
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
