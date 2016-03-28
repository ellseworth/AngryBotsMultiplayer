using UnityEngine;

public class MechAttackMoveController : MonoBehaviour
{
	// Public member data
	public MovementMotor motor;
	public Transform head;

	public float targetDistanceMin = 3.0f, targetDistanceMax = 4.0f;

	public MonoBehaviour[] weaponBehaviours;
	public float fireFrequency = 2;

	// Private memeber data
	private AI ai;
	private Transform character, player;
	private bool inRange = false;
	private float nextRaycastTime = 0, lastRaycastSuccessfulTime = 0, noticeTime = 0;

	private bool firing = false;
	private float lastFireTime = -1;
	private int nextWeaponToFire = 0;

	private void Awake ()
	{
		character = motor.transform;
		player = GameObject.FindWithTag ("Player").transform;
		ai = transform.parent.GetComponentInChildren<AI>();
	}

	private void OnEnable ()
	{
		inRange = false;
		nextRaycastTime = Time.time + 1;
		lastRaycastSuccessfulTime = Time.time;
		noticeTime = Time.time;
	}

	private void OnDisable ()
	{
		Shoot (false);
	}

	private void Shoot (bool state)
	{
		firing = state;
	}

	private void Fire ()
	{
		if (weaponBehaviours[nextWeaponToFire])
		{
			weaponBehaviours[nextWeaponToFire].SendMessage ("Fire");
			nextWeaponToFire = (nextWeaponToFire + 1) % weaponBehaviours.Length;
			lastFireTime = Time.time;
		}
	}

	private void Update ()
	{
		// Calculate the direction from the player to this character
		Vector3 playerDirection = (player.position - character.position);
		playerDirection.y = 0;
		float playerDist = playerDirection.magnitude;
		playerDirection /= playerDist;
	
		// Set this character to face the player,
		// that is, to face the direction from this character to the player
		motor.facingDirection = playerDirection;
	
		// For a short moment after noticing player,
		// only look at him but don't walk towards or attack yet.
		if (Time.time < noticeTime + 1.5f)
		{
			motor.movementDirection = Vector3.zero;
			return;
		}
	
		if (inRange && playerDist > targetDistanceMax) inRange = false;
		if (!inRange && playerDist < targetDistanceMin) inRange = true;
	
		if (inRange) motor.movementDirection = Vector3.zero;
		else motor.movementDirection = playerDirection;
	
		if (Time.time > nextRaycastTime)
		{
			nextRaycastTime = Time.time + 1;
			if (ai.CanSeePlayer ())
			{
				lastRaycastSuccessfulTime = Time.time;
				if (IsAimingAtPlayer ()) Shoot (true);
				else Shoot (false);
			}
			else
			{
				Shoot (false);
				if (Time.time > lastRaycastSuccessfulTime + 5) ai.OnLostTrack ();
			}
		}
	
		if (firing)
		{
			if (Time.time > lastFireTime + 1 / fireFrequency) Fire ();
		}
	}

	private bool IsAimingAtPlayer ()
	{
		Vector3 playerDirection = player.position - head.position;
		playerDirection.y = 0;
		return Vector3.Angle (head.forward, playerDirection) < 15;
	}
}