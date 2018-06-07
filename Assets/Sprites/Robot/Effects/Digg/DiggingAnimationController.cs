using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiggingAnimationController : MonoBehaviour {
	public GameObject diggingAnimationPref;

	GameObject diggingAnimation;
	// Use this for initialization
	void Start () {
		diggingAnimation = Instantiate(diggingAnimationPref,new Vector3(322.5f,309.5f,-5), Quaternion.identity);
		diggingAnimation.SetActive(false);
	}

	int i = 0;
	// Update is called once per frame
	void Update () {
	
	}

	public void AnimActive(Vector3 coordinate,char dir) {
		diggingAnimation.SetActive(true);
		diggingAnimation.transform.position = coordinate;
		Quaternion quaternion = new Quaternion(0,0,0,0);
		switch (dir)
		{
			case 'U': quaternion.eulerAngles = new Vector3(0, 0, 0); break;
			case 'D': quaternion.eulerAngles = new Vector3(0, 0, 180); break;
			case 'R': quaternion.eulerAngles = new Vector3(0, 0, 270); break;
			case 'L': quaternion.eulerAngles = new Vector3(0, 0, 90); break;
		}
		diggingAnimation.transform.rotation = quaternion;

		diggingAnimation.GetComponent<DiggAnimation>().activation();
	}


}
