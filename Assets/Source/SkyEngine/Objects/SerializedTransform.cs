using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.Objects
{
    [AddComponentMenu("SkyEngine/Objects/Serialized Transform")]
    public class SerializedTransform : MonoBehaviour
    {
        public Transform Target;

        public Vector3 Position;
        public Quaternion QRotation;
        public Vector3 Rotation;
        public Vector3 Scale = Vector3.one;
    }
}