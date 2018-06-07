using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class otherplayercontroller : MonoBehaviour {

	public int skin;
	public Vector3 OtherPlayerPos;
	public char Angle;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		#region Повороты
		switch (Angle)
		{
			case 'u':
				transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
				break;
			case 'd':
				transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
				break;
			case 'l':
				transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
				break;
			case 'r':
				transform.rotation = Quaternion.Euler(new Vector3(0, 0, 270));
				break;
		}
		#endregion
		#region Перемещение
		transform.position = Vector3.MoveTowards(transform.position, OtherPlayerPos, 15f * Time.deltaTime);
		#endregion
	}
}
