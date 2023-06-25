using SkySoft;
using SkySoft.Objects;
using UnityEngine;

[System.Serializable]
public class CameraPreset
{
    public HumanBodyBones TargetBone;
    public Vector3 Location;
    public float FOV = 70;
}

[RequireComponent(typeof(Camera))]
public class CharEditCamera : MonoBehaviour
{
    private Camera Cam => GetComponent<Camera>();
    public AnimationCurve Interpolation = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    public float AnimationLength = 0.75f;
    [Space]
    public Charactermanager Character;
    public CameraPreset DefaultView;
    public CameraPreset CloseupView;
    public CameraPreset TargetView => TargetFace ? CloseupView : DefaultView;
    public CameraPreset OtherView => TargetFace ? DefaultView : CloseupView;

    [SerializeField, Combo(True: "Face", False: "Body", Label = "Target")] private bool m_TargetFace;
    public bool TargetFace { get => m_TargetFace; set { if (m_TargetFace != value) { AnimationProgress = 0; } m_TargetFace = value; } }

    private float AnimationProgress;

    private void OnValidate()
    {
        try
        {
            Update();
        }
        catch { }
    }

    public void Update()
    {
        Animator Anim;

        if (Anim = Character.TargetAnimator)
        {
            AnimationProgress += Time.unscaledDeltaTime / AnimationLength;

            Vector3 PositionA = Anim.GetBoneTransform(TargetView.TargetBone).position + TargetView.Location;
            Vector3 PositionB = Anim.GetBoneTransform(OtherView.TargetBone).position + OtherView.Location;

            transform.position = Vector3.Lerp(PositionB, PositionA, Interpolation.Evaluate(AnimationProgress));
            Cam.fieldOfView = Mathf.Lerp(OtherView.FOV, TargetView.FOV, Interpolation.Evaluate(AnimationProgress));
        }
    }
}
