using System;
using UnityEngine;

namespace CustomPhysics2D
{
	public struct CollisionInfo : IEquatable<CollisionInfo>
	{
		/// <summary>
		/// Self collider
		/// </summary>
		internal Collider2D collider;
		/// <summary>
		/// Other collider
		/// </summary>
		internal Collider2D hitCollider;
		internal HitColliderDirection HitDirection;
		internal Vector2 position;

		public bool Equals(CollisionInfo obj)
		{
			return (collider == obj.collider) && (hitCollider == obj.hitCollider) || (collider == obj.hitCollider) && (hitCollider == obj.collider);
		}

		public override int GetHashCode()
		{
			return this.collider.GetHashCode() + this.hitCollider.GetHashCode();
		}

		public void Reset()
		{
			HitDirection = HitColliderDirection.None;
			collider = null;
			hitCollider = null;
			position.x = 0.0f;
			position.y = 0.0f;
		}
	}

	public enum HitColliderDirection
	{
		None = -1,

		Left,
		Right,
		Up,
		Down,
	}
}
