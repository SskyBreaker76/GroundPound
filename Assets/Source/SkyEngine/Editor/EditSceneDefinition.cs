using UnityEngine;
using UnityEditor;
using SkySoft.Objects;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEditor.Overlays;
using UnityEngine.UIElements;


namespace SkySoft.Editor
{
    public class EditSceneDefinition : EditorWindow
    {
        [MenuItem("SkyEngine/Input Settings &i")]
        public static void OpenInputSettings()
        {
            AssetDatabase.OpenAsset(Resources.Load("SkyEngine/InputSystem/SkyEngineInput"));
        }

        [MenuItem("SkyEngine/Setup Scene &s")]
        public static void SetupScene()
        {
            if (!FindObjectOfType<SceneDefinition>())
            {
                GameObject Obj = new GameObject("SceneDefinition");
                Obj.AddComponent<SceneDefinition>();
            }

            SceneDefinition Def;

            if (Def = FindObjectOfType<SceneDefinition>())
            {
                Def.transform.SetAsFirstSibling();
                ShowWindow(Def);
            }
        }

        private static void ShowWindow(SceneDefinition Definition)
        {
            CurrentDefinition = Definition;
            Wind = FindObjectOfType<VRM_Wind>();
            GetWindow<EditSceneDefinition>(true, $"{EditorSceneManager.GetActiveScene().name} Properties", true);
            foreach (Light L in FindObjectsOfType<Light>())
            {
                if (L.type == LightType.Directional)
                {
                    Sun = L;
                    break;
                }
            }
        }

        private static SceneDefinition CurrentDefinition;
        private static VRM_Wind Wind;
        private static Light Sun;
        private Vector2 Scroll;
        
        private void OnGUI()
        {
            GUIStyle HeaderStyle = new GUIStyle(EditorStyles.helpBox);
            HeaderStyle.fontSize = 24;
            HeaderStyle.fontStyle = FontStyle.Bold;

            Scroll = EditorGUILayout.BeginScrollView(Scroll);
            if (CurrentDefinition)
            {
                EditorGUILayout.BeginVertical(HeaderStyle);
                GUILayout.Label("Level Settings", HeaderStyle);
                UnityEditor.Editor CreateEditor = UnityEditor.Editor.CreateEditor(CurrentDefinition);
                CreateEditor.OnInspectorGUI();
                CreateEditor.serializedObject.ApplyModifiedProperties();
                EditorGUILayout.EndVertical();

                if (Sun)
                {
                    EditorGUILayout.BeginVertical(HeaderStyle);
                    GUILayout.Label("Sun Settings", HeaderStyle);
                    UnityEditor.Editor CreateSunTransformEditor = UnityEditor.Editor.CreateEditor(Sun.transform);
                    UnityEditor.Editor CreateSunEditor = UnityEditor.Editor.CreateEditor(Sun);
                    CreateSunTransformEditor.OnInspectorGUI();
                    CreateSunTransformEditor.serializedObject.ApplyModifiedProperties();
                    CreateSunEditor.OnInspectorGUI();
                    CreateSunEditor.serializedObject.ApplyModifiedProperties();
                    EditorGUILayout.EndVertical();
                }

                if (Wind)
                {
                    EditorGUILayout.BeginVertical(HeaderStyle);
                    GUILayout.Label("Wind Settings", HeaderStyle);
                    UnityEditor.Editor CreateWindTransformEditor = UnityEditor.Editor.CreateEditor(Wind.transform);
                    UnityEditor.Editor CreateWindEditor = UnityEditor.Editor.CreateEditor(Wind);
                    CreateWindTransformEditor.OnInspectorGUI();
                    CreateWindTransformEditor.serializedObject.ApplyModifiedProperties();
                    CreateWindEditor.OnInspectorGUI();
                    CreateWindEditor.serializedObject.ApplyModifiedProperties();
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    WindZone Z;

                    if (Z = FindObjectOfType<WindZone>())
                    {
                        if (GUILayout.Button("Add Wind"))
                        {
                            Wind = Z.gameObject.AddComponent<VRM_Wind>();

                            Wind.Scalar = 0.2f;

                            Wind.MinimumMultiplier = -1;
                            Wind.MaximumMultiplier = 2;

                            Wind.MinimumChangeWait = 0;
                            Wind.MaximumChangeWait = 0.1f;
                        }
                    }
                }
            }
            else
            {
                Close();
            }
            EditorGUILayout.EndScrollView();
        }
    }
}