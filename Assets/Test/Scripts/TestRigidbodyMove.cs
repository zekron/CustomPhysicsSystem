﻿using UnityEngine;
using System.Collections;

namespace CustomPhysics2D.Test
{
    public class TestRigidbodyMove : MonoBehaviour
    {
        public Material normalMaterial;

        public Material hitMaterial;

        public float randomSpeed = 1;

        private SpriteRenderer _renderer;

        private JRigidbody _jRigidbody;

        private Vector2 _destPoint = Vector2.zero;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _jRigidbody = GetComponent<JRigidbody>();
            _jRigidbody.onCollisionEnter += CollisionEnter;
            _jRigidbody.onCollisionExit += CollisionExit;
        }

        private void Start()
        {
            SetDestPoint();
            _jRigidbody.velocity = (_destPoint - (Vector2)transform.position).normalized * randomSpeed;
        }

        // Update is called once per frame
        void Update()
        {
            if (((Vector2)transform.position - _destPoint).magnitude < 1f)
            {
                SetDestPoint();
            }
            if (_jRigidbody.collisionInfo.hitCollider != null)
            {
                SetDestPoint();
            }
            //_jRigidbody.velocity = ( _destPoint - (Vector2)transform.position ).normalized * randomSpeed;
            _jRigidbody.velocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) * 10;
        }

        private void SetDestPoint()
        {
            var width = _jRigidbody.ItemRect.width;
            var height = _jRigidbody.ItemRect.height;
            var worldRect = JPhysicsManager.instance.quadTree.WorldRect;
            //_destPoint.x = Random.Range( worldRect.xMin, worldRect.xMax );
            //_destPoint.y = Random.Range( worldRect.yMin, worldRect.yMax );
            _destPoint.x = Random.Range(worldRect.xMin + width / 2, worldRect.xMax - width / 2);
            _destPoint.y = Random.Range(worldRect.yMin + height / 2, worldRect.yMax - height / 2);
        }

        private void CollisionEnter(CollisionInfo collisionInfo)
        {
            _renderer.material = hitMaterial;
        }

        private void CollisionExit(CollisionInfo collisionInfo)
        {
            _renderer.material = normalMaterial;
        }
    }
}
