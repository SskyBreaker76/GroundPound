/*
    Developed by Sky MacLennan
 */

using Fusion;
using SkySoft;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sky.GroundPound
{
    /// <summary>
    /// Base class for handling NetworkEntities in GroundPound.
    /// </summary>
    [RequireComponent(typeof(NetworkTransform)), RequireComponent(typeof(CircleCollider2D)), RequireComponent(typeof(AudioSource))]
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

        /// <summary>
        /// Shortcut for GameManager.Instance :3
        /// </summary>
        public GameManager GameManager => GameManager.Instance;

        #region Physics Variables
        private Vector2 m_Velocity = Vector2.zero;
        private Vector2 m_DeltaVelocity => m_Velocity * Time.fixedDeltaTime;
        public Vector2 Velocity
        {
            get
            {
                return m_Velocity;
            }
            private set
            {
                m_Velocity = value;
            }
        }

        [Header("Physics")]
        [SerializeField, Tooltip("Is this Entity affected by the laws of Physics(2D)?")] protected bool m_EnablePhysics = true;
        [SerializeField, Tooltip("How far a collision check extends beyond this Entity's Collider.\nDoes not affect collision checks on other Entity's"), Range(0.001f, 0.1f)] protected float m_CollisionSkin = 0.025f;
        [SerializeField, Tooltip("How large the collision circle is")] protected float m_CollisionSize = 0.975f;
        [SerializeField, Tooltip("This Entity will only be able to collide with objects on these layers.")] protected LayerMask m_CollidesWith;
        [SerializeField, Tooltip("Determines which tag this Entity will treat as a platform")] protected string m_PlatformTag = "Platform";
        [SerializeField, Tooltip("This Entity will not collide with any objects using these tags")] protected List<string> m_IgnoredTags;
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

        protected void OnDrawGizmosSelected()
        {
            SkyEngine.Gizmos.Matrix = Matrix4x4.TRS(transform.position + Vector3.up * 0.5f, Quaternion.identity, Vector3.one);

            SkyEngine.Gizmos.Colour = Color.red;
            SkyEngine.Gizmos.DrawCircle(Vector2.right * m_CollisionSkin, m_CollisionSize);
            SkyEngine.Gizmos.DrawCircle(Vector2.left * m_CollisionSkin, m_CollisionSize);
            SkyEngine.Gizmos.DrawCircle(Vector2.down * m_CollisionSkin, m_CollisionSize);

            SkyEngine.Gizmos.Colour = Color.yellow;
            SkyEngine.Gizmos.DrawCircle(Vector2.right * m_CollisionSkin, m_CollisionSize + m_CollisionSkin);
            SkyEngine.Gizmos.DrawCircle(Vector2.left * m_CollisionSkin, m_CollisionSize + m_CollisionSkin);
            SkyEngine.Gizmos.DrawCircle(Vector2.down * m_CollisionSkin, m_CollisionSize + m_CollisionSkin);
        }

        /// <summary>
        /// Use normalised directions.
        /// </summary>
        /// <example>Up = 0, 1
        /// Left = -1, 0
        /// Down = 0, -1
        /// Right = 1, 0</example>
        /// <param name="Direction"></param>
        /// <param name="CollisionPosition"></param>
        /// <returns></returns>
        protected bool CollisionCheck(Vector2 Direction, out Vector2 CollisionPosition)
        {
            // We use OverlapCircleAll so we can filter out any collisions that shouldn't be registered
            Collider2D[] Collisions = Physics2D.OverlapCircleAll((Vector2)transform.position + Vector2.up * 0.5f + Direction * m_CollisionSkin, m_CollisionSize + m_CollisionSkin, m_CollidesWith, transform.position.z);
            List<Collider2D> FilteredCollisions = new List<Collider2D>();

            Collider2D WinningCollider = null;
            float WinningColliderDistance = Mathf.Infinity;

            foreach (Collider2D Collider in Collisions)
            {
                float Distance = 0;

                if (WinningCollider == null || (Distance = Vector2.Distance(transform.position, Collider.transform.position)) < WinningColliderDistance)
                {
                    WinningCollider = Collider;
                    WinningColliderDistance = Distance;
                }

                if (!m_IgnoredTags.Contains(Collider.tag))
                    FilteredCollisions.Add(Collider);
            }

            if (WinningCollider)
                CollisionPosition = WinningCollider.ClosestPoint((Vector2)transform.position + Vector2.up * 0.5f);
            else
                CollisionPosition = Vector2.zero;

            return FilteredCollisions.Count > 0;
        }

        /// <summary>
        /// Checks if there is any ground underneath the Entity and return the Height of the ground
        /// </summary>
        /// <param name="GroundHeight"></param>
        /// <returns></returns>
        public bool CheckForGround(out float GroundHeight)
        {
            Vector2 Position = Vector2.zero;
            bool IsColliding = CollisionCheck(Vector2.down, out Position);
            GroundHeight = Position.y;
            return IsColliding;
        }

        public bool CheckLeft()
        {
            Vector2 Dummy = Vector2.zero; // Yes, I know this looks dumb. I don't care
            return CollisionCheck(Vector2.left, out Dummy);
        }

        public bool CheckRight()
        {
            Vector2 Dummy = Vector2.zero; // DUMB DUMB DUMB DUMB
            return CollisionCheck(Vector2.right, out Dummy);
        }

        public bool IsOnPlatform { get; private set; }
        private bool RunPlatformCheck()
        {
            // We use OverlapCircleAll so we can filter out any collisions that shouldn't be registered
            Collider2D[] Collisions = Physics2D.OverlapCircleAll((Vector2)transform.position + Vector2.up * 0.5f + Vector2.down * m_CollisionSkin, m_CollisionSize + m_CollisionSkin, m_CollidesWith, transform.position.z);
            List<Collider2D> FilteredCollisions = new List<Collider2D>();

            Collider2D WinningCollider = null;
            float WinningColliderDistance = Mathf.Infinity;

            foreach (Collider2D Collider in Collisions)
            {
                float Distance = 0;

                if (WinningCollider == null || (Distance = Vector2.Distance(transform.position, Collider.transform.position)) < WinningColliderDistance)
                {
                    WinningCollider = Collider;
                    WinningColliderDistance = Distance;
                }

                if (!m_IgnoredTags.Contains(Collider.tag))
                    FilteredCollisions.Add(Collider);
            }

            if (WinningCollider && WinningCollider.tag == m_PlatformTag)
            {
                IsOnPlatform = true;
                transform.parent = WinningCollider.transform;
            }
            else
            {
                IsOnPlatform = false;
                transform.parent = null;
            }

            return IsOnPlatform;
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

        protected void Jump()
        {
            InterruptJump = false;

            if (!IsOnPlatform && !CheckForGround(out _))
                return;

            m_Velocity.y = m_Velocity.y = m_JumpCurve.Evaluate(0);
            StartCoroutine(IJump());
        }

        private bool InterruptJump = false;

        private IEnumerator IJump()
        {
            float JumpTime = 0;

            while (JumpTime < m_JumpCurve.keys[m_JumpCurve.length - 1].time)
            {
                m_Velocity.y = m_JumpCurve.Evaluate(JumpTime);
                JumpTime += Time.fixedDeltaTime * 100;
                yield return new WaitForFixedUpdate();

                if (InterruptJump)
                    break;
            }
        }

        /// <summary>
        /// DO NOT OVERRIDE THIS FUNCTION.
        /// </summary>
        /// <remarks>
        /// If you want to add custom Network logic in the update loop, please override OnNetworkUpdate instead
        /// </remarks>
        public override void FixedUpdateNetwork()
        {
            IsLocalPlayer = HasStateAuthority;

            if (!HasStateAuthority) // We don't want other Clients being able to control our Entity
                return;

            #region Physics Loop
            if (m_EnablePhysics)
            {
                RunPlatformCheck(); // This sets up IsOnPlatform for use of sorts in derived classes.
                bool Colliding = CheckForGround(out float GroundHeight);

                if (!Colliding)
                {
                    m_Velocity.y += Physics2D.gravity.y * Time.fixedDeltaTime;
                }
                else
                {
                    if (transform.position.y < GroundHeight)
                        transform.position = new Vector3(transform.position.x, GroundHeight, transform.position.z);

                    InterruptJump = true;
                }
                // Below we run our left/right collision checks. Should result in the player stopping if they try to run into a wall.
                // Later we should also try to make sure that if a wall pushes the player they move with it

                if (CheckRight())
                    if (m_Velocity.x > 0)
                        m_Velocity.x = 0;

                if (CheckLeft())
                    if (m_Velocity.x < 0)
                        m_Velocity.x = 0;

                Vector2 V = m_DeltaVelocity; // This makes our physics code run slightly faster on shit CPU's
                transform.position += new Vector3(V.x, V.y, 0);
            }
            #endregion

            OnNetworkUpdate();
        }

        /// <summary>
        /// Logic to run each Network Update (~10ms like base FixedUpdate)
        /// </summary>
        protected virtual void OnNetworkUpdate()
        {

        }
    }
}