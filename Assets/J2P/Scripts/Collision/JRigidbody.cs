﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace CustomPhysics2D
{
	public class JRigidbody : JCollisionController
	{
		public enum CollisionDetectionMode
		{
			WhenMoving = 0,
			Continuous = 1
		}

		public float gravityScale = 1.0f;

		[SerializeField] private CollisionDetectionMode collisionDetectionMode;

		private Vector2 _velocity;
		private CollisionInfo _collisionInfo;
		private CollisionInfo _triggerInfo;
		private bool _colliderIsTrigger = false;
		private JPhysicsManager _physicsManager;
		private Vector3 _movement = Vector3.zero;
		private HashSet<Collider2D> _currentDetectionHitTriggers = new HashSet<Collider2D>();
		private HashSet<Collider2D> _currentDetectionHitColliders = new HashSet<Collider2D>();
		private Vector2 _raycastDirection;

		#region Properties
		public Vector2 velocity
		{
			get
			{
				return _velocity;
			}
			set
			{
				_velocity = value;
			}
		}

		public float velocityX
		{
			get
			{
				return _velocity.x;
			}
			set
			{
				_velocity.x = value;
			}
		}

		public float velocityY
		{
			get
			{
				return _velocity.y;
			}
			set
			{
				_velocity.y = value;
			}
		}

		public CollisionInfo collisionInfo
		{
			get
			{
				return _collisionInfo;
			}
		}
		#endregion

		protected override void Awake()
		{
			base.Awake();
			_physicsManager = JPhysicsManager.instance;
			_physicsManager.PushRigidbody(this);
			this.collisionMask = _physicsManager.setting.GetCollisionMask(this.gameObject.layer);

			// Add myself's collider to ignoredColliders list
			_ignoredColliders.Add(this.SelfCollider);
		}

		private void OnDestroy()
		{
			_physicsManager.RemoveRigidbody(this);
		}

		public override void Simulate(float deltaTime)
		{
			base.Simulate(deltaTime);

			// Add velocity generated by gravity
			var gravity = _physicsManager.setting.gravity;
			var gravityRatio = gravityScale * deltaTime;
			_velocity.x += gravity.x * gravityRatio;
			_velocity.y += gravity.y * gravityRatio;

			// Movement
			_movement.x = _velocity.x * deltaTime;
			_movement.y = _velocity.y * deltaTime;

			// Reset Collision Info Before Collision
			this.ResetStatesBeforeCollision();

			if (this.SelfCollider == null || !this.SelfCollider.enabled)
			{
				return;
			}

			this.CollisionDetect();

			this.Move();

			this.FixInsertion();

			//// Landing Platform
			//if( !_collisionInfo.isBelowCollision )
			//{
			//	_landingPlatform = null;
			//}

			this.FixVelocity();

			// Reset Collision Info After Collision
			this.ResetStatesAfterCollision();
		}

		private void ResetStatesBeforeCollision()
		{
			_colliderIsTrigger = SelfCollider.isTrigger;
			_collisionInfo.Reset();
			_triggerInfo.Reset();
			_raycastOrigins.Reset();
		}

		private void ResetStatesAfterCollision()
		{
		}

		private void CollisionDetect()
		{
			Profiler.BeginSample("CollisionDetect");
			// Clear Hit Triggers
			_currentDetectionHitTriggers.Clear();
			_currentDetectionHitColliders.Clear();

			// Prepare Collision Info
			this.PrepareCollisionInfo();

			if (float.IsNaN(_movement.x))
			{
				_movement.x = 0.0f;
			}
			if (float.IsNaN(_movement.y))
			{
				_movement.y = 0.0f;
			}

			// Horizontal
			this.HorizontalCollisionDetect();

			// Vertical
			this.VerticalCollisionDetect();
			Profiler.EndSample();
		}

		public void Move()
		{
			if (SelfCollider == null || !SelfCollider.enabled)
			{
				return;
			}

			MovePosition(ref _movement);
			UpdateRect();
		}

		private void FixInsertion()
		{
			_currentDetectionHitColliders.Clear();
		}

		private void FixVelocity()
		{
			switch (_collisionInfo.DirectionInfo)
			{
				// The Horizontal velocity should be zero if the rigidbody is facing some 'solid' collider.
				case CollisionDirection.Left when _velocity.x < 0f:
				case CollisionDirection.Right when _velocity.x > 0f:
					_velocity.x = 0.0f;
					break;
				// The Vertical velocity should be zero if the rigidbody is facing some 'solid' collider.
				case CollisionDirection.Up when _velocity.y > 0f:
				case CollisionDirection.Down when _velocity.y < 0f:
					_velocity.y = 0.0f;
					break;
				default:
					break;
			}
			//// The Horizontal velocity should be zero if the rigidbody is facing some 'solid' collider.
			//if ((_velocity.x < 0.0f && _collisionInfo.DirectionInfo == CollisionDirection.Left)
			//	|| (_velocity.x > 0.0f && _collisionInfo.DirectionInfo == CollisionDirection.Right))
			//{
			//	_velocity.x = 0.0f;
			//}

			//// The Vertical velocity should be zero if the rigidbody is facing some 'solid' collider.
			//if ((_velocity.y > 0.0f && _collisionInfo.DirectionInfo == CollisionDirection.Up)
			//	|| (_velocity.y < 0.0f && _collisionInfo.DirectionInfo == CollisionDirection.Down))
			//{
			//	_velocity.y = 0.0f;
			//}
		}

		private void MovePosition(ref Vector3 movement)
		{
			if (float.IsNaN(movement.x))
			{
				movement.x = 0.0f;
			}

			if (float.IsNaN(movement.y))
			{
				movement.y = 0.0f;
			}

			_transform.position += movement;
		}

		private void PrepareCollisionInfo()
		{
			this.UpdateRaycastOrigins();
		}

		private void HorizontalCollisionDetect()
		{
			int detectionCount = 1; //检测次数，运动时只检测运动方向
			if (_movement.x == 0)
			{
				if (collisionDetectionMode == CollisionDetectionMode.WhenMoving) return;

				detectionCount = 2;
			}

			var directionX = _movement.x >= 0 ? Vector2.right : Vector2.left;

			for (int cnt = 0; cnt < detectionCount; cnt++)
			{
				directionX = cnt == 0 ? directionX : -directionX;   //优先检测移动方向
				var rayOrigin = (directionX == Vector2.right) ? _raycastOrigins.bottomRight : _raycastOrigins.bottomLeft;
				var rayLength = Mathf.Abs(_movement.x) + _shrinkWidth;
				if (_movement.x == 0f)
				{
					rayLength += _minRayLength;
				}

				for (int i = 0; i < horizontalRayCount; i++)
				{
					_raycastDirection = directionX;

					Debug.DrawLine(rayOrigin, rayOrigin + _raycastDirection * rayLength, Color.red);
					if (JPhysicsManager.useUnityRayCast)
					{
						var hitCount = Physics2D.RaycastNonAlloc(rayOrigin, _raycastDirection, _raycastHit2D, rayLength, this.collisionMask);
						for (int j = 0; j < hitCount; j++)
						{
							var hit = _raycastHit2D[j];
							if (_ignoredColliders.Contains(hit.collider))
							{
								continue;
							}
							HandleHitResult(hit.collider, hit.point, hit.distance, directionX.ToCollisionDirection());
						}
					}
					else
					{
						_jraycastHitList.Clear();
						JPhysics.Raycast(JPhysicsManager.instance.quadTree, rayOrigin, _raycastDirection, ref _jraycastHitList, rayLength, this.collisionMask);
						for (int j = 0; j < _jraycastHitList.count; j++)
						{
							var hit = _jraycastHitList[j];
							if (_ignoredColliders.Contains(hit.collider))
							{
								continue;
							}
							HandleHitResult(hit.collider, hit.point, hit.distance, directionX.ToCollisionDirection());
						}
					}
					rayOrigin.y += _horizontalRaySpace;
				}
			}
		}

		private void HandleHorizontalHitResult(Collider2D hitCollider, Vector2 hitPoint, float hitDistance, CollisionDirection directionX)
		{
			var myLayer = this.gameObject.layer;
			//Trigger
			if (HitTrigger(hitCollider, hitPoint, directionX))
			{
				return;
			}

			// Collision Info
			_collisionInfo.collider = SelfCollider;
			_collisionInfo.hitCollider = hitCollider;
			_collisionInfo.position = hitPoint;

			// Collision Direction
			_collisionInfo.DirectionInfo = directionX;

			//Push Collision 
			if (!_currentDetectionHitColliders.Contains(hitCollider))
			{
				_physicsManager.PushCollision(_collisionInfo);
				_currentDetectionHitColliders.Add(hitCollider);
			}

			//Fix movement
			if (_movement.x != 0.0f)
			{
				if (Mathf.Abs(hitDistance - _shrinkWidth) < Mathf.Abs(_movement.x))
				{
					_movement.x = (hitDistance - _shrinkWidth) * (directionX == CollisionDirection.Left ? -1 : 1);
				}
			}
		}

		private void VerticalCollisionDetect()
		{
			int detectionCount = 1;
			if (_movement.y == 0)
			{
				if (collisionDetectionMode != CollisionDetectionMode.WhenMoving)
				{
					detectionCount = 2;
				}
			}

			var directionY = _movement.y > 0 ? Vector2.up : Vector2.down;

			for (int d = 0; d < detectionCount; d++)
			{
				directionY = d == 0 ? directionY : -directionY;
				var rayOrigin = (directionY == Vector2.up) ? _raycastOrigins.topLeft : _raycastOrigins.bottomLeft;
				rayOrigin.x += _movement.x;

				var rayLength = Mathf.Abs(_movement.y) + _shrinkWidth;
				if (_movement.y == 0f)
				{
					rayLength += _minRayLength;
				}
				for (int i = 0; i < verticalRayCount; i++)
				{
					_raycastDirection = directionY;

					Debug.DrawLine(rayOrigin, rayOrigin + _raycastDirection * rayLength, Color.red);
					if (JPhysicsManager.useUnityRayCast)
					{
						var hitCount = Physics2D.RaycastNonAlloc(rayOrigin, _raycastDirection, _raycastHit2D, rayLength, collisionMask);
						for (int j = 0; j < hitCount; j++)
						{
							var hit = _raycastHit2D[j];

							var hitCollider = hit.collider;
							// Ignored Collider?
							if (_ignoredColliders.Contains(hitCollider))
							{
								continue;
							}
							HandleHitResult(hit.collider, hit.point, hit.distance, directionY.ToCollisionDirection());
						}
					}
					else
					{
						_jraycastHitList.Clear();
						JPhysics.Raycast(JPhysicsManager.instance.quadTree, rayOrigin, _raycastDirection, ref _jraycastHitList, rayLength, collisionMask);
						for (int j = 0; j < _jraycastHitList.count; j++)
						{
							var hit = _jraycastHitList[j];
							if (_ignoredColliders.Contains(hit.collider))
							{
								continue;
							}
							HandleHitResult(hit.collider, hit.point, hit.distance, directionY.ToCollisionDirection());
						}
					}

					rayOrigin.x += _verticalRaySpace;
				}
			}
		}

		private void HandleVerticalHitResult(Collider2D hitCollider, Vector2 hitPoint, float hitDistance, CollisionDirection directionY)
		{
			var myLayer = this.gameObject.layer;
			// Trigger?
			if (HitTrigger(hitCollider, hitPoint, directionY))
			{
				return;
			}

			// Collision Info
			_collisionInfo.collider = this.SelfCollider;
			_collisionInfo.hitCollider = hitCollider;
			_collisionInfo.position = hitPoint;

			// Collision Direction
			_collisionInfo.DirectionInfo = directionY;

			// Need Push Collision ?
			if (!_currentDetectionHitColliders.Contains(hitCollider))
			{
				_physicsManager.PushCollision(_collisionInfo);
				_currentDetectionHitColliders.Add(hitCollider);
			}

			//Fix movement
			if (_movement.y != 0.0f)
			{
				if (Mathf.Abs(hitDistance - _shrinkWidth) < Mathf.Abs(_movement.y))
				{
					_movement.y = (hitDistance - _shrinkWidth) * (directionY == CollisionDirection.Up ? 1 : -1);
				}
			}
		}
		
		private void HandleHitResult(Collider2D hitCollider, Vector2 hitPoint, float hitDistance, CollisionDirection direction)
		{
			var myLayer = this.gameObject.layer;
			// Trigger?
			if (HitTrigger(hitCollider, hitPoint, direction))
			{
				return;
			}

			// Collision Info
			_collisionInfo.collider = this.SelfCollider;
			_collisionInfo.hitCollider = hitCollider;
			_collisionInfo.position = hitPoint;

			// Collision Direction
			_collisionInfo.DirectionInfo = direction;

			// Need Push Collision ?
			if (!_currentDetectionHitColliders.Contains(hitCollider))
			{
				_physicsManager.PushCollision(_collisionInfo);
				_currentDetectionHitColliders.Add(hitCollider);
			}

			switch (direction)
			{
				case CollisionDirection.Left:
				case CollisionDirection.Right:
					if (_movement.x != 0.0f)
					{
						if (Mathf.Abs(hitDistance - _shrinkWidth) < Mathf.Abs(_movement.x))
						{
							_movement.x = (hitDistance - _shrinkWidth) * direction.GetMagnitude();
						}
					}
					break;
				case CollisionDirection.Up:
				case CollisionDirection.Down:
					if (_movement.y != 0.0f)
					{
						if (Mathf.Abs(hitDistance - _shrinkWidth) < Mathf.Abs(_movement.y))
						{
							_movement.y = (hitDistance - _shrinkWidth) * direction.GetMagnitude();
						}
					}
					break;
			}
		}

		private bool HitTrigger(Collider2D hitCollider, Vector2 point, CollisionDirection direction)
		{
			// Trigger?
			if (hitCollider.isTrigger || _colliderIsTrigger)
			{
				_triggerInfo.collider = SelfCollider;
				_triggerInfo.hitCollider = hitCollider;
				_triggerInfo.position.x = point.x;
				_triggerInfo.position.y = point.y;
				_triggerInfo.DirectionInfo = direction;

				if (!_currentDetectionHitTriggers.Contains(hitCollider))
				{
					_physicsManager.PushCollision(_triggerInfo);
					_currentDetectionHitTriggers.Add(hitCollider);
				}
				return true;
			}
			return false;
		}
	}
}
