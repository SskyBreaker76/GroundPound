using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.UI.Tweens
{
    public enum TweenStarter
    {
        Awake,
        Start,
        OnEnable,
        External
    }

    public enum TweenerType
    {
        Single,
        Multi
    }

    [AddComponentMenu("SkyEngine/UI/Tweens/Tweener")]
    public abstract class Tweener : MonoBehaviour
    {
        public static bool IsTweening { get; private set; }

        [Header("Base Settings")]
        public TweenStarter Starter;
        [Space]
        public AnimationCurve Curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        public float Duration = 1;

        private Action OnTweenIsDone;

        public void RunTween(GameObject Target = null, Action OnTweenFinished = null)
        {
            IsTweening = true;

            if (Target == null)
                Target = gameObject;

            if (OnTweenFinished == null)
                OnTweenFinished = new Action(() => { });

            OnTweenIsDone = new Action(() =>
            {
                OnTweenFinished();
                IsTweening = false;
            });

            OnRunTween(Target, OnTweenIsDone);
        }

        protected abstract void OnRunTween(GameObject Target, Action OnTweenFinished);
    }
}
