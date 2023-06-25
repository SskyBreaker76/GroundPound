using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.Objects
{
    [CreateAssetMenu(fileName = "New WeaponModel", menuName = "SkyEngine/Weapon Model")]
    public class WeaponModel : ScriptableObject
    {
        public GameObject Prefab;
        public Vector3 PositionOffset;
        public Vector3 RotationOffset;
    }
}