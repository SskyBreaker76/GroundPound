using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SkySoft.LevelManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkySoft.Events.Graph
{
    public class LoadLevel : ActionNode
    {
        public override Color NodeTint => Color.red;
        public override string DecorativeName => "Load Level";
        public int Level;
        public FadeColour FadeColour;

        public override void Run(Action OnDone)
        {
            EventGraphManager.IsProcessingEvent = false;
            OnDone();
            if (FadeColour == FadeColour.Custom)
            {
                FadeColour = FadeColour.Black;
            }
            LevelManager.LoadLevel(Level, FadeColour, () => { });
        }

        public override void DrawInspector()
        {
#if UNITY_EDITOR
            LevelDefinition[] AllLevels = SkyEngine.Levels.GetLevels;
            List<string> LevelNames = new List<string>();
            foreach (LevelDefinition Level in AllLevels)
            {
                LevelNames.Add($"{Level.ShortKey} ({Level.DisplayName})");
            }
            Level = EditorGUILayout.Popup(Level, LevelNames.ToArray());
            FadeColour = (FadeColour)EditorGUILayout.EnumPopup("Fade Colour", FadeColour);
#endif
        }
    }
}