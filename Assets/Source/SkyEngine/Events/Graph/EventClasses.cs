using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkySoft.Entities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkySoft.Events.Graph
{
    public enum PhaseType
    {
        TalkToNPC,
        FetchItem,
        KillEnemies,
        WaitHours,
        SpecificTime,
        GotoLocation,
        EndQuest
    }

    public enum PhaseCompletion
    {
        Incomplete,
        Passed,
        Failed
    }

    [System.Serializable]
    public class NodeProperties
    {
        public virtual void DrawInspector()
        {
        }
    }

    [System.Serializable]
    public class ShowTextProperties : NodeProperties
    {
        public bool IsControlledBySoftTarget = false;
        public string SpeakerID = "";
        public Entity SpeakerObject
        {
            get
            {
                foreach (Entity E in Object.FindObjectsOfType<Entity>())
                {
                    if (E.InstanceID == SpeakerID)
                        return E;
                }

                return null;
            }
        }
        public string m_Speaker = "";
        public string Speaker => SpeakerObject != null ? SpeakerObject.Properties.Name : m_Speaker;
        [TextArea] public string Dialogue = "";
        public DialoguePanel.PanelLocation Position = DialoguePanel.PanelLocation.Bottom;
        public bool SpokenAudio = false;
        public AudioClip Audio = null;
        // TODO[Sky] Write LipSync system
        // public LipMotion Motion;

        private bool ShowMarkupHelp;

        public override void DrawInspector()
        {
#if UNITY_EDITOR
            if (GUILayout.Button(ShowMarkupHelp ? "Hide Formatting" : "Show Formatting"))
                ShowMarkupHelp = !ShowMarkupHelp;

            if (ShowMarkupHelp)
            {
                EditorGUILayout.HelpBox(
                    $"\\S => Speaker Name\n\\N => Player Name"
                    , MessageType.Info);
            }

            GUI.enabled = string.IsNullOrEmpty(SpeakerID) && !IsControlledBySoftTarget;
            m_Speaker = EditorGUILayout.TextField("Speaker Name", m_Speaker);
            GUI.enabled = true;
            Position = (DialoguePanel.PanelLocation)EditorGUILayout.EnumPopup("Position", Position);
            GUILayout.Label("Dialogue");
            Dialogue = GUILayout.TextArea(Dialogue);
            SpokenAudio = EditorGUILayout.Toggle("Has Spoken Dialogue", SpokenAudio);
            GUI.enabled = SpokenAudio;
            Audio = (AudioClip)EditorGUILayout.ObjectField("AudioClip", Audio, typeof(AudioClip), false);
            // Motion = (LipMotion)EditorGUILayout.ObjectField("Facial Animation", Motion, typeof(LipMotion), false);
            GUI.enabled = true;
#endif      
        }
    }

    #region DEPRICATED
    [System.Serializable]
    public class ShowTextInformation
    {
        public string ParentQuest;

        public string ID;
        public string Title;
        public PhaseCompletion State = PhaseCompletion.Incomplete;

        public bool UpdatesQuestDescription;
        [TextArea] public string Description;

        public PhaseType PhaseType;

        public string TargetID;
        public int Hour;

        public Vector3 Location;
        public float LocationRadius;

        public List<string> PossibleOutcomes = new List<string>();

        public bool CompletePhase(int NextPhase)
        {
            //string N = QuestManager.GetQuest(ParentQuest).GetPhase(PossibleOutcomes[NextPhase]).ID;

            //Quest Q = QuestManager.GetQuest(ParentQuest);

            //if (Q != null)
            //{
            //    Q.ShownPhases.Add(Q.GetPhase(N));

            //    Q.ActivePhase = N;

            //    return true;
            //}

            //return false;
            return true;
        }

        public void ShowQuestUpdate()
        {

        }
    }

    [System.Serializable]
    public class Quest
    {
        public string StartPhase = "";

        public string ID;
        public string Title;
        public string Description;

        public List<ShowTextInformation> ShownPhases = new List<ShowTextInformation>();

        public string ActivePhase;

        public ShowTextInformation[] Phases;

        public ShowTextInformation GetPhase(string ID)
        {
            foreach (ShowTextInformation Phase in Phases)
            {
                if (Phase.ID == ID)
                {
                    return Phase;
                }
            }

            return null;
        }

        public bool HasPhase(string ID)
        {
            foreach (ShowTextInformation Phase in Phases)
            {
                if (Phase.ID == ID)
                    return true;
            }

            return false;
        }

        public void Initialize()
        {
            foreach (ShowTextInformation Phase in Phases)
            {
                Phase.ParentQuest = ID;
            }
        }
    }
    #endregion
}
