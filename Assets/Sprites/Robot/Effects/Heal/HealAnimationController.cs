using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealAnimationController : MonoBehaviour {

	public GameObject healAnimationPref;

	GameObject healAnimation;
	// Use this for initialization
	void Start()
	{
		healAnimation = Instantiate(healAnimationPref, new Vector3(322.5f, 309.5f, -5), Quaternion.identity);
		healAnimation.SetActive(false);
	}

	// Update is called once per frame
	void Update()
	{
		
	}

	public void AnimActive(Transform robotTransform)
	{
		healAnimation.SetActive(true);
		healAnimation.transform.position = robotTransform.position;

		healAnimation.GetComponent<HealAnimation>().Activation(robotTransform);
	}
}
