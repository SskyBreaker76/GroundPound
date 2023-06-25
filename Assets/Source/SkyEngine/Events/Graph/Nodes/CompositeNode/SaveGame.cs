using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkySoft.UI;
using SkySoft.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkySoft.Events.Graph
{
    public class SaveGame : CompositeNode
    {
        public override Color NodeTint => Color.Lerp(Color.green, Color.cyan, 0.5f);
        public override string DecorativeName => OpensSaveMenu ? $"Open {(LoadMode ? "Load" : "Save")} Dialogue" : "Save Game";
        public override string Description => OpensSaveMenu ? "" : $"To Slot {DesiredSlot + 1} at {(PS2Speed ? "Slow Speed" : "High Speed")}";

        public int DesiredSlot;
        public bool PS2Speed;
        public bool OpensSaveMenu = true;
        public bool LoadMode;

        public override void Run(Action OnDone)
        {
            if (!OpensSaveMenu)
            {
                SkyEngine.SaveGame(() => { EventGraphManager.Instance.Dialogue.CloseMenu(); Connections[0].Target.Run(OnDone); }, DesiredSlot, PS2Speed, Progress =>
                {
                    EventGraphManager.Instance.Dialogue.CloseMenu();

                    EventGraphManager.Instance.Dialogue.ShowText("", $"Saving Progress {(Progress * 100).ToString("0.0")}%...\nPlease do not turn off system.", DialoguePanel.PanelLocation.Centre, 0, new DialoguePanel.DialogueChoice[] { }, () => { }, 0, 0, true);
                });
            }
            else
            {
                SaveMenu.SummonMenu(SkyEngine.Properties.SaveMenu, () => Connections[0].Target.Run(OnDone), LoadMode);
            }
        }

        public override void DrawInspector()
        {
#if UNITY_EDITOR
            if (OpensSaveMenu = EditorGUILayout.Toggle("Opens Save Menu", OpensSaveMenu))
            {
                LoadMode = EditorGUILayout.Popup("Menu Type", LoadMode ? 1 : 0, new string[] { "Save Menu", "Load Menu" }) == 1;
            }
            else
            {
                List<string> SlotOptions = new List<string>();

                for (int I = 0; I < FileManager.MaxSaves; I++)
                {
                    SlotOptions.Add($"Slot {I + 1}");
                }

                DesiredSlot = EditorGUILayout.Popup("Slot", DesiredSlot, SlotOptions.ToArray());
            }
            PS2Speed = EditorGUILayout.Popup("Drive Speed", PS2Speed ? 1 : 0, new string[] { "User Drive", "Memory Card (250 KB per second)" }) == 1;
#endif
        }
    }
}