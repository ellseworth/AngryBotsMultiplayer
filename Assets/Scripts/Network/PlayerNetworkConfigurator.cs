using System;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(NetworkIdentity))]
public class PlayerNetworkConfigurator : NetworkBehaviour
{
	[SerializeField] private MonoBehaviour[] _ownerScripts;
	[SerializeField] private MonoBehaviour[] _avatarScripts;

	private void Start()
	{
		Array.ForEach(_ownerScripts, s => { if (s) s.enabled = isLocalPlayer; });
		Array.ForEach(_avatarScripts, s => { if (s) s.enabled = !isLocalPlayer; });
	}
}
