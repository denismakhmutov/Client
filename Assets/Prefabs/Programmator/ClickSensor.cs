using UnityEngine;

public class ClickSensor : MonoBehaviour {
	public AllProgramBlocksController blocksController;

	private void OnMouseUp()
	{
		blocksController.keyReadBlock();
	}
}
