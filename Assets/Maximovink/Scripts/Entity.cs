using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MaximovInk
{
    [ExecuteInEditMode]
    public class Entity : MonoBehaviour
    {
        public bool dynamic = false;

        [SerializeField] private float floorHeight;
        private float m_spriteLowerBound;
        private float m_spriteHalfWidth;
        private readonly float m_tan30 = Mathf.Tan(Mathf.PI / 5);

        private void OnEnable()
        {
            Sort();
        }

        private void Awake()
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                m_spriteLowerBound = spriteRenderer.bounds.size.y * 0.5f;
                m_spriteHalfWidth = spriteRenderer.bounds.size.x * 0.5f;
            }
            else
            {
                m_spriteLowerBound = 0.5f;
                m_spriteHalfWidth = 0.5f;
            }

Sort();
        }

// Use this condition for objects that don�t move in the scene.

        protected virtual void LateUpdate()
        {
#if UNITY_EDITOR
            // Use this condition for objects that don�t move in the scene.
            if (!Application.isPlaying || dynamic)
            {
                Sort();
            }
#else
        if (dynamic)
        {
           Sort();
        }
#endif
        }

        public void Sort()
        {
            transform.position = new Vector3
            (
                transform.position.x,
                transform.position.y,
                (transform.position.y - m_spriteLowerBound + floorHeight) * m_tan30
            );
        }

        void OnDrawGizmos()
        {
            Vector3 floorHeightPos = new Vector3
            (
                transform.position.x,
                transform.position.y - m_spriteLowerBound + floorHeight,
                transform.position.z
            );

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(floorHeightPos + Vector3.left * m_spriteHalfWidth,
                floorHeightPos + Vector3.right * m_spriteHalfWidth);
        }

        /*protected virtual void LateUpdate()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y);
        }*/
    }
}