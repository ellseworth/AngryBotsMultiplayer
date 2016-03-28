using UnityEngine;

public class SimpleBullet : MonoBehaviour
{
	public float speed = 10, lifeTime = 0.5f, dist = 10000;

	private float spawnTime = 0.0f;
	private Transform tr;

	private void OnEnable()
	{
		tr = transform;
		spawnTime = Time.time;
	}

	private void Update()
	{
		tr.position += tr.forward * speed * Time.deltaTime;
		dist -= speed * Time.deltaTime;
		if (Time.time > spawnTime + lifeTime || dist < 0)
		{
			Spawner.Destroy(gameObject);
		}
	}
}