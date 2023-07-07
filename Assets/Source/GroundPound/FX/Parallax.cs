using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sky.GroundPound
{
    [AddComponentMenu("Ground Pound/Parallax Effect")]
    public class Parallax : MonoBehaviour
    {
        public Camera Camera;
        public float ParallaxAmount;
        private Vector3 m_StartPosition;
        private Vector2 m_Length;

        private void Start()
        {
            m_StartPosition = transform.position;
            m_Length = GetComponentInChildren<SpriteRenderer>().bounds.size;
        }

        private void Update()
        {
            Vector3 RelativePosition = Camera.transform.position * ParallaxAmount;
            Vector3 Distance = Camera.transform.position - RelativePosition;
            if (Distance.x > m_StartPosition.x + m_Length.x)
                m_StartPosition.x += m_Length.x;
            if (Distance.x < m_StartPosition.x - m_Length.x)
            {
                m_StartPosition.x -= m_Length.x;
            }

            RelativePosition.z = m_StartPosition.z;
            transform.position = m_StartPosition + RelativePosition;
        }
    }
}