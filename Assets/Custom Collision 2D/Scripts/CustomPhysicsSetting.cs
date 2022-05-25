using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CustomPhysics2D
{
	[System.Serializable]
	public class CollisionLayerMask
	{
		public int layer;

		public LayerMask layerMask;
	}

	public class CustomPhysicsSetting : ScriptableObject
	{
		#region Gravity
		public Vector2 Gravity => _gravity * _gravityScale;

		public float GravityScale
		{
			get
			{
				return _gravityScale;
			}

			set
			{
				_gravityScale = value;
			}
		}
		protected float _gravityScale = 1.0f;

		[SerializeField]
		private Vector2 _gravity = new Vector2( 0.0f, -150.0f );
		#endregion

		/// <summary>
		/// LayerMask
		/// </summary>
		[Space( 10 )]
		public List<CollisionLayerMask> collisionLayerMasks = new List<CollisionLayerMask>();

		public LayerMask GetCollisionMask( int layer )
		{
			for( int i = 0; i < collisionLayerMasks.Count; i++ )
			{
				var collisionLayerMask = collisionLayerMasks[i];
				if( collisionLayerMask.layer == layer )
				{
					return collisionLayerMask.layerMask;
				}
			}

			throw new System.Exception( "Please give a right layer" );
		}
	}
}
