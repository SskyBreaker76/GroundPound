using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.Audio
{
    [AddComponentMenu("SkyEngine/Audio/Ignore Listener Pause")]
    public class IgnoreListenerPause : MonoBehaviour
    {
        private void Awake()
        {
            AudioSource Src;
            if (Src = GetComponent<AudioSource>())
            {
                Src.ignoreListenerPause = true;
            }
            else
            {
                Debug.LogWarning("IgnoreListenerPause should only be used on objects that contain an AudioSource!");
            }
        }
    }
}