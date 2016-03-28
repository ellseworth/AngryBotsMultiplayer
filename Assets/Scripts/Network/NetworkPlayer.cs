using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

internal class NetworkPlayer : NetworkBehaviour
{
	//private CachedComponent<PlayerMoveController> _playerMoveController;
	//private CachedComponent<NetworkIdentity> _netId;

	private void Awake()
	{
		//_playerMoveController = new CachedComponent<PlayerMoveController>(gameObject);
		//_netId = new CachedComponent<NetworkIdentity>(gameObject);
		//_playerMoveController.Value.enabled = false;
	}
	
	public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer();
		//_playerMoveController.Value.enabled = true;
    }
	public override void OnStartClient()
	{
		base.OnStartClient();
		//_playerMoveController.Value.enabled = false;
	}
}