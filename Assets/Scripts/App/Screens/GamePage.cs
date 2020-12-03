using UnityEngine;
using UnityEngine.UI;

namespace Prototype.TopDown2DNetworked
{
    public class GamePage : IUIElement
    {
        private GameObject _selfPage;

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private INetworkManager _networkManager;

        private Button _mainMenuButton;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _networkManager = GameClient.Get<INetworkManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/GamePage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _mainMenuButton = _selfPage.transform.Find("Button_Menu").GetComponent<Button>();
            _mainMenuButton.onClick.AddListener(MainMenuButtonOnClickHandler);

            Hide();
        }

        public void Hide()
        {
            _selfPage.SetActive(false);
        }

        public void Show()
        {
            _mainMenuButton.interactable = true;
            _selfPage.SetActive(true);
        }

        public void Update()
        {
			if (_selfPage.activeInHierarchy)
			{
                // use better implementation in future
				if (Input.GetKeyDown(KeyCode.Escape) && _mainMenuButton.interactable)
				{
                    MainMenuButtonOnClickHandler();
                }
			}
        }

        public void Dispose()
        {
        }

        private void MainMenuButtonOnClickHandler()
		{
            _networkManager.LeaveRoom();
            _mainMenuButton.interactable = false;
        }
    }
}
