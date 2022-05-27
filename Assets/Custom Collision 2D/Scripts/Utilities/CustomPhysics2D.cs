using UnityEngine;
using System.Collections;

namespace CustomPhysics2D
{
	public static class CustomPhysics2D
	{
		private const float MINRECTWIDTH = 0.1F;
		private const float MINRECTHEIGHT = 0.1f;

		/// <summary>
		/// CreateRect By a 2 vector2 point
		/// </summary>
		public static Rect CreateRect(Vector2 start, Vector2 end)
		{
			var segment = end - start;
			var rect = new Rect();

			rect.x = Mathf.Min(start.x, end.x);
			rect.y = Mathf.Min(start.y, end.y);

			rect.width = Mathf.Abs(segment.x);
			rect.height = Mathf.Abs(segment.y);
			if (rect.width == 0f)
			{
				rect.width = MINRECTWIDTH;
			}
			if (rect.height == 0f)
			{
				rect.height = MINRECTHEIGHT;
			}

			return rect;
		}

		/// <summary>
		/// Raycast item's Rect with Ray.Rect in <paramref name="quadTree"/>
		/// </summary>
		/// <param name="quadTree"></param>
		/// <param name="origin">Origin of ray</param>
		/// <param name="direction">Direction of ray</param>
		/// <param name="hitList">Hit list which stores item are casted</param>
		/// <param name="distance">Distance of ray</param>
		/// <param name="layMask">Layer in which can be casted</param>
		public static void Raycast(QuadTree quadTree, Vector2 origin, Vector2 direction, ref CustomRaycastHitList hitList, float distance, int layMask)
		{
			var hitCount = 0;
			var ray = direction.normalized * distance;
			var destPoint = origin + ray;

			var rayRect = CreateRect(origin, destPoint);
			var itemList = quadTree.GetItems(rayRect);
			foreach (IQuadTreeItem item in itemList)
			{
				var collider = item.SelfCollider;
				var layer = collider.gameObject.layer;
				if (!layMask.Contains(layer))
				{
					continue;
				}
				if (!collider.gameObject.activeInHierarchy)
				{
					continue;
				}
				if (!collider.enabled)
				{
					continue;
				}
				CalculateRayHit(collider, origin, direction, ref hitList, distance, ref hitCount);
			}
		}

		/// <summary>
		/// 计算一条射线和AABB是否相交
		/// </summary>
		private static void CalculateRayHit(CustomCollider2D collider, Vector2 origin, Vector2 dir, ref CustomRaycastHitList hitList, float distance, ref int hitCount)
		{
			if (RaycastHitExist(collider, hitList)) return;

			var bounds = collider.SelfBounds;
			if (bounds.Contains(origin))
			{
				AddRayHitToList(collider, origin, 0f, ref hitList, ref hitCount);
				return;
			}
			if (distance <= 0f) return;

			var hit = false;
			var destPoint = origin + dir * distance;
			//ax + by + c = 0 => (y2 - y1)a + (x1 - x2)b + (x2 * y1) - (y2 * x1) = 0
			var a = destPoint.y - origin.y;
			var b = origin.x - destPoint.x;
			var c = destPoint.x * origin.y - destPoint.y * origin.x;

			var rayRectXMin = Mathf.Min(origin.x, destPoint.x);
			var rayRectXMax = Mathf.Max(origin.x, destPoint.x);
			var rayRectYMin = Mathf.Min(origin.y, destPoint.y);
			var rayRectYMax = Mathf.Max(origin.y, destPoint.y);

			var boundsX = new float[] { bounds.min.x, bounds.max.x };
			var boundsY = new float[] { bounds.min.y, bounds.max.y };

			var hitDistance = float.MaxValue;
			var hitPoint = Vector2.zero;
			var tempHitPoint = Vector2.zero;
			//先根据x求y，这种情况下b不能等于0，a可能等于0。
			//把xMin和xMax分别带入直线方程得到y值，当y值在yMin和yMax区间且 xMin或xMax在x1和x2区间 产生交点。
			//y = (-c - a * x )/b
			if (b != 0f)
			{
				for (int cnt = 0; cnt < boundsX.Length; cnt++)
				{
					if (boundsX[cnt] < rayRectXMax && boundsX[cnt] > rayRectXMin)
					{
						tempHitPoint.x = boundsX[cnt];
						var y = (-c - a * boundsX[cnt]) / b;
						if (y < boundsY[1] && y > boundsY[0])
						{
							tempHitPoint.y = y;
							hit = true;
							var dis = (tempHitPoint - origin).magnitude;
							if (dis < hitDistance)
							{
								hitPoint = tempHitPoint;
								hitDistance = dis;
							}
						}
					}
				}
			}
			//再根据y求x，这种情况下a不能等于0，b可能等于0。
			//x = (-c - b * y )/a
			if (a != 0f)
			{
				for (int cnt = 0; cnt < boundsY.Length; cnt++)
				{
					if (boundsY[cnt] < rayRectYMax && boundsY[cnt] > rayRectYMin)
					{
						tempHitPoint.y = boundsY[cnt];
						var x = (-c - b * boundsY[cnt]) / a;
						if (x < boundsX[1] && x > boundsX[0])
						{
							tempHitPoint.x = x;
							hit = true;
							var dis = (tempHitPoint - origin).magnitude;
							if (dis < hitDistance)
							{
								hitPoint = tempHitPoint;
								hitDistance = dis;
							}
						}
					}
				}
			}
			if (hit)
			{
				AddRayHitToList(collider, hitPoint, hitDistance, ref hitList, ref hitCount);
			}
		}

		private static void AddRayHitToList(CustomCollider2D collider, Vector2 hitPoint, float distance, ref CustomRaycastHitList hitList, ref int hitCount)
		{
			hitCount++;
			if (hitCount < hitList.MaxLength - 1)
			{
				hitList.Add(collider, distance, hitPoint);
			}
			else
			{
				Debug.LogError(collider.gameObject.name + "'s collision count is greater than [" + hitList.count + "]");
			}
		}

		private static bool RaycastHitExist(CustomCollider2D collider, CustomRaycastHitList hitList)
		{
			for (int i = 0; i < hitList.count; i++)
			{
				var hit = hitList[i];
				if (hit.collider == collider)
				{
					return true;
				}
			}
			return false;
		}
	}
}
