using UnityEngine;

namespace SkySoft.PortraitRoom
{
    public class PortraitRoomController : MonoBehaviour
    {
        public Camera Cam;
        public Animator[] Targets;
        public int Current;
        public Vector3 CameraPosition;
        public Vector3 CameraRotation;
        public float Spacing;

        private void Update()
        {
            Current = Mathf.Clamp(Current, 0, Targets.Length - 1);

            Cam.transform.position = Targets[Current].GetBoneTransform(HumanBodyBones.Head).position + CameraPosition;
            Cam.transform.eulerAngles = CameraRotation;

            for (int I = 0; I < Targets.Length; I++)
            {
                Targets[I].transform.position = new Vector3(Spacing * I, 0, 0);
            }

            if (Application.isPlaying)
            {
                if (SkyEngine.Input.Menus.Confirm.WasPerformedThisFrame())
                {
                    ScreenCapture.CaptureScreenshot($"{Targets[Current].name}_Portrait.png");
                    Current++;
                }
            }
        }
    }
}