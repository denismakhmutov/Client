using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSettings : MonoBehaviour {

	public GameObject settingsPanel;

	public bool gunRadius;
	public bool autoRepair;
	public bool localChat;
	public bool alarms;
	public bool teleportationOnBuriedTelePad;
	public bool fullScreen;
	public bool music;
	public bool sound;

	// Use this for initialization
	void Start () {
		gunRadius = false;
		autoRepair = false;
		localChat = true;
		alarms = true;
		teleportationOnBuriedTelePad = true;
		fullScreen = false;
		music = true;
		sound = true;
}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void PanelSettings() {
		settingsPanel.SetActive(!settingsPanel.activeSelf);
	}

	public void GunRadius()
	{
		gunRadius = !gunRadius;
	}

	public void AutoRepair()
	{
		autoRepair = !autoRepair;
	}

	public void LocalChat()
	{
		localChat = !localChat;
	}

	public void Alarms()
	{
		alarms = !alarms;
	}

	public void TeleportationOnBuriedTelePad()
	{
		teleportationOnBuriedTelePad = !teleportationOnBuriedTelePad;
	}

	public void ResetRespawn()
	{
		
	}

	public void QuitClan()
	{

	}

	public void CoordinatesOfClanTower()
	{

	}

	public void FullScreen()
	{
		fullScreen = !fullScreen;
	}

	public void Music()
	{
		music = !music;
	}

	public void Sound()
	{
		sound = !sound;
	}
}
