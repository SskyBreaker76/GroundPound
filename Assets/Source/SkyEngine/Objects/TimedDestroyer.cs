using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.Objects
{
    [AddComponentMenu("SkyEngine/Objects/Despawn")]
    public class TimedDestroyer : MonoBehaviour
    {
        public bool DestroyOnAwake = true;
        public float Duration = 5;

        private void Awake()
        {
            if (DestroyOnAwake)
            {
                Destroy();
            }
        }

        public void Destroy()
        {
            Destroy(gameObject, Duration);
        }
    }
}