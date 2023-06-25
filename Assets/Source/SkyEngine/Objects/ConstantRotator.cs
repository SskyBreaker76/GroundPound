using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.Objects
{
    [AddComponentMenu("SkyEngine/Objects/Constant Rotator")]
    public class ConstantRotator : MonoBehaviour
    {
        public Vector3 RotationRate;

        private void Update()
        {
            transform.Rotate(RotationRate * Time.deltaTime);
        }
    }
}