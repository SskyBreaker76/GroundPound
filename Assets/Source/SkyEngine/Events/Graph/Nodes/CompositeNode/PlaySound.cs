using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkySoft.Events.Graph
{
    public class PlaySound : CompositeNode
    {
        public override Color NodeTint => Color.Lerp(Color.red, Color.yellow, 0.5f) * 2;
        public override string DecorativeName => "Play Sound";

        public AudioClip Sound;
        public bool WaitUntilFinished;

        public override void Run(Action OnDone)
        {
            if (Sound)
            {
                if (WaitUntilFinished)
                {
                    EventGraphManager.Instance.PlaySound(Sound, () => ConnectionDict[0].Target.Run(OnDone));
                } 
                else
                {
                    EventGraphManager.Instance.PlaySound(Sound);
                    ConnectionDict[0].Target.Run(OnDone);
                }
            }
        }

        public override void DrawInspector()
        {
#if UNITY_EDITOR
            Sound = (AudioClip)EditorGUILayout.ObjectField("Sound", Sound, typeof(AudioClip), false);
            WaitUntilFinished = EditorGUILayout.Popup(new GUIContent("Wait", "Will the Event wait until this sound has finished playing?"), WaitUntilFinished ? 1 : 0, new string[] { "Off", "On" }) == 1;
#endif
        }
    }
}