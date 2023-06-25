using System;
using UnityEngine;

namespace SkySoft.UI.Tweens
{
    [AddComponentMenu("SkyEngine/UI/Tweens/Scale")]
    public class ScaleTween : Tweener
    {
        [Header("Scale Settings")]
        public Vector3 TargetScale = Vector3.zero;

        protected override void OnRunTween(GameObject Target, Action OnTweenFinished)
        {
            LeanTween.scale(Target, TargetScale, Duration).setEase(Curve).setOnComplete(OnTweenFinished);
        }
    }
}