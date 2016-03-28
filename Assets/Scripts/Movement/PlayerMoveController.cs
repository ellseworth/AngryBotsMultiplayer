﻿using UnityEngine;
using System.Collections;

public class PlayerMoveController : StandartAssets.PlayerMoveController
{
	// Objects to drag in
	public MovementMotor motor;
#pragma warning disable 649
	[SerializeField] private Joystick joystickLeft, joystickRight;
#pragma warning restore 649

	//Settings
	//[SerializeField] private Vector3 cameraOffset = Vector3.zero;
	[SerializeField] private Vector3 initOffsetToPlayer;

	// Private memeber data
	private Camera mainCamera;
	private Transform cursorObject;
	private Vector3 cameraVelocity = Vector3.zero;
	//private Vector3 cursorScreenPosition;
	private Plane playerMovementPlane;

	private void Awake()
	{
		motor.movementDirection = Vector2.zero;
		motor.facingDirection = Vector2.zero;

		// Set main camera
		mainCamera = Camera.main;
		mainCameraTransform = mainCamera.transform;

		// Ensure we have character set
		// Default to using the transform this component is on
		if (!character) character = transform;

		//initOffsetToPlayer = mainCameraTransform.position - character.position;

#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
		if (joystickPrefab) {
			// Create left joystick
			var joystickLeftGO : GameObject = Instantiate (joystickPrefab) as GameObject;
			joystickLeftGO.name = "Joystick Left";
			joystickLeft = joystickLeftGO.GetComponent.<Joystick> ();
			
			// Create right joystick
			joystickRightGO = Instantiate (joystickPrefab) as GameObject;
			joystickRightGO.name = "Joystick Right";
			joystickRight = joystickRightGO.GetComponent.<Joystick> ();			
		}
#elif !UNITY_FLASH
		if (cursorPrefab) cursorObject = (Instantiate(cursorPrefab) as GameObject).transform;
#endif

		// Save camera offset so we can use it in the first frame
		//cameraOffset = mainCameraTransform.position - character.position;
		// Set the initial cursor position to the center of the screen
		//cursorScreenPosition = new Vector3(0.5f * Screen.width, 0.5f * Screen.height, 0);
		// caching movement plane
		playerMovementPlane = new Plane(character.up, character.position + character.up * cursorPlaneHeight);
	}
	private void OnDisable()
	{
		if (joystickLeft) joystickLeft.enabled = false;
		if (joystickRight) joystickRight.enabled = false;
	}
	private void OnEnable()
	{
		if (joystickLeft) joystickLeft.enabled = true;
		if (joystickRight) joystickRight.enabled = true;
	}
	private void Update()
	{
		// HANDLE CHARACTER MOVEMENT DIRECTION
#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
			motor.movementDirection = joystickLeft.position.x * screenMovementRight + joystickLeft.position.y * screenMovementForward;
#else
		motor.movementDirection =
			Input.GetAxis("Horizontal") * screenMovementRight + Input.GetAxis("Vertical") * screenMovementForward;
#endif

		// Make sure the direction vector doesn't exceed a length of 1
		// so the character can't move faster diagonally than horizontally or vertically
		if (motor.movementDirection.sqrMagnitude > 1) motor.movementDirection.Normalize();

		// HANDLE CHARACTER FACING DIRECTION AND SCREEN FOCUS POINT
		// First update the camera position to take into account how much the character moved since last frame
		//mainCameraTransform.position = Vector3.Lerp (mainCameraTransform.position, character.position + cameraOffset, Time.deltaTime * 45.0f * deathSmoothoutMultiplier);
		// Set up the movement plane of the character, so screenpositions
		// can be converted into world positions on this plane
		//playerMovementPlane = new Plane (Vector3.up, character.position + character.up * cursorPlaneHeight);
		// optimization (instead of newing Plane):

		playerMovementPlane.normal = character.up;
		playerMovementPlane.distance = -character.position.y + cursorPlaneHeight;

		// used to adjust the camera based on cursor or joystick position

		Vector3 cameraAdjustmentVector = Vector3.zero;

#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
		// On mobiles, use the thumb stick and convert it into screen movement space
		motor.facingDirection = joystickRight.position.x * screenMovementRight + joystickRight.position.y * screenMovementForward;	
		cameraAdjustmentVector = motor.facingDirection;		
#else

	#if !UNITY_EDITOR && (UNITY_XBOX360 || UNITY_PS3)
		// On consoles use the analog sticks
		var axisX : float = Input.GetAxis("LookHorizontal");
		var axisY : float = Input.GetAxis("LookVertical");
		motor.facingDirection = axisX * screenMovementRight + axisY * screenMovementForward;
		cameraAdjustmentVector = motor.facingDirection;		
	#else

		// On PC, the cursor point is the mouse position
		Vector3 cursorScreenPosition = Input.mousePosition;
		// Find out where the mouse ray intersects with the movement plane of the player
		Vector3 cursorWorldPosition = ScreenPointToWorldPointOnPlane(cursorScreenPosition, playerMovementPlane, mainCamera);

		float halfWidth = Screen.width / 2.0f, halfHeight = Screen.height / 2.0f, maxHalf = Mathf.Max(halfWidth, halfHeight);

		// Acquire the relative screen position			
		Vector3 posRel = cursorScreenPosition - new Vector3(halfWidth, halfHeight, cursorScreenPosition.z);
		posRel.x /= maxHalf;
		posRel.y /= maxHalf;

		cameraAdjustmentVector = posRel.x * screenMovementRight + posRel.y * screenMovementForward;
		cameraAdjustmentVector.y = 0.0f;

		// The facing direction is the direction from the character to the cursor world position
		motor.facingDirection = (cursorWorldPosition - character.position);
		motor.facingDirection.y = 0;
		// Draw the cursor nicely
		HandleCursorAlignment(cursorWorldPosition);
	#endif
#endif

		// HANDLE CAMERA POSITION
		// Set the target position of the camera to point at the focus point
		Vector3 cameraTargetPosition = character.position + initOffsetToPlayer + cameraAdjustmentVector * cameraPreview;
		// Apply some smoothing to the camera movement
		mainCameraTransform.position =
			Vector3.SmoothDamp(mainCameraTransform.position, cameraTargetPosition, ref cameraVelocity, cameraSmoothing);
		// Save camera offset so we can use it in the next frame
		//cameraOffset = mainCameraTransform.position - character.position;
	}

	public void HandleCursorAlignment(Vector3 cursorWorldPosition)
	{
		if (!cursorObject) return;
		// HANDLE CURSOR POSITION
		// Set the position of the cursor object
		cursorObject.position = cursorWorldPosition;
#if !UNITY_FLASH
		// Hide mouse cursor when within screen area, since we're showing game cursor instead
		Cursor.visible =
			Input.mousePosition.x < 0 || Input.mousePosition.x > Screen.width ||
			Input.mousePosition.y < 0 || Input.mousePosition.y > Screen.height;
#endif

		// HANDLE CURSOR ROTATION
		Quaternion cursorWorldRotation = cursorObject.rotation;
		if (motor.facingDirection != Vector3.zero) cursorWorldRotation = Quaternion.LookRotation(motor.facingDirection);
		// Calculate cursor billboard rotation
		Vector3 cursorScreenspaceDirection =
			Input.mousePosition - mainCamera.WorldToScreenPoint(transform.position + character.up * cursorPlaneHeight);
		cursorScreenspaceDirection.z = 0;
		Quaternion cursorBillboardRotation =
			mainCameraTransform.rotation * Quaternion.LookRotation(cursorScreenspaceDirection, -Vector3.forward);
		// Set cursor rotation
		cursorObject.rotation = Quaternion.Slerp(cursorWorldRotation, cursorBillboardRotation, cursorFacingCamera);

		// HANDLE CURSOR SCALING
		// The cursor is placed in the world so it gets smaller with perspective.
		// Scale it by the inverse of the distance to the camera plane to compensate for that.
		float compensatedScale = 0.1f * Vector3.Dot(cursorWorldPosition - mainCameraTransform.position, mainCameraTransform.forward);
		// Make the cursor smaller when close to character
		float cursorScaleMultiplier = Mathf.Lerp(0.7f, 1.0f, Mathf.InverseLerp(0.5f, 4.0f, motor.facingDirection.magnitude));
		// Set the scale of the cursor
		cursorObject.localScale = Vector3.one * Mathf.Lerp(compensatedScale, 1, cursorSmallerWithDistance) * cursorScaleMultiplier;

		// DEBUG - REMOVE LATER
		if (Input.GetKey(KeyCode.O)) cursorFacingCamera += Time.deltaTime * 0.5f;
		if (Input.GetKey(KeyCode.P)) cursorFacingCamera -= Time.deltaTime * 0.5f;
		cursorFacingCamera = Mathf.Clamp01(cursorFacingCamera);

		if (Input.GetKey(KeyCode.K)) cursorSmallerWithDistance += Time.deltaTime * 0.5f;
		if (Input.GetKey(KeyCode.L)) cursorSmallerWithDistance -= Time.deltaTime * 0.5f;
		cursorSmallerWithDistance = Mathf.Clamp01(cursorSmallerWithDistance);
	}
}

