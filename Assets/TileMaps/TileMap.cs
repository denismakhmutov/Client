using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMap : MonoBehaviour {

	/// <summary> размер чанка в блоках </summary>
	public int size_x,size_z;
	public float tileSize = 1.0f;

	public Texture2D terrainTiles;
	public int tileResolution;

	public Material testmaterial;//Материал-источник для материала объекта

	[System.NonSerialized]
	//активен ли чанк. или ждет очереди на перемещение
	public bool isActiveChank = false;

	public GameObject buildingSpritePref;//префаб для отображения здания на экране
	List<GameObject> buildSpritesList = new List<GameObject>();//лист спрайтов, выводимых на экран

	GameObject chankLoaderObj;
	ChankLoad chunkLoad;//чанклоадер

	GameObject characterObj;

	GameObject chancSpawnerObj;

	GameObject characterDataAndGUIControllers;//контроллеры персонажа и гуих(нужен для спрайтов зданий)
	GUIItemsController guiItemsController;//ссылка на итемконтроллер со спрайтами зданий

	static Color[][] debugGFXAtlas;//Массив тайлов всех блоков
	static bool loadSprites = true;//нужно ли загружать спрайты в массив

	byte[,] chunkBuffer = new byte[32, 32];//Массив для хранения состояния чанка. Нужен бля проверки на изменение чанка с прошлого пробега

	//переменные для обновления текстуры в функции UpdateTexture()
	MeshRenderer mesh_rendererChunk;
	Texture2D textureChunk;

	/// <summary>
	/// позиция чанка в мире 
	/// </summary>
	Vector3 chuncPos;
	/// <summary>
	/// позиция чанка в мире деленная на 32
	/// </summary>
	Vector3 chuncPos32;

	// Use this for initialization
	void Start () {
		chankLoaderObj = GameObject.Find("ChunkLoader");
		chunkLoad = chankLoaderObj.GetComponent<ChankLoad>();

		characterObj = GameObject.Find("Сharacter");
		chancSpawnerObj = GameObject.Find("ChunkSpawner");
		characterDataAndGUIControllers = GameObject.Find("CharacterDataAndGUIControllers");
		guiItemsController = characterDataAndGUIControllers.GetComponent<GUIItemsController>();

		if (loadSprites)
		{
			loadSprites = false;
			debugGFXAtlas = new Color[44][];
			{
				debugGFXAtlas[0] = terrainTiles.GetPixels(0, 160, tileResolution, tileResolution);
				debugGFXAtlas[1] = terrainTiles.GetPixels(0, 128, tileResolution, tileResolution);
				debugGFXAtlas[2] = terrainTiles.GetPixels(16, 128, tileResolution, tileResolution);
				debugGFXAtlas[3] = terrainTiles.GetPixels(32, 128, tileResolution, tileResolution);
				debugGFXAtlas[4] = terrainTiles.GetPixels(48, 128, tileResolution, tileResolution);
				debugGFXAtlas[5] = terrainTiles.GetPixels(64, 128, tileResolution, tileResolution);
				debugGFXAtlas[6] = terrainTiles.GetPixels(80, 48, tileResolution, tileResolution);
				debugGFXAtlas[7] = terrainTiles.GetPixels(32, 48, tileResolution, tileResolution);
				debugGFXAtlas[8] = terrainTiles.GetPixels(7 * 16, 3 * 16, tileResolution, tileResolution);
				debugGFXAtlas[9] = terrainTiles.GetPixels(1 * 16, 3 * 16, tileResolution, tileResolution);
				debugGFXAtlas[10] = terrainTiles.GetPixels(7 * 16, 4 * 16, tileResolution, tileResolution);
				debugGFXAtlas[11] = terrainTiles.GetPixels(6 * 16, 3 * 16, tileResolution, tileResolution);
				debugGFXAtlas[12] = terrainTiles.GetPixels(10 * 16, 4 * 16, tileResolution, tileResolution);
				debugGFXAtlas[13] = terrainTiles.GetPixels(11 * 16, 4 * 16, tileResolution, tileResolution);
				debugGFXAtlas[14] = terrainTiles.GetPixels(12 * 16, 4 * 16, tileResolution, tileResolution);
				debugGFXAtlas[15] = terrainTiles.GetPixels(13 * 16, 4 * 16, tileResolution, tileResolution);
				debugGFXAtlas[16] = terrainTiles.GetPixels(14 * 16, 4 * 16, tileResolution, tileResolution);
				debugGFXAtlas[17] = terrainTiles.GetPixels(15 * 16, 4 * 16, tileResolution, tileResolution);
				debugGFXAtlas[18] = terrainTiles.GetPixels(11 * 16, 5 * 16, tileResolution, tileResolution);
				debugGFXAtlas[19] = terrainTiles.GetPixels(12 * 16, 5 * 16, tileResolution, tileResolution);
				debugGFXAtlas[20] = terrainTiles.GetPixels(13 * 16, 5 * 16, tileResolution, tileResolution);
				debugGFXAtlas[21] = terrainTiles.GetPixels(14 * 16, 5 * 16, tileResolution, tileResolution);
				debugGFXAtlas[22] = terrainTiles.GetPixels(0 * 16, 4 * 16, tileResolution, tileResolution);
				debugGFXAtlas[23] = terrainTiles.GetPixels(1 * 16, 4 * 16, tileResolution, tileResolution);
				debugGFXAtlas[24] = terrainTiles.GetPixels(2 * 16, 4 * 16, tileResolution, tileResolution);
				debugGFXAtlas[25] = terrainTiles.GetPixels(3 * 16, 4 * 16, tileResolution, tileResolution);
				debugGFXAtlas[26] = terrainTiles.GetPixels(4 * 16, 4 * 16, tileResolution, tileResolution);
				debugGFXAtlas[27] = terrainTiles.GetPixels(6 * 16, 5 * 16, tileResolution, tileResolution);
				debugGFXAtlas[28] = terrainTiles.GetPixels(3 * 16, 3 * 16, tileResolution, tileResolution);
				debugGFXAtlas[29] = terrainTiles.GetPixels(5 * 16, 7 * 16, tileResolution, tileResolution);
				debugGFXAtlas[30] = terrainTiles.GetPixels(4 * 16, 3 * 16, tileResolution, tileResolution);
				debugGFXAtlas[31] = terrainTiles.GetPixels(3 * 16, 7 * 16, tileResolution, tileResolution);
				debugGFXAtlas[32] = terrainTiles.GetPixels(6 * 16, 7 * 16, tileResolution, tileResolution);
				debugGFXAtlas[33] = terrainTiles.GetPixels(4 * 16, 7 * 16, tileResolution, tileResolution);
				debugGFXAtlas[34] = terrainTiles.GetPixels(2 * 16, 7 * 16, tileResolution, tileResolution);
				debugGFXAtlas[35] = terrainTiles.GetPixels(7 * 16, 7 * 16, tileResolution, tileResolution);
				debugGFXAtlas[36] = terrainTiles.GetPixels(5 * 16, 4 * 16, tileResolution, tileResolution);
				debugGFXAtlas[37] = terrainTiles.GetPixels(6 * 16, 4 * 16, tileResolution, tileResolution);
				debugGFXAtlas[38] = terrainTiles.GetPixels(9 * 16, 3 * 16, tileResolution, tileResolution);
				debugGFXAtlas[39] = terrainTiles.GetPixels(1 * 16, 7 * 16, tileResolution, tileResolution);
				debugGFXAtlas[40] = terrainTiles.GetPixels(0 * 16, 7 * 16, tileResolution, tileResolution);
				debugGFXAtlas[41] = terrainTiles.GetPixels(8 * 16, 3 * 16, tileResolution, tileResolution);
				debugGFXAtlas[42] = terrainTiles.GetPixels(10 * 16, 3 * 16, tileResolution, tileResolution);
				debugGFXAtlas[43] = terrainTiles.GetPixels(10 * 16, 5 * 16, tileResolution, tileResolution);
			}//Инициализация всех видов блоков
		}


		//BuildMesh();
		BuildTexture();

		mesh_rendererChunk = GetComponent<MeshRenderer>();
		textureChunk = (Texture2D)mesh_rendererChunk.sharedMaterial.mainTexture;
	}

	// Update is called once per frame
	void Update()
	{
		if (isActiveChank) {

			chuncPos = gameObject.transform.position;
			chuncPos32.x = (chuncPos.x / 32);
			chuncPos32.y = (chuncPos.y / 32);

			UpdateBuilds();//обновление спрайтов зданий(проверка тоже внутри)

			if (System.Math.Abs(chuncPos.x - (chuncPos.x + 1)) < 32 &&
			System.Math.Abs(chuncPos.y - (chuncPos.y + 1)) < 32
			&& (Time.frameCount % 4) == 0)
			{
				UpdateTexture();//обновление текстуры чанка(проверка на нужность внутри функции)
			}
		}

		ChankCleaning();
	}

	void ChankCleaning() {
		if (System.Math.Abs(characterObj.transform.position.x - (gameObject.transform.position.x)) > 48 ||
			System.Math.Abs(characterObj.transform.position.y - (gameObject.transform.position.y)) > 48)
		{
			isActiveChank = false;
			for (int i = 0; i < buildSpritesList.Count; i++)
			{
				buildSpritesList[i].SetActive(false);
			}
			chancSpawnerObj.GetComponent<ChankSpawn>().chankIsLoaded[(int)(gameObject.transform.position.x / 32), (int)(gameObject.transform.position.y / 32)] = false;
		}
	}

	void UpdateBuilds()
	{
		if (chunkLoad.chankBuildingsDataArray[(int)chuncPos32.x, (int)chuncPos32.y] != null)
		{
			int count = chunkLoad.chankBuildingsDataArray[(int)chuncPos32.x, (int)chuncPos32.y].Count;
			if (count != 0) {
				//Debug.Log(chunkLoad.chankBuildingsDataArray[(int)chuncPos32.x, (int)chuncPos32.y].Count);
				if (buildSpritesList.Count < count)
				{
					int n = count - buildSpritesList.Count;
					for (int i = 0; i < n; i++)
					{
						buildSpritesList.Add(Instantiate(buildingSpritePref, new Vector3(0,0,0),Quaternion.identity));
					}
				}
				else
				{
					for (int i = 0; i < count; i++)
					{
						buildSpritesList[i].transform.position = new Vector3(chunkLoad.chankBuildingsDataArray[(int)chuncPos32.x, (int)chuncPos32.y][i].x + 0.5f, chunkLoad.chankBuildingsDataArray[(int)chuncPos32.x, (int)chuncPos32.y][i].y + 0.5f, -6);

						SpriteRenderer spriteRenderer = buildSpritesList[i].GetComponent<SpriteRenderer>();
						spriteRenderer.sprite = guiItemsController.spriteItems[chunkLoad.chankBuildingsDataArray[(int)chuncPos32.x, (int)chuncPos32.y][i].packType];

						buildSpritesList[i].SetActive(true);
					}
				}
			}
		}
	}

	bool isNool = false;
	//обновляет текстуру чанка
	public void UpdateTexture() {
		bool apply = false;

		if (chunkLoad.chankMap[(int)transform.position.x / 32, (int)transform.position.y / 32] == null)
		{
			isNool = true;
		}
		else {
			isNool = false;
		}

		for (int y = 0; y < size_z; y++)
			for (int x = 0; x < size_x; x++)
			{
				if (!isNool)
				{
					if (chunkBuffer[x, y] != chunkLoad.chankMap[(int)transform.position.x / 32, (int)transform.position.y / 32][x, y])
					{
						textureChunk.SetPixels(x * tileResolution, y * tileResolution, tileResolution, tileResolution, debugGFXAtlas[chunkLoad.chankMap[(int)transform.position.x / 32, (int)transform.position.y / 32][x, y]]);
						chunkBuffer[x, y] = chunkLoad.chankMap[(int)transform.position.x / 32, (int)transform.position.y / 32][x, y];
						apply = true;
					}
				}
				else {
					if (chunkBuffer[x, y] != 0)
					{
						textureChunk.SetPixels(x * tileResolution, y * tileResolution, tileResolution, tileResolution, debugGFXAtlas[0]);
						chunkBuffer[x, y] = 0;
						apply = true;
					}
				}
			}
		if(apply)textureChunk.Apply();
	}

	void BuildTexture()
	{
		int texWidth = size_x * tileResolution;
		int texHeigth = size_z * tileResolution;
		Texture2D texture = new Texture2D(texWidth, texHeigth);
		/*gameObject.GetComponent<ChankLoad>().chankMap[(int)transform.position.x / 32, (int)transform.position.y / 32][x, y]*/

		if (chunkLoad.chankMap[(int)transform.position.x / 32, (int)transform.position.y / 32] != null)
		{
			for (int y = 0; y < size_z; y++)
				for (int x = 0; x < size_x; x++)
				{
					chunkBuffer[x, y] = chunkLoad.chankMap[(int)transform.position.x / 32, (int)transform.position.y / 32][x, y];

					texture.SetPixels(x * tileResolution, y * tileResolution, tileResolution, tileResolution, debugGFXAtlas[chunkLoad.chankMap[(int)transform.position.x / 32, (int)transform.position.y / 32][x, y]]);
				}
		}
		else {
			for (int y = 0; y < size_z; y++)
				for (int x = 0; x < size_x; x++)
				{
					texture.SetPixels(x * tileResolution, y * tileResolution, tileResolution, tileResolution, debugGFXAtlas[0]);
				}
		}
		texture.filterMode = FilterMode.Bilinear;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.Apply();

		MeshRenderer mesh_renderer = GetComponent<MeshRenderer>();

		Material materialobj = new Material(testmaterial);//материал обьекта
		materialobj.mainTexture = texture;
		mesh_renderer.sharedMaterial = materialobj;
	}

	void BuildMesh()
	{
		int vsize_x = size_x + 1;
		int vsize_z = size_z + 1;
		int numVerts = vsize_x * vsize_z;
		int numTiles = size_x * size_z;
		int numTris = numTiles * 2;

		// генерация меш дата
		Vector3[] vertices = new Vector3[numVerts];
		Vector3[] normals = new Vector3[numVerts];
		Vector2[] uv = new Vector2[numVerts];

		int[] triangles = new int[numTris * 3];
		int x, z;

		for (z = 0; z < vsize_z; z++)
			for (x = 0; x < vsize_x; x++)
			{
				vertices[z * vsize_x + x] = new Vector3(x * tileSize, 0, z * tileSize);
				normals[z * vsize_x + x] = Vector3.up;
				uv[z * vsize_x + x] = new Vector2((float)x / size_x, (float)z / size_z);
			}

		for (z = 0; z < size_z; z++)
			for (x = 0; x < size_x; x++)
			{
				int squareIndex = z * size_x + x;
				int trioffset = squareIndex * 6;

				triangles[trioffset + 0] = z * vsize_x + x;
				triangles[trioffset + 1] = z * vsize_x + x + vsize_x;
				triangles[trioffset + 2] = z * vsize_x + x + vsize_x + 1;

				triangles[trioffset + 3] = z * vsize_x + x;
				triangles[trioffset + 4] = z * vsize_x + x + vsize_x + 1;
				triangles[trioffset + 5] = z * vsize_x + x + 1;
			}

		// Создание мэша
		Mesh mesh = new Mesh
		{
			vertices = vertices,
			triangles = triangles,
			normals = normals,
			uv = uv
		};

		MeshFilter mesh_filter = GetComponent<MeshFilter>();

		mesh_filter.mesh = mesh;
	}
}
