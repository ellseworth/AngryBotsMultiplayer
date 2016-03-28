using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Collider), typeof(NetworkStartPosition), typeof(NetworkIdentity))]
public class PlayerSpawnPositionController : NetworkBehaviour
{
	private readonly HashSet<int> _blockers = new HashSet<int>();

	[SerializeField] private LayerMask _trackMask;

	private Collider _trigger;
	private NetworkStartPosition _spawn;

	private void Awake()
	{
		_trigger = GetComponent<Collider>();
		_trigger.isTrigger = true;
		_trigger.enabled = false;
		_spawn = GetComponent<NetworkStartPosition>();
	}
	private void OnTriggerEnter(Collider other)
	{
		GameObject otherGO = other.gameObject;
        int colliderLayer = otherGO.layer;
		if ((colliderLayer & _trackMask.value) == colliderLayer)
		{
			_blockers.Add(otherGO.GetInstanceID());
			UpdateSpawner();
		}
	}
	private void OnTriggerExit(Collider other)
	{
		GameObject otherGO = other.gameObject;
		int colliderLayer = otherGO.layer;
		if ((colliderLayer & _trackMask.value) == colliderLayer)
		{
			_blockers.Remove(otherGO.GetInstanceID());
			UpdateSpawner();
		}
	}
	public override void OnStartClient()
	{
		base.OnStartClient();
		_trigger.enabled = false;
		_blockers.Clear();
	}
	public override void OnStartServer()
	{
		base.OnStartServer();
		_trigger.enabled = true;
	}

	private void UpdateSpawner()
	{
		RpcSwitch(_spawn.enabled = _blockers.Count == 0);
	}

	[ClientRpc]
	private void RpcSwitch(bool on) { _spawn.enabled = on; }
}
