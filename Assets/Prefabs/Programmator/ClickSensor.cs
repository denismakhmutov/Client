using UnityEngine;

public class ClickSensor : MonoBehaviour {
	public AllProgramBlocksController blocksController;
	void OnMouseDown()
	{
		blocksController.keyReadBlock();
	}
}
