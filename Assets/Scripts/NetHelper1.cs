#region Подключенная всякая нужная хуйня
using Newtonsoft.Json;//для работы с жсонами
using SignalR.Client._20.Hubs;//связь с сервером
using System.Collections;//коллекции
using System.Threading.Collections;//потоки и вся связанная с ними ебота
using System.Collections.Generic;//вроде потокозащищенные колекции
using UnityEngine;//Юнька
using Models;//Наша либа
using System.Linq;//Все что связанно с сылками и поиском, например поиск определенного игрока в списке
using System.Threading;
#endregion

public class NetHelper1 : MonoBehaviour
{
    #region +100500 переменных
    public HubConnection hub;//Хаб
    private IHubProxy proxy;//Прокси
    private string url;//Адрес хоста
    public GameObject ChunkSpawner;//Отрисовщик тайлов
    public MinimapController minimap;//Контроллер миникарты
    public byte[,] chank;//УДАЛИТЬ(НЕ УДАЛЯТЬ)
    public int chankx;//Позиция чанка x/32
    public int chanky;//Позиция чанка y/32
    public Transform player;//Позиция игрока
	public CharactControl control;

	[SerializeField] CharacterDataScript CharacterData;
    byte[,] chankBufer = new byte[ChankLoad.chanksX, ChankLoad.chanksY];//Буфер для проверки имеется ли этот чанк на клиенте
    ConcurrentQueue<PlayerModel> createObjectPool = new ConcurrentQueue<PlayerModel>();//позиции игроков с сервака
    ConcurrentQueue<int> leavepool = new ConcurrentQueue<int>();//Ливнувшие игроки(используется только потому что нельзя блять вызвать дестрой или ремув из вторичного потока)
    List<GameObject> players_gameobjects = new List<GameObject>();//Список живых игроков
    [SerializeField] ChankLoad chunkload;//скрипт с картой
    public GameObject player_prefab;//префаб прочих игроков
	public Vector3 Pos;

	
    #endregion
    #region Стартап
    void Awake()
    {
		CharacterData.charactCrystalls = new uint[6];//создаем массив кри, чтоб пока не пришел с серва не сыпались 0
		url = "http://localhost:62377/";//Адресс хоста
        //url = "http://testserv222.somee.com/";
       // url = "http://185.231.153.8:62377/server/";
        Connect();
        

		
	}
    #region Коннект
	private void Connect()
    {

        hub = new HubConnection(url);

        proxy = hub.CreateProxy("Game");
        //подписки, что-то вроде каналов передачи
        Subscription sub = proxy.Subscribe("updateModel");
        sub.Data += Sub_Update_Data;
        Subscription sub_create = proxy.Subscribe("createModel");
        sub_create.Data += sub_create_Data;
        Subscription sub_debug = proxy.Subscribe("debug");
        Subscription sub_cri = proxy.Subscribe("CriSend");
        Subscription sub_leave = proxy.Subscribe("playerleave");
		Subscription sub_skill = proxy.Subscribe("skilldata");
		Subscription sub_Pos = proxy.Subscribe("Pos");
		sub_Pos.Data += Sub_Pos_Data;
		sub_skill.Data += Sub_skill_Data;
		sub_cri.Data += Sub_cri_Data;
        sub_debug.Data += Sub_debug_Data;
        sub_leave.Data += Sub_leave_Data;
		StartCoroutine("HubConnect");
		StartCoroutine("getmap");
		StartCoroutine("sendpos");
		StartCoroutine("netcheck");
		//proxy.Invoke("CheckConnection");
		//proxy.Invoke("GetMap", 0, 0);
	}
	
	IEnumerator HubConnect()
	{
		hub.Start();
		proxy.Invoke("Connect", 0);//тут будем отправлять Viewer_id + Auth_Key
		yield return new WaitForSeconds(0.5f);
		StopCoroutine("HubConnect");
	}

	private void Sub_Pos_Data(object[] obj)
	{
		Debug.Log(obj[0]);
		Vec2 player = JsonConvert.DeserializeObject<Vec2>(obj[0].ToString());
		Pos = new Vector3(player.x, player.y,-5);
		Debug.Log(Pos);
		
	}
	#region Прием Данных о скилах
	private void Sub_skill_Data(object[] obj)
	{
		Debug.Log(obj[0].ToString());
	}
	#endregion

	#endregion
	#endregion
	#region Прием инфы о кри

	bool criUpdated = true;
	private void Sub_cri_Data(object[] obj)
	{
		CharacterData.charactCrystalls= JsonConvert.DeserializeObject<uint[]>(obj[0].ToString());
		//cri = JsonConvert.DeserializeObject<int[]>(obj[0].ToString());
		//criUpdated = true;
	}
	#endregion
	#region Уведомление о покинувшем игру печальном челике
	private void Sub_leave_Data(object[] obj)
    {
        int id = int.Parse(obj[0].ToString());
        leavepool.Enqueue(id);
        //Destroy(players_gameobjects.First(p => p.name == id.ToString()));
        Debug.Log("Ливнул челик с Id: " + id);
    }
    #endregion
    //ОТПРАВКА
    #region Отправка позиции на сервер
    IEnumerator sendpos()
    {
        while (hub != null)
        {

            CheckPlayer();
            proxy.Invoke("SendPos", (int)player.position.x, (int)player.position.y);

            yield return new WaitForSeconds(0.5f);
        }
    }
    #endregion
    #region Запрос карты
    IEnumerator getmap()
    {
        
        while (hub != null)
        {
            //Где 0 Область рендера т.е. обновляемая при надобности с сервера
            #region Буфер Очистки

            chankBufer[(int)player.position.x / 32, (int)player.position.y / 32 + 3] = 0;
            chankBufer[(int)player.position.x / 32+1, (int)player.position.y / 32 + 3] = 0;
            chankBufer[(int)player.position.x / 32+2, (int)player.position.y / 32 + 3] = 0;
            chankBufer[(int)player.position.x / 32+3, (int)player.position.y / 32 + 3] = 0;
            chankBufer[(int)player.position.x / 32-1, (int)player.position.y / 32 + 3] = 0;
            chankBufer[(int)player.position.x / 32-2, (int)player.position.y / 32 + 3] = 0;
            chankBufer[(int)player.position.x / 32-3, (int)player.position.y / 32 + 3] = 0;
            //1 1 1 1 1 1 1
            //0 0 0 0 0 0 0
            //0 0 0 0 0 0 0
            //0 0 0 0 0 0 0
            //0 0 0 0 0 0 0
            //0 0 0 0 0 0 0
            //0 0 0 0 0 0 0

            chankBufer[(int)player.position.x / 32, (int)player.position.y / 32 - 3] = 0;
            chankBufer[(int)player.position.x / 32 + 1, (int)player.position.y / 32 - 3] = 0;
            chankBufer[(int)player.position.x / 32 + 2, (int)player.position.y / 32 - 3] = 0;
            chankBufer[(int)player.position.x / 32 + 3, (int)player.position.y / 32 - 3] = 0;
            chankBufer[(int)player.position.x / 32 - 1, (int)player.position.y / 32 - 3] = 0;
            chankBufer[(int)player.position.x / 32 - 2, (int)player.position.y / 32 - 3] = 0;
            chankBufer[(int)player.position.x / 32 - 3, (int)player.position.y / 32 - 3] = 0;
            //1 1 1 1 1 1 1
            //0 0 0 0 0 0 0
            //0 0 0 0 0 0 0
            //0 0 0 0 0 0 0
            //0 0 0 0 0 0 0
            //0 0 0 0 0 0 0
            //1 1 1 1 1 1 1
            chankBufer[(int)player.position.x / 32+3, (int)player.position.y / 32] = 0;
            chankBufer[(int)player.position.x / 32+3, (int)player.position.y / 32+1] = 0;
            chankBufer[(int)player.position.x / 32+3, (int)player.position.y / 32+2] = 0;
            chankBufer[(int)player.position.x / 32+3, (int)player.position.y / 32-1] = 0;
            chankBufer[(int)player.position.x / 32+3, (int)player.position.y / 32-2] = 0;
            //1 1 1 1 1 1 1
            //0 0 0 0 0 0 1
            //0 0 0 0 0 0 1
            //0 0 0 0 0 0 1
            //0 0 0 0 0 0 1
            //0 0 0 0 0 0 1
            //1 1 1 1 1 1 1
            chankBufer[(int)player.position.x / 32 - 3, (int)player.position.y / 32] = 0;
            chankBufer[(int)player.position.x / 32 - 3, (int)player.position.y / 32 + 1] = 0;
            chankBufer[(int)player.position.x / 32 - 3, (int)player.position.y / 32 + 2] = 0;
            chankBufer[(int)player.position.x / 32 - 3, (int)player.position.y / 32 - 1] = 0;
            chankBufer[(int)player.position.x / 32 - 3, (int)player.position.y / 32 - 2] = 0;
            //1 1 1 1 1 1 1
            //1 0 0 0 0 0 1
            //1 0 0 0 0 0 1
            //1 0 0 0 0 0 1
            //1 0 0 0 0 0 1
            //1 0 0 0 0 0 1
            //1 1 1 1 1 1 1
            #endregion
            #region Запрос Чанков
            bool l = false;
            //yield return new WaitForSeconds(0.4f);
            Debug.Log("Pos" + (int)player.position.x / 32 + " " + (int)player.position.y / 32);
            if (chankBufer[(int)player.position.x / 32, (int)player.position.y / 32] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x, (int)player.position.y);
                l = true;
               
            }

            if (chankBufer[(int)player.position.x / 32 + 1, (int)player.position.y / 32 + 1] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x + 32, (int)player.position.y + 32);
                l = true;
                Debug.Log("get");
            }

            if (chankBufer[(int)player.position.x / 32 - 1, (int)player.position.y / 32 - 1] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x - 32, (int)player.position.y - 32);
                l = true;
            }

            if (chankBufer[(int)player.position.x / 32 + 1, (int)player.position.y / 32 - 1] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x + 32, (int)player.position.y - 32);
                l = true;
            }
            if (chankBufer[(int)player.position.x / 32 - 1, (int)player.position.y / 32 + 1] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x - 32, (int)player.position.y + 32);
                l = true;
            }
            if (chankBufer[(int)player.position.x / 32 - 1, (int)player.position.y / 32] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x - 32, (int)player.position.y);
                l = true;
            }
            if (chankBufer[(int)player.position.x / 32 + 1, (int)player.position.y / 32] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x + 32, (int)player.position.y);
                l = true;
                Debug.Log("get");
            }
            if (chankBufer[(int)player.position.x / 32, (int)player.position.y / 32 - 1] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x, (int)player.position.y - 32);
                l = true;
            }
            if (chankBufer[(int)player.position.x / 32, (int)player.position.y / 32 + 1] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x, (int)player.position.y + 32);
                l = true;
            }
            if (chankBufer[(int)player.position.x / 32, (int)player.position.y / 32 + 2] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x, (int)player.position.y + 64);
                l = true;
            }
            if (chankBufer[(int)player.position.x / 32+1, (int)player.position.y / 32 + 2] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x+32, (int)player.position.y + 64);
                l = true;
            }
            if (chankBufer[(int)player.position.x / 32 - 1, (int)player.position.y / 32 + 2] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x - 32, (int)player.position.y + 64);
                l = true;
            }
            if (chankBufer[(int)player.position.x / 32 + 2, (int)player.position.y / 32 + 2] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x + 64, (int)player.position.y + 64);
                l = true;
            }
            if (chankBufer[(int)player.position.x / 32 - 2, (int)player.position.y / 32 + 2] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x - 64, (int)player.position.y + 64);
                l = true;
            }
            if (chankBufer[(int)player.position.x / 32 + 2, (int)player.position.y / 32 - 2] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x + 64, (int)player.position.y - 64);
                l = true;
            }
            if (chankBufer[(int)player.position.x / 32 - 2, (int)player.position.y / 32 - 2] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x - 64, (int)player.position.y - 64);
                l = true;
            }
            if (chankBufer[(int)player.position.x / 32, (int)player.position.y / 32 - 2] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x, (int)player.position.y - 64);
                l = true;
            }
            if (chankBufer[(int)player.position.x / 32 + 1, (int)player.position.y / 32 - 2] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x + 32, (int)player.position.y - 64);
                l = true;
            }
            if (chankBufer[(int)player.position.x / 32 -1, (int)player.position.y / 32 - 2] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x -32, (int)player.position.y - 64);
                l = true;
            }
            if (chankBufer[(int)player.position.x / 32 + 2, (int)player.position.y / 32 - 1] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x + 64, (int)player.position.y - 32);
                l = true;
            }
            if (chankBufer[(int)player.position.x / 32 + 2, (int)player.position.y / 32 + 1] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x + 64, (int)player.position.y + 32);
                l = true;
            }
            if (chankBufer[(int)player.position.x / 32 + 2, (int)player.position.y / 32] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x + 64, (int)player.position.y);
                l = true;
            }
            if (chankBufer[(int)player.position.x / 32 - 2, (int)player.position.y / 32 - 1] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x - 64, (int)player.position.y - 32);
                l = true;
            }
            if (chankBufer[(int)player.position.x / 32 - 2, (int)player.position.y / 32 + 1] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x - 64, (int)player.position.y + 32);
                l = true;
            }
            if (chankBufer[(int)player.position.x / 32 - 2, (int)player.position.y / 32] == 0)
            {
                proxy.Invoke("GetChunk", (int)player.position.x - 64, (int)player.position.y);
                l = true;
            }
            if (!l)
                yield return new WaitForSeconds(1f);
            else
                yield return new WaitForSeconds(1f);
            #endregion
        }
    }
    #endregion
    #region Тут должна была быть проверка соединения, но ее пока нет
    IEnumerator netcheck()
    {
        while (hub != null)
        {
            yield return new WaitForSeconds(1f);
            proxy.Invoke("CheckConnection");

        }
    }
    #endregion
    #region Копа
    bool iskop = true;//Проверка копы
    #region Запрос на изменение блока на сервере
    public void ChangeMap(char param, char dir)
    {
        if (iskop)
        {
            iskop = false;
            StartCoroutine(kopdelay(param,dir));
            Debug.Log("ChangeChunk");
        }
    }
    #endregion
    #region Задержка копы
    IEnumerator kopdelay(char param, char dir)
    {
        proxy.Invoke("ChangeChunk", param,dir);//на самом деле запрос тут
        yield return new WaitForSeconds(0.1f);
        iskop = true;
        StopCoroutine("kopdelay");
    }
	#endregion
	#endregion
	public void ChangePos(char param, char dir)
	{
		proxy.Invoke("ChangePos", param, dir);
	}
	#region Геология
	bool isgeo = true;
	public void GeoChangeMap(int x, int y, int value)
	{
		if (isgeo)
		{
			isgeo = false;
			StartCoroutine(geodelay(x, y, value));
			
			Debug.Log("GeoChangeChunk");
		}
	}
	IEnumerator geodelay(int x, int y, int value)
	{
		proxy.Invoke("ChangeChunk", x, y, (int)player.position.x, (int)player.position.y, value, 'g');//на самом деле запрос тут
		yield return new WaitForSeconds(0.1f);
		isgeo = true;
		StopCoroutine("kopdelay");
	}
	#endregion
	//ПОЛУЧЕНИЕ
	#region Прием чанка
	private void Sub_debug_Data(object[] obj)
    {
        chank = new byte[32, 32];//объявляем буферный чанк
        //string[] cols = new string[32]; уже ненужная хуета
       // Debug.Log(obj[0].ToString()); оказывается этот дебаг нехуево так тормозит
        //pos = JsonConvert.DeserializeObject<MoveModel>(obj[1].ToString());
        ChunkModel chunk = JsonConvert.DeserializeObject<ChunkModel>(obj[0].ToString());
		#region OLD
		//Debug.Log(JsonConvert.SerializeObject(chunk.chunk).ToString());
		// int i = 0;
		//chunk.data.Reverse();
		//foreach (var item in chunk.data)
		//{
		//    cols[i] = item;
		//    // Debug.Log(cols[i]);
		//    i++;
		//}
		//chunk.data.Clear();
		//for (int x = 0; x < 32; x++)
		//{
		//    string[] bufer = new string[32];//массив символов
		//    bufer = cols[x].Split(',');//режем
		//    for (int y = 0; y < 32; y++)
		//    {

		//        //Debug.Log(bufer[y]);
		//        chank[x, 31 - y] = byte.Parse(bufer[y]);//заполняем
		//                                                // Debug.Log(JsonConvert.SerializeObject(chank));
		//    }
		//}
		//Debug.Log(chunk.chunks);
		//byte[,] chnk = new byte[32,32];
		//chnk= JsonConvert.DeserializeObject<byte[,]>(chunk.chunks);
		#endregion
		#region NEW
		int l = 32;
		foreach (var item in chunk.data)
		{
			l--;
			for (int x = 0; x < 32; x++)
			{
				chank[x, 31-l] = item[x];//ПОКА КОСТЫЛЬ ПОКА НЕТ НОРМАЛЬНОЙ ПОДГРУЗКИ НА СЕРВЕРЕ
			}
		}

		#endregion
		#region SET
		chankx = chunk.x;
        chanky = chunk.y;
        // updatechank = true;
        chankBufer[chankx, chanky] = 1;
        chunkload.chankMap[chankx, chanky] = new byte[32, 32];
		chunkload.chankMap[chankx, chanky] = chank;//JsonConvert.DeserializeObject<byte[,]>(chunk.chunks);
        //chunkload.chankMap[0, 0] = chank;

        // ChunkSpawner.SetActive(true);
        Debug.Log("x: " + chankx + "  y: " + chanky);
		//Debug.Log(JsonConvert.SerializeObject(chank));


		// chunkload.chankMap[chunk.x, chunk.y] = chunk.data;
		//chunk = obj[0].ToString();
		//Debug.Log("x:"+pos.X+"  y:"+pos.Y);

		//Debug.Log(JsonConvert.SerializeObject(chunk.data));
		//Debug.Log(JsonConvert.SerializeObject(chunkload.chankMap[pos.X, pos.Y]));

		//debugtext.text = mapstring;
		#endregion
	}
	#endregion
	#region Прием игроков в пул(вроде не используется нодо прочекать)
	private void sub_create_Data(object[] obj)
    {
        //foreach (var item in createObjectPool)
        //{
        //    if (!players.Any(p => p.Id == item.Id))
        //    {
        //        players.Add(item);
        //    }
        //}
        //createObjectPool.Clear();
        foreach (var item in obj)
        {
            PlayerModel player = JsonConvert.DeserializeObject<PlayerModel>(item.ToString());
            createObjectPool.Enqueue(player);

        }
    }
    #endregion
    #region Прием позиций игроков
    private void Sub_Update_Data(object[] obj)
    {

        Debug.Log(obj[0].ToString() + " " + obj[1].ToString() + " " + obj[2].ToString() + " " + obj[3].ToString());
        PlayerModel other_player = new PlayerModel();//JsonConvert.DeserializeObject<PlayerModel>(obj[0].ToString());
        // syncObjectPool.Add(other_player);
        other_player.x = int.Parse(obj[0].ToString());
        other_player.y = int.Parse(obj[1].ToString());
        other_player.angle = char.Parse(obj[2].ToString());
        other_player.Id = int.Parse(obj[3].ToString());
        //Debug.Log(other_player.x + "  " + other_player.y+" "+ other_player.Id);
        createObjectPool.Enqueue(other_player);


    }
    #endregion
    #region Мусор
    //public void GetMap()
    //{
    //    proxy.Invoke("GetMap", 0, 0);
    //    proxy.Invoke("GetMap", 8, 8);
    //    proxy.Invoke("GetMap", 8, 9);
    //    proxy.Invoke("GetMap", 8, 10);
    //    proxy.Invoke("GetMap", 8, 11);
    //    proxy.Invoke("GetMap", 8, 12);
    //    proxy.Invoke("GetMap", 8, 13);
    //    proxy.Invoke("GetMap", 9, 8);
    //    proxy.Invoke("GetMap", 9, 9);
    //    proxy.Invoke("GetMap", 9, 10);
    //    proxy.Invoke("GetMap", 9, 11);
    //    proxy.Invoke("GetMap", 9, 12);
    //    proxy.Invoke("GetMap", 9, 13);
    //    proxy.Invoke("GetMap", 10, 8);
    //    proxy.Invoke("GetMap", 10, 9);
    //    proxy.Invoke("GetMap", 10, 10);
    //    proxy.Invoke("GetMap", 10, 11);
    //    proxy.Invoke("GetMap", 10, 12);
    //    proxy.Invoke("GetMap", 10, 13);
    //    proxy.Invoke("GetMap", 11, 8);
    //    proxy.Invoke("GetMap", 11, 9);
    //    proxy.Invoke("GetMap", 11, 10);
    //    proxy.Invoke("GetMap", 11, 11);
    //    proxy.Invoke("GetMap", 11, 12);
    //    proxy.Invoke("GetMap", 11, 13);



    //}
    #endregion
    #region Дебаг функции
    public void Enablespawner()
    {
        ChunkSpawner.SetActive(true);//ОНО ТУТ ДЛЯ ДЕБАГА
        minimap.enabled = true;
    }
    #endregion
    #region Тут вся механника прочих игроков а так же метод Update
    void Update()
    {
		if (Pos.x != -1)
		{
			control.ReturnToRespawn(Pos);
			Pos.x = -1;
		}	//	player.transform.position = Pos;
		#region Обновление Инфо панели

		#endregion
		if (!createObjectPool.IsEmpty)
        {
            foreach (var item in createObjectPool)
            {
                //int check=players.FirstOrDefault(p => p.Id != createObjectPool.FirstOrDefault(k => k.Id!=p.Id).Id).Id;
                //   players.Remove(players.FirstOrDefault(p => p.Id == check));
                //string PM = players_gameobjects.FirstOrDefault(p => p.name == item.Id.ToString()).name;
                if (!players_gameobjects.Any(p => p.name == item.Id.ToString()))// != item.Id.ToString())
                {
                    GameObject other_player = Instantiate(player_prefab, new Vector3(item.x + 0.5f, item.y + 0.5f,-5), Quaternion.identity) as GameObject;
                    other_player.name = item.Id.ToString();
                    // players.Add(item);
                    players_gameobjects.Add(other_player);
                    //Debug.Log(item.x + "  " + item.y);
                }
                else
                {

                    // PlayerModel curPlayer = players.FirstOrDefault(p => p.Id == item.Id);



                    GameObject curPlayerGO = players_gameobjects.FirstOrDefault(p => p.name == item.Id.ToString());
                    curPlayerGO.transform.position = new Vector3((float)item.x + 0.5f, (float)item.y + 0.5f,-5f);//ТУТ ДОБАВИТЬ ПЛАВНОСТИ
					switch (item.angle) {
						case 'u':
							curPlayerGO.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
						break;
						case 'd':
							curPlayerGO.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
							break;
						case 'r':
							curPlayerGO.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
							break;
						case 'l':
							curPlayerGO.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 270));
							break;
					}

                }

            }

            createObjectPool.Clear();

        }
        foreach (var item in leavepool)
        {
            Destroy(players_gameobjects.First(p => p.name == item.ToString()));
            players_gameobjects.Remove(players_gameobjects.First(p => p.name == item.ToString()));
        }
        leavepool.Clear();
    }
	
    void CheckPlayer()
    {
        List<GameObject> renderplayers = new List<GameObject>();
        foreach (var item in players_gameobjects)
        {
            if (player.position.x - 45 <= item.transform.position.x && player.position.x + 45 >= item.transform.position.x &&
                player.position.y - 45 <= item.transform.position.y && player.position.y + 45 >= item.transform.position.y)
            {
                //renderplayers.Add(item);

                renderplayers.Add(item);
            }
            else
            {
                //players_gameobjects.Remove(item);
                Destroy(item);


                Debug.Log("Destroy1");
            }
        }
        players_gameobjects.Clear();
        foreach (var item in renderplayers.Except(players_gameobjects))
        {
            players_gameobjects.Add(item);
        }
        renderplayers.Clear();

    }
    #endregion
	public void Controller(string param)
	{
		proxy.Invoke("controller", param);
	}
    #region Выход из приложения
    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit() " + Time.time + " seconds");

        hub.Stop();
    }
    #endregion
}