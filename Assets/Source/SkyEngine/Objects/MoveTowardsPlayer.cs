using SkySoft;
using SkySoft.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.Objects
{
    [AddComponentMenu("SkyEngine/Objects/Homer")]
    public class MoveTowardsPlayer : MonoBehaviour
    {
        private bool Move = false;

        public bool IsAwake { get; set; }
        [Combo(True: "Awake", False: "Asleep", Label = "Start State")] public bool StartAwake = false;
        [Combo(Label: "Trigger", "Manual", "OnTriggerEnter()", "Time")] public int WakeMode;
        private float SpawnTime;
        public float Timer = 2;
        public Rigidbody Body;
        public LootBox Loot;
        [Button("Quick Setup")] public bool Setup;
        [Space]
        public float InteractionDistance;
        private float Speed = 0;
        public float Accelleration = 2;
        public float Radius = 0;

        private void OnDrawGizmosSelected()
        {
            SkyEngine.Gizmos.Colour = Color.yellow;
           SkyEngine.Gizmos.DrawWireSphere(transform.position, Radius);
            SkyEngine.Gizmos.Colour = Color.red;
           SkyEngine.Gizmos.DrawWireSphere(transform.position, InteractionDistance);
        }

        private void Awake()
        {
            SpawnTime = Time.time;
        }

        private void OnValidate()
        {
            if (Setup)
            {
                Body = GetComponentInParent<Rigidbody>();

                Setup = false;
            }
        }

        private bool HasInteracted = false;

        private void Update()
        {
            if (WakeMode == 2 && Time.time - SpawnTime > Timer)
            {
                IsAwake = true;
            }

            if (!IsAwake)
                return;

            Vector3 Target = SkyEngine.PlayerEntity.transform.position + Vector3.up;

            Move = Vector3.Distance(transform.position, Target) < Radius;

            if (Move)
            {
                Speed += Time.deltaTime * Accelleration;
                Body.isKinematic = true;
            }
            else
            {
                Speed -= Time.deltaTime * Accelleration;
                Body.isKinematic = false;
            }

            if (Speed < 0)
                Speed = 0;

            Body.transform.position = Vector3.MoveTowards(Body.transform.position, Target, Speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, Target) < InteractionDistance && !HasInteracted)
            {
                Loot.Interact(SkyEngine.PlayerEntity);
            }
        }
    }
}