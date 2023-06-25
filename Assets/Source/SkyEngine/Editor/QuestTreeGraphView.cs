using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using System;
using System.Linq;
using SkySoft.Events.Graph;
using Node = SkySoft.Events.Graph.Node;
using SkySoft.Entities;

public class QuestTreeGraphView : GraphView
{
    /*
    public Action<NodeView> OnNodeSelected;
    public new class UxmlFactory : UxmlFactory<QuestTreeGraphView, GraphView.UxmlTraits> { }
    public EventTree Tree { get; private set; }

    public QuestTreeGraphView()
    {
        Insert(0, new GridBackground());

        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var StyleTree = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Source/SkyEngine/Events/Dialogue/Editor/QuestTreeEditor.uss");
        styleSheets.Add(StyleTree);
    }

    NodeView FindNodeView(Node Node)
    {
        if (Node != null && GetNodeByGuid(Node.GUID) != null)
            return GetNodeByGuid(Node.GUID) as NodeView;

        return null;
    }
    NodeView GetNodeView(string GUID)
    {
        NodeView N = GetNodeByGuid(GUID) as NodeView;
        if (N != null)
            return N;
        return null;
    }

    internal void Populate(EventTree Tree, bool RunTwice = true)
    {
        this.Tree = Tree;

        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChanged;

        // Creates NodeViews
        Tree.Nodes.ForEach(N => CreateNodeView(N));

        List<DefinitionNode> Definitions = new List<DefinitionNode>();
        List<ShowText> Texts = new List<ShowText>();
        List<MoveEntity> Movers = new List<MoveEntity>();

        // Creates Edges
        Tree.Nodes.ForEach(N =>
        {
            N.ParentTree = Tree;

            DefinitionNode Def;
            ShowText Text;
            MoveEntity Mover;

            if (Def = N as DefinitionNode)
            {
                Definitions.Add(Def);
            }
            if (Text = N as ShowText)
            {
                Texts.Add(Text);
            }
            if (Mover = N as MoveEntity)
            {
                Movers.Add(Mover);
            }

            ConditionalBranch CBranch = N as ConditionalBranch;

            if (CBranch)
            {
                CBranch.Options = "When False/;/When True";
            }

            var Children = Tree.GetChildren(N);

            for (int I = 0; I < Children.Count; I++)
            {
                Node Child = Children[I];

                try
                {
                    NodeView ParentView = FindNodeView(N);
                    NodeView ChildView = FindNodeView(Child);

                    if (ParentView.Output.capacity == Port.Capacity.Multi)
                    {
                        Edge Edge = ParentView.Output.ConnectTo(ChildView.Input);
                        AddElement(Edge);
                    }
                    else
                    {
                        Edge Edge = ParentView.Outputs[I].ConnectTo(ChildView.Input);
                        AddElement(Edge);
                    }

                    SubEvent AsSub = ParentView.Node as SubEvent;
                    if (AsSub)
                    {
                        for (int J = 0; J < AsSub.Target.Variables.Count; J++) 
                        {
                            for (int K = 0; K < AsSub.Connections.Count; K++)
                            {
                                if (AsSub.Connections[K].Index == J)
                                {
                                    string GUID = AsSub.Connections[K].Target.GUID;

                                    NodeView V = GetNodeView(GUID);
                                    if (V != null)
                                    {
                                        ParentView.VariableInputs[J].tabIndex = K;
                                        Edge Edge = V.Output.ConnectTo(ParentView.VariableInputs[K]);
                                        AddElement(Edge);
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
            }
        });

        foreach (DefinitionNode Definition in Definitions)
        {
            foreach (ShowText Text in Texts)
            {
                if ((!string.IsNullOrEmpty(Text.Properties.SpeakerID) && Text.Properties.SpeakerID == Definition.TargetID) || Text.InputNodeGUID == Definition.GUID)
                {
                    Text.Properties.m_Speaker = Definition.TargetName;

                    NodeView ParentView = FindNodeView(Definition);
                    NodeView ChildView = FindNodeView(Text);

                    Edge Edge = ParentView.Output.ConnectTo(ChildView.VariableInputs[0]);

                    AddElement(Edge);
                }
            }

            foreach (MoveEntity Mover in Movers)
            {
                if (Mover.DestinationGUID == Definition.GUID)
                {
                    Mover.Location = Definition.GetLocation;
                }
            }
        }

        if (RunTwice)
            Populate(Tree, false);

        QuestTreeEditor.ActiveEditor.Properties.UpdateSelection(Tree);
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(
           endPort => endPort.direction != startPort.direction 
        && endPort.node != startPort.node 
        && 
        ((startPort.portType == endPort.portType) || 
        (startPort.portType == typeof(Speaker) && endPort.portType == typeof(ImaginarySpeaker)) || 
        (endPort.portType == typeof(Location) && (startPort.portType == typeof(Speaker) || 
        startPort.portType == typeof(Location))))).ToList();
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange GraphViewChange)
    {
        if (GraphViewChange.elementsToRemove != null)
        {
            GraphViewChange.elementsToRemove.ForEach(Elem =>
            {
                NodeView NodeView = Elem as NodeView;
                if (NodeView != null)
                {
                    QuestTreeEditor.DeleteNode(NodeView.Node, Tree);
                }

                Edge Edge = Elem as Edge;
                if (Edge != null)
                {
                    NodeView ParentView = Edge.output.node as NodeView;
                    NodeView ChildView = Edge.input.node as NodeView;

                    if (ChildView.Node is SubEvent)
                    {
                        (ChildView.Node as SubEvent).RemoveConnection(Edge.input.tabIndex);
                    }

                    Tree.RemoveChild(ParentView.Node, ChildView.Node);
                }
            });

            QuestTreeEditor.ActiveEditor.MarkDirty();
        }

        if (GraphViewChange.edgesToCreate != null)
        {
            GraphViewChange.edgesToCreate.ForEach(Edge =>
            {
                NodeView ParentView = Edge.output.node as NodeView;
                NodeView ChildView = Edge.input.node as NodeView;

                if (ParentView.Node is DefinitionNode)
                {
                    Debug.Log($"SetConnection({Edge.input.tabIndex}, {ParentView.Node.GUID})");
                    SubEvent AsSub = ChildView.Node as SubEvent;
                    AsSub.SetConnection(Edge.input.tabIndex, ParentView.Node as DefinitionNode);
                }

                Tree.AddChild(ParentView.Node, ChildView.Node);
            });

            QuestTreeEditor.ActiveEditor.MarkDirty();
        }

        if (EventTree.RequestGraphRefresh)
            Populate(Tree);

        return GraphViewChange;
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent Event)
    {
        Event.menu.AppendAction("Comment", (A) => CreateNode(typeof(Comment), Event.localMousePosition));

        Event.menu.AppendSeparator();

        Event.menu.AppendAction("Define/Entity", (A) => CreateNode<DefinitionNode>(Event.localMousePosition));
        Event.menu.AppendAction("Define/Location", (A) => CreateNode<LocationDefinition>(Event.localMousePosition));

        Event.menu.AppendAction("Get/Variable", (A) => CreateNode<GetVariable>(Event.localMousePosition));

        Event.menu.AppendAction("Logic/Branch (User Choice)", (A) => CreateNode<Branch>(Event.localMousePosition));
        Event.menu.AppendAction("Logic/Branch (Switch)", (A) => CreateNode<ConditionalBranch>(Event.localMousePosition));
        Event.menu.AppendAction("Logic/Branch (Random)", (A) => CreateNode<RandomBranch>(Event.localMousePosition));
        Event.menu.AppendAction("Logic/Set Switch", (A) => CreateNode<SetSwitch>(Event.localMousePosition));
        Event.menu.AppendAction("Logic/Sub Event", (A) => CreateNode<SubEvent>(Event.localMousePosition));

        Event.menu.AppendAction("Dialogue/Show Speech", (A) => CreateNode<ShowText>(Event.localMousePosition));
        Event.menu.AppendAction("Dialogue/Move Entity", (A) => CreateNode<MoveEntity>(Event.localMousePosition));

        Event.menu.AppendSeparator();
        Event.menu.AppendAction("End", (A) => CreateNode<EndDialogue>(Event.localMousePosition));
        // OLD MENU
        /*
            Event.menu.AppendAction("Define Entity", (A) => CreateNode(typeof(DefinitionNode), Event.mousePosition));
            Event.menu.AppendAction("Define Location", (A) => CreateNode(typeof(LocationDefinition), Event.mousePosition));

            Event.menu.AppendSeparator();

            {
                var Types = TypeCache.GetTypesDerivedFrom<CompositeNode>();
                foreach (var Type in Types)
                {
                    Event.menu.AppendAction($"{Type.Name}", (A) => CreateNode(Type, Event.mousePosition));
                }
            }

            Event.menu.AppendSeparator();

            {
                var Types = TypeCache.GetTypesDerivedFrom<DecoratorNode>();
                foreach (var Type in Types)
                {
                    Event.menu.AppendAction($"{Type.Name}", (A) => CreateNode(Type, Event.mousePosition));
                }
            }

            Event.menu.AppendSeparator();

            {
                var Types = TypeCache.GetTypesDerivedFrom<ActionNode>();
                foreach (var Type in Types)
                {
                    Event.menu.AppendAction($"{Type.Name}", (A) => CreateNode(Type, Event.mousePosition));
                }
            }
    }

    void CreateNode<T>(Vector2 MousePosition) where T : Node
    {
        CreateNode(typeof(T), MousePosition);
    }

    void CreateNode(Type Type, Vector2 Position)
    {
        Node Node = QuestTreeEditor.CreateNode(Type, Tree);
        Node.ParentTree = Tree;
        Node.Position = Position;
        Populate(Tree, false);
    }

    void CreateNodeView(Node Node)
    {
        NodeView NodeView = new NodeView(Node);
        NodeView.OnNodeSelected = OnNodeSelected;
        AddElement(NodeView);
    }
    */
}