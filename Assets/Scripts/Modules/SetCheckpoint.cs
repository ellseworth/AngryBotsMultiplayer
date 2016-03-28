using UnityEngine;

public class SetCheckpoint : MonoBehaviour
{
	[SerializeField] private Transform spawnTransform;

	private void OnTriggerEnter(Collider other)
	{
		SpawnAtCheckpoint checkpointKeeper = other.GetComponent<SpawnAtCheckpoint>() as SpawnAtCheckpoint;
		checkpointKeeper.checkpoint = spawnTransform;
	}
}