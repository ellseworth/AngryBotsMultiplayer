using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

[RequireComponent(typeof(Rigidbody))]
public class PlayerNetworkMovement : NetworkBehaviour
{
	[SerializeField] private float _correctDistance = 1, _interpolateRate = 0.5f, _interpolatePositionItertia = 3;

	private Transform _transform;
	private Rigidbody _rigidBody;

	private Vector3 _recievedPosition, _guessPosition;
	private float _receivedRotation, _guessRotation, _receiveTime, _updateDelay;

	private void Awake()
	{
		_transform = transform;
		_rigidBody = GetComponent<Rigidbody>();
	}
	private IEnumerator Start()
	{
		_updateDelay = 1f / CustomManager.SyncRate;
        if (!isLocalPlayer) yield break;
		WaitForSeconds delay = new WaitForSeconds(_updateDelay);
		while (enabled)
		{
			CmdSendSync(_transform.position, _transform.eulerAngles.y);
			yield return delay;
		}
	}

	private void Update()
	{
		if (isLocalPlayer) return;
		if(Vector3.Distance(_recievedPosition, _transform.position) > _correctDistance)
		{
			_transform.position = _recievedPosition;
			_transform.eulerAngles = new Vector3(0, _receivedRotation, 0);
		}

		float fromReceiveTime = Time.realtimeSinceStartup - _receiveTime,
			lerpRateBase = fromReceiveTime / _updateDelay,
			lerpRate = lerpRateBase / _interpolatePositionItertia;
		lerpRate = Mathf.Clamp01(lerpRate);

		Debug.DrawLine(_transform.position, _guessPosition, Color.green);
		_transform.position = Vector3.Lerp(_transform.position, _guessPosition, lerpRate);

		float lerpedY = Mathf.LerpAngle(_transform.eulerAngles.y, _guessRotation, lerpRateBase);
		_transform.eulerAngles = new Vector3(0, lerpedY, 0);
	}

	[Command] private void CmdSendSync(Vector3 pos, float rot) { RpcReceiveSync(pos, rot); }

	[Client] [ClientRpc]
	private void RpcReceiveSync(Vector3 pos, float rot)
	{
		_receiveTime = Time.realtimeSinceStartup;

		Vector3 lastPos = _recievedPosition, posDelta = pos - _recievedPosition;
		_recievedPosition = pos;
		posDelta *= (_interpolateRate * _interpolatePositionItertia);
		_guessPosition = pos + posDelta;
		Debug.DrawLine(lastPos, _guessPosition, Color.yellow, 3);

		float rotDelta = rot - _receivedRotation;
		if (rotDelta > 180) rotDelta -= 360;
		else if (rotDelta < -180) rotDelta += 360;
		_receivedRotation = rot;
		rotDelta *= _interpolateRate;
		rotDelta %= 180;
		_guessRotation = rot + rotDelta;
	}
	private void FixedUpdate()
	{
		if (isLocalPlayer) return;
		_rigidBody.velocity = new Vector3(0, _rigidBody.velocity.y, 0);
		_rigidBody.angularVelocity = Vector3.zero;
	}
}
