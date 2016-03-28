using UnityEngine;

public class PatrolPoint : MonoBehaviour
{
	public Vector3 position;

	private void Awake()
	{
		position = transform.position;
	}
}