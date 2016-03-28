using System.Collections;
using System.Linq;
using UnityEngine;

public class GlowPlane : MonoBehaviour
{
	private Transform playerTransform;
	private Vector3 pos, scale;

	public float minGlow = 0.2f, maxGlow = 0.5f;
	public Color glowColor = Color.white;

	private Material mat;

	private void Start()
	{
		pos = transform.position;
		scale = transform.localScale;
		mat = GetComponent<Renderer>().material;
		enabled = false;
	}
	private void OnDrawGizmos() { DrawGizmosCommon(0.25f); }
	private void OnDrawGizmosSelected() { DrawGizmosCommon(1); }
	private void OnBecameVisible()
	{
		enabled = true;
		StartCoroutine(ChooseNearestPlayer());
	}
	private void OnBecameInvisible() { enabled = false; }
	private void Update()
	{
		if(!playerTransform)
		{
			enabled = false;
			return;
		}
		Vector3 vec = pos - playerTransform.position;
		vec.y = 0.0f;
		var distance = vec.magnitude;
		transform.localScale = Vector3.Lerp(Vector3.one * minGlow, scale, Mathf.Clamp01(distance * 0.35f));
		mat.SetColor("_TintColor", glowColor * Mathf.Clamp(distance * 0.1f, minGlow, maxGlow));
	}

	private void DrawGizmosCommon(float alpha)
	{
		Gizmos.color = new Color(glowColor.r, glowColor.g, glowColor.b, maxGlow * alpha);
		Gizmos.matrix = transform.localToWorldMatrix;
		Vector3 scale = 5.0f * Vector3.Scale(Vector3.one, new Vector3(1, 0, 1));
		Gizmos.DrawCube(Vector3.zero, scale);
		Gizmos.matrix = Matrix4x4.identity;
	}

	private IEnumerator ChooseNearestPlayer()
	{
		WaitForSeconds delay = new WaitForSeconds(1f);
		while(enabled)
		{
			playerTransform = CustomManager.Players == null || CustomManager.Players.Count == 0 ?
				null :
				CustomManager.Players.OrderBy(p => Vector3.Distance(transform.position, p.position)).First();

			yield return delay;
        }
	}
}