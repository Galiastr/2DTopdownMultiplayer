using UnityEngine;

namespace Prototype.TopDown2DNetworked
{
    public class MainApp : MonoBehaviour
    {
        private static MainApp _Instance;
        public static MainApp Instance
        {
            get { return _Instance; }
            private set { _Instance = value; }
        }

        private void Awake()
        {
            if(Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            if (Instance == this)
            {
                GameClient.Instance.Init();     
            }
        }

        private void Update()
        {
            if (Instance == this)
            {
                GameClient.Instance.Update();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                GameClient.Instance.Dispose();
            }
        }
    }
}