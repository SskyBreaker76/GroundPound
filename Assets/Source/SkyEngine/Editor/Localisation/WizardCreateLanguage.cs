using SkySoft;
using UnityEditor;
using UnityEngine;

public class WizardCreateLanguage : ScriptableWizard
{
    public string ID;
    public string DisplayName;
    public string GameTitle;

    [MenuItem("SkyEngine/Create Language")]
    public static void CreateWizard()
    {
        WizardCreateLanguage Wiz = DisplayWizard<WizardCreateLanguage>("Create Language", "Create");
        Wiz.GameTitle = Application.productName;
    }

    private void OnWizardCreate()
    {
        LocalisationFile File = ScriptableObject.CreateInstance<LocalisationFile>();
        LocalisationFile English = Resources.Load<LocalisationFile>("SkyEngine/Localisations/en-uk");

        File.Graphs = new System.Collections.Generic.List<LocalisedGraph>();
        File.Strings = new System.Collections.Generic.List<LocalisedString>();
        File.Categories = new System.Collections.Generic.List<StringCategory>();

        foreach (LocalisedGraph Graph in English.Graphs)
        {
            LocalisedGraph G = new LocalisedGraph { GraphID = Graph.GraphID, Nodes = new LocalisedNode[Graph.Nodes.Length] };
            for (int I = 0; I < Graph.Nodes.Length; I++)
            {
                G.Nodes[I] = new LocalisedNode { GUID = Graph.Nodes[I].GUID, Text = Graph.Nodes[I].Text };
            }
            File.Graphs.Add(G);
        }

        if (English.Strings != null && English.Strings.Count > 0)
        {
            foreach (LocalisedString String in English.Strings)
            {
                File.Strings.Add(new LocalisedString { Key = String.Key, Text = String.Text });
            }
        }

        foreach (StringCategory Category in English.Categories)
        {
            StringCategory Cat = new StringCategory { Name = Category.Name, Strings = new System.Collections.Generic.List<LocalisedString>() };
            foreach (LocalisedString String in Category.Strings)
            {
                Cat.Strings.Add(new LocalisedString { Key = String.Key, Text = String.Text });
            }
        }

        File.GameName = GameTitle;
        File.Language = ID;
        File.DisplayName = DisplayName;

        AssetDatabase.CreateAsset(File, $"Assets/Resources/SkyEngine/Localisations/{ID}.asset");
        AssetDatabase.Refresh();
    }
}
