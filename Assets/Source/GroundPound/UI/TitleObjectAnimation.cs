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

public class TitleObjectAnimation : MonoBehaviour
{
    public AnimationCurve Animation = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.1f, 0.975f), new Keyframe(0.3f, 1), new Keyframe(0.4f, 1));

    private void Update()
    {
        if (BGM.ActiveAudioSource)
        {
            transform.localScale = Vector3.one * Animation.Evaluate(BGM.ActiveAudioSource.time);
        }
    }
}
