using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.Audio
{
    [RequireComponent(typeof(AudioSource)), AddComponentMenu("SkyEngine/Audio/Non-Directional Filter")]
    public class NonDirectionalAudioSource : MonoBehaviour
    {
        public float BaseVolume = 1;
        private AudioSource m_Source => GetComponent<AudioSource>();

        private float TargetVolume 
        { 
            get
            {
                float Dist = Vector3.Distance(transform.position, FindObjectOfType<AudioListener>().transform.position);
                float Dist01 = (Dist - m_Source.minDistance) / (m_Source.maxDistance - m_Source.minDistance);
                return Mathf.Lerp(BaseVolume, 0, Dist01);
            }
        }

        private void Update()
        {
            m_Source.volume = Mathf.MoveTowards(m_Source.volume, TargetVolume, Time.deltaTime * 10);
        }
    }
}
