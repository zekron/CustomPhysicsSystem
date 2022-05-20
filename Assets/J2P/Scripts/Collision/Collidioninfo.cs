using System;
using UnityEngine;

namespace CustomPhysics2D
{
	public struct CollisionInfo : IEquatable<CollisionInfo>
	{
		internal CollisionDirection DirectionInfo;
		internal Collider2D collider;
		internal Collider2D hitCollider;
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
			DirectionInfo = CollisionDirection.None;
			collider = null;
			hitCollider = null;
			position.x = 0.0f;
			position.y = 0.0f;
		}
	}

	public enum CollisionDirection
	{
		None = -1,

		Left,
		Right,
		Up,
		Down,
	}
}
