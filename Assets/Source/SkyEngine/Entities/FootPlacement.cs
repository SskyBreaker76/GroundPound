using SkySoft.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.Entities
{
    [AddComponentMenu("SkyEngine/Entities/Foot Placement")]
    public class FootPlacement : MonoBehaviour
    {
        public Objects.Charactermanager Root;

        [Combo(True: "Enabled", False: "Disabled")]
        public bool State = true;
        [SerializeField, Combo("0", "0.25", "0.5", "0.75", "1", "1.25", "1.5", "1.75", "2")]
        private float m_SampleDistance = 5;
        public float SampleDistance
        {
            get
            {
                switch (m_SampleDistance)
                {
                    default:
                        return 1.5f;
                    case 0:
                        return 0;
                    case 1:
                        return 0.25f;
                    case 2:
                        return 0.5f;
                    case 3:
                        return 0.75f;
                    case 4:
                        return 1;
                    case 5:
                        return 1.25f;
                    case 6:
                        return 1.5f;
                    case 7:
                        return 1.75f;
                    case 8:
                        return 2;
                }
            }
        }

        [Range(0, 1)] public float WeightPositionRight = 1;
        [Range(0, 1)] public float WeightRotationRight = 1;
        [Range(0, 1)] public float WeightPositionLeft = 1;
        [Range(0, 1)] public float WeightRotationLeft = 1;

        private Animator Anim;
        public Vector3 PositionOffset;
        public Vector3 RotationOffset;
        public float BodyHeightOffset;
        public LayerMask RayMask;

        private void Awake()
        {
            Anim = GetComponent<Animator>();
        }

        RaycastHit Hit;

        private Vector3 LastPelvisPosition;
        private Vector3 LeftFootIKPosition, RightFootIKPosition;

        [DisplayOnly] public float IKCheckDistance;

        public float StanceOffset = 0;
        [DisplayOnly] public float AnimatedOffset = 0;

        private void OnAnimatorIK(int layerIndex)
        {
            IKCheckDistance = Anim.GetFloat("IK_TargetDistanceModifier") * 1.5f;
            float OffsetMultiplier = Anim.GetFloat("IK_TargetDistanceModifier") * 2; // Multiply by two because at least in base animations, IK_TargetDistanceModifier only ever goes up to 0.5

            OffsetMultiplier = Mathf.Clamp01(OffsetMultiplier); // Just in-case

            Vector3 BasePosition = transform.parent.parent.position;

            float WeightPositionRight = this.WeightPositionRight * Anim.GetFloat("0_RFootIK");
            float WeightPositionLeft = this.WeightPositionLeft * Anim.GetFloat("0_LFootIK");
            float WeightRotationRight = this.WeightRotationRight * Anim.GetFloat("0_RFootIK");
            float WeightRotationLeft = this.WeightRotationLeft * Anim.GetFloat("0_LFootIK");

            if (State && ConfigManager.GetOption("AdvancedIK", 1, "Graphics") == 1 && Vector3.Distance(transform.position, Camera.main.transform.position) < 30)
            {
                Vector3 TargetPos = new Vector3(0, (StanceOffset + AnimatedOffset) * OffsetMultiplier, 0);

                transform.parent.localPosition = Vector3.MoveTowards(transform.parent.localPosition, TargetPos, Time.deltaTime * 2);

                float RFootY = 0, LFootY;

                Vector3 FootPosition = Anim.GetIKPosition(AvatarIKGoal.RightFoot);
                Vector3 FootForward = Anim.GetBoneTransform(HumanBodyBones.RightFoot).forward;

                if (UnityEngine.Physics.Raycast(new Vector3(FootPosition.x, BasePosition.y, FootPosition.z) + Vector3.up, Vector3.down, out Hit, SampleDistance + IKCheckDistance, RayMask))
                {
                    Anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, WeightPositionRight * OffsetMultiplier);
                    Anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, WeightRotationRight * OffsetMultiplier);

                    Anim.SetIKPosition(AvatarIKGoal.RightFoot, Hit.point + PositionOffset);
                    RightFootIKPosition = Hit.point + PositionOffset;

                    if (WeightRotationRight > 0)
                    {
                        Quaternion Rot = Quaternion.LookRotation(Vector3.ProjectOnPlane(FootForward, Hit.normal));
                        Anim.SetIKRotation(AvatarIKGoal.RightFoot, Rot);
                    }

                    RFootY = Hit.point.y;
                }
                else
                {
                    Anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
                    Anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
                }

                FootPosition = Anim.GetIKPosition(AvatarIKGoal.LeftFoot);
                FootForward = Anim.GetBoneTransform(HumanBodyBones.LeftFoot).forward;

                if (UnityEngine.Physics.Raycast(new Vector3(FootPosition.x, BasePosition.y, FootPosition.z) + Vector3.up, Vector3.down, out Hit, SampleDistance + IKCheckDistance, RayMask))
                {
                    Anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, WeightPositionLeft * OffsetMultiplier);
                    Anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, WeightRotationLeft * OffsetMultiplier);

                    Anim.SetIKPosition(AvatarIKGoal.LeftFoot, Hit.point + PositionOffset);
                    LeftFootIKPosition = Hit.point + PositionOffset;

                    if (WeightRotationLeft > 0)
                    {
                        Quaternion Rot = Quaternion.LookRotation(Vector3.ProjectOnPlane(FootForward + RotationOffset, Hit.normal));
                        Anim.SetIKRotation(AvatarIKGoal.LeftFoot, Rot);
                    }

                    LFootY = Hit.point.y;
                }
                else
                {
                    Anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
                    Anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);

                    RFootY = 0;
                    LFootY = 0;
                }

                float LowestFoot = RFootY < LFootY ? RFootY : LFootY;
                AnimatedOffset = transform.parent.parent.InverseTransformPoint(new Vector3(0, LowestFoot, 0)).y;
            }
        }
    }
}