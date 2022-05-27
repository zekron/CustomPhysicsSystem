using System;
using UnityEngine;

namespace CustomPhysics2D
{
	public struct CollisionInfo2D : IEquatable<CollisionInfo2D>
	{
		/// <summary>
		/// Self collider
		/// </summary>
		public CustomCollider2D collider { get; internal set; }
		/// <summary>
		/// Other collider
		/// </summary>
		public CustomCollider2D hitCollider { get; internal set; }
		internal HitColliderDirection HitDirection;
		internal Vector2 position;

		public bool Equals(CollisionInfo2D obj)
		{
			return (collider == obj.collider) && (hitCollider == obj.hitCollider) || (collider == obj.hitCollider) && (hitCollider == obj.collider);
		}

		public override int GetHashCode()
		{
			return collider.GetHashCode() + hitCollider.GetHashCode();
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
