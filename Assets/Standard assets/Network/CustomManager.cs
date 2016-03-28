using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

public class CustomManager : NetworkManager
{
	private static readonly HashSet<NetworkConnection> _connectionList = new HashSet<NetworkConnection>();

	public static float SyncRate { get { return 10; } }
	public static ReadOnlyCollection<Transform> Players { get; private set; }

	/*
	private IEnumerator Start()
	{
		Debug.Log("CustomManager.Start");
		NetworkMatch match = gameObject.AddComponent<NetworkMatch>();
		//===========================================================
		Debug.Log("CustomManager.Start: getting match list...");
		bool isRequestSucceded = false;
		MatchDesc matchToConnect = null;
		do
			yield return match.ListMatches(
				0, 100, string.Empty,
				resp => { isRequestSucceded = resp.success; matchToConnect = resp.matches.FirstOrDefault(m => !m.isPrivate); }
				);
		while (!isRequestSucceded);
		//===========================================================
		NetworkID networkId = NetworkID.Invalid;
		if (matchToConnect == null)
		{
			Debug.Log("CustomManager.Start: match to join not found, creating new one...");
			isRequestSucceded = false;
			do
				yield return match.CreateMatch(
					 "huipizdajigurda" + UnityEngine.Random.Range(0, 1000).ToString(), 100, false, string.Empty,
					 resp => { isRequestSucceded = resp.success; networkId = resp.networkId; }
					 );
			while (!isRequestSucceded);
			Debug.Log("CustomManager.Start: new match created");
		}
		else networkId = matchToConnect.networkId;
		//===========================================================
		Debug.Log("CustomManager.Start: joining to match: " + networkId.ToString() + " ...");
		isRequestSucceded = false;
		do
			yield return match.JoinMatch(
				networkId, string.Empty,
				resp => { isRequestSucceded = resp.success; }
				);
		while (!isRequestSucceded);
		Debug.Log("CustomManager.Start: joined to match");
	}
	*/

	public override void OnClientConnect(NetworkConnection conn)
	{
		base.OnClientConnect(conn);
		_connectionList.Add(conn);
		UpdatePlayers();
	}
	public override void OnClientDisconnect(NetworkConnection conn)
	{
		base.OnClientDisconnect(conn);
		_connectionList.Remove(conn);
		UpdatePlayers();
	}

	private void UpdatePlayers()
	{
		Players = _connectionList.SelectMany(c => c.playerControllers).Select(p => p.gameObject.transform).ToList().AsReadOnly();
	}
}
