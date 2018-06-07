using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiggAnimation : MonoBehaviour {

	public bool active = false;//активная ли в данный момент эта анимация
	float lifeTime = 0.5f;
	float startTime;

	Animator animator;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		if ((Time.time - startTime) >= lifeTime) {
			gameObject.SetActive(false);
		}
	}

	public void activation() {
		gameObject.SetActive(false);
		startTime = Time.time;
		gameObject.SetActive(true);
	}
}
