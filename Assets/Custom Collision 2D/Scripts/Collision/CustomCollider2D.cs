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

		private Transform colliderTransform;
		private Bounds bounds;
		private Rect rect;

		public Bounds SelfBounds => bounds;
		public Rect SelfRect => rect;
		public bool IsTrigger => isTrigger;

#if !UNITY_EDITOR
		private void Awake()
		{
			Initialize();
		}
#else
		void OnValidate()
		{
			Initialize();
		}
#endif

		void FixedUpdate()
		{
			rect.center = colliderTransform.position;
			bounds.center = colliderTransform.position;
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(SelfRect.center, SelfRect.size);
		}

		private void Initialize()
		{
			colliderTransform = transform;
			Vector2 tempSize = size;
			Vector2 tempOffset = offset;
			//TODO: ROTATE
			//tempSize.y *= Mathf.Cos(transform.eulerAngles.x * Mathf.Deg2Rad);
			//tempSize.x *= Mathf.Cos(transform.eulerAngles.y * Mathf.Deg2Rad);
			tempSize.Scale(colliderTransform.localScale.ToVector2());
			tempOffset.Scale(colliderTransform.localScale.ToVector2());

			bounds = new Bounds(colliderTransform.position.ToVector2() + tempOffset, tempSize);
			//SelfBounds.Expand(transform.localScale);
			rect = new Rect(bounds.min, tempSize);
		}

		public void Initialize(Vector2 offset, Vector2 size)
		{
			this.offset = offset;
			this.size = size;
			Initialize();
		}
	}
}
