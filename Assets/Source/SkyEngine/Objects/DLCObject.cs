using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.Objects 
{
    [AddComponentMenu("SkyEngine/Objects/DLC Dependent Object")]
    public class DLCObject : MonoBehaviour
    {
        public string DLC;

        private void Awake()
        {
            gameObject.SetActive(SkyEngine.OwnsDLC(DLC));
        }
    }
}