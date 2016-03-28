using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class NetPlayerSyncronizer : NetworkBehaviour
{
	public float? LastUpdateTime { get; private set; }
	public Vector3 Position { get; private set; }
	public Vector2 Velocity { get; private set; }
	public float Rotation { get; private set; }
	public Vector3 Aim { get; private set; }

	public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer();
		StartCoroutine(UpdateFlowDataCoroutine());
	}

	[Command]
	private void CmdUpdateTransform(Vector3 position, Vector2 velocity, float rotation, Vector3 aim)
	{
		if (isLocalPlayer)
		{
			LastUpdateTime = null;
			return;
		}
		LastUpdateTime = Time.realtimeSinceStartup;
		Position = position;
		Velocity = velocity;
		Rotation = rotation;
		Aim = aim;
	}

	private IEnumerator UpdateFlowDataCoroutine()
	{
		while(enabled && isLocalPlayer)
		{
			CmdUpdateTransform(transform.position, Vector2.zero, transform.eulerAngles.y, transform.position + transform.forward);
			yield return new WaitForSeconds(1f / CustomManager.SyncRate);
		}
	}
}
