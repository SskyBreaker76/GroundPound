using System.Linq;
using SkySoft.Entities;
using UnityEngine;

namespace SkySoft.Events
{
    public enum TriggerType
    {
        TriggerEnter,
        TriggerStay,
        TriggerLeave
    }

    [AddComponentMenu("SkyEngine/Events/Object Trigger")]
    public class ObjectTrigger : Trigger
    {
        [Tooltip("This is only needed if you have an entity that uses hitboxes and this is being used as a daamage trigger.")]
        public Entity ParentEntity;
        public TriggerType Mode;

        private Entity ToDamage;

        public void SetDamageTarget(Entity Entity)
        {
            if (ParentEntity == null || (Entity != ParentEntity && !ParentEntity.GetComponentsInChildren<Entity>().ToList().Contains(Entity)))
            {
                ToDamage = Entity;
            }
        }

        public void DamageTarget(float Damage)
        {
            if (ToDamage)
            {
                ToDamage.Damage(Damage);
            }
        }

        public void DamageEntity(Entity E, float Damage)
        {
            E.Damage(Damage);
        }

        private void OnTriggerEnter(Collider Other)
        {
            Debug.Log("TriggerEnter()");

            if (Mode == TriggerType.TriggerEnter)
            {
                Entity E;

                if (E = Other.GetComponent<Entity>())
                {
                    PerformTrigger(E);
                }
            }
        }

        private void OnTriggerStay(Collider Other)
        {
            if (Mode == TriggerType.TriggerStay)
            {
                Entity E;

                if (E = Other.GetComponent<Entity>())
                {
                    PerformTrigger(E);
                }
            }
        }

        private void OnTriggerExit(Collider Other)
        {
            if (Mode == TriggerType.TriggerLeave)
            {
                Entity E;

                if (E = Other.GetComponent<Entity>())
                {
                    PerformTrigger(E);
                }
            }
        }
    }
}