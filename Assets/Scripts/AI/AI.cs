using UnityEngine;

public class AI : MonoBehaviour
{ 
	// Public member data
	public MonoBehaviour behaviourOnSpotted;
	public AudioClip soundOnSpotted;
	public MonoBehaviour behaviourOnLostTrack;

	// Private memeber data
	private Transform character;
	private Transform player;
	private bool insideInterestArea = true;

	private CachedComponent<AudioSource> _audioSource;

	private void Awake ()
	{
		_audioSource = new CachedComponent<AudioSource>(gameObject);
		character = transform;
		player = GameObject.FindWithTag ("Player").transform;
	}

	private void OnEnable ()
	{
		behaviourOnLostTrack.enabled = true;
		behaviourOnSpotted.enabled = false;
	}

	private void OnTriggerEnter (Collider other)
	{
		if (other.transform == player && CanSeePlayer ()) OnSpotted ();
	}

	public void OnEnterInterestArea ()
	{
		insideInterestArea = true;
	}

	public void OnExitInterestArea ()
	{
		insideInterestArea = false;
		OnLostTrack ();
	}

	private void OnSpotted ()
	{
		if (!insideInterestArea) return;
		if (!behaviourOnSpotted.enabled)
		{
			behaviourOnSpotted.enabled = true;
			behaviourOnLostTrack.enabled = false;
		
			if (_audioSource.Value && soundOnSpotted)
			{
				_audioSource.Value.clip = soundOnSpotted;
				_audioSource.Value.Play ();
			}
		}
	}

	public void OnLostTrack ()
	{
		if (!behaviourOnLostTrack.enabled)
		{
			behaviourOnLostTrack.enabled = true;
			behaviourOnSpotted.enabled = false;
		}
	}

	public bool CanSeePlayer ()
	{
		Vector3 playerDirection = player.position - character.position;
		RaycastHit hit;
		Physics.Raycast (character.position, playerDirection, out hit, playerDirection.magnitude);
		if (hit.collider && hit.collider.transform == player) return true;
		return false;
	}
}