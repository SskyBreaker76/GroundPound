using System;
using UnityEngine;

namespace SkySoft.UI.Tweens
{
    [AddComponentMenu("SkyEngine/UI/Tweens/Position")]
    public class PositionTween : Tweener
    {
        [Header("Position Settings")]
        public Vector2 Offset;
        [Button(Label = "Snap to Offset Position", Tooltip = "Useful for if you need to test the positioning on your UI")] public bool JumpToPos;

        private Vector3 GetTargetPosition(Transform Target)
        {
            return Target.position + new Vector3(Offset.x * Screen.width, Offset.y * Screen.height);
        }

        private void OnValidate()
        {
            if (JumpToPos)
            {
#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(this, gameObject.name + ".PositionTween");
#endif
                transform.position = GetTargetPosition(transform);
                JumpToPos = false;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawLine(transform.position, GetTargetPosition(transform));
        }

        protected override void OnRunTween(GameObject Target, Action OnTweenFinished)
        {
            LeanTween.move(Target, GetTargetPosition(Target.transform), Duration).setEase(Curve).setOnComplete(OnTweenFinished);
        }
    }
}
