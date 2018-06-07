using UnityEngine;
using UnityEngine.UI;

public class FPS : MonoBehaviour
{
	public float updateInterval = 0.5F;
	public Text fpsText;
	double lastInterval;
	int frames = 0;
	float fps;

	void Start()
	{
		lastInterval = Time.realtimeSinceStartup;
		frames = 0;
	}

	void Update()
	{
		++frames;

		float timeNow = Time.realtimeSinceStartup;
		if (timeNow > lastInterval + updateInterval)
		{
			fps = frames / (float)(timeNow - lastInterval);
			frames = 0;
			lastInterval = timeNow;
			fpsText.text = "fps:" + fps.ToString("f");
			if (fps > 60) {
				fpsText.color = new Color(0,1,0);
			}
			else {
				fpsText.color = new Color(1f - (0.0166f * fps), 0.0166f * fps, 0);
			}
		}
	}
}