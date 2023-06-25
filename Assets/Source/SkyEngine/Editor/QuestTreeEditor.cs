using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using SkySoft.Events.Graph;
using Node = SkySoft.Events.Graph.Node;

public class QuestTreeEditor : EditorWindow
{
    /*
    public QuestTreeGraphView TreeView;
    public InspectorView Inspector;
    public InspectorView Properties;

    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [OnOpenAsset]
    public static bool OpenAsset(int InstanceID, int Line)
    {
        Object Target = EditorUtility.InstanceIDToObject(InstanceID);

        if (Target is EventTree)
        {
            OpenWindow();
            Selection.activeObject = Target;

            EventTree Tree = Selection.activeObject as EventTree;

            if (Tree)
            {
                EditorPrefs.SetInt("com.SkyBreakerSoftworks.QuestTreeEditor.LastOpenedAsset", InstanceID);

                if (Tree.RootNode == null)
                {
                    Tree.RootNode = CreateNode(typeof(StartNode), Tree);
                }

                VisualElement Root = ActiveEditor.rootVisualElement;

                Label Title = Root.Q<Label>("ProjectTitle");
                if (Title != null)
                    Title.text = $"Edit Event: {Tree.name}";

                ActiveEditor.TreeView.Populate(Tree);
            }

            return true;
        }

        return false;
    }
    
    public static void StartEditor()
    {
        if (EditorPrefs.HasKey("com.SkyBreakerSoftworks.QuestTreeEditor.LastOpenedAsset"))
        {
            OpenAsset(EditorPrefs.GetInt("com.SkyBreakerSoftworks.QuestTreeEditor.LastOpenedAsset"), 0);
        }
        else
        {
            EditorUtility.DisplayDialog("Dialogue Graph", "To start, you must open a Dialogue Graph", "Close");
        }
    }

    private static void OpenWindow()
    {
        QuestTreeEditor wnd = GetWindow<QuestTreeEditor>();
        ActiveEditor = wnd;
        wnd.titleContent = new GUIContent("Event Editor");
    }

    public static QuestTreeEditor ActiveEditor;

    private void OnEnable()
    {
        ActiveEditor = this;
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement Root = rootVisualElement;

        var VisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Source/SkyEngine/Events/Dialogue/Editor/QuestTreeEditor.uxml");
        VisualTree.CloneTree(Root);

        var StyleTree = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Source/SkyEngine/Events/Dialogue/Editor/QuestTreeEditor.uss");
        Root.styleSheets.Add(StyleTree);

        TreeView = Root.Q<QuestTreeGraphView>();
        Inspector = Root.Q<InspectorView>("InspectorView");
        Properties = Root.Q<InspectorView>("PropertiesView");

        UnityEditor.UIElements.ToolbarButton SaveBtn = Root.Q<UnityEditor.UIElements.ToolbarButton>("SaveBtn");
        SaveBtn.clicked += () =>
        {
            SaveChanges();
        };

        UnityEditor.UIElements.ToolbarButton RefreshBtn = Root.Q<UnityEditor.UIElements.ToolbarButton>("RefreshBtn");
        if (RefreshBtn != null)
        {
            RefreshBtn.clicked += () =>
            {
                TreeView.Populate(TreeView.Tree);
            };
        }
        UnityEditor.UIElements.ToolbarButton TestBtn = Root.Q<UnityEditor.UIElements.ToolbarButton>("TestBtn");
        if (TestBtn != null)
        {
            TestBtn.clicked += () =>
            {
                SaveChanges();
                DialogueTester.CreateWindow(ActiveEditor.TreeView.Tree.GetInstanceID());
            };
        }

        TreeView.OnNodeSelected = OnNodeSelectionChanged;
        ActiveEditor = this;
    }

    public void MarkDirty()
    {
        hasUnsavedChanges = true;
    }

    public static Node CreateNode(System.Type Type, EventTree Base)
    {
        Node Node = Base.CreateNode(Type);
        AssetDatabase.AddObjectToAsset(Node, Base);
        ActiveEditor.MarkDirty();

        return Node;
    }

    public static void DeleteNode(Node Node, EventTree Base)
    {
        Base.DeleteNode(Node);
        AssetDatabase.RemoveObjectFromAsset(Node);
        ActiveEditor.MarkDirty();
    }

    public override void SaveChanges()
    {
        if (hasUnsavedChanges)
        {
            EditorUtility.DisplayProgressBar("Saving Event...", $"Saving Node: {TreeView.Tree.RootNode.DecorativeName}...", 1);
            EditorUtility.SetDirty(TreeView.Tree.RootNode);
            AssetDatabase.SaveAssetIfDirty(TreeView.Tree.RootNode);

            foreach (Node Child in TreeView.Tree.Nodes)
            {
                EditorUtility.DisplayProgressBar("Saving Event...", $"Saving Node: {Child.DecorativeName}...", 1);
                EditorUtility.SetDirty(Child);
                AssetDatabase.SaveAssetIfDirty(Child);
            }

            EditorUtility.DisplayProgressBar("Saving Event...", "Finishing Up...", 1);
            EditorUtility.SetDirty(TreeView.Tree);
            AssetDatabase.SaveAssetIfDirty(TreeView.Tree);

            TreeView.Populate(TreeView.Tree);
            base.SaveChanges();
            hasUnsavedChanges = false;
            EditorUtility.ClearProgressBar();
        }
    }

    private void OnNodeSelectionChanged(NodeView Node)
    {
        Inspector.UpdateSelection(Node);

        Properties.UpdateSelection(TreeView.Tree);
    }
    */
}