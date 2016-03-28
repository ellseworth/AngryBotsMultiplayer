using UnityEngine;

namespace StandartAssets
{
	public  class PlayerMoveController : MonoBehaviour
	{
		public static Vector3 PlaneRayIntersection(Plane plane, Ray ray)
		{
			float dist;
			plane.Raycast(ray, out dist);
			return ray.GetPoint(dist);
		}
		public static Vector3 ScreenPointToWorldPointOnPlane(Vector3 screenPoint, Plane plane, Camera camera)
		{
			// Set up a ray corresponding to the screen position
			Ray ray = camera.ScreenPointToRay(screenPoint);
			// Find out where the ray intersects with the plane
			return PlaneRayIntersection(plane, ray);
		}

		// Objects to drag in
		public Transform character;
		public GameObject cursorPrefab, joystickPrefab;

		// Settings
		public float cameraSmoothing = 0.01f, cameraPreview = 2.0f,
			cursorPlaneHeight = 0, cursorFacingCamera = 0, cursorSmallerWithDistance = 0, cursorSmallerWhenClose = 1;

		// Private memeber data
		protected Transform mainCameraTransform;
		private GameObject joystickRightGO;
		private Quaternion screenMovementSpace;
		protected Vector3 screenMovementForward, screenMovementRight;

		private void Start()
		{
#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
			// Move to right side of screen
			var guiTex : GUITexture = joystickRightGO.GetComponent.<GUITexture> ();
			guiTex.pixelInset.x = Screen.width - guiTex.pixelInset.x - guiTex.pixelInset.width;			
#endif
			// it's fine to calculate this on Start () as the camera is static in rotation
			screenMovementSpace = Quaternion.Euler(0, mainCameraTransform.eulerAngles.y, 0);
			screenMovementForward = screenMovementSpace * Vector3.forward;
			screenMovementRight = screenMovementSpace * Vector3.right;
		}
	}
}