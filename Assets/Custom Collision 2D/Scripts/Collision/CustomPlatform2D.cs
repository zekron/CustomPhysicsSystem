using UnityEngine;
using System.Collections;

namespace CustomPhysics2D
{
	public class CustomPlatform2D : CustomCollisionController
	{
		protected override void Awake()
		{
			base.Awake();
		}

		private void OnEnable()
		{
			CustomPhysicsManager.instance.PushPlatform(this);
			UpdateRect();
		}
		private void OnDisable()
		{
			CustomPhysicsManager.instance.RemovePlatform(this);
		}
		private void OnDestroy()
		{
			CustomPhysicsManager.instance.RemovePlatform(this);
		}
	}
}
