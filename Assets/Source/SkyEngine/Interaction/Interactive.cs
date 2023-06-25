using SkySoft.Entities;
using SkySoft.Events;
using SkySoft.Events.Graph;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace SkySoft.Interaction
{
    [AddComponentMenu("SkyEngine/Interaction/Interactive")]
    public class Interactive : Entity
    {
        public bool Enabled = true;
        [Combo(True: "Local-Space", False: "World-Space", Label = "Icon Positioning")]
        public bool IsIconLocal = true;
        [Combo(True: "Player", False: "This", Label = "Icon Target")]
        public bool IconTargetsPlayer = false;
        [Space]
        public InteractionType Icon;
        [SerializeField] private Vector3 m_IconOffset;
        public Vector3 IconPosition
        {
            get
            {
                if (!IconTargetsPlayer)
                {
                    if (IsIconLocal)
                    {
                        return transform.position + (transform.right * m_IconOffset.x + transform.up * m_IconOffset.y + transform.forward * m_IconOffset.z);
                    }
                    else
                    {
                        return transform.position + (Vector3.right * m_IconOffset.x + Vector3.up * m_IconOffset.y + Vector3.forward * m_IconOffset.z);
                    }
                }
                else
                {
                    Transform Player = SkyEngine.PlayerEntity != null ? SkyEngine.PlayerEntity.transform : transform;

                    if (IsIconLocal)
                    {
                        return Player.position + (Player.right * m_IconOffset.x + Player.up * m_IconOffset.y + Player.forward * m_IconOffset.z);
                    }
                    else
                    {
                        return Player.position + (Vector3.right * m_IconOffset.x + Vector3.up * m_IconOffset.y + Vector3.forward * m_IconOffset.z);
                    }
                }
            }
        }

        public Vector3 IconOffset { get => m_IconOffset; set => m_IconOffset = value; }

        public UnityEvent<Entities.Entity> OnInteract;

        [Button("Add Triggers")]
        public bool AddTriggers;
        [Button("Convert to Event")]
        public bool ConvertToEvent;

        public override void OnValidate()
        {
            base.OnValidate();

            if (AddTriggers)
            {
                ObjectTrigger A = gameObject.AddComponent<ObjectTrigger>();
                ObjectTrigger B = gameObject.AddComponent<ObjectTrigger>();
                A.Mode = TriggerType.TriggerEnter;
                B.Mode = TriggerType.TriggerLeave;
                AddTriggers = false;
            }

            if (ConvertToEvent)
            {
                gameObject.AddComponent<SkyEngineEvent>();
                DestroyImmediate(this);

                ConvertToEvent = false;
            }
        }

        protected virtual void OnDrawGizmosSelected()
        {
            SkyEngine.Gizmos.Colour = Color.yellow;
           SkyEngine.Gizmos.DrawWireSphere(IconPosition, 0.25f);
        }

        public void AllowInteract()
        {
            InteractionManager.Instance.SetSelected(this);
        }

        public void DisallowInteract()
        {
            if (InteractionManager.Instance.GetIsSelected(this))
                InteractionManager.Instance.ClearSelected();
        }

        public void Interact(Entities.Entity Entity)
        {
            if (Enabled)
            {
                OnInteract.Invoke(Entity);

                Interaction(Entity);
            }
        }

        protected virtual void Interaction(Entities.Entity Entity = null) { }

        [Obsolete()]
        public void AddFollower(int FollowerIndex)
        {
            //FollowerManager.AddFollower(FollowerIndex);
        }

        [Obsolete()]
        public void RemoveFollower(int FollowerIndex)
        {
            //FollowerManager.RemoveFollower(FollowerIndex);
        }

        public void RunEvent(EventTree Event)
        {
            EventGraphManager.Instance.ExecuteEvent(Event, this);
        }
    }
}