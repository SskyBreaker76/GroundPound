/*
    Developed by Sky MacLennan
 */

using UnityEngine;
using SkySoft;
using Fusion;

namespace Sky.GroundPound
{
    public struct GroundPoundInputData : INetworkInput
    {
        public bool Jump => mJump == Player.BUTTON_ON;
        public bool Dash => mDash == Player.BUTTON_ON;
        public bool GroundPound => mGroundPound == Player.BUTTON_ON;
        public bool Bolt => mBolt == Player.BUTTON_ON;

        public byte mJump, mDash, mGroundPound, mBolt;
        public float Direction;
    }

    public enum PlayerState
    {
        Normal,
        Dash,
        GroundPound,
        GroundPoundLAND
    }

    [AddComponentMenu("Ground Pound/Player (Networked Entity)")]
    public class Player : NetworkedEntity
    {
        public const byte BUTTON_ON = 0x01;
        public const byte BUTTON_OFF = 0x00;

        private PlayerState m_State = PlayerState.Normal;
        public static Player LocalPlayer;

        [Header("PLAYER")]
        public GameObject DeathParticle;
        [Space]
        public AudioClip JumpSound;
        private float LastJump;
        public AudioClip DashSound;
        private Animator m_Animator;
        protected Animator Animator
        {
            get
            {
                if (!m_Animator)
                        m_Animator = GetComponentInChildren<Animator>();
                
                return m_Animator;
            }
        }
        private SpriteRenderer m_Renderer;
        protected SpriteRenderer Renderer
        {
            get
            {
                if (!m_Renderer)
                    m_Renderer = GetComponentInChildren<SpriteRenderer>();

                return m_Renderer;
            }
        }
        
        [Networked] public NetworkBool Facing { get; set; }
        public float GroundPoundDuration = 0.3f;
        public LayerMask GroundLayers;
        private Vector2 GroundPoundStart;
        private Vector2 GroundPoundEnd;
        public AnimationCurve GroundPoundAnimation = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        private float m_GroundPoundV = 0;
        public float JumpForce = 70;
        public float JumpInputDuration = 1.1f;
        public float DashLength = 0.3f;
        public float DashDelay = 1;
        private float DashDirection = 0;
        private float LastDash = 0;
        private float DashTimer = 0;
        public float AirMovementSensitivity = 3;
        [SerializeField, DisplayOnly] private float JumpForceThisInput = 1;

        [SerializeField, DisplayOnly] private float Horizontal;
        [SerializeField, DisplayOnly] private bool Jump;

        private float LastGrounded;
        private float LastH;

        private bool DashRight, DashLeft;

        public override void OnDefeat()
        {
            Transform Effect = Instantiate(DeathParticle, transform.position, Quaternion.identity).transform;

            if (Renderer)
            {
                ParticleSystem Particles;
                if (Particles = Effect.GetComponent<ParticleSystem>())
                {
                    ParticleSystem.MainModule M = Particles.main;
                    M.startColor = Renderer.color;
                }
            }

            Effect.LookAt(GameManager.Instance.MapCentre);
            GameManager.Instance.ResetLevel();
        }

        protected override void InputTick(GroundPoundInputData InputData, out Vector2 FinalVelocity, out bool DoGravity)
        {
            if (Renderer)
                Renderer.transform.localScale = new Vector3(Facing ? 1 : -1, 1, 1);

            Vector2 TargetVelocity = BaseVelocity;
            Vector2 GroundPosition = Vector2.zero;

            bool Grounded = GetIsGrounded(out GroundPosition); // This looks odd but it's more efficient to do this because base.IsGrounded actually runs a whole function when it's retrieved
            bool CanJump = (Grounded || Time.time - LastGrounded < m_CoyoteTime);

            bool DoGrav = !CanJump; // This means that during CoyoteTime the player will actually hold their position in the air

            #region Animation Behaviour
            if (Animator)
            {
                switch (m_State)
                {
                    case PlayerState.Normal:
                        if (!Grounded)
                            Animator.Play("Airborne");
                        else
                            if (Mathf.Approximately(TargetVelocity.x, 0))
                            Animator.Play("Idle");
                        else
                            Animator.Play("Run");
                        break;
                    case PlayerState.Dash:
                        Animator.Play("Dash");
                        break;
                    case PlayerState.GroundPound:
                        if (!Grounded)
                            Animator.Play("GroundPound_Lp");
                        else
                            m_State = PlayerState.GroundPoundLAND;
                        break;
                    case PlayerState.GroundPoundLAND:
                        Animator.Play("GroundPoundLand");
                        break;
                }
            }   
            #endregion

            if (HasInputAuthority)
                LocalPlayer = this;

            if (m_State != PlayerState.GroundPound && m_State != PlayerState.GroundPoundLAND)
            {
                m_GroundPoundV = 0;

                Horizontal = m_State == PlayerState.Normal ? InputData.Direction : DashDirection;
                Jump = InputData.Jump && HasReachedApex;

                bool DashInput = Time.time - LastDash > DashDelay && InputData.Dash;

                if (DashInput)
                {
                    DashRight = Horizontal > 0;
                    DashLeft = Horizontal < 0;
                }

                if (Horizontal < 0)
                    Facing = false;
                if (Horizontal > 0)
                    Facing = true;

                // Please don't look. It's ugly, but it works and it's fast
                if (DashTimer <= 0)
                {
                    m_State = PlayerState.Normal;

                    bool DoDash = false;

                    if (DashRight)
                    {
                        DashDirection = 1;
                        DoDash = true;
                        DashRight = false;
                    }
                    if (DashLeft)
                    {
                        DashDirection = -1;
                        DoDash = true;
                        DashLeft = false;
                    }

                    if (DoDash)
                    {
                        if (Audio && DashSound)
                        {
                            Audio.PlayOneShot(DashSound);
                        }

                        HasReachedApex = true;
                        m_State = PlayerState.Dash;
                        LastDash = Time.time;
                        DashTimer = DashLength;
                    }
                }

                DashTimer -= Time.fixedDeltaTime;

                if (Grounded)
                {
                    LastGrounded = Time.time;
                }
                if (CanJump)
                {
                    if (Jump)
                        if (Audio && JumpSound && Time.time - LastJump > 0.2f) // Should take less than 0.2 seconds for the game to no longer allow the player to jump
                        {
                            Audio.PlayOneShot(JumpSound);
                            LastJump = Time.time;
                        }

                    JumpForceThisInput = 1;
                }

                if (Jump)
                {
                    if (JumpForceThisInput > 0)
                    {
                        m_GravityEffect = Physics2D.gravity.y;
                        HasReachedApex = false;
                        TargetVelocity.y = JumpForce * JumpForceThisInput;
                    }
                }

                if (!CanJump)
                {
                    JumpForceThisInput -= Time.fixedDeltaTime / JumpInputDuration;

                    if (!Jump)
                        JumpForceThisInput = 0;
                }

                if (!Grounded)
                {
                    if (InputData.GroundPound)
                    {
                        RaycastHit2D Hit;

                        if (Hit = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, GroundLayers))
                        {
                            if (Vector2.Distance(transform.position, Hit.point) > 1.25f) // Prevents the player from slamming the ground when they're too close to the ground
                            {
                                GroundPoundStart = transform.position;
                                GroundPoundEnd = Hit.point;
                                m_State = PlayerState.GroundPound;
                            }
                        }
                    }
                }

                if (m_State != PlayerState.Dash)
                {
                    if (Grounded)
                        TargetVelocity.x = Horizontal * m_BaseMoveSpeed;
                    else
                        TargetVelocity.x += (Horizontal * m_BaseMoveSpeed) * (Time.fixedDeltaTime * 2);
                }
                else
                {
                    m_GravityEffect = 0;
                    TargetVelocity.y = 0;
                    TargetVelocity.x = DashDirection * (m_MoveSpeed * 6);
                }

                if (m_State != PlayerState.Dash)
                    TargetVelocity.x = Mathf.Clamp(TargetVelocity.x, -m_BaseMoveSpeed, m_BaseMoveSpeed);
                else
                    DoGrav = false;

                FinalVelocity = TargetVelocity;

                LastH = Horizontal;
            }
            else
            {
                HasReachedApex = true;

                m_GroundPoundV += Time.fixedDeltaTime / GroundPoundDuration;

                transform.position = Vector2.Lerp(GroundPoundStart, GroundPoundEnd, GroundPoundAnimation.Evaluate(m_GroundPoundV * SkyEngine.GetCurveDuration(GroundPoundAnimation)));

                if (m_GroundPoundV >= 1)
                    m_State = PlayerState.Normal;

                FinalVelocity = Vector2.zero;
            }

            DoGravity = DoGrav;
        }
    }
}
