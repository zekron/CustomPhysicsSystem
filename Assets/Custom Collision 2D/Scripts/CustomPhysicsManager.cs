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

		public QuadTree quadTree => _quadTree;

		public CustomPhysicsSetting setting
		{
			get; private set;
		}

		private void Awake()
		{
			setting = Instantiate(Resources.Load<CustomPhysicsSetting>("JPhysics Settings"));

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

				this.HandleCollidersEnter();

				this.HandleCollidersExit();

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
					ContactEvent(currentFrameCollision, true);
					_lastFrameHitColliders.Add(currentFrameCollision);
				}
			}

			// New Rigidbody Collisions This Frame
			foreach (var collision in _currentFrameHitRigidbodies)
			{
				if (!_lastFrameHitRigidbodies.Contains(collision))
				{
					ContactEvent(collision, true);
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
					this.ContactEvent(lastFrameCollision, false);
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
					this.ContactEvent(collision, false);
					_toBeRemovedCollisions.Add(collision);
				}
			}

			foreach (var collision in _toBeRemovedCollisions)
			{
				_lastFrameHitRigidbodies.Remove(collision);
			}

			_toBeRemovedCollisions.Clear();
		}

		private void ContactEvent(CollisionInfo2D collisionInfo, bool isBeginEvent)
		{
			if (collisionInfo.hitCollider == null || collisionInfo.collider == null) return;

			if (collisionInfo.collider.IsTrigger || collisionInfo.hitCollider.IsTrigger)
			{
				// Trigger Event
				SendCollisionMessage(collisionInfo, isBeginEvent, true);
			}
			else
			{
				// Collision Event
				SendCollisionMessage(collisionInfo, isBeginEvent, false);
			}
		}

		private void SendCollisionMessage(CollisionInfo2D collisionInfo, bool isBeginEvent, bool isTriggerEvent)
		{
			var rigidbody = GetRigidbody(collisionInfo.collider);
			var hitCollider = collisionInfo.hitCollider;
			var hitRigidbody = GetRigidbody(collisionInfo.hitCollider);

			if (isBeginEvent)
			{
				if (rigidbody != null)  //本方刚体
				{
					if (isTriggerEvent)
					{
						rigidbody.onTriggerEnter?.Invoke(collisionInfo);
					}
					else
					{
						rigidbody.onCollisionEnter?.Invoke(collisionInfo);
					}
				}

				// Switch collider & hitCollider
				if (hitRigidbody != null)   //对方刚体
				{
					collisionInfo.hitCollider = collisionInfo.collider;
					collisionInfo.collider = hitCollider;

					if (isTriggerEvent)
					{
						hitRigidbody.onTriggerEnter?.Invoke(collisionInfo);
					}
					else
					{
						hitRigidbody.onCollisionEnter?.Invoke(collisionInfo);
					}
				}
				else
				{
					var controller = hitCollider.GetComponent<CustomCollisionController>();
					if (controller is CustomPlatform2D)
					{
						Debug.Log("Hit platform");
					}
					//collisionInfo.hitCollider.gameObject.SendMessage( isTriggerEvent ? _triggerBeginEventName : _collisionBeginEventName,
					//	collisionInfo, SendMessageOptions.DontRequireReceiver );
				}
			}
			else
			{
				if (rigidbody != null)
				{
					if (isTriggerEvent)
					{
						rigidbody.onTriggerExit?.Invoke(collisionInfo);
					}
					else
					{
						rigidbody.onCollisionExit?.Invoke(collisionInfo);
					}
				}

				// Switch collider & hitCollider
				if (hitRigidbody != null)
				{
					if (isTriggerEvent)
					{
						hitRigidbody.onTriggerExit?.Invoke(collisionInfo);
					}
					else
					{
						hitRigidbody.onCollisionExit?.Invoke(collisionInfo);
					}
				}
				else
				{
					//collisionInfo.hitCollider.gameObject.SendMessage( isTriggerEvent ? _triggerEndEventName : _collisionEndEventName,
					//	collisionInfo, SendMessageOptions.DontRequireReceiver );
				}
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
	}
}
