using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CustomPhysics2D
{
	public static class Extension
	{
		/// <summary>
		/// Get CppRigidbody
		/// </summary>
		/// <param name="collider"></param>
		/// <returns></returns>
		public static JRigidbody Rigidbody(this Collider2D collider)
		{
			return JPhysicsManager.instance.GetRigidbody(collider);
		}

		/// <summary>
		/// Check if a LayerMask contains a specific layer.
		/// </summary>
		/// <param name="">Layer That Contains</param>
		/// <returns></returns>
		public static bool Contains(this int layerMask, int layer)
		{
			if (layerMask == (layerMask | (1 << layer)))
			{
				return true;
			}

			return false;
		}

		public static void ClearAllDelegates(this CollisionEvent collisionEvent)
		{
			if (collisionEvent != null)
			{
				foreach (Delegate d in collisionEvent.GetInvocationList())
				{
					collisionEvent -= (CollisionEvent)d;
				}
			}
		}

		public static bool Intersects(this Rect rect, Rect target)
		{
			if (rect.yMin > target.yMax)
			{
				return false;
			}
			if (rect.yMax < target.yMin)
			{
				return false;
			}
			if (rect.xMax < target.xMin)
			{
				return false;
			}
			if (rect.xMin > target.xMax)
			{
				return false;
			}
			return true;
		}

		public static CollisionDirection ToCollisionDirection(this Vector2 vector)
		{
			if (vector == Vector2.left) return CollisionDirection.Left;
			else if (vector == Vector2.right) return CollisionDirection.Right;
			else if (vector == Vector2.up) return CollisionDirection.Up;
			else if (vector == Vector2.down) return CollisionDirection.Down;
			else throw new Exception(string.Format("Wrong vector: {0}", vector));
		}
		public static Vector2 ToVector2(this CollisionDirection direction)
		{
			switch (direction)
			{
				case CollisionDirection.Left: return Vector2.left;
				case CollisionDirection.Right: return Vector2.right;
				case CollisionDirection.Up: return Vector2.up;
				case CollisionDirection.Down: return Vector2.down;
				default:
					throw new Exception(string.Format("Wrong vector: {0}", direction));
			}
		}
		public static int GetMagnitude(this CollisionDirection direction)
		{
			switch (direction)
			{
				case CollisionDirection.Left: 
				case CollisionDirection.Down: 
					return -1;
				case CollisionDirection.Right:
				case CollisionDirection.Up:
					return 1;
				default:
					throw new Exception(string.Format("Wrong vector: {0}", direction));
			}
		}
	}
}