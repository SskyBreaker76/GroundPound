/*
    Developed by Sky MacLennan
 */

using Fusion;
using SkySoft;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sky.GroundPound
{
    /// <summary>
    /// Base class for handling NetworkEntities in GroundPound.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D)), RequireComponent(typeof(AudioSource)), AddComponentMenu("Ground Pound/Networked Entity")]
    public abstract class NetworkedEntity : NetworkBehaviour
    {
        private CircleCollider2D m_Collider;
        protected CircleCollider2D Collider
        {
            get
            {
                if (!m_Collider)
                    m_Collider = GetComponent<CircleCollider2D>();

                return m_Collider;
            }
        }

        private NetworkRigidbody2D m_Rigidbody;
        protected NetworkRigidbody2D Rigidbody
        {
            get
            {
                if (!m_Rigidbody)
                    m_Rigidbody = GetComponent<NetworkRigidbody2D>();

                return m_Rigidbody;
            }
        }

        /// <summary>
        /// Shortcut for GameManager.Instance :3
        /// </summary>
        public GameManager GameManager => GameManager.Instance;

        #region Physics Variables
        public Vector2 Velocity
        {
            get
            {
                return Rigidbody.Rigidbody.velocity;
            }
            protected set
            {
                Rigidbody.Rigidbody.velocity = value;
            }
        }

        public bool IsGrounded
        {
            get
            {
                foreach (Collider2D C in Physics2D.OverlapCircleAll((Vector2)transform.position + Vector2.up * 0.49f, Collider.radius))
                {
                    if (C != Collider)
                    {
                        if (C.CompareTag("Platform"))
                        {
                            if (transform.position.y > C.transform.position.y + C.bounds.size.y / 2)
                                return true;
                        }
                        else
                            return true;
                    }
                }

                return false;
            }
        }

        [SerializeField, Tooltip("The base movement speed of this Entity. We specify BaseMoveSpeed so that naming remains correct if an Entity can modify its move speed")] protected float m_BaseMoveSpeed = 4;
        /// <summary>
        /// All movement code should use this as the movement speed of the Entity, not m_BaseMoveSpeed!
        /// </summary>
        protected virtual float m_MoveSpeed
        {
            get
            {
                return m_BaseMoveSpeed;
            }
        }
        [SerializeField, Tooltip("How long (in seconds) after an Entity slips before it's no longer allowed to jump")] protected float m_CoyoteTime = 0.1f;
        [SerializeField, Tooltip("This defines how the Entity's jump will look and work.")] protected AnimationCurve m_JumpCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 3), new Keyframe(1, 0));
        #endregion

        #region Hitpoint Variables
        [Header("Hitpoints")]
        [SerializeField, Tooltip("Disable this if you don't want this Entity to be destructable")] protected bool m_Mortal = true;
        [SerializeField, Tooltip("The amount of HitPoints this Entity will have at the start of its life")] protected int m_StartHitpoints = 1;
        [DisplayOnly] public int Hitpoints = 1;
        [DisplayOnly] public bool IsLocalPlayer;
        #endregion

        /// <summary>
        /// This is called whenever the Entity's health is modified
        /// </summary>
        /// <param name="Amount"></param>
        public virtual void ModifyHitpoints(int Amount)
        {
            if (!m_Mortal) return; // If the Entity isn't mortal then we shouldn't deal damage to it

            Hitpoints += Amount;
        }

        /// <summary>
        /// Logic for when this Entity is destroyed or defeated
        /// </summary>
        public virtual void OnDefeat()
        {
            /* No default behaviour */
        }

        /// <summary>
        /// DO NOT OVERRIDE THIS FUNCTION.
        /// </summary>
        /// <remarks>
        /// If you want to add custom Network logic in the update loop, please override OnNetworkUpdate instead
        /// </remarks>
        public override void FixedUpdateNetwork()
        {
            if (GetInput(out GroundPoundInputData InputData))
            {
                IsLocalPlayer = true;

                InputTick(InputData, out Vector2 TargetVelocity);
                Velocity = TargetVelocity;
            }
            else
            {
                IsLocalPlayer = false;
            }
        }

        /// <summary>
        /// Logic to run each Network Update (~10ms like base FixedUpdate)
        /// </summary>
        protected virtual void InputTick(GroundPoundInputData InputData, out Vector2 FinalVelocity)
        {
            FinalVelocity = Vector2.zero; // Static object behaviour
        }
    }
}