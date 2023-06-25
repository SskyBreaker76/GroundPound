/*
    Developed by Sky MacLennan
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sky.GroundPound
{
    [AddComponentMenu("Ground Pound/Camera Controller")]
    public class CameraController : MonoBehaviour
    {
        public const float TargetAspectRatio = 16f / 9f;

        private Camera m_Camera;
        public Camera Camera
        {
            get
            {
                if (!m_Camera)
                    m_Camera = GetComponent<Camera>();

                return m_Camera;
            }
        }

        public Transform Target;
        public float SmoothSpeed = 0.125f;
        public Vector3 Offset = new Vector3(0, 0, -10);

        private Vector3 Velocity;

        private void LateUpdate()
        {
            if (!Target)
                if (Player.LocalPlayer)
                    Target = Player.LocalPlayer.transform;

            if (!Target) return;

            Vector3 DesiredPosition = Target.position + Offset;

            DesiredPosition.x = Mathf.Clamp(DesiredPosition.x, GameManager.Instance.CameraMin.x, GameManager.Instance.CameraMax.x);
            DesiredPosition.y = Mathf.Clamp(DesiredPosition.y, GameManager.Instance.CameraMin.y, GameManager.Instance.CameraMax.y);

            Vector3 SmoothedPosition = Vector3.SmoothDamp(transform.position, DesiredPosition, ref Velocity, SmoothSpeed);
            transform.position = SmoothedPosition;
        }

        private void Update()
        {
            float CurrentAspectRatio = (float)Screen.width / Screen.height;
            float ScaleHeight = CurrentAspectRatio / TargetAspectRatio;

            if (ScaleHeight < 1)
            {
                Rect R = Camera.rect;

                R.width = 1;
                R.height = ScaleHeight;
                R.x = 0;
                R.y = (1 - ScaleHeight) / 2f;

                Camera.rect = R;
            }
            else
            {
                Rect R = Camera.rect;

                R.width = TargetAspectRatio / CurrentAspectRatio;
                R.height = 1f;
                R.x = (1f - R.width) / 2f;
                R.y = 0;

                Camera.rect = R;
            }
        }
    }
}
