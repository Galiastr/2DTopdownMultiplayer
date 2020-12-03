using System;
using System.Collections.Generic;

namespace Prototype.TopDown2DNetworked
{
    public class GameClient
    {
        private static object _sync = new object();

        private static GameClient _Instance;
        public static GameClient Instance
        {
            get
            {
                if (_Instance == null)
                {
                    lock (_sync)
                    {
                        _Instance = new GameClient();
                    }
                }
                return _Instance;
            }
        }

        private Dictionary<Type, IRoot> _managers;

        internal GameClient()
        {
            _managers = new Dictionary<Type, IRoot>()
            {
                { typeof(ILoadObjectsManager), new LoadObjectsManager() },
                { typeof(IAppStateManager), new AppStateManager() },
                { typeof(IUIManager), new UIManager() },
                { typeof(IGameplayManager), new GameplayManager() },
                { typeof(INetworkManager), new NetworkManager() },
            };
        }

        public static T Get<T>()
        {
            return Instance.GetManager<T>();
        }

        public void Dispose()
		{
            foreach (var item in _managers)
            {
                item.Value?.Dispose();
            }
            _managers.Clear();
        }

        public void Init()
		{
            foreach (var item in _managers)
            {
                item.Value?.Init();
            }

            Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.JoinRoom);
        }

        public void Update()
        {
            foreach(var item in _managers)
			{
                item.Value?.Update();
			}
        }

        protected T GetManager<T>()
		{
            _managers.TryGetValue(typeof(T), out IRoot manager);
            return (T)manager;
		}
    }
}