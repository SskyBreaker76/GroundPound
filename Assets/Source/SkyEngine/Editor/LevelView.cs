using System.Collections.Generic;
using SkySoft.Events.Graph;
using SkySoft.Inventory;
using SkySoft.LevelManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SkyEngineDB
{
    public static EditorBuildSettingsScene[] GetAllScenes()
    {
        return EditorBuildSettings.scenes;
    }

    public static LevelView.ItemCache[] GetAllItems()
    {
        List<LevelView.ItemCache> AllItms = new List<LevelView.ItemCache>();

        foreach (Item Itm in Resources.LoadAll<Item>("SkyEngine/Items/"))
        {
            AllItms.Add(new LevelView.ItemCache(Itm));
        }

        return AllItms.ToArray();
    }

    public static LevelView.EventCache[] GetAllEvents()
    {
        List<LevelView.EventCache> AllEvs = new List<LevelView.EventCache>();

        foreach (EventTree Event in Resources.LoadAll<EventTree>("SkyEngine/Events/"))
        {
            AllEvs.Add(new LevelView.EventCache(Event));
        }

        return AllEvs.ToArray();
    }
}

public class LevelView : EditorWindow
{
    public class ItemCache
    {
        public string Path;
        public string Name;
        public string Description;

        public ItemCache(Item Base)
        {
            Path = AssetDatabase.GetAssetPath(Base);
            Name = Base.Name;
            Description = Base.Description;
        }
    }

    public class EventCache
    {
        public string Path;
        public string Name;
        public string Description;
        public int NodeCount;

        public EventCache(EventTree Event)
        {
            Path = AssetDatabase.GetAssetPath(Event);
            Name = string.IsNullOrEmpty((Event.RootNode as StartNode).Name) ? Event.name : (Event.RootNode as StartNode).Name;
            Description = (Event.RootNode as StartNode).Description;
            NodeCount = Event.Nodes.Count;
        }
    }

    private int Tab = 0;

    [MenuItem("SkyEngine/Level View &o")]
    public static void OpenView()
    {
        LevelView V = GetWindow<LevelView>($"{Application.productName} Database", true);
    }

    Vector2 Scroll = Vector2.zero;
    string Filter = "";
    private string[] Tabs = new string[] { "Levels", "Items", "Events" };

    EditorBuildSettingsScene[] Scenes = { };
    ItemCache[] AllItems = { };
    EventCache[] AllEvents = { };

    private void OnFocus()
    {
        Scenes = SkyEngineDB.GetAllScenes();
        AllItems = SkyEngineDB.GetAllItems();
        AllEvents = SkyEngineDB.GetAllEvents();
    }

    GUIContent[] AllTabs => new GUIContent[]
    {
        new GUIContent("Levels", EditorGUIUtility.IconContent("SceneAsset Icon").image),
        new GUIContent("Items", EditorGUIUtility.IconContent("FilterByType").image),
        new GUIContent("Events", EditorGUIUtility.IconContent("console.infoicon").image)
    };

    private void OnGUI()
    {
        int Results = 0;
        int TotalAssets = 0;

        titleContent = new GUIContent($"{Application.productName} Database - {AllTabs[Tab].text}", AllTabs[Tab].image, AllTabs[Tab].tooltip);

        LevelManagement Lvl = Resources.Load<LevelManagement>("Levels");

        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
        {
            OnFocus();
        }
        Filter = EditorGUILayout.DelayedTextField(Filter, EditorStyles.toolbarSearchField);
        GUI.enabled = Filter.Length > 0;
        if (GUILayout.Button("Cancel", EditorStyles.toolbarButton, GUILayout.Width(64)))
        {
            Filter = "";
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();

        Tab = GUILayout.SelectionGrid(Tab, AllTabs, Tabs.Length, EditorStyles.toolbarButton);

        Scroll = GUILayout.BeginScrollView(Scroll, GUILayout.ExpandHeight(true));
        {
            switch (Tab)
            {
                case 0:
                    TotalAssets = Scenes.Length;

                    for (int I = 0; I < Scenes.Length; I++)
                    {
                        if (Lvl.GetSceneAt(I).ToLower().Contains(Filter.ToLower()))
                        {
                            Results++;
                            GUILayout.BeginHorizontal(EditorStyles.toolbar);
                            GUILayout.Label(Lvl.GetShortKey(I), GUILayout.Width(position.width / 3));
                            GUILayout.Label("|");
                            GUILayout.Label(Lvl.GetDisplayName(I), GUILayout.Width(position.width / 3));
                            if (GUILayout.Button("Load Level", EditorStyles.miniButton))
                            {
                                if (!EditorApplication.isPlaying)
                                {
                                    List<Scene> OpenScenes = new List<Scene>();

                                    for (int J = 0; J < EditorSceneManager.sceneCount; J++)
                                    {
                                        OpenScenes.Add(EditorSceneManager.GetSceneAt(J));
                                    }

                                    EditorSceneManager.SaveModifiedScenesIfUserWantsTo(OpenScenes.ToArray());
                                    EditorSceneManager.OpenScene(Scenes[I].path, OpenSceneMode.Single);
                                }
                                else
                                {
                                    LevelManager.LoadLevel(I, FadeColour.White, () => { });
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                    break;
                case 1:
                    TotalAssets = AllItems.Length;

                    for (int I = 0; I < AllItems.Length; I++)
                    {
                        ItemCache Itm = AllItems[I];
                        if (Itm.Name.ToLower().Contains(Filter.ToLower()))
                        {
                            Results++;
                            GUILayout.BeginHorizontal(EditorStyles.toolbar);
                            GUILayout.Label($"{Itm.Name}", GUILayout.Width((position.width / 3) * 2));
                            if (GUILayout.Button("Edit Item", EditorStyles.miniButton))
                            {
                                Selection.activeObject = AssetDatabase.LoadAssetAtPath(Itm.Path, typeof(Item));
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                    break;
                case 2:
                    TotalAssets = AllEvents.Length;

                    for (int I = 0; I < AllEvents.Length; I++)
                    {
                        EventCache Event = AllEvents[I];
                        if (Event.Name.ToLower().Contains(Filter.ToLower()))
                        {
                            Results++;
                            GUILayout.BeginHorizontal(EditorStyles.toolbar);
                            GUILayout.Label(new GUIContent(Event.Name, Event.Description), GUILayout.Width(position.width / 3));
                            GUILayout.Label(new GUIContent("|", Event.Description));
                            GUILayout.Label(new GUIContent($"Complexity: {Event.NodeCount}", Event.Description), GUILayout.Width(position.width / 3));
                            if (GUILayout.Button(new GUIContent("Open Event", $"{Event.Name}\n{Event.Description}")))
                            {
                                EventTreeEditor.OpenAsset(Event.Path);
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                    break;
            }

            if (Results == 0)
            {
                EditorGUILayout.HelpBox($"No results were found for the search term \"{Filter}\"\nMake sure you've spelled everything correctly and try again", MessageType.Error);
            }
        }
        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal(GUI.skin.box);
        GUILayout.Label($"Showing {Results} assets of {TotalAssets} total.", EditorStyles.miniLabel);

        GUIStyle S = new GUIStyle(EditorStyles.miniLabel);
        S.alignment = TextAnchor.MiddleRight;

        GUILayout.Label($"SkyEngine Developed by SkyBreaker Softworks 2023. DO NOT DISTRIBUTE!", S);
        GUILayout.EndHorizontal();
    }
}
