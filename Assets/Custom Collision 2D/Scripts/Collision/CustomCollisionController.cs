using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;

namespace CustomPhysics2D
{
	public delegate void CollisionEvent(CollisionInfo2D collisionInfo);

	[RequireComponent(typeof(CustomCollider2D))]
	public class CustomCollisionController : MonoBehaviour, IQuadTreeItem
	{
		protected const int MAX_HIT_COLLIDER_COUNT = 20;

		public CollisionEvent onCollisionEnter;
		public CollisionEvent onCollisionExit;
		public CollisionEvent onTriggerEnter;
		public CollisionEvent onTriggerExit;

		protected Transform _transform;

		[SerializeField] private bool showDebugGizoms = false;
		[SerializeField] protected int horizontalRayCount = 4;
		[SerializeField] protected int verticalRayCount = 4;

		/// <summary>
		/// When collision detection, place the start point at the vertex of the bounds with zoom in by this distance
		/// </summary>
		protected float _shrinkWidth = 0.01f;
		/// <summary>
		/// When movement is zero, rayLength = _shringkWidth + _expandWidth
		/// </summary>
		protected float _minRayLength = 0.01f;
		protected CustomCollider2D _collider2D;

		protected int collisionMask;
		protected float _horizontalRaySpace;
		protected float _verticalRaySpace;

		/// <summary>
		/// Colliders that won't collide with this collider
		/// </summary>
		protected HashSet<CustomCollider2D> _ignoredColliders = new HashSet<CustomCollider2D>();
		protected RaycastOrigins _raycastOrigins = new RaycastOrigins();

		protected RaycastHit2D[] _raycastHit2D;
		protected CustomRaycastHitList _jraycastHitList;

		private PositionInQuadTree _lastPosInQuadTree;
		private PositionInQuadTree _currentPosInQuadTree;

		private Rect _rect;

		public CustomCollider2D SelfCollider => _collider2D;
		protected Bounds SelfBounds => _collider2D.SelfBounds;
		public Vector2 ItemSize => _collider2D.SelfBounds.size;
		public Vector2 ItemCenter => _collider2D.SelfBounds.center;
		public Rect ItemRect => _rect;
		public PositionInQuadTree LastPosInQuadTree
		{
			get
			{
				return _lastPosInQuadTree;
			}
			set
			{
				_lastPosInQuadTree = value;
			}
		}
		public PositionInQuadTree CurrentPosInQuadTree
		{
			get
			{
				return _currentPosInQuadTree;
			}
			set
			{
				_currentPosInQuadTree = value;
			}
		}

		protected virtual void Awake()
		{
			_collider2D = GetComponent<CustomCollider2D>();

			_raycastHit2D = new RaycastHit2D[MAX_HIT_COLLIDER_COUNT];
			_jraycastHitList = new CustomRaycastHitList(MAX_HIT_COLLIDER_COUNT);
			_transform = transform;
		}

		void Start()
		{
			_rect = new Rect(_collider2D.SelfBounds.min, _collider2D.SelfBounds.size);
		}

		private void OnDestroy()
		{
			onCollisionEnter.ClearAllDelegates();
			onCollisionExit.ClearAllDelegates();
			onTriggerEnter.ClearAllDelegates();
			onCollisionExit.ClearAllDelegates();
		}

		private void OnDrawGizmos()
		{
			//if (!showDebugGizoms) return;

			Gizmos.color = Color.red;
			_collider2D = GetComponent<CustomCollider2D>();
			_rect = new Rect(_collider2D.SelfBounds.min, _collider2D.SelfBounds.size);
			Gizmos.DrawWireCube(_rect.center, _rect.size);

			UpdateRaycastOrigins();
			Gizmos.color = Color.green;
			for (int i = 0; i < horizontalRayCount; i++)
			{
				Gizmos.DrawSphere(new Vector3(_raycastOrigins.bottomLeft.x,
											  _raycastOrigins.bottomLeft.y + _horizontalRaySpace * i),
								  0.05f);
				Gizmos.DrawSphere(new Vector3(_raycastOrigins.bottomRight.x,
											  _raycastOrigins.bottomRight.y + _horizontalRaySpace * i),
								  0.05f);
			}
			Gizmos.color = Color.blue;
			for (int i = 0; i < verticalRayCount; i++)
			{
				Gizmos.DrawSphere(new Vector3(_raycastOrigins.bottomLeft.x + _verticalRaySpace * i,
											  _raycastOrigins.bottomLeft.y),
								  0.05f);
				Gizmos.DrawSphere(new Vector3(_raycastOrigins.topLeft.x + _verticalRaySpace * i,
											  _raycastOrigins.topLeft.y),
								  0.05f);
			}
		}

		public virtual void Simulate(float deltaTime) { }

		public void InitializePosInQuadTree(QuadTree quadTree)
		{
			_lastPosInQuadTree = new PositionInQuadTree(quadTree.GetDepth(ItemSize));
			_currentPosInQuadTree = new PositionInQuadTree(quadTree.GetDepth(ItemSize));
		}

		protected void UpdateRect()
		{
			_rect.center = _transform.position;
		}

		protected void CalculateRaySpace(ref Bounds bounds)
		{
			bounds.Expand(_shrinkWidth * -2);
			var boundsSize = bounds.size;
			_horizontalRaySpace = boundsSize.y / (horizontalRayCount - 1);
			_verticalRaySpace = boundsSize.x / (verticalRayCount - 1);
		}

		protected void UpdateRaycastOrigins()
		{
			//把原碰撞框内缩返回一个新的碰撞框,记录新碰撞框四个顶点用于射线检测的起点
			var bounds = SelfBounds;
			CalculateRaySpace(ref bounds);

			// Top Left
			_raycastOrigins.topLeft.x = bounds.min.x;
			_raycastOrigins.topLeft.y = bounds.max.y;

			// Top Right
			_raycastOrigins.topRight.x = bounds.max.x;
			_raycastOrigins.topRight.y = bounds.max.y;

			// Bottom Left
			_raycastOrigins.bottomLeft.x = bounds.min.x;
			_raycastOrigins.bottomLeft.y = bounds.min.y;

			// Bottom Right
			_raycastOrigins.bottomRight.x = bounds.max.x;
			_raycastOrigins.bottomRight.y = bounds.min.y;
		}
	}
}
