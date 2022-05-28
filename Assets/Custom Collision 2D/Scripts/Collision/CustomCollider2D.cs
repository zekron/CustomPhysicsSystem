using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomPhysics2D
{
	[AddComponentMenu("Custom Component/Custom Collider2D")]
	public class CustomCollider2D : MonoBehaviour
	{
		[SerializeField] private bool isTrigger;
		[SerializeField] private Vector2 offset = Vector2.zero;
		[SerializeField] private Vector2 size = Vector2.one;
		private Vector2 scaledOffset;
		private Vector2 scaledSize;

		private Transform colliderTransform;
		private Bounds bounds = new Bounds();
		private Rect rect = new Rect();

		public Bounds SelfBounds => bounds;
		public Rect SelfRect => rect;
		public bool IsTrigger => isTrigger;

		private void OnEnable()
		{
			Initialize();
		}
		void OnValidate()
		{
			Initialize();
		}

		void FixedUpdate()
		{
			rect.center = bounds.center = colliderTransform.position.ToVector2() + scaledOffset;
			RotateRect(colliderTransform.eulerAngles);
		}

		private void OnDrawGizmosSelected()
		{
			Initialize();
			Gizmos.color = Color.yellow;
			var tempScale = rect.size;
			tempScale.Scale(new Vector2(0.99f, 0.99f));
			Gizmos.DrawWireCube(rect.center, tempScale);
			//UnityEditor.Handles.Label(bounds.min, bounds.min.ToString());
			//UnityEditor.Handles.Label(bounds.center, bounds.center.ToString());
		}

		[ContextMenu("Initialize")]
		private void Initialize()
		{
			colliderTransform = transform;
			scaledSize = size;
			scaledOffset = offset;
			ScaleVector();

			rect.size = bounds.size = scaledSize;
			rect.center = bounds.center = colliderTransform.position.ToVector2() + scaledOffset;
			RotateRect(colliderTransform.eulerAngles);
		}

		private void RotateRect(Vector3 eulerAngles)
		{
			RotateAroundXAxis(eulerAngles.x * Mathf.Deg2Rad);
			RotateAroundYAxis(eulerAngles.y * Mathf.Deg2Rad);
			RotateAroundZAxis(eulerAngles.z * Mathf.Deg2Rad);
		}

		private void RotateAroundXAxis(float rad)
		{
			rect.height *= Mathf.Cos(rad);
			rect.center = bounds.center;
		}

		private void RotateAroundYAxis(float rad)
		{
			rect.width *= Mathf.Cos(rad);
			rect.center = bounds.center;
		}

		private void RotateAroundZAxis(float rad)
		{
			Vector2 bottomLeft = new Vector2(rect.xMin, rect.yMin);
			Vector2 bottomRight = new Vector2(rect.xMax, rect.yMin);
			Vector2 topLeft = new Vector2(rect.xMin, rect.yMax);
			Vector2 topRight = new Vector2(rect.xMax, rect.yMax);

			bottomLeft = MyMath.LinePoint.RotateAroundPoint(bottomLeft, rect.center, rad);
			bottomRight = MyMath.LinePoint.RotateAroundPoint(bottomRight, rect.center, rad);
			topLeft = MyMath.LinePoint.RotateAroundPoint(topLeft, rect.center, rad);
			topRight = MyMath.LinePoint.RotateAroundPoint(topRight, rect.center, rad);

			rect.xMin = Mathf.Min(bottomLeft.x, bottomRight.x, topLeft.x, topRight.x);
			rect.xMax = Mathf.Max(bottomLeft.x, bottomRight.x, topLeft.x, topRight.x);
			rect.yMin = Mathf.Min(bottomLeft.y, bottomRight.y, topLeft.y, topRight.y);
			rect.yMax = Mathf.Max(bottomLeft.y, bottomRight.y, topLeft.y, topRight.y);

			bounds.min = new Vector3(rect.xMin, rect.yMin);
			bounds.max = new Vector3(rect.xMax, rect.yMax);
		}

		private void ScaleVector()
		{
			scaledSize.Scale(colliderTransform.localScale.ToVector2());
			scaledOffset.Scale(colliderTransform.localScale.ToVector2());
		}

		public void Initialize(Vector2 offset, Vector2 size)
		{
			this.offset = offset;
			this.size = size;
			Initialize();
		}
	}
}
