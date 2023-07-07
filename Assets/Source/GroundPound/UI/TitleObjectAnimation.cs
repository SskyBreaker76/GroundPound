/*
    Developed by Sky MacLennan
 */

using SkySoft.Audio;
using SkySoft;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using SkySoft.Generated;
using Sky.GroundPound;

public class TitleObjectAnimation : MonoBehaviour
{
    public AnimationCurve Animation = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.1f, 0.975f), new Keyframe(0.3f, 1), new Keyframe(0.4f, 1));
    public AnimationCurve Tilt = new AnimationCurve();

    private void Update()
    {
        if (BGM.ActiveAudioSource)
        {
            transform.localScale = Vector3.one * Animation.Evaluate(BGM.ActiveAudioSource.time / Game.BeatLength);
            transform.localEulerAngles = new Vector3(0, 0, Tilt.Evaluate(BGM.ActiveAudioSource.time / Game.BeatLength));
        }
    }
}
