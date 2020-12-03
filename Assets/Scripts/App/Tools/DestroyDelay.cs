using UnityEngine;

namespace Prototype.TopDown2DNetworked
{
    public class DestroyDelay : MonoBehaviour
    {
        private bool _started;
        private float _timer;

        public float delay;
        public bool runAtAwake = true;

        private void Awake()
        {
            _timer = delay;

			if (runAtAwake)
			{
                _started = true;
			}
        }

        private void Update()
        {
			if (_started)
			{
                _timer -= Time.deltaTime;

                if(_timer <= 0)
				{
                    MonoBehaviour.Destroy(gameObject);
				}
            }
        }

		public void Run()
		{
            _started = true;
		}
	}
}