using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SkySoft.Rendering
{
    [RequireComponent(typeof(DecalProjector)), AddComponentMenu("SkyEngine/Rendering/Shadow")]
    public class Shadow : MonoBehaviour
    {
        private DecalProjector m_Projector => GetComponent<DecalProjector>();
        private Camera m_Camera => GetComponentInChildren<Camera>();

        public float ShadowSize = 3;

        private void Update()
        {
            m_Projector.size = new Vector3(ShadowSize, ShadowSize, m_Projector.size.z);
            m_Projector.pivot = new Vector3(0, 0, m_Projector.size.z / 2);
            m_Camera.orthographicSize = ShadowSize / 2;
        }
    }
}