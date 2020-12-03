using ExitGames.Client.Photon;
using Prototype.TopDown2DNetworked.Common;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Prototype.TopDown2DNetworked
{
	public class NetworkManager : IRoot, INetworkManager, IConnectionCallbacks, IMatchmakingCallbacks, IInRoomCallbacks
	{
		public event Action<Enumerators.NetworkStatus, string> OnJoinRoomStatusChangedEvent;
		public event Action OnRoomLeaveEvent;

		private IAppStateManager _appStateManager;

		private bool _connectedToServer;

		public List<PlayerInfo> PlayersInRoom { get; private set; }

		public void Dispose()
		{
			if (PhotonNetwork.IsConnected)
				PhotonNetwork.Disconnect();
		}

		public void Update()
		{
		}

		public void Init()
		{
			_appStateManager = GameClient.Get<IAppStateManager>();
			PlayersInRoom = new List<PlayerInfo>();

			PhotonNetwork.ConnectUsingSettings();
			PhotonNetwork.AddCallbackTarget(this);

			PhotonNetwork.NetworkingClient.EventReceived += NetworkEventReceivedHandler;
		}

		public void JoinRoom()
		{
			if (_connectedToServer && PhotonNetwork.IsConnectedAndReady)
			{

				if(PhotonNetwork.InRoom)
				{
					OnJoinRoomStatusChangedEvent?.Invoke(Enumerators.NetworkStatus.JoinFailed, "You already in room.");
					return;
				}

				PhotonNetwork.JoinOrCreateRoom(Constants.MultiplayerRoomName, new RoomOptions()
				{
					IsVisible = true,
					MaxPlayers = Constants.MaxPlayersInRoom,
				}, TypedLobby.Default);
			}
			else
			{
				PhotonNetwork.ConnectUsingSettings();
				OnJoinRoomStatusChangedEvent?.Invoke(Enumerators.NetworkStatus.JoinFailed, "Connection to server is closed. Trying to reconnect");
			}
		}

		public void LeaveRoom()
		{
			if (PhotonNetwork.InRoom && _connectedToServer && PhotonNetwork.IsConnectedAndReady)
			{
				PhotonNetwork.LeaveRoom(false);
			}
			else
			{
				_appStateManager.ChangeAppState(Enumerators.AppState.JoinRoom);
			}
		}

		public void SendNetworkEvent(Enumerators.NetworkEvent networkEvent, object[] data, bool all = true)
		{
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = all ? ReceiverGroup.All : ReceiverGroup.Others };
			SendOptions sendOptions = new SendOptions { Reliability = true};
			PhotonNetwork.RaiseEvent((byte)networkEvent, data, raiseEventOptions, sendOptions);
		}

		public void RefreshPlayersList()
		{
			PhotonNetwork.PlayerList.OrderBy(item => item.ActorNumber).ToList().ForEach(OnPlayerEnteredRoom);
		}

		#region network event handlers
		private void NetworkEventReceivedHandler(EventData photonEvent)
		{
			Enumerators.NetworkEvent eventCode = (Enumerators.NetworkEvent)photonEvent.Code;

			switch (eventCode)
			{
				case Enumerators.NetworkEvent.PlayerInfo:
					{
						object[] data = (object[])photonEvent.CustomData;

						var playerInfo = PlayersInRoom.Find(item => item.id == (int)data[0]);

						if (playerInfo != null)
						{
							var player = GameClient.Get<IGameplayManager>().Players.Find(item => item.Id == playerInfo.id);
						}
					}
					break;
				case Enumerators.NetworkEvent.PlayerHit:
					{
						object[] data = (object[])photonEvent.CustomData;

						var playerInfo = PlayersInRoom.Find(item => item.id == (int)data[0]);

						if (playerInfo != null)
						{
							var player = GameClient.Get<IGameplayManager>().Players.Find(item => item.Id == playerInfo.id);
							player?.Hit();
						}
					}
					break;
				case Enumerators.NetworkEvent.PlayerRevive:
					{
						object[] data = (object[])photonEvent.CustomData;

						var playerInfo = PlayersInRoom.Find(item => item.id == (int)data[0]);

						if (playerInfo != null)
						{
							var player = GameClient.Get<IGameplayManager>().Players.Find(item => item.Id == playerInfo.id);
							player?.ReviveNetworked((Vector3)data[1]);
						}
					}
					break;
				case Enumerators.NetworkEvent.PlayerTransform:
					{
						object[] data = (object[])photonEvent.CustomData;

						var playerInfo = PlayersInRoom.Find(item => item.id == (int)data[0]);

						if (playerInfo != null)
						{
							var player = GameClient.Get<IGameplayManager>().Players.Find(item => item.Id == playerInfo.id);
							player?.SetTransformNetworked((Vector3)data[1], (Quaternion)data[2]);
						}
					}
					break;
			}
		}

		public void OnConnected()
		{
		}

		public void OnConnectedToMaster()
		{
			_connectedToServer = true;
		}

		public void OnDisconnected(DisconnectCause cause)
		{
		}

		public void OnRegionListReceived(RegionHandler regionHandler)
		{
		}

		public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
		{
		}

		public void OnCustomAuthenticationFailed(string debugMessage)
		{
		}

		public void OnFriendListUpdate(List<FriendInfo> friendList)
		{
		
		}

		public void OnCreatedRoom()
		{
		
		}

		public void OnCreateRoomFailed(short returnCode, string message)
		{
			
		}

		public void OnJoinedRoom()
		{
			_appStateManager.ChangeAppState(Enumerators.AppState.Game);
			OnJoinRoomStatusChangedEvent?.Invoke(Enumerators.NetworkStatus.Joined, string.Empty);
		}

		public void OnJoinRoomFailed(short returnCode, string message)
		{
			OnJoinRoomStatusChangedEvent?.Invoke(Enumerators.NetworkStatus.JoinFailed, message);
		}

		public void OnJoinRandomFailed(short returnCode, string message)
		{	
		}

		public void OnLeftRoom()
		{
			PlayersInRoom.Clear();

			_appStateManager.ChangeAppState(Enumerators.AppState.JoinRoom);
		}

		public void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
		{
			if (PlayersInRoom.FindAll(item => item.id == newPlayer.ActorNumber).Count > 0)
				return;

			if (newPlayer.IsInactive)
				return;

			PlayerInfo playerInfo = new PlayerInfo()
			{
				id = newPlayer.ActorNumber,
				isLocal = newPlayer.IsLocal,
			};

			PlayersInRoom.Add(playerInfo);

			var player = GameClient.Get<IGameplayManager>().CreateNewPlayer(playerInfo);
		}

		public void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
		{
			var playerInfo = PlayersInRoom.Find(item => item.id == otherPlayer.ActorNumber);

			if (playerInfo != null)
			{
				GameClient.Get<IGameplayManager>().DeletePlayer(playerInfo.id);
				PlayersInRoom.Remove(playerInfo);
			}
		}

		public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
		{
		}

		public void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
		{
		}

		public void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
		{
		}
		#endregion

		public class PlayerInfo
		{
			public int id;
			public bool isLocal;
		}
	}
}