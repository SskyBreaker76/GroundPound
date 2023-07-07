using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using SkySoft;
using System.Linq;

public class LocalisationEditor : EditorWindow
{
    private LocalisationFile[] Languages = { };
    private int SelectedLanguage;

    [MenuItem("SkyEngine/Languages")]
    public static void OpenEditor()
    {
        GetWindow<LocalisationEditor>("Localisation Manager");
    }

    public List<LocalisationFile> DirtyFiles = new List<LocalisationFile>();
    
    private void OnFocus()
    {
        titleContent = new GUIContent("Localisation Manager", EditorGUIUtility.IconContent("ToolHandleGlobal").image);
        Languages = Resources.LoadAll<LocalisationFile>("SkyEngine/Localisations/");
    }

    Vector2 LanguageScroll = new Vector2();
    Vector2 StringsScroll = new Vector2();
    private string Filter = "";

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
        {
            if (GUILayout.Button(new GUIContent("Save Languages", EditorGUIUtility.IconContent("SaveAs").image), EditorStyles.toolbarButton))
            {
                SaveChanges();
            }
            if (GUILayout.Button(new GUIContent("Sync Languages", EditorGUIUtility.IconContent("Refresh").image), EditorStyles.toolbarButton))
            {
                if (EditorUtility.DisplayDialog("Sync Languages", "Are you sure you want to sync languages? This action can't be undone!", "Yes", "Nevermind, I don't want to!"))
                {
                    LocalisationFile English = Languages[0];

                    foreach (LocalisationFile Language in Languages)
                    {
                        if (Language != English)
                        {
                            if (Language.Strings.Count > 0)
                            {
                                if (Language.Categories == null || Language.Categories.Count <= 0)
                                {
                                    if (EditorUtility.DisplayDialog("Sync Languages", $"The language {Language.DisplayName} ({Language.name}) still uses the old format.\nWould you like to upgrade it now?\n\n(Selecting no will simply skip this language)", "Yes", "No"))
                                    {
                                        Language.UpdateClass();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                            }

                            foreach (StringCategory SCat in English.Categories)
                            {
                                if (!Language.ContainsCategory(SCat.Name))
                                {
                                    List<LocalisedString> Strs = new List<LocalisedString>();

                                    foreach (LocalisedString String in English.Strings)
                                    {
                                        Strs.Add(new LocalisedString { Key = String.Key, Text = String.Text });
                                    }

                                    Language.Categories.Add(new StringCategory
                                    {
                                        Name = SCat.Name,
                                        Strings = Strs
                                    });
                                }
                                else
                                {
                                    foreach (StringCategory SCat2 in Language.Categories)
                                    {
                                        if (SCat.Name == SCat2.Name)
                                        {
                                            foreach (LocalisedString String in SCat.Strings)
                                            {
                                                if (!SCat2.ContainsString(String.Key))
                                                {
                                                    SCat2.Strings.Add(new LocalisedString { Key = String.Key, Text = String.Text });
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        EditorUtility.SetDirty(Language);
                        hasUnsavedChanges = true;
                        DirtyFiles.Add(Language);
                    }
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.BeginVertical(GUI.skin.window, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            {
                GUILayout.Label("Languages", EditorStyles.boldLabel);
                LanguageScroll = EditorGUILayout.BeginScrollView(LanguageScroll, GUILayout.Width(256), GUILayout.ExpandHeight(true));
                {
                    List<string> LanguageNames = new List<string>();
                    foreach (LocalisationFile File in Languages)
                    {
                        LanguageNames.Add($"{File.DisplayName}{(DirtyFiles.Contains(File) ? "*" : "")}");
                    }

                    SelectedLanguage = GUILayout.SelectionGrid(SelectedLanguage, LanguageNames.ToArray(), 1, EditorStyles.toolbarButton);
                }
                EditorGUILayout.EndScrollView();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Add New"))
                {
                    ScriptableWizard.DisplayWizard<WizardCreateLanguage>("Create Language", "Create");
                    OnFocus();
                }
                if (GUILayout.Button($"Delete {Languages[SelectedLanguage].name}"))
                {
                    if (EditorUtility.DisplayDialog("Delete Language", $"Are you sure you want to delete {Languages[SelectedLanguage].name}?\nThis action can't be undone!", "Yes", "No"))
                    {
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(Languages[SelectedLanguage]));
                        AssetDatabase.Refresh();
                        SelectedLanguage = 0;
                        OnFocus();
                    }
                }
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(GUI.skin.window);
            {
                LocalisationFile File = Languages[SelectedLanguage];

                GUILayout.Label($"{File.DisplayName} ({File.name})", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                {
                    if (string.IsNullOrEmpty(File.GameName))
                    {
                        File.GameName = Application.productName;
                    }

                    File.GameName = EditorGUILayout.TextField("Title", File.GameName);

                    bool IsLegacyFile = File.Strings.Count > 0 && (File.Categories == null || File.Categories.Count <= 0);

                    if (!IsLegacyFile)
                    {
                        Filter = EditorGUILayout.DelayedTextField(Filter, EditorStyles.toolbarSearchField, GUILayout.ExpandWidth(true));
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("This language still uses the Legacy format!\nClick the button below to automatically convert your language to the new format!", MessageType.Warning);

                        if (GUILayout.Button("Update Language"))
                        {
                            File.UpdateClass();
                        }
                    }

                    StringsScroll = EditorGUILayout.BeginScrollView(StringsScroll, GUILayout.Width(position.width - 256), GUILayout.ExpandHeight(true));
                    {
                        if (IsLegacyFile)
                        {
                            EditorGUI.BeginChangeCheck();
                            {
                                int StringCounter = 0;

                                foreach (LocalisedString String in File.Strings)
                                {
                                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                                    GUILayout.Label($"[{StringCounter}] => ");
                                    String.Key = EditorGUILayout.TextField("Key", String.Key, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));
                                    GUILayout.Label("|", GUILayout.ExpandWidth(false));
                                    String.Text = EditorGUILayout.TextField("Value", String.Text, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));
                                    if (GUILayout.Button("Remove"))
                                    {
                                        File.Strings.RemoveAt(StringCounter);
                                        break;
                                    }
                                    EditorGUILayout.EndHorizontal();

                                    StringCounter++;
                                }
                            }
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (!DirtyFiles.Contains(File))
                                {
                                    EditorUtility.SetDirty(File);
                                    DirtyFiles.Add(File);
                                    hasUnsavedChanges = true;
                                }
                            }
                        }
                        else
                        {
                            EditorGUI.BeginChangeCheck();
                            {
                                foreach (StringCategory Category in File.Categories)
                                {
                                    List<string> Matches = new List<string>();

                                    foreach (LocalisedString S in Category.Strings)
                                    {
                                        if (string.IsNullOrEmpty(Filter) || S.Key.ToLower().Contains(Filter.ToLower()))
                                        {
                                            Matches.Add(S.Key);
                                        }
                                    }

                                    if (Matches.Count > 0) 
                                    {
                                        if (Category.IsFlagOpen = EditorGUILayout.Foldout(Category.IsFlagOpen, $"{Category.Name} ({Matches.Count} {(Matches.Count == 1 ? "Entry" : "Entries")})", true, EditorStyles.foldoutHeader))
                                        {
                                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                                            {
                                                Category.Name = EditorGUILayout.TextField("Name", Category.Name);
                                                EditorGUILayout.Space();

                                                if (Category.IsListOpen = EditorGUILayout.Foldout(Category.IsListOpen, Category.Strings.Count == 1 ? "Entry" : "Entries", true))
                                                {
                                                    int StringCounter = 0;
                                                    EditorGUILayout.BeginVertical(GUI.skin.box);
                                                    foreach (LocalisedString String in Category.Strings)
                                                    {
                                                        if (Matches.Contains(String.Key)) 
                                                        { 
                                                            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                                                            GUILayout.Label($"[{StringCounter}] => ");
                                                            String.Key = EditorGUILayout.TextField("Key", String.Key, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));
                                                            GUILayout.Label("|", GUILayout.ExpandWidth(false));
                                                            String.Text = EditorGUILayout.TextField("Value", String.Text, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));
                                                            if (GUILayout.Button("Remove"))
                                                            {
                                                                Category.Strings.RemoveAt(StringCounter);
                                                                break;
                                                            }
                                                            EditorGUILayout.EndHorizontal();
                                                        }

                                                        StringCounter++;
                                                    }
                                                    EditorGUILayout.EndVertical();
                                                    if (GUILayout.Button("+", EditorStyles.toolbarButton))
                                                    {
                                                        Category.Strings.Add(new LocalisedString { Key = "new_text", Text = "New Text" });
                                                    }
                                                }
                                            }
                                            EditorGUILayout.EndVertical();
                                        }
                                    }
                                }
                            }
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (!DirtyFiles.Contains(File))
                                {
                                    EditorUtility.SetDirty(File);
                                    DirtyFiles.Add(File);
                                    hasUnsavedChanges = true;
                                }
                            }
                        }
                    }
                    EditorGUILayout.EndScrollView();
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    {
                        if (GUILayout.Button("+", EditorStyles.toolbarButton))
                        {
                            if (IsLegacyFile)
                            {
                                File.Strings.Add(new LocalisedString { Key = "hello.world", Text = "Hello World!" });
                            }
                            else
                            {
                                File.Categories.Add(new StringCategory { Name = "hello", IsFlagOpen = true, IsListOpen = true, Strings = new List<LocalisedString> { new LocalisedString { Key = "world", Text = "Hello World!" } } });
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
    }

    public override void SaveChanges()
    {
        CommonTextKeyAttributeDrawer.HasGotAllKeys = false;

        foreach (LocalisationFile File in DirtyFiles)
        {
            EditorUtility.SetDirty(File);
            AssetDatabase.SaveAssetIfDirty(File);
        }

        DirtyFiles.Clear();

        hasUnsavedChanges = false;
    }
}
