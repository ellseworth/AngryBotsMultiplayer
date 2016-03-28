using UnityEngine;

public class PerFrameRaycast : MonoBehaviour
{
	private RaycastHit hitInfo;
	private Transform tr;

	private void Awake ()
	{
		tr = transform;
	}

	private void Update ()
	{
		// Cast a ray to find out the end point of the laser
		hitInfo = new RaycastHit ();
		Physics.Raycast (tr.position, tr.forward, out hitInfo);
	}

	public RaycastHit GetHitInfo ()
	{
		return hitInfo;
	}
}