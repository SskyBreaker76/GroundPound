using System;
using UnityEngine;

[Obsolete("This is just a lag machine!")]
public class Grass : MonoBehaviour
{
    [SerializeField] private WindZone Wind;
    public Material GrassMaterial;
    [Space]
    public float MinimumScalar = 1.0f;
    public float MaximumScalar = 10f;

    private float CurrentScalar = 1;
    private float TargetScalar = 1;
    [Space]
    public float MinimumScalarWait = 0.6f;
    public float MaximumScalarWait = 1.4f;
    private float ScalarWait = 0;
    private float LastScalarUpdate = 0;

    private Vector3 Velocity;
    private AnimationCurve Curve;
    private float AnimationTicker;

    private void Update()
    {
        AnimationTicker += Time.deltaTime;

        if (Time.time - LastScalarUpdate > ScalarWait)
        {
            TargetScalar = UnityEngine.Random.Range(MinimumScalar, MaximumScalar);
            ScalarWait = UnityEngine.Random.Range(MinimumScalarWait, MaximumScalarWait);
            Curve = new AnimationCurve(new Keyframe(0, CurrentScalar), new Keyframe(ScalarWait, TargetScalar));
            AnimationTicker = 0;
            LastScalarUpdate = Time.time;
        }

        CurrentScalar = Curve.Evaluate(AnimationTicker);

        Velocity = (Wind.transform.forward * Wind.windMain) * CurrentScalar;

        GrassMaterial.SetVector("_WindVelocity", Velocity);
        GrassMaterial.SetFloat("_WindFrequency", Wind.windPulseFrequency);
    }
}