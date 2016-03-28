using UnityEngine;

public class SpawnAtCheckpoint : MonoBehaviour
{
	public Transform checkpoint;

	public void OnSignal()
	{
		transform.position = checkpoint.position;
		transform.rotation = checkpoint.rotation;

		ResetHealthOnAll();
	}

	public static void ResetHealthOnAll()
	{
		Health[] healthObjects = FindObjectsOfType<Health>();
		foreach (Health health in healthObjects)
		{
			health.dead = false;
			health.health = health.maxHealth;
		}
	}
}
