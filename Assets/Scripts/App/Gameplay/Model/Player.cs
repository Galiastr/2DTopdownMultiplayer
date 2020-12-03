using Prototype.TopDown2DNetworked.Common;
using Photon.Pun;
using UnityEngine;

namespace Prototype.TopDown2DNetworked
{
	public class Player
	{
		private ILoadObjectsManager _loadObjectsManager;

		private GameObject _selfObject;

		private GameObject _viewObject;

		private SpriteRenderer _marker;

		private Transform _hpBar;

		private float _movementSpeed = 5f;

		private int _hp;

		private OnBehaviourHandler _onBehaviourHandler;

		private Transform _parent;

		private bool _initialized;

		private float _sendDataDelay;

		private Vector3 _targetPositon;

		private Quaternion _targetRotation;

		private float _netDistance;
		private float _netAngle;

		public int Id { get; private set; }
		public int Index { get; private set; }
		public bool IsLocal { get; private set; }

		public Player(int id, Transform parent, bool isLocal, int index = 0)
		{
			_loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

			Id = id;
			IsLocal = isLocal;
			Index = index;

			_parent = parent;

			_selfObject = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>($"Prefabs/Gameplay/Players/Player_{Index}"),
				Vector3.zero,
				Quaternion.identity);
			_selfObject.transform.SetParent(_parent, false);

			_viewObject = _selfObject.transform.Find("View").gameObject;
			_marker = _viewObject.transform.Find("Marker").GetComponent<SpriteRenderer>();
			_hpBar = _selfObject.transform.Find("HPBar/Fill");

			_onBehaviourHandler = _selfObject.GetComponent<OnBehaviourHandler>();
			_onBehaviourHandler.Trigger2DEntered += Trigger2DEnteredHandler;

			_initialized = true;

			GameClient.Get<INetworkManager>().SendNetworkEvent(Enumerators.NetworkEvent.PlayerInfo, new object[] { Id }, false);

			_hp = Constants.MaxPlayerHP;
			_marker.color = IsLocal ? Color.green : Color.red;

			if (isLocal)
			{
				_selfObject.transform.position = new Vector2(Random.Range(-7f, 7f), Random.Range(-4f, 4f));
			}
		}

		public void Update()
		{
			if (!_initialized)
				return;

			if (IsLocal)
			{
				HandleInput();

				_sendDataDelay -= Time.deltaTime;

				if (_sendDataDelay <= 0)
				{
					GameClient.Get<INetworkManager>().SendNetworkEvent(Enumerators.NetworkEvent.PlayerTransform, new object[] { Id, _selfObject.transform.position, _viewObject.transform.rotation }, false);

					_sendDataDelay = 1.0f / (float)PhotonNetwork.SerializationRate;
				}
			}
			else
			{
				_selfObject.transform.position = Vector3.MoveTowards(_selfObject.transform.position, _targetPositon, _netDistance * (1.0f / PhotonNetwork.SerializationRate));
				_viewObject.transform.rotation = Quaternion.RotateTowards(_viewObject.transform.rotation, _targetRotation, _netAngle * (1.0f / PhotonNetwork.SerializationRate));
			}

			CheckForBorders();
		}

		public void Dispose()
		{
			if (!_initialized)
				return;

			MonoBehaviour.Destroy(_selfObject);
		}
		
		public void SetTransformNetworked(Vector3 position, Quaternion rotation)
		{
			if (!_initialized)
				return;

			_targetPositon = position;
			_targetRotation = rotation;

			_netDistance = Vector3.Distance(_selfObject.transform.position, _targetPositon);
			_netAngle = Quaternion.Angle(_viewObject.transform.rotation, _targetRotation);

			if (_netDistance > 1f)
				_selfObject.transform.position = _targetPositon;
		}

		public void DestroyFromNetwork()
		{
			_initialized = false;
		}

		private void HandleInput()
		{
			Vector2 direction = Vector2.zero;

			if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
			{
				direction.y = 1;
			} 
			else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
			{
				direction.y = -1;
			}

			if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
			{
				direction.x = -1;
			}
			else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
			{
				direction.x = 1;
			}

			Movement(direction);
		}

		private void Movement(Vector2 direction)
		{
			if (direction == Vector2.zero)
				return;

			_selfObject.transform.Translate(direction * _movementSpeed * Time.deltaTime, Space.Self);

			// todo make smoothing
			var rotation = _selfObject.transform.eulerAngles;
			rotation.z = Mathf.Atan2(direction.y, direction.x) * 180 / Mathf.PI - 90;
			rotation.z = (360 + Mathf.Round(rotation.z)) % 360;

			_viewObject.transform.eulerAngles = rotation;
		}

		private void Trigger2DEnteredHandler(Collider2D collider)
		{
			// todo perhaps need improvements 
			if (collider.CompareTag(Constants.PlayerTag) && IsLocal)
			{
				GameClient.Get<INetworkManager>().SendNetworkEvent(Enumerators.NetworkEvent.PlayerHit, new object[] { Id }, false);

				Hit();
			}
		}

		private void CheckForDie()
		{
			if (_hp <= 0)
			{
				GameObject particle = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>($"Prefabs/Gameplay/VFX/PlayerDieVFX"));
				particle.transform.position = _selfObject.transform.position;

				if (IsLocal)
					Revive();
			}
		}

		private void Revive()
		{
			_hp = Constants.MaxPlayerHP;

			Vector3 scale = _hpBar.localScale;
			scale.x = (float)_hp / (float)Constants.MaxPlayerHP;
			_hpBar.localScale = scale;

			_selfObject.transform.localPosition = new Vector2(Random.Range(-7f, 7f), Random.Range(-4f, 4f));

			GameClient.Get<INetworkManager>().SendNetworkEvent(Enumerators.NetworkEvent.PlayerRevive, new object[] { Id, _selfObject.transform.localPosition }, false);
		}

		public void ReviveNetworked(Vector3 position)
		{
			_hp = Constants.MaxPlayerHP;

			Vector3 scale = _hpBar.localScale;
			scale.x = (float)_hp / (float)Constants.MaxPlayerHP;
			_hpBar.localScale = scale;

			_selfObject.transform.localPosition = position;
		}

		public void Hit()
		{
			_hp--;

			Vector3 scale = _hpBar.localScale;
			scale.x = (float)_hp / (float)Constants.MaxPlayerHP;
			_hpBar.localScale = scale;

			CheckForDie();
		}

		private void CheckForBorders()
		{
			Vector2 position = _selfObject.transform.position;

			// todo use information about level or use colliders instead
			position.x = Mathf.Clamp(position.x, -8.5f, 8.5f);
			position.y = Mathf.Clamp(position.y, -4.75f, 4.75f);

			_selfObject.transform.position = position;
		}
	}
}