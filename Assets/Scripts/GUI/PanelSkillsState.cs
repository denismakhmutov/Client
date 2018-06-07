using UnityEngine;
using UnityEngine.UI;

public class PanelSkillsState : MonoBehaviour {

	[SerializeField]
	Image[] iconSkillToUP = new Image[21];//имейджи всех прокачанных скиллов, готовых для апгрейда
	[SerializeField]
	GameObject[] skillPanel = new GameObject[8];//Ссылка на объекты панелек со скиллами, которые еще не прокачаны(для выключения их на сцене)
	GUIUPController guiUPController;//ссылка на контроллер, в котором хранится инфа о уровнях и иконки скиллов
	[SerializeField]
	Scrollbar scrollbar;//скроллбар для скрола по непрокачанным скиллам
	[SerializeField]
	GameObject scrollbarObj;//ссылка на скроллбар для его скрытия
	[SerializeField]
	Image[] imageIcon = new Image[8];//иконки скиллов в панельках
	[SerializeField]
	Image[] statusLine = new Image[8];//статус прокачки скиллов
	[SerializeField]
	Text[] textPanelSkill = new Text[8];//название и % прокачки скилла

	public int[] noUPSkillsArray = new int[20];//массив с отобранными скиллами без прокачки(айдишники непрокачанных)
	public int[] upSkillsArray = new int[20];//массив с отобранными скиллами прокачки(айдишники прокачанных)

	// Use this for initialization
	void Start () {
		guiUPController = gameObject.GetComponent<GUIUPController>();

		for (int i = 0; i < 21; i++)
		{
			iconSkillToUP[i].color = new Color(1, 1, 1, 0);
		}
		for (int i = 0; i < 8; i++)
		{
			skillPanel[i].SetActive(false);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.frameCount % 10 == 0) {
			int startSkill = 0;//с какого скилла начинать вывод в 8 полей прокачки
			int noUPSkillsCount = 0;//кол-во не прокачанных скиллов
			int upSkillsCount = 0;//кол-во прокачанных скиллов

			int arrayNoUpIter = 0;//итератор для массива айдишников не апнутых скиллов. нужен, чтобы исключить все пустые или прокачанные ячейки
			int arrayUpIter = 0;//итератор для массива айдишников апнутых скиллов. нужен, чтобы исключить все пустые или непрокачанные ячейки
			for (int i = 0; i < 20; i++)
			{
				if (guiUPController.skillsInfoValues[i, 0] >= 0) {
					if (guiUPController.skillsInfoValues[i, 3] < guiUPController.skillsInfoValues[i, 4])
					{
						++noUPSkillsCount;
						noUPSkillsArray[arrayNoUpIter] = i;
						++arrayNoUpIter;
					}
					else {
						++upSkillsCount;
						upSkillsArray[arrayUpIter] = i;
						++arrayUpIter;
					}
				}
			}
			//обновление прокачанных скиллов
			for (int i = 0; i < 21; i++)
			{
				if (upSkillsCount > i)
				{
					iconSkillToUP[i].color = new Color(1, 1, 1, 1);
					iconSkillToUP[i].sprite = guiUPController.skillIcons[guiUPController.skillsInfoValues[upSkillsArray[i], 0]];
				}
				else {
					iconSkillToUP[i].color = new Color(1, 1, 1, 0);
				}
			}

			//ОБновление панели не прокачанных скиллов
			if (noUPSkillsCount > 8)
			{
				//Debug.Log("noUPSkillsCount " + noUPSkillsCount);
				startSkill = (int)((noUPSkillsCount - 8f) * scrollbar.value);
				scrollbar.size = 1f / ((float)noUPSkillsCount / 8f);
				scrollbarObj.SetActive(true);
				//Debug.Log("startSkill " + startSkill);
			}
			else {
				scrollbarObj.SetActive(false);
				startSkill = 0;
			}

			for (int i = 0; i < 8; i++)
			{
				if (i < noUPSkillsCount)
				{
					int skillIter = noUPSkillsArray[i + startSkill];
					float progress = (float)guiUPController.skillsInfoValues[skillIter, 3] / (float)guiUPController.skillsInfoValues[skillIter, 4];

					textPanelSkill[i].text = (int)(progress * 100) + "% (" + guiUPController.skillsInfoValues[skillIter, 3] + ")";
					statusLine[i].fillAmount = progress;
					imageIcon[i].sprite = guiUPController.skillIcons[guiUPController.skillsInfoValues[skillIter, 0]];
					skillPanel[i].SetActive(true);
				}
				else
				{
					skillPanel[i].SetActive(false);
				}
			}
		}
	}
}
