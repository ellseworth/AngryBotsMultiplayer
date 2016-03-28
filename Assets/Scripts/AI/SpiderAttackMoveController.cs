using UnityEngine;

public class SpiderAttackMoveController : MonoBehaviour
{
	// Public member data
	public MovementMotor motor;

	public float targetDistanceMin = 2.0f, targetDistanceMax = 3.0f, proximityDistance = 4.0f, damageRadius = 5.0f,
		proximityBuildupTime = 2.0f, proximityOfNoReturn = 0.6f, damageAmount = 30.0f;
	public Renderer proximityRenderer;
	public AudioSource audioSource;
	public SelfIlluminationBlink[] blinkComponents;
	public GlowPlane blinkPlane;

	public GameObject intentionalExplosion;
	public MonoBehaviour animationBehaviour;

	// Private memeber data
	private AI ai;

	private Transform character, player;

	private bool inRange = false;
	private float nextRaycastTime = 0, lastRaycastSuccessfulTime = 0, proximityLevel = 0, lastBlinkTime = 0, noticeTime = 0;

	private void Awake()
	{
		character = motor.transform;
		player = GameObject.FindWithTag("Player").transform;
		ai = transform.parent.GetComponentInChildren<AI>();
		if (blinkComponents == null)
			blinkComponents = transform.parent.GetComponentsInChildren<SelfIlluminationBlink>();
	}

	private void OnEnable()
	{
		inRange = false;
		nextRaycastTime = Time.time;
		lastRaycastSuccessfulTime = Time.time;
		noticeTime = Time.time;
		animationBehaviour.enabled = true;
		if (blinkPlane) blinkPlane.GetComponent<Renderer>().enabled = false;
	}

	private void OnDisable()
	{
		if (proximityRenderer == null) Debug.LogError("proximityRenderer is null", this);
		else if (proximityRenderer.material == null) Debug.LogError("proximityRenderer.material is null", this);
		else proximityRenderer.material.color = Color.white;
		if (blinkPlane) blinkPlane.GetComponent<Renderer>().enabled = false;
	}

	private void Update()
	{
		if (Time.time < noticeTime + 0.7f)
		{
			motor.movementDirection = Vector3.zero;
			return;
		}

		// Calculate the direction from the player to this character
		Vector3 playerDirection = player.position - character.position;
		playerDirection.y = 0;
		float playerDist = playerDirection.magnitude;
		playerDirection /= playerDist;

		// Set this character to face the player,
		// that is, to face the direction from this character to the player
		//motor.facingDirection = playerDirection;

		if (inRange && playerDist > targetDistanceMax) inRange = false;
		if (!inRange && playerDist < targetDistanceMin) inRange = true;

		if (inRange) motor.movementDirection = Vector3.zero;
		else motor.movementDirection = playerDirection;

		if ((playerDist < proximityDistance && Time.time < lastRaycastSuccessfulTime + 1) || proximityLevel > proximityOfNoReturn)
			proximityLevel += Time.deltaTime / proximityBuildupTime;
		else
			proximityLevel -= Time.deltaTime / proximityBuildupTime;

		proximityLevel = Mathf.Clamp01(proximityLevel);
		//proximityRenderer.material.color = Color.Lerp (Color.blue, Color.red, proximityLevel);
		if (proximityLevel == 1) Explode();

		if (Time.time > nextRaycastTime)
		{
			nextRaycastTime = Time.time + 1;
			if (ai.CanSeePlayer()) lastRaycastSuccessfulTime = Time.time;
			else
			{
				if (Time.time > lastRaycastSuccessfulTime + 2) ai.OnLostTrack();
			}
		}

		float deltaBlink = 1 / Mathf.Lerp(2, 15, proximityLevel);
		if (Time.time > lastBlinkTime + deltaBlink)
		{
			lastBlinkTime = Time.time;
			proximityRenderer.material.color = Color.red;
			if (audioSource.enabled) audioSource.Play();
			foreach (SelfIlluminationBlink comp in blinkComponents) comp.Blink();
			if (blinkPlane) blinkPlane.GetComponent<Renderer>().enabled = !blinkPlane.GetComponent<Renderer>().enabled;
		}
		if (Time.time > lastBlinkTime + 0.04f) proximityRenderer.material.color = Color.white;
	}

	private void Explode()
	{
		float damageFraction = 1 - (Vector3.Distance(player.position, character.position) / damageRadius);

		Health targetHealth = player.GetComponent<Health>();
		if (targetHealth)
		{
			// Apply damage
			targetHealth.OnDamage(damageAmount * damageFraction, character.position - player.position);
		}
		player.GetComponent<Rigidbody>().AddExplosionForce(10, character.position, damageRadius, 0.0f, ForceMode.Impulse);
		Spawner.Spawn(intentionalExplosion, transform.position, Quaternion.identity);
		Spawner.Destroy(character.gameObject);
	}
}