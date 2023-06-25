/*
    Developed by Sky MacLennan
 */

using UnityEngine;
using SkySoft;

namespace Sky.GroundPound
{
    public enum PlayerState
    {
        Normal,
        Dash,
        GroundPound
    }

    [AddComponentMenu("Ground Pound/Player (Networked Entity)")]
    public class Player : NetworkedEntity
    {
        private PlayerState m_State = PlayerState.Normal;
        public static Player LocalPlayer;

        [Header("PLAYER")]
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
        
        private bool Facing;

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
        [SerializeField, DisplayOnly] private float JumpForceThisInput = 1;

        [SerializeField, DisplayOnly] private float Horizontal;
        [SerializeField, DisplayOnly] private bool Jump;

        private float LastGrounded;
        private float LastH;

        private bool DashInput;
        private bool DashRight, DashLeft;

        private void Update()
        {
            if (Time.time - LastDash > DashDelay && SkyEngine.Input.Gameplay.Dash.WasPressedThisFrame())
            {
                DashInput = true;
            }
        }

        protected override void InputTick(out Vector2 FinalVelocity)
        {
            LocalPlayer = this;

            if (Renderer)
                Renderer.flipX = !Facing;

            if (m_State != PlayerState.GroundPound)
            {
                m_GroundPoundV = 0;

                Vector2 Input = SkyEngine.Input.Gameplay.Move.ReadValue<Vector2>();
                Horizontal = m_State == PlayerState.Normal ? Input.x : DashDirection;
                Jump = SkyEngine.Input.Gameplay.Jump.IsPressed() || Input.y > 0;

                if (DashInput)
                {
                    DashRight = Horizontal > 0;
                    DashLeft = Horizontal < 0;
                    DashInput = false;
                }

                if ((int)Horizontal != (int)LastH)
                {
                    if (Horizontal < 0)
                        Facing = false;
                    if (Horizontal > 0)
                        Facing = true;
                }

                bool Grounded = IsGrounded; // This looks odd but it's more efficient to do this because base.IsGrounded actually runs a whole function when it's retrieved
                bool CanJump = Grounded || Time.time - LastGrounded < m_CoyoteTime;

                Vector2 TargetVelocity = Velocity;

                // Please don't look. It's ugly, but it works and it's fast
                if (DashTimer <= 0)
                {
                    m_State = PlayerState.Normal;

                    bool DoDash = false;

                    if (DashRight)
                    {
                        DashDirection = 6;
                        DoDash = true;
                        DashRight = false;
                    }
                    if (DashLeft)
                    {
                        DashDirection = -6;
                        DoDash = true;
                        DashLeft = false;
                    }

                    if (DoDash)
                    {
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
                    JumpForceThisInput = 1;
                }

                if (Jump)
                {
                    if (JumpForceThisInput > 0)
                    {
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
                    if (Input.y < 0)
                    {
                        RaycastHit2D Hit;

                        if (Hit = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, GroundLayers))
                        {
                            if (Vector2.Distance(transform.position, Hit.point) > 2f) // Prevents the player from slamming the ground when they're too close to the ground
                            {
                                GroundPoundStart = transform.position;
                                GroundPoundEnd = Hit.point;
                                m_State = PlayerState.GroundPound;
                            }
                        }
                    }
                }

                TargetVelocity.x = Horizontal * m_BaseMoveSpeed;
                FinalVelocity = TargetVelocity;
                LastH = Horizontal;
            }
            else
            {
                m_GroundPoundV += Time.fixedDeltaTime / GroundPoundDuration;

                transform.position = Vector2.Lerp(GroundPoundStart, GroundPoundEnd, GroundPoundAnimation.Evaluate(m_GroundPoundV));

                if (m_GroundPoundV >= 1)
                    m_State = PlayerState.Normal;

                FinalVelocity = Vector2.zero;
            }
        }
    }
}
