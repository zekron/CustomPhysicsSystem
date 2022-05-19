using UnityEngine;
using System.Collections;

namespace CustomPhysics2D
{
	public class JPlatform : JCollisionController
	{
		protected override void Awake()
		{
			base.Awake();
			JPhysicsManager.instance.PushPlatform( this );
		}

		private void OnDestroy()
		{
			JPhysicsManager.instance.RemovePlatform( this );
		}
	}
}
