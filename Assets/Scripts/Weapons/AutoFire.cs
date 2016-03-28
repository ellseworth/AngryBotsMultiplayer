using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(PerFrameRaycast))]
public class AutoFire : NetworkBehaviour
{
	public GameObject bulletPrefab;
	public Transform spawnPoint;
	public float frequency = 10, coneAngle = 1.5f;
	public bool firing = false;
	public float damagePerSecond = 20.0f, forcePerSecond = 20.0f, hitSoundVolume = 0.5f;

	public GameObject muzzleFlashFront;

	public float lastFireTime = -1;
	public PerFrameRaycast raycast;

	private AudioSource _audio;

	private void Awake()
	{
		muzzleFlashFront.SetActive(false);

		raycast = GetComponent<PerFrameRaycast>();
		if (spawnPoint == null) spawnPoint = transform;
		_audio = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (firing)
		{
			if (Time.time > lastFireTime + 1 / frequency)
			{
				// Spawn visual bullet
				var coneRandomRotation =
					Quaternion.Euler(Random.Range(-coneAngle, coneAngle), Random.Range(-coneAngle, coneAngle), 0);
				GameObject go =
					Spawner.Spawn(bulletPrefab, spawnPoint.position, spawnPoint.rotation * coneRandomRotation) as GameObject;
				SimpleBullet bullet = go.GetComponent<SimpleBullet>();
				CmdShot(spawnPoint.position, bullet.transform.rotation);

				lastFireTime = Time.time;

				// Find the object hit by the raycast
				RaycastHit hitInfo = raycast.GetHitInfo();
				if (hitInfo.transform)
				{
					// Get the health component of the target if any
					Health targetHealth = hitInfo.transform.GetComponent<Health>();
					if (targetHealth)
					{
						// Apply damage
						targetHealth.OnDamage(damagePerSecond / frequency, -spawnPoint.forward);
					}

					// Get the rigidbody if any
					if (hitInfo.rigidbody)
					{
						// Apply force to the target object at the position of the hit point
						Vector3 force = transform.forward * (forcePerSecond / frequency);
						hitInfo.rigidbody.AddForceAtPosition(force, hitInfo.point, ForceMode.Impulse);
					}

					// Ricochet sound
					AudioClip sound = MaterialImpactManager.GetBulletHitSound(hitInfo.collider.sharedMaterial);
					AudioSource.PlayClipAtPoint(sound, hitInfo.point, hitSoundVolume);

					bullet.dist = hitInfo.distance;
				}
				else
				{
					bullet.dist = 1000;
				}
			}
		}
	}

	public void OnStartFire()
	{
		if (!isLocalPlayer || Time.timeScale == 0) return;
		firing = true;
		muzzleFlashFront.SetActive(true);
		if (_audio) _audio.Play();
	}

	public void OnStopFire()
	{
		firing = false;
		muzzleFlashFront.SetActive(false);
		if (_audio) _audio.Stop();
	}

	[Command]
	private void CmdShot(Vector3 origin, Quaternion direction) { if (isLocalPlayer) RpcShot(origin, direction); }
	[Client] [ClientRpc]
	private void RpcShot(Vector3 origin, Quaternion direction) { Spawner.Spawn(bulletPrefab, origin, direction); }
}