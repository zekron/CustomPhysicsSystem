using System;
using UnityEngine;

namespace CustomPhysics2D
{
	public struct CustomRaycastHitList
	{
		public CustomRaycastHit[] hits;
		public int count;

		public int MaxLength => hits.Length;
		public CustomRaycastHit this[int index] => hits[index];

		public CustomRaycastHitList(int length)
		{
			hits = new CustomRaycastHit[length];
			count = 0;
		}

		public void Add(CustomCollider2D collider, float distance, Vector2 point)
		{
			if (count >= hits.Length)
			{
				Debug.LogError("HitArray is full.");
			}
			else
			{
				hits[count].collider = collider;
				hits[count].distance = distance;
				hits[count].point = point;
				count++;
			}
		}

		public void Clear()
		{
			count = 0;
		}
	}

	public struct CustomRaycastHit : IComparable
	{
		public CustomCollider2D collider;

		public float distance;

		public Vector2 point;

		public CustomRaycastHit(CustomCollider2D collider, float distance, Vector2 point)
		{
			this.collider = collider;
			this.distance = distance;
			this.point = point;
		}

		public static implicit operator bool(CustomRaycastHit hit)
		{
			if (hit.collider != null)
			{
				return true;
			}
			return false;
		}

		public int CompareTo(object obj)
		{
			var other = (CustomRaycastHit)obj;
			if (collider == null)
			{
				if (other.collider == null)
				{
					return 0;
				}
				else
				{
					return 1;
				}
			}
			else
			{
				if (other.collider == null)
				{
					return -1;
				}
			}
			if (distance > other.distance)
			{
				return 1;
			}
			else if (distance == other.distance)
			{
				return 0;
			}
			else
			{
				return -1;
			}
		}
	}
}
