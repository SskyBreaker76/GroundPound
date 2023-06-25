using Codice.CM.Common.Tree;
using SkySoft;
using SkySoft.Events.Graph;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Node = SkySoft.Events.Graph.Node;
using Object = UnityEngine.Object;

public class EventTreeEditor : EditorWindow
{
    public EventTreeGraphView TreeView;
    public InspectorView Inspector;
    public InspectorView Properties;
    public Toggle DebugMode;
    public Label TransformDebug;
    public Label ErrorMessage;

    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;


    private bool DoFrameAll = false;

    public static bool OpenAsset(string Path)
    {
        EventTree Asset = AssetDatabase.LoadAssetAtPath<EventTree>(Path);
        return OpenAsset(Asset.GetInstanceID(), 0);
    }

    [OnOpenAsset]
    public static bool OpenAsset(int InstanceID, int Line)
    {
        Object Target = EditorUtility.InstanceIDToObject(InstanceID);

        if (Target is EventTree)
        {   
            EventTree Tree = Target as EventTree;
            if (Tree)
            {
                EventTreeEditor Window = OpenWindow(Tree);

                Window.DoFrameAll = true;

                EditorPrefs.SetInt("com.SkyBreakerSoftworks.EventTreeEditor.LastOpenedAsset", InstanceID);

                if (Tree.RootNode == null)
                {
                    Tree.RootNode = CreateNode<StartNode>(Tree);
                }

                VisualElement Root = ActiveEditor.rootVisualElement;
                Label Title = Root.Q<Label>("ProjectTitle");
                if (Title != null)
                    Title.text = $"Edit Event: {Tree.name}";

                Window.TreeView.Populate(Tree);
            }

            return true;
        }

        return false;
    }

    private static EventTreeEditor OpenWindow(EventTree Tree = null)
    {
        EventTreeEditor Wnd = GetWindow<EventTreeEditor>();
        Wnd.titleContent = new GUIContent($"{(Tree != null ? $"{Tree.name}" : "SkyEngine Event Graph")}", EditorGUIUtility.IconContent("console.infoicon").image, $"{(Tree != null ? $"EventID: {Tree.GetInstanceID()}" : "No Event Loaded!")}");
        Wnd.ShowNotification(new GUIContent("Load Done!"));
        ActiveEditor = Wnd;
        return Wnd;
    }

    private void OnDisable()
    {
        ActiveEditor = null;
    }

    public static EventTreeEditor ActiveEditor;

    private void OnEnable()
    {
        ActiveEditor = this;
        if (EditorPrefs.HasKey("com.SkyBreakerSoftworks.EventTreeEditor.LastOpenedAsset"))
            OpenAsset(EditorPrefs.GetInt("com.SkyBreakerSoftworks.EventTreeEditor.LastOpenedAsset"), 0);
    }

    private void Awake()
    {
        OnEnable();
    }

    public Vector2 Position => new Vector2(TreeView.viewTransform.position.x, TreeView.viewTransform.position.y);

    private void OnGUI()
    {
        ErrorMessage.text = "";

        if (TreeView != null)
            if (TreeView.Tree == null)
                if (EditorPrefs.HasKey("com.SkyBreakerSoftworks.EventTreeEditor.LastOpenedAsset"))
                    OpenAsset(EditorPrefs.GetInt("com.SkyBreakerSoftworks.EventTreeEditor.LastOpenedAsset"), 0);
    
        if (TransformDebug != null)
        {
            TransformDebug.text = $"View Position: [{Position.x}, {Position.y}], Zoom: {1 / TreeView.viewTransform.scale.x}";
        }

        if (TreeView != null)
        {
            if (TreeView.DoneRefresh)
            {
                if (DoFrameAll)
                {
                    TreeView.FrameAll();
                    DoFrameAll = false;
                }
            }
        }

        foreach (NodeView Node in TreeView.nodes)
        {
            foreach (Port P in Node.Outputs)
            {
                if (P.portType == typeof(Flow))
                {
                    if (P.connected == false)
                    {
                        ErrorMessage.text = "Not all branches have an end!";
                    }
                }
            }
        }
    }

    public VisualTreeAsset VisualTree;
    public StyleSheet StyleTree;

    public void CreateGUI()
    {
        VisualElement Root = rootVisualElement;

        VisualTree.CloneTree(Root);
        Root.styleSheets.Add(StyleTree);

        TreeView = Root.Q<EventTreeGraphView>();
        Inspector = Root.Q<InspectorView>("InspectorView");
        Properties = Root.Q<InspectorView>("PropertiesView");
        DebugMode = Root.Q<Toggle>("DebugMode");
        TransformDebug = Root.Q<Label>("TransformView");
        ErrorMessage = Root.Q<Label>("ErrorMessage");

        UnityEditor.UIElements.ToolbarButton SaveButton = Root.Q<UnityEditor.UIElements.ToolbarButton>("SaveBtn");
        SaveButton.clicked += SaveChanges;
        
        UnityEditor.UIElements.ToolbarButton RefreshButton = Root.Q<UnityEditor.UIElements.ToolbarButton>("RefreshBtn");
        RefreshButton.clicked += () => TreeView.Populate(TreeView.Tree);

        UnityEditor.UIElements.ToolbarButton LocaliseBtn = Root.Q<UnityEditor.UIElements.ToolbarButton>("Localise");
        LocaliseBtn.clicked += () =>
        {
            if (TreeView.Tree)
            {
                Localisation.GenerateLocalisation(TreeView.Tree);
            }
        };

        TreeView.OnNodeSelected = OnNodeSelectionChanged;
        ActiveEditor = this;
    }

    public void MarkDirty() => hasUnsavedChanges = true;

    public static T CreateNode<T>(EventTree Base) where T : Node
    {
        T Node = Base.CreateNode<T>();
        AssetDatabase.AddObjectToAsset(Node, Base);
        ActiveEditor.MarkDirty();
        return Node;
    }

    public static Node CreateNode(Type Type, EventTree Base)
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
        if (!string.IsNullOrEmpty(ErrorMessage.text))
        {
            EditorUtility.DisplayDialog("Save Event", "Can't save Event while there are Errors!\nPlease fix your scripts errors then try again...", "Okay");
            return;
        }

        DateTime SaveStart = DateTime.Now;

        if (hasUnsavedChanges)
        {
            EditorUtility.DisplayProgressBar("Saving Event...", "Please be patient!", 1);
            List<Object> ToSave = new List<Object>
            {
                TreeView.Tree.RootNode
            };

            foreach (Node Child in TreeView.Tree.Nodes)
            {
                ToSave.Add(Child);
            }

            for (int I = 0; I < ToSave.Count; I++)
            {
                EditorUtility.SetDirty(ToSave[I]);
                AssetDatabase.SaveAssetIfDirty(ToSave[I]);
            }

            TreeView.Populate(TreeView.Tree);
            base.SaveChanges();
            hasUnsavedChanges = false;
            EditorUtility.ClearProgressBar();
        }

        DateTime SaveEnd = DateTime.Now;

        ShowNotification(new GUIContent($"Save finished in {(SaveEnd - SaveStart).TotalSeconds} seconds!"));
    }

    private void OnNodeSelectionChanged(NodeView Node)
    {
        Node.Node.OnStartInspector();
        Inspector.UpdateSelection(Node);
        Properties.UpdateSelection(TreeView.Tree);
    }
}
