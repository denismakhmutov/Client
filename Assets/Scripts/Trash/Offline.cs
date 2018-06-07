using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Offline : MonoBehaviour {
	public CharactControl charactControl;
	public ChankLoad chunkLoad;
	public GUIUPController upController;
	public GeologyFall geologyFall;

	public bool _charactControl;
	public bool _chunkLoad;
	public bool _upController;
	public bool _geologyFall;
	
	// Update is called once per frame
	void Update () {
		if(Time.frameCount % 10 == 0){ 
			charactControl.offline = _charactControl;
			chunkLoad.offline = _chunkLoad;
			upController.offline = _upController;
			geologyFall.offline = _geologyFall;
		}
	}
}
