using UnityEngine;

public class EnterSensor : MonoBehaviour {
	public static Programator programator;

	public int x;
	public int y;

	private void OnMouseEnter()
	{
		programator.CellIsSelect(x, y);
	}
}
