using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CustomPhysics2D
{
	public class CustomPhysicsManager : MonoBehaviour
	{
		private static CustomPhysicsManager _instance = null;
		public static bool useUnityRayCast = true;

		public static CustomPhysicsManager instance
		{
			get
			{
				if (_instance == null && Application.isPlaying)
				{
					_instance = FindObjectOfType<CustomPhysicsManager>();

					if (_instance == null)
					{
						var obj = new GameObject("Physics Manager");
						_instance = obj.AddComponent<CustomPhysicsManager>();
					}
					//DontDestroyOnLoad( obj );
				}
				return _instance;
			}
		}

		public static bool DestroyInstance()
		{
			if (_instance == null)
			{
				return false;
			}

			Destroy(_instance.gameObject);
			_instance = null;

			return true;
		}

		private WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();
		private QuadTree _quadTree;
		private bool needUpdateCollision = false;

		#region Datas
		private HashSet<CollisionInfo2D> _lastFrameHitColliders = new HashSet<CollisionInfo2D>();
		private HashSet<CollisionInfo2D> _lastFrameHitRigidbodies = new HashSet<CollisionInfo2D>();
		private HashSet<CollisionInfo2D> _currentFrameHitColliders = new HashSet<CollisionInfo2D>();
		private HashSet<CollisionInfo2D> _currentFrameHitRigidbodies = new HashSet<CollisionInfo2D>();
		private HashSet<CollisionInfo2D> _toBeRemovedCollisions = new HashSet<CollisionInfo2D>();
		private Dictionary<CustomCollider2D, CustomRigidbody2D> _rigidbodies = new Dictionary<CustomCollider2D, CustomRigidbody2D>();
		private Dictionary<CustomCollider2D, CustomPlatform2D> _platforms = new Dictionary<CustomCollider2D, CustomPlatform2D>();
		#endregion

		public QuadTree SelfQuadTree => _quadTree;

		public CustomPhysicsSetting setting
		{
			get; private set;
		}

		private void Awake()
		{
			setting = Instantiate(Resources.Load<CustomPhysicsSetting>("Custom Physics Settings"));

			useUnityRayCast = false;
			needUpdateCollision = true;
			StartCoroutine(UpdateCollisions());
		}

		private void Start()
		{
			if (useUnityRayCast == false)
			{
				foreach (CustomRigidbody2D rigidbody in _rigidbodies.Values)
				{
					rigidbody.InitializePosInQuadTree(_quadTree);
					_quadTree.UpdateItem(rigidbody);
				}
				foreach (CustomPlatform2D platform in _platforms.Values)
				{
					platform.InitializePosInQuadTree(_quadTree);
					_quadTree.UpdateItem(platform);
				}
			}
		}

		public void CreateQuadTree(Rect worldRect, int maxDepth)
		{
			_quadTree = new QuadTree(worldRect, maxDepth);
		}

		private void FixedUpdate()
		{
			// Rigidbodies
			foreach (var rigidbody in _rigidbodies.Values)
			{
				if (!rigidbody.isActiveAndEnabled || !rigidbody.gameObject.activeInHierarchy) continue;

				rigidbody.Simulate(Time.fixedDeltaTime);
				if (useUnityRayCast == false)
				{
					_quadTree.UpdateItem(rigidbody);
				}
			}
		}

		private void OnDestroy()
		{
			needUpdateCollision = false;
		}

		public void PushCollision(CollisionInfo2D collisionInfo)
		{
			if (GetRigidbody(collisionInfo.collider) != null && GetRigidbody(collisionInfo.hitCollider) != null)
			{
				if (!_currentFrameHitRigidbodies.Contains(collisionInfo))
				{
					_currentFrameHitRigidbodies.Add(collisionInfo);
				}
			}
			else
			{
				_currentFrameHitColliders.Add(collisionInfo);
			}
		}

		private IEnumerator UpdateCollisions()
		{
			while (needUpdateCollision)
			{
				yield return _waitForFixedUpdate;

				HandleCollidersEnter();

				HandleCollidersExit();

				_currentFrameHitColliders.Clear();
				_currentFrameHitRigidbodies.Clear();
			}
		}

		private void HandleCollidersEnter()
		{
			// New Collisions This Frame
			foreach (var currentFrameCollision in _currentFrameHitColliders)
			{
				if (!_lastFrameHitColliders.Contains(currentFrameCollision))
				{
					ContactEnterEvent(currentFrameCollision);
					_lastFrameHitColliders.Add(currentFrameCollision);
				}
			}

			// New Rigidbody Collisions This Frame
			foreach (var collision in _currentFrameHitRigidbodies)
			{
				if (!_lastFrameHitRigidbodies.Contains(collision))
				{
					ContactEnterEvent(collision);
					_lastFrameHitRigidbodies.Add(collision);
				}
			}
		}

		private void HandleCollidersExit()
		{
			foreach (var lastFrameCollision in _lastFrameHitColliders)
			{
				if (!_currentFrameHitColliders.Contains(lastFrameCollision) || lastFrameCollision.collider == null || lastFrameCollision.hitCollider == null)
				{
					ContactExitEvent(lastFrameCollision);
					_toBeRemovedCollisions.Add(lastFrameCollision);
				}
			}

			foreach (var collision in _toBeRemovedCollisions)
			{
				_lastFrameHitColliders.Remove(collision);
			}

			_toBeRemovedCollisions.Clear();
			foreach (var collision in _lastFrameHitRigidbodies)
			{
				if (!_currentFrameHitRigidbodies.Contains(collision) || collision.collider == null || collision.hitCollider == null)
				{
					ContactExitEvent(collision);
					_toBeRemovedCollisions.Add(collision);
				}
			}

			foreach (var collision in _toBeRemovedCollisions)
			{
				_lastFrameHitRigidbodies.Remove(collision);
			}

			_toBeRemovedCollisions.Clear();
		}
		private void ContactEnterEvent(CollisionInfo2D collisionInfo)
		{
			if (collisionInfo.hitCollider == null || collisionInfo.collider == null) return;

			SendEnterEventMessage(collisionInfo,
						 collisionInfo.collider.IsTrigger || collisionInfo.hitCollider.IsTrigger);
		}
		private void ContactExitEvent(CollisionInfo2D collisionInfo)
		{
			if (collisionInfo.hitCollider == null || collisionInfo.collider == null) return;

			SendExitEventMessage(collisionInfo,
						collisionInfo.collider.IsTrigger || collisionInfo.hitCollider.IsTrigger);
		}

		private void SendEnterEventMessage(CollisionInfo2D collisionInfo, bool isTriggerEvent)
		{
			var rigidbody = GetRigidbody(collisionInfo.collider);
			var hitCollider = collisionInfo.hitCollider;
			var hitRigidbody = GetRigidbody(collisionInfo.hitCollider);
			var platform = GetPlatform(collisionInfo.hitCollider);
			if (rigidbody != null)  //本方刚体
			{
				RaiseEnterEvent(collisionInfo, isTriggerEvent, rigidbody);

				//只有刚体与其他物体才能发生碰撞
				if (platform != null)
				{
					collisionInfo.hitCollider = collisionInfo.collider;
					collisionInfo.collider = hitCollider;
					Debug.LogFormat("{0} hits platform {1}.", collisionInfo.hitCollider.name, collisionInfo.collider.name);

					RaiseEnterEvent(collisionInfo, isTriggerEvent, platform);
				}
			}

			// Switch collider & hitCollider
			if (hitRigidbody != null)   //对方刚体
			{
				collisionInfo.hitCollider = collisionInfo.collider;
				collisionInfo.collider = hitCollider;

				RaiseEnterEvent(collisionInfo, isTriggerEvent, hitRigidbody);
			}
			else
			{
				//collisionInfo.hitCollider.gameObject.SendMessage( isTriggerEvent ? _triggerBeginEventName : _collisionBeginEventName,
				//	collisionInfo, SendMessageOptions.DontRequireReceiver );
			}
		}

		private void SendExitEventMessage(CollisionInfo2D collisionInfo, bool isTriggerEvent)
		{
			var rigidbody = GetRigidbody(collisionInfo.collider);
			var hitCollider = collisionInfo.hitCollider;
			var hitRigidbody = GetRigidbody(collisionInfo.hitCollider);
			var platform = GetPlatform(collisionInfo.hitCollider);

			if (rigidbody != null)
			{
				RaiseExitEvent(collisionInfo, isTriggerEvent, rigidbody);

				//只有刚体与其他物体才能发生碰撞
				if (platform != null)
				{
					collisionInfo.hitCollider = collisionInfo.collider;
					collisionInfo.collider = hitCollider;
					Debug.LogFormat("{0} leaves platform {1}.", collisionInfo.hitCollider.name, collisionInfo.collider.name);

					RaiseExitEvent(collisionInfo, isTriggerEvent, platform);
				}
			}

			// Switch collider & hitCollider
			if (hitRigidbody != null)
			{
				RaiseExitEvent(collisionInfo, isTriggerEvent, hitRigidbody);
			}
			else
			{
				//collisionInfo.hitCollider.gameObject.SendMessage( isTriggerEvent ? _triggerEndEventName : _collisionEndEventName,
				//	collisionInfo, SendMessageOptions.DontRequireReceiver );
			}
		}

		private static void RaiseEnterEvent(CollisionInfo2D collisionInfo, bool isTriggerEvent, CustomCollisionController controller)
		{
			Debug.LogFormat("Controller {0} raises {1} enter event.", controller.name, isTriggerEvent ? "trigger" : "collision");
			if (isTriggerEvent)
			{
				controller.onTriggerEnter?.Invoke(collisionInfo);
			}
			else
			{
				controller.onCollisionEnter?.Invoke(collisionInfo);
			}
		}

		private static void RaiseExitEvent(CollisionInfo2D collisionInfo, bool isTriggerEvent, CustomCollisionController controller)
		{
			Debug.LogFormat("Controller {0} raises {1} exit event.", controller.name, isTriggerEvent ? "trigger" : "collision");
			if (isTriggerEvent)
			{
				controller.onTriggerExit?.Invoke(collisionInfo);
			}
			else
			{
				controller.onCollisionExit?.Invoke(collisionInfo);
			}
		}

		public void PushRigidbody(CustomRigidbody2D rigidbody)
		{
			if (rigidbody == null) return;

			if (!_rigidbodies.ContainsKey(rigidbody.SelfCollider))
			{
				_rigidbodies.Add(rigidbody.SelfCollider, rigidbody);
			}
			else
			{
				throw new System.ArgumentException("The rigidbody has already existed", "rigidbody");
			}
			if (_quadTree == null) return;

			rigidbody.InitializePosInQuadTree(_quadTree);
			_quadTree.UpdateItem(rigidbody);
		}

		public void RemoveRigidbody(CustomRigidbody2D rigidbody)
		{
			if (_rigidbodies == null || _rigidbodies.Count == 0) return;
			if (rigidbody == null) return;

			_rigidbodies.Remove(rigidbody.SelfCollider);
		}

		public CustomRigidbody2D GetRigidbody(CustomCollider2D collider)
		{
			if (collider == null) return null;

			CustomRigidbody2D rigidbody = null;
			_rigidbodies.TryGetValue(collider, out rigidbody);
			return rigidbody;
		}

		public void PushPlatform(CustomPlatform2D platform)
		{
			if (platform == null) return;

			if (!_platforms.ContainsKey(platform.SelfCollider))
			{
				_platforms.Add(platform.SelfCollider, platform);
			}
			if (_quadTree == null) return;

			platform.InitializePosInQuadTree(_quadTree);
			_quadTree.UpdateItem(platform);
		}

		public void RemovePlatform(CustomPlatform2D platform)
		{
			if (_platforms == null || _platforms.Count == 0) return;
			if (platform == null) return;

			_platforms.Remove(platform.SelfCollider);
		}

		public CustomPlatform2D GetPlatform(CustomCollider2D collider)
		{
			if (collider == null) return null;

			CustomPlatform2D platform = null;
			_platforms.TryGetValue(collider, out platform);
			return platform;
		}
	}
}
