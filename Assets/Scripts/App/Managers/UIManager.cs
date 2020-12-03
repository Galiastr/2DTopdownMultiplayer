using Prototype.TopDown2DNetworked.Common;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Prototype.TopDown2DNetworked
{
    public class UIManager : IRoot, IUIManager
    {
        private IAppStateManager _appStateManager;

        private List<IUIElement> _uiPages;

        public IUIElement CurrentPage { get; set; }

        public CanvasScaler CanvasScaler { get; set; }
        public GameObject Canvas { get; set; }

        public void Dispose()
        {
            foreach (var page in _uiPages)
                page.Dispose();
        }

        public void Init()
        {
            _appStateManager = GameClient.Get<IAppStateManager>();
			_appStateManager.AppStateChangedEvent += AppStateChangedEventHandler;

            Canvas = GameObject.Find("Canvas");
            CanvasScaler = Canvas.GetComponent<CanvasScaler>();

            _uiPages = new List<IUIElement>();
            _uiPages.Add(new JoinRoomPage());
            _uiPages.Add(new GamePage());    

            foreach (var page in _uiPages)
                page.Init();
        }

        public void Update()
        {
            foreach (var page in _uiPages)
                page.Update();
        }

        public void HideAllPages()
        {
            foreach (var _page in _uiPages)
            {
                _page.Hide();
            }
        }

        public void SetPage<T>(bool hideAll = false) where T : IUIElement
        {
            if (hideAll)
            {
                HideAllPages();
            }
            else
            {
                if (CurrentPage != null)
                    CurrentPage.Hide();
            }

            foreach (var _page in _uiPages)
            {
                if (_page is T)
                {
                    CurrentPage = _page;
                    break;
                }
            }
            CurrentPage.Show();
        }

        public IUIElement GetPage<T>() where T : IUIElement
        {
            IUIElement page = null;
            foreach (var _page in _uiPages)
            {
                if (_page is T)
                {
                    page = _page;
                    break;
                }
            }

            return page;
        }

        private void AppStateChangedEventHandler(Enumerators.AppState appState)
        {
            switch (appState)
            {
                case Enumerators.AppState.Game:
                    SetPage<GamePage>();
                    break;
                case Enumerators.AppState.JoinRoom:
                    SetPage<JoinRoomPage>();
                    break;
            }
        }
    }
}