using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEngine;

public class SkyEngineBuilder : EditorWindow
{
    [MenuItem("SkyEngine/Builder")]
    public static void CreateBuilderWindow()
    {
        GetWindow<SkyEngineBuilder>($"{Application.productName} - Build");
    }

    Vector2 Scroll = Vector2.zero;
    BuildTarget ActivePlatform;

    private void OnGUI()
    {
        BuildTargetGroup ActiveGroup = BuildPipeline.GetBuildTargetGroup(ActivePlatform);

        GUILayout.BeginHorizontal();
        EditorPrefs.SetString("com.SkyEngine.Builder.BuildPath", EditorGUILayout.DelayedTextField("Build Path", EditorPrefs.GetString("com.SkyEngine.Builder.BuildPath")));
        if (GUILayout.Button("..."))
        {
            string Path = EditorUtility.OpenFolderPanel($"Select {Application.productName} Build Path", "", "");
            if (!string.IsNullOrEmpty(Path))
            {
                EditorPrefs.SetString("com.SkyEngine.Builder.BuildPath", Path);
            }
        }
        GUILayout.EndHorizontal();

        Scroll = GUILayout.BeginScrollView(Scroll, GUI.skin.box, GUILayout.ExpandHeight(true));
        {
            ActivePlatform = (BuildTarget)EditorGUILayout.EnumPopup("Target Platform", ActivePlatform);
        }
        GUILayout.EndScrollView();

        if (BuildPipeline.IsBuildTargetSupported(ActiveGroup, ActivePlatform))
        {
            GUI.enabled = true;
        }
        else
        {
            EditorGUILayout.HelpBox("The chosen build target is not supported!\nMake sure you have installed the proper build packages from their respective sources!", MessageType.Error);
            GUI.enabled = false;
        }
        if (GUILayout.Button("Start Build"))
        {
            BuildPlayerOptions BuildConfig = new BuildPlayerOptions
            {
                target = ActivePlatform,
                locationPathName = $"{EditorPrefs.GetString("com.SkyEngine.Builder.BuildPath")}/{ActivePlatform}/{Application.productName.Split()[0]}",
                options = BuildOptions.ShowBuiltPlayer
            };
            BuildPipeline.BuildPlayer(BuildConfig);
        }
        GUI.enabled = true;

        GUIStyle S = new GUIStyle(EditorStyles.miniLabel);
        S.alignment = TextAnchor.MiddleRight;

        GUILayout.Label($"SkyEngine Developed by SkyBreaker Softworks 2023. DO NOT DISTRIBUTE!", S);
    }
}
