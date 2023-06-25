using SkySoft;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.Physics
{
    [RequireComponent(typeof(BoxCollider)), AddComponentMenu("SkyEngine/Physics/Wind Area")]
    public class WindArea : MonoBehaviour
    {
        public static List<WindArea> ActiveWindZones = new List<WindArea>();

        public float ArrowSize = 1;

        [Combo(True: "Override", False: "Additive", Label = "Mode"), Tooltip("True = Override, False = Additive")]
        public bool OverrideMode;
        [Range(-100, 100)] public int Priority = 0;
        [Range(-180, 180)] public float DirectionOffset;
        private Transform ForwardHelper;
        public Vector3 GetForward
        {
            get
            {
                if (ForwardHelper != null)
                {
                    ForwardHelper.eulerAngles = transform.eulerAngles + new Vector3(0, DirectionOffset, 0);
                    return ForwardHelper.forward;
                }

                return transform.forward;
            }
        }
        [Space]
        public float Scalar;
        [Space]
        public float MinimumForce;
        public float MaximumForce;

        private void OnDrawGizmos()
        {
            SkyEngine.Gizmos.Colour = Color.yellow;

            SkyEngine.Gizmos.Matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            SkyEngine.Gizmos.DrawWireCube(GetComponent<BoxCollider>().center, GetComponent<BoxCollider>().size);
            SkyEngine.Gizmos.Matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one * ArrowSize);

            SkyEngine.Gizmos.DrawArrow(Vector3.zero, Quaternion.Euler(0, DirectionOffset, 0));
        }

        private void OnValidate()
        {
            GetComponent<BoxCollider>().isTrigger = true;
            gameObject.layer = LayerMask.NameToLayer("Trigger");
        }

        private void Awake()
        {
            ForwardHelper = new GameObject($"{name}_ForwardHelp").transform;
            ActiveWindZones.Clear();
        }

        private void OnTriggerStay(Collider Other)
        {
            if (Other.gameObject == SkyEngine.PlayerEntity.gameObject)
            {
                if (!ActiveWindZones.Contains(this))
                {
                    ActiveWindZones.Add(this);
                }
            }
        }

        private void OnTriggerExit(Collider Other)
        {
            if (Other.gameObject == SkyEngine.PlayerEntity.gameObject)
            {
                ActiveWindZones.Remove(this);
            }
        }
    }
}