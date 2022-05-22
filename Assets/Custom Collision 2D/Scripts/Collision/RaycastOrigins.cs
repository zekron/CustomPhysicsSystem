using UnityEngine;
using System.Collections;

namespace CustomPhysics2D
{
	/// <summary>
	/// 射线起始位置
	/// </summary>
	public struct RaycastOrigins
	{
		public Vector2 topLeft;
		public Vector2 topRight;
		public Vector2 bottomLeft;
		public Vector2 bottomRight;

		//public bool topLeftInCollider;

		//public bool topRightInCollider;

		//public bool bottomLeftInCollider;

		//public bool bottomRightInCollider;

		public void Reset()
		{
			topLeft = topRight = bottomLeft = bottomRight = Vector2.zero;

			//topLeft.x = 0.0f;
			//topLeft.y = 0.0f;

			//topRight.x = 0.0f;
			//topRight.y = 0.0f;

			//bottomLeft.x = 0.0f;
			//bottomLeft.y = 0.0f;

			//topRight.x = 0.0f;
			//topRight.y = 0.0f;

			//this.bottomLeftInCollider = false;
			//this.bottomRightInCollider = false;
			//this.topLeftInCollider = false;
			//this.topRightInCollider = false;
		}
	}
}
