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
		currentProgElem = element;
		for (int i = 0; i < 5; i++)
		{
			if (element == (ProgrElem)i) {
				programElements[i].SetActive(true);
				gameObject.GetComponent<Image>().sprite = sprite;
				if (element > 0) {
					inputFields[i - 1].text = Vec2iToSTR(programmatorref.ProgramAdressesAndData[x,y + programmatorref.firstLine]);
				}
			}
			else {
				programElements[i].SetActive(false);
			}
		}
	}

	public void SetDebugMarker(bool marker)
	{
		if (marker) {
			gameObject.GetComponent<Image>().color = new Color(1, 0, 0);
		}
		else {
			gameObject.GetComponent<Image>().color = new Color(1, 1, 1);
		}
	}

	string Vec2iToSTR(Vec2i vec) {
		string sx;
		string sy;

		if (vec.x < 0) {;
			return "";
		}
		else  {
			if (vec.x < 10)
			{
				sx = string.Concat("0", vec.x.ToString());
			}
			else {
				sx = vec.x.ToString();
			}

			if (vec.y < 10)
			{
				sy = string.Concat("0", vec.y.ToString());
			}
			else
			{
				sy = vec.y.ToString();
			}

			return (sx + sy);
		}

	}

	public void SetProgrElem(int element, Sprite sprite)
	{
		currentProgElem = (ProgrElem)element;
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

	//принимает сообщение про изменение в строке в инпут фиелде
	//игнорирует изменения если это изменения программные (если клава разблочена, значит изменение из-за скролла)
	public void AdrChange() {
		if (!programmatorref.keyboardRead) {
			int adrx = 0;
			int adry = 0;

			string text = inputFields[(int)currentProgElem - 1].text;
			if (text.Length == 4)
			{
				adrx = int.Parse(text.Substring(0, 2));
				adry = int.Parse(text.Substring(2, 2));
			}
			programmatorref.AdresIsChanged(x, y, adrx, adry);
		}
	}

	public void keyReadBlock() {
		programmatorref.BlockKeyboard();
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
