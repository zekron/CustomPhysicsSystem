using UnityEngine;
using System.Collections;

namespace CustomPhysics2D
{
	public class JPlatform : JCollisionController
	{
		protected override void Awake()
		{
			base.Awake();
		}

		private void OnEnable()
		{
			JPhysicsManager.instance.PushPlatform(this);
			UpdateRect();
		}
		private void OnDisable()
		{
			JPhysicsManager.instance.RemovePlatform(this);
		}
		private void OnDestroy()
		{
			JPhysicsManager.instance.RemovePlatform(this);
		}
	}
}
