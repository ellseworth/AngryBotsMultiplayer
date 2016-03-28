using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PerFrameRaycast))]
public class LaserScope : MonoBehaviour
{
	public float scrollSpeed = 0.5f, pulseSpeed = 1.5f, noiseSize = 1.0f, maxWidth = 0.5f, minWidth = 0.2f;
	public GameObject pointer = null;

	private LineRenderer lRenderer;
	private float
		//aniTime = 0.0f,
		aniDir = 1.0f;

	private PerFrameRaycast raycast;

	private void Start()
	{
		lRenderer = gameObject.GetComponent<LineRenderer>();
		//aniTime = 0.0f;
		// Change some animation values here and there
		StartCoroutine(ChoseNewAnimationTargetCoroutine());
		raycast = GetComponent<PerFrameRaycast>();
	}

	private IEnumerator ChoseNewAnimationTargetCoroutine()
	{
		while (true)
		{
			aniDir = aniDir * 0.9f + Random.Range(0.5f, 1.5f) * 0.1f;
			yield return null;
			minWidth = minWidth * 0.8f + Random.Range(0.1f, 1.0f) * 0.2f;
			yield return new WaitForSeconds (1.0f + Random.value * 2.0f - 1.0f);
		}
	}

	private void Update()
	{
		GetComponent<Renderer>().material.mainTextureOffset += new Vector2(Time.deltaTime * aniDir * scrollSpeed, 0);
		GetComponent<Renderer>().material.SetTextureOffset("_NoiseTex", new Vector2(-Time.time * aniDir * scrollSpeed, 0));

		float aniFactor = Mathf.PingPong(Time.time * pulseSpeed, 1);
		aniFactor = Mathf.Max(minWidth, aniFactor) * maxWidth;
		lRenderer.SetWidth(aniFactor, aniFactor);

		// Cast a ray to find out the end point of the laser
		RaycastHit hitInfo = raycast.GetHitInfo();
		if (hitInfo.transform)
		{
			lRenderer.SetPosition(1, (hitInfo.distance * Vector3.forward));
			GetComponent<Renderer>().material.mainTextureScale = new Vector2(0.1f * (hitInfo.distance), 0);
			GetComponent<Renderer>().material.SetTextureScale(
				"_NoiseTex", new Vector2(0.1f * hitInfo.distance * noiseSize, noiseSize)
				);

			// Use point and normal to align a nice & rough hit plane
			if (pointer)
			{
				pointer.GetComponent<Renderer>().enabled = true;
				pointer.transform.position = hitInfo.point + (transform.position - hitInfo.point) * 0.01f;
				pointer.transform.rotation = Quaternion.LookRotation(hitInfo.normal, transform.up);
				pointer.transform.eulerAngles = new Vector3(90.0f, pointer.transform.eulerAngles.y, pointer.transform.eulerAngles.z);
			}
		}
		else
		{
			if (pointer) pointer.GetComponent<Renderer>().enabled = false;
			float maxDist = 200.0f;
			lRenderer.SetPosition(1, (maxDist * Vector3.forward));
			Material mat = GetComponent<Renderer>().material;
			mat.mainTextureScale = new Vector2(0.1f * maxDist, mat.mainTextureScale.y); 
			GetComponent<Renderer>().material.SetTextureScale("_NoiseTex", new Vector2(0.1f * (maxDist) * noiseSize, noiseSize));
		}
	}
}