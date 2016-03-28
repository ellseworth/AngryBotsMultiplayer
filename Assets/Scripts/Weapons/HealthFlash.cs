using UnityEngine;

public class HealthFlash : MonoBehaviour
{
	public Health playerHealth;
	public Material healthMaterial;

	private float healthBlink = 1.0f, oneOverMaxHealth = 0.5f;

	private void Start()
	{
		oneOverMaxHealth = 1.0f / playerHealth.maxHealth;
	}

	private void Update()
	{
		float relativeHealth = playerHealth.health * oneOverMaxHealth;
		healthMaterial.SetFloat("_SelfIllumination", relativeHealth * 2.0f * healthBlink);

		if (relativeHealth < 0.45f) healthBlink = Mathf.PingPong(Time.time * 6.0f, 2.0f);
		else healthBlink = 1.0f;
	}
}