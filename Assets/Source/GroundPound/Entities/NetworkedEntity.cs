/*
    Developed by Sky MacLennan
 */

using ExitGames.Client.Photon.StructWrapping;
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
        public NetworkRigidbody2D Rigidbody
        {
            get
            {
                if (!m_Rigidbody)
                    m_Rigidbody = GetComponent<NetworkRigidbody2D>();

                return m_Rigidbody;
            }
        }

        private AudioSource m_Audio;
        public AudioSource Audio
        {
            get
            {
                if (!m_Audio)
                    m_Audio = GetComponent<AudioSource>();

                return m_Audio;
            }
        }

        public float CollisionCentre
        {
            get
            {
                if (Collider)
                {
                    return Collider.offset.y;
                }

                return 0.5f;
            }
        }

        private Vector2 m_KnockbackForce = Vector2.zero;
        protected Vector2 KnockbackForce => m_KnockbackForce;

        /// <summary>
        /// Shortcut for GameManager.Instance :3
        /// </summary>
        public GameManager GameManager => GameManager.Instance;

        #region Physics Variables
        public float GravityMultiplier = 1.1f;
        public float GravitySpeed = -1;
        protected float m_GravityEffect;
        public Vector2 Velocity
        {
            get
            {
                return Rigidbody.ReadVelocity();
            }
            protected set
            {
                Rigidbody.Rigidbody.velocity = value;
            }
        }

        [DisplayOnly] public bool HasReachedApex = true;

        /// <summary>
        /// This exists souly to prevent stack overflow errors
        /// </summary>
        protected bool m_WasGroundedLastCheck = true;
        public bool IsGrounded
        {
            get
            {
                return GetIsGrounded(out _);
            }
        }

        public virtual bool GetIsGrounded(out Vector2 GroundPosition)
        {
            List<Collider2D> MyColliders = GetComponentsInChildren<Collider2D>().ToList();

            foreach (Collider2D C in Physics2D.OverlapCircleAll((Vector2)transform.position + Vector2.up * (CollisionCentre * 0.95f), Collider.radius))
            {
                if (C.tag == "Cloud") continue;

                if (!MyColliders.Contains(C))
                {
                    if (C.CompareTag("Platform"))
                    {
                        if (transform.position.y > C.transform.position.y)
                        {
                            GroundPosition = C.ClosestPoint(transform.position);
                            return true;
                        }
                    }
                    else
                    {
                        GroundPosition = C.ClosestPoint(transform.position);
                        return true;
                    }
                }
            }

            GroundPosition = Vector2.zero;
            return false;
        }

        public Collider2D[] GetCollisions(float Radius)
        {
            List<Collider2D> MyColliders = GetComponentsInChildren<Collider2D>().ToList();
            List<Collider2D> Value = new List<Collider2D>();

            foreach (Collider2D C in Physics2D.OverlapCircleAll((Vector2)transform.position + Vector2.up * CollisionCentre, Radius))
            {
                if (!MyColliders.Contains(C))
                {
                    Value.Add(C);
                }
            }
            return Value.ToArray();
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_ApplyForce(float ForceX, float ForceY)
        {
            Debug.Log($"ApplyForce (ForceX: {ForceX}, ForceY: {ForceY})");
            m_KnockbackForce = new Vector2 (ForceX, ForceY);
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
        [SerializeField, Tooltip("This defines how the Entity's jump will look and how long it lasts.")] protected AnimationCurve m_JumpCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 3), new Keyframe(1, 0));
        [SerializeField, Tooltip("The total height of a players jump")] protected float m_JumpHeight = 3;
        [SerializeField, Tooltip("This is just for if you're lazy and can't be bothered properly timing the Jump Curve")] protected float m_JumpAnimationScale = 0.75f;
        
        public Vector2 NearestPoint(Vector2 Target)
        {
            return Collider.ClosestPoint(Target);
        }
        #endregion

        #region Hitpoint Variables
        [Header("Hitpoints")]
        [SerializeField, Tooltip("Disable this if you don't want this Entity to be destructable")] protected bool m_Mortal = true;
        [SerializeField, Tooltip("The amount of HitPoints this Entity will have at the start of its life")] protected int m_StartHitpoints = 1;
        [DisplayOnly] public int Hitpoints = 1;
        [DisplayOnly] public bool IsLocalPlayer;
        public float KnockbackReductionSpeed = 2;
        [Tooltip("Keep the curve's time within the range of 0-1! When evaluating the game takes the decimal value of the players health which is always between 0 and 1")]
        public AnimationCurve KnockbackDuration = new AnimationCurve(new Keyframe(0, 1000000), new Keyframe(0.15f, 5), new Keyframe(1, 0.5f));
        #endregion

        protected virtual void OnValidate()
        {
            Hitpoints = m_StartHitpoints;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
        public virtual void RPC_Kill()
        {
            Hitpoints = 0;
            OnDefeat();
        }

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

        protected Vector2 BaseVelocity { get; private set; } = new Vector2();

        /// <summary>
        /// DO NOT OVERRIDE THIS FUNCTION.
        /// </summary>
        /// <remarks>
        /// If you want to add custom Network logic in the update loop, please override OnNetworkUpdate instead
        /// </remarks>
        public override void FixedUpdateNetwork()
        {
            m_KnockbackForce = Vector2.MoveTowards(m_KnockbackForce, Vector2.zero, Time.fixedDeltaTime * (KnockbackReductionSpeed * KnockbackDuration.Evaluate(Hitpoints / (float)m_StartHitpoints)));

            if (GetInput(out GroundPoundInputData InputData))
            {
                IsLocalPlayer = true;
                bool RunGravityTick;
                InputTick(InputData, out Vector2 TargetVelocity, out RunGravityTick);

                if (!RunGravityTick)
                    m_GravityEffect = 0;
                else
                    m_GravityEffect += Time.fixedDeltaTime * GravitySpeed * GravityMultiplier;

                if (m_GravityEffect < Physics2D.gravity.y * GravityMultiplier)
                    m_GravityEffect = Physics2D.gravity.y * GravityMultiplier;

                BaseVelocity = new Vector2(TargetVelocity.x, TargetVelocity.y + m_GravityEffect);
                TargetVelocity += KnockbackForce;
                Velocity = TargetVelocity;
            }
            else
            {
                IsLocalPlayer = false;
            }

            if (Velocity.y < 0)
                HasReachedApex = true;
        }

        /// <summary>
        /// Logic to run each Network Update (~10ms like base FixedUpdate)
        /// </summary>
        protected virtual void InputTick(GroundPoundInputData InputData, out Vector2 FinalVelocity, out bool RunGravityTick)
        {
            RunGravityTick = true;
            FinalVelocity = Vector2.zero; // Static object behaviour
        }
    }
}