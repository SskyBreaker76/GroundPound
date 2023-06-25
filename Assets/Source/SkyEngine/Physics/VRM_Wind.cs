using SkySoft;
using SkySoft.Physics;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using VRM;

[RequireComponent(typeof(WindZone))]
public class VRM_Wind : MonoBehaviour
{
    public float Scalar;
    [Space]
    public float MinimumMultiplier = 0.3f;
    public float MaximumMultiplier = 1.6f;
    private float Multiplier;
    private float Rand;
    private float LastRandGen;
    private float NextRand;
    [Space]
    public float MinimumChangeWait = 0.1f;
    public float MaximumChangeWait = 0.8f;
    [Space]
    [Combo(True: "VRM_Wind.Gravity", False: "Physics.gravity", Label = "Gravity Source")] public bool UseCustomGravity;
    public Vector3 Gravity = Physics.gravity;
    public float StiffnessModifier = 2;
    public float SpeedModifier = 0.5f;

    /// <summary>
    /// Base wind value taken before any WindAreas are accounted for
    /// </summary>
    public static Vector3 CurrentWind { get; private set; }

    private void Update()
    {
        if (Time.time - LastRandGen > NextRand)
        {
            Rand = Random.value;
            LastRandGen = Time.time;
            NextRand = Random.Range(MinimumChangeWait, MaximumChangeWait);
        }

        Multiplier = Mathf.MoveTowards(Multiplier, Rand, Time.deltaTime);

        Vector3 WindForce = (transform.forward * Scalar) * Mathf.Lerp(MinimumMultiplier, MaximumMultiplier, Multiplier);
        CurrentWind = WindForce;

        if (WindArea.ActiveWindZones.Count > 0)
        {
            bool WasOverride = false;
            WindArea WinningArea = null;

            foreach (WindArea Area in WindArea.ActiveWindZones)
            {
                if (Area.OverrideMode)
                {
                    WasOverride = true;

                    if (WinningArea == null || WinningArea.Priority < Area.Priority) { }
                    {
                        WinningArea = Area;
                    }
                }
                else
                {
                    if (!WasOverride)
                    {
                        WindForce += (Area.GetForward * Area.Scalar) * Mathf.Lerp(Area.MinimumForce, Area.MaximumForce, Multiplier);
                    }
                }
            }

            if (WasOverride)
            {
                WindForce = (WinningArea.transform.forward * WinningArea.Scalar) * Mathf.Lerp(WinningArea.MinimumForce, WinningArea.MaximumForce, Multiplier);
            }
        }

        VRMSpringBone.UseCustomGravity = UseCustomGravity;
        VRMSpringBone.CustomGravity = Gravity;
        VRMSpringBone.GlobalStiffness = StiffnessModifier;
        VRMSpringBone.DeltaTimeMod = SpeedModifier;
        VRMSpringBone.WindForce = WindForce;
    }
}
