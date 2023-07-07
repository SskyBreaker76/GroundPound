using System.Threading.Tasks;
using System;
using UnityEngine;
using SkySoft.Generated;
using System.Linq;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using SkySoft.Steam;

namespace SkySoft.Objects 
{
    [System.Serializable]
    public class SceneDefinitionFile
    {
        public bool ForcedPerspective;
        public Transform CameraPosition;
        public string DisplayName;
        [Space]
        public bool AllowCombat = true;
        public bool EnableTimeCycle = true;
        public string Area;
        public float BaseBrightness = 1;
        public bool RespawnsNPCs = false;
        [Space]
        [SerializeField, Header("Default will use the maps DisplayName")]
        private string m_StatusMessage = "";
        [Combo(True: "Yes", False: "No", Label = "Update RichPresence")]
        public bool SetStatusMessage = true;
        public string StatusMessage => string.IsNullOrEmpty(m_StatusMessage) ? SkyEngine.Levels.GetDisplayName(SkyEngine.Levels.GetShortKey(SceneManager.GetActiveScene().name)) : m_StatusMessage;
        [SerializeField] private string m_StatusHeading = "Exploring";
        public bool UseAutoHeading = true;
        public string StatusHeading => UseAutoHeading ? (Steamhook.LobbyMemberCount > 1 ? "Travelling in Convoy" : "Exploring Solo") : m_StatusHeading;
    }

    [ExecuteInEditMode]
    [AddComponentMenu("SkyEngine/Scene Definition")]
    public class SceneDefinition : MonoBehaviour
    {
        public static bool DoneFirstLoad = false;
        public static SceneDefinitionFile ActiveDefinition;
        public SceneDefinitionFile Properties;

        private void OnValidate()
        {
            SceneDefinition[] Definitions = FindObjectsOfType<SceneDefinition>(); // We only ever want ONE Definition

            if (Definitions.Length > 1)
            {
                for (int I = 1; I < Definitions.Length; I++)
                {
                    Destroy(Definitions[I].gameObject);
                }
            }

            if (!GetComponent<ReflectionProbe>())
            {
                ReflectionProbe R = gameObject.AddComponent<ReflectionProbe>();

                R.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
                R.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.EveryFrame;
                R.timeSlicingMode = UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.NoTimeSlicing;

                R.size = new Vector3(10000, 10000, 10000);
                R.resolution = 16;
                R.cullingMask = LayerMask.GetMask();
                R.hdr = true;
            }
        }

        private void Awake()
        {
            SceneLoaded(Properties, () => 
            { 
                if (Properties.SetStatusMessage)
                {
                    SkyEngine.SetRichPresence(string.IsNullOrEmpty(Properties.StatusHeading) ? Properties.StatusMessage : Properties.StatusHeading, string.IsNullOrEmpty(Properties.StatusHeading) ? "" : Properties.StatusMessage);
                }
            });
        }

        public static async void SceneLoaded(SceneDefinitionFile Scene, Action OnComplete)
        {
            if (!Application.isPlaying)
                return;

            if (Scene != null)
            {
                Debug.Log($"SceneLoaded({Scene.Area})");
                bool Done = false;

                if (Scene.Area == "Retain" || (ActiveDefinition != null && ActiveDefinition.Area == Scene.Area))
                {
                    Debug.Log("Retain current music");

                    if (DoneFirstLoad)
                    {
                        Debug.Log("Return");

                        Done = true;
                        OnComplete();

                        return;
                    }
                }

                if (Scene.Area != "None")
                {
                    if (ActiveDefinition != null && ActiveDefinition.Area != "Title")
                    {
                        Debug.Log($"ActiveDefinition = {ActiveDefinition.Area}");
                    }

                    SkyEngine.BGM.StartBGM(Scene.Area.ToString(), () =>
                    {
                        Done = true;
                        DoneFirstLoad = true;
                    });
                }
                else
                {
                    SkyEngine.BGM.FadeoutBGM(1, () =>
                    {
                        Done = true;
                        DoneFirstLoad = true;
                    });
                }

                ActiveDefinition = Scene;

                if (!Done) await Task.Yield();

                OnComplete();

                if (Scene.RespawnsNPCs)
                {
                    PersistentData.ForgetEverything("NPC");
                }
            }
        }

        private void OnDrawGizmos()
        {
            foreach (Camera Camera in FindObjectsOfType<Camera>(true))
            { 
                if (!Camera.GetComponent<EnhancedCameraGizmos>())
                {
                    Camera.gameObject.AddComponent<EnhancedCameraGizmos>();
                }
            }

            foreach (Light Light in FindObjectsOfType<Light>(true))
            {
                if (!Light.GetComponent<EnhancedLightGizmos>())
                {
                    Light.gameObject.AddComponent<EnhancedLightGizmos>();
                }
            }
        }

        private void Update()
        {

        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/Sky Engine/Scene Definition")]
        public static void CreateSceneDefintion()
        {
            new GameObject("SceneDefinition").AddComponent<SceneDefinition>();
        }
#endif
    }
}