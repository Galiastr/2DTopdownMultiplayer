using Prototype.TopDown2DNetworked.Common;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Prototype.TopDown2DNetworked
{
	public class GameplayManager : IRoot, IGameplayManager
	{
		private IAppStateManager _appStateManager;

		private INetworkManager _networkManager;

		private List<Player> _players;

		private Transform _parentOfPlayers;

		private bool _gameplayStarted;

		private List<int> _availableViews = new List<int>();

		public List<Player> Players => _players;

		public void Dispose()
		{
		}

		public void Init()
		{
			_appStateManager = GameClient.Get<IAppStateManager>();
			_networkManager = GameClient.Get<INetworkManager>();
			
			_appStateManager.AppStateChangedEvent += AppStateChangedEventHandler;

			SceneManager.sceneLoaded += SceneLoadedHandler;
		}

		public void Update()
		{
			if (_gameplayStarted)
			{
				foreach (var player in _players)
				{
					player.Update();
				}
			}
		}

		public void StartGameplay()
		{
			if (_gameplayStarted)
				return;

			_parentOfPlayers = GameObject.Find("Level/Players").transform;

			_players = new List<Player>();
			_availableViews.AddRange(new int[] { 0, 1, 2 });

			_gameplayStarted = true;

			_networkManager.RefreshPlayersList();
		}

		public void StopGameplay()
		{
			if (!_gameplayStarted)
				return;

			foreach (var player in _players)
			{
				player.Dispose();
			}
			_players.Clear();

			_gameplayStarted = false;

			SceneManager.LoadScene("Main");
		}

		public Player CreateNewPlayer(NetworkManager.PlayerInfo player)
		{
			Player pl = new Player(player.id, _parentOfPlayers, player.isLocal, _availableViews[0]);

			_players.Add(pl);

			_availableViews.RemoveAt(0);

			return pl;
		}

		public void DeletePlayer(int playerId)
		{
			var player = _players.Find(item => item.Id == playerId);

			if (player != null)
			{
				player.Dispose();
				_players.Remove(player);

				_availableViews.Insert(0, player.Index);
			}
		}

		private void AppStateChangedEventHandler(Enumerators.AppState appState)
		{
			switch (appState)
			{
				case Enumerators.AppState.Game:
					SceneManager.LoadScene("GameLevel_0");
					break;
				case Enumerators.AppState.JoinRoom:
					StopGameplay();			
					break;
			}
		}

		private void SceneLoadedHandler(Scene scene, LoadSceneMode mode)
		{
			// todo user better implementation
			if(scene.name.StartsWith("GameLevel_"))
			{
				StartGameplay();
			}
		}
	}
}