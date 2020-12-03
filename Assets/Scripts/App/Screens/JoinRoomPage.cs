using Prototype.TopDown2DNetworked.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Prototype.TopDown2DNetworked
{
    public class JoinRoomPage : IUIElement
    {
        private GameObject _selfPage;

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private INetworkManager _networkManager;

        private Button _joinRoomButton,
                       _joinRoomFailedPanelCloseButton;

        private Text _joinRoomFailedText;

        private GameObject _joinRoomFailedPanel;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _networkManager = GameClient.Get<INetworkManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/JoinRoomPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);


            _joinRoomFailedText = _selfPage.transform.Find("Panel_JoinRoomFailed/Image_Background/Text_Info").GetComponent<Text>();
            _joinRoomFailedPanel = _selfPage.transform.Find("Panel_JoinRoomFailed").gameObject;

            _joinRoomButton = _selfPage.transform.Find("Button_JoinRoom").GetComponent<Button>();
            _joinRoomFailedPanelCloseButton = _selfPage.transform.Find("Panel_JoinRoomFailed/Button_Close").GetComponent<Button>();

            _joinRoomButton.onClick.AddListener(JoinRoomButtonOnClickHandler);
            _joinRoomFailedPanelCloseButton.onClick.AddListener(JoinRoomFailedPanelCloseButtonOnClickHandler);

            _networkManager.OnJoinRoomStatusChangedEvent += OnJoinRoomStatusChangedEventHandler;

            Hide();
		}

        public void Hide()
        {
            _selfPage.SetActive(false);
        }

        public void Show()
        {
            ResetScreen();

            _selfPage.SetActive(true);
        }

		public void Update()
        {
            if (_selfPage.activeInHierarchy)
            {
                if (Input.GetKeyDown(KeyCode.Escape) )
                {
                    Application.Quit();
                }
            }
        }

        public void Dispose()
        {
            if (_selfPage == null)
                return;

            Object.Destroy(_selfPage);
            _selfPage = null;
            _networkManager.OnJoinRoomStatusChangedEvent -= OnJoinRoomStatusChangedEventHandler;
            _joinRoomButton = null;
        }

        private void ResetScreen()
		{
            _joinRoomFailedPanel.SetActive(false);
            _joinRoomFailedText.text = string.Empty;
            _joinRoomButton.interactable = true;
        }

        private void JoinRoomButtonOnClickHandler()
		{
            _networkManager.JoinRoom();
            _joinRoomButton.interactable = false;
        }

        private void JoinRoomFailedPanelCloseButtonOnClickHandler()
		{
            ResetScreen();
        }

        private void OnJoinRoomStatusChangedEventHandler(Enumerators.NetworkStatus networkStatus, string message)
		{
            if(networkStatus == Enumerators.NetworkStatus.JoinFailed)
			{
                ResetScreen();
                _joinRoomFailedPanel.SetActive(true);
                _joinRoomFailedText.text = $"Join room failed by reason:\n{message}";
            }
        }
    }
}