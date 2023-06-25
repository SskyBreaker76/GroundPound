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

public class EventTreeGraphView : GraphView
{
    public bool DoneRefresh = false;
    public Action<NodeView> OnNodeSelected;
    public new class UxmlFactory : UxmlFactory<EventTreeGraphView, GraphView.UxmlTraits> { }
    public EventTree Tree { get; private set; }

    public StyleSheet StyleTree;

    public ContentZoomer Zoomer;
    ContentDragger Dragger;

    public float Zoom => 1 / transform.scale.x;

    public EventTreeGraphView()
    {
        Insert(0, new GridBackground());

        // TODO[Sky] Re-add zooming and make it compatible with node placement
        this.AddManipulator(Zoomer = new ContentZoomer()); 
        this.AddManipulator(Dragger = new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        if (!StyleTree)
            StyleTree = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Source/SkyEngine/Events/Graph/Editor/Styles/EventTreeEditor.uss");

        if (StyleTree)
            styleSheets.Add(StyleTree);
    }

    NodeView FindNodeView(Node Node)
    {
        if (Node != null)
        {
            NodeView N = GetNodeByGuid(Node.GUID) as NodeView;
            if (N != null)
                return N;
        }
        return null;
    }

    NodeView FindNodeView(string GUID)
    {
        NodeView N = GetNodeByGuid(GUID) as NodeView;
        if (N != null)
            return N;
        return null;
    }

    internal void Populate(EventTree Tree)
    {
        DoneRefresh = false;
        this.Tree = Tree;

        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChanged;

        foreach (Node Node in Tree.Nodes)
        {
            Node.Setup();
        }

        Tree.UpdateFlow();

        // This loop is used to make the NodeViews
        foreach (Node Node in Tree.Nodes)
        {
            CreateNodeView(Node);
        }

        // This looks really dumb, but we need two loops through the same list because on the first loop not
        // all NodeViews exist yet
        foreach (Node Node in Tree.Nodes)
        {
            Node.ParentTree = Tree;

            for (int I = 0; I < Node.Connections.Count; I++)
            {
                ConnectionInfo Connection = Node.Connections[I];

                if (Connection.Target != null)
                {
                    NodeView ParentNode = FindNodeView(Node);
                    NodeView ChildNode = FindNodeView(Connection.Target);

                    if (ParentNode.Node && ChildNode.Node)
                    {
                        Edge E = ParentNode.Outputs[Connection.OutputIndex].ConnectTo(ChildNode.Inputs[Connection.InputIndex]);
                        AddElement(E);
                    }
                }
            }
        }

        EventTreeEditor.ActiveEditor.Properties.UpdateSelection(Tree);
        DoneRefresh = true;
    }

    public override List<Port> GetCompatiblePorts(Port StartPort, NodeAdapter NodeAdapter)
    {
        return ports.ToList().Where(EndPort =>
        EndPort.direction != StartPort.direction &&
        EndPort.node != StartPort.node &&
        ArePortsCompatible(StartPort, EndPort)).ToList();
    }

    List<Type> EntityTypes = new List<Type>
    {
        typeof(Entity),
        typeof(Speaker)
    };

    List<Type> StringTypes = new List<Type>
    {
        typeof(string),
        typeof(Entity),
        typeof(Speaker),
        typeof(ImaginarySpeaker)
    };

    List<Type> LocationTypes = new List<Type>
    {
        typeof(Vector3),
        typeof(Location),
        typeof(Speaker)
    };

    public bool ArePortsCompatible(Port StartPort, Port EndPort)
    {
        if (EndPort.portType == typeof(AnyType))
            return true;
        if (EndPort.portType == StartPort.portType)
            return true;
        if (EntityTypes.Contains(EndPort.portType) && EntityTypes.Contains(StartPort.portType))
            return true;
        if (!EntityTypes.Contains(EndPort.portType) && StringTypes.Contains(EndPort.portType) && StringTypes.Contains(StartPort.portType))
            return true;
        if (LocationTypes.Contains(EndPort.portType) && LocationTypes.Contains(StartPort.portType) && !EntityTypes.Contains(EndPort.portType))
            return true;

        return false;
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange Change)
    {
        bool ShouldRefresh = false;

        if (Change.elementsToRemove != null)
        {
            foreach (GraphElement Element in Change.elementsToRemove)
            {
                NodeView NodeView = Element as NodeView;
                if (NodeView != null)
                {
                    EventTreeEditor.DeleteNode(NodeView.Node, Tree);
                }

                Edge Edge = Element as Edge;
                if (Edge != null)
                {
                    NodeView ParentView = Edge.output.node as NodeView;
                    NodeView ChildView = Edge.input.node as NodeView;

                    ParentView.Node.Disconnect(Edge.output.tabIndex, ChildView.Node);
                }
            }

            EventTreeEditor.ActiveEditor.MarkDirty();
        }

        if (Change.edgesToCreate != null)
        {
            foreach (Edge Edge in Change.edgesToCreate)
            {
                NodeView ParentView = Edge.output.node as NodeView;
                NodeView ChildView = Edge.input.node as NodeView;

                ParentView.Node.ConnectTo(Edge.output.tabIndex, Edge.input.tabIndex, ChildView.Node);
            }

            EventTreeEditor.ActiveEditor.MarkDirty();
        }

        if (ShouldRefresh)
            Populate(Tree);

        return Change;
    }

    public static List<Type> DefaultTypes { get; private set; } = new List<Type>
    {
        typeof(EndDialogue),
        typeof(Branch),
        typeof(ConditionalBranch),
        typeof(MoveEntity),
        typeof(RandomBranch),
        typeof(SetSwitch),
        typeof(SetVariable),
        typeof(ShowText),
        typeof(SubEvent),
        typeof(Comment),
        typeof(GetSwitch),
        typeof(GetVariable),
        typeof(LocationDefinition),
        typeof(ActionNode),
        typeof(CompositeNode),
        typeof(DecoratorNode),
        typeof(DefinitionNode),
        typeof(CompareNode),
        typeof(Node),
        typeof(StartNode)
    };

    public override void BuildContextualMenu(ContextualMenuPopulateEvent Event)
    {
        GraphView GraphView = this as GraphView;

        Vector2 MousePosition = this.ChangeCoordinatesTo(contentViewContainer, Event.localMousePosition);

        Event.menu.AppendAction("Comment", A => CreateNode<Comment>(MousePosition));
        Event.menu.AppendAction("Event", A => CreateNode<SubEvent>(MousePosition));
        Event.menu.AppendSeparator();
        Event.menu.AppendAction("Define/Entity", A => CreateNode<DefinitionNode>(MousePosition));
        Event.menu.AppendAction("Define/Location", A => CreateNode<LocationDefinition>(MousePosition));

        Event.menu.AppendAction("Get/Variable", A => CreateNode<GetVariable>(MousePosition), Tree.Variables.Count > 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
        Event.menu.AppendAction("Get/Switch", A => CreateNode<GetSwitch>(MousePosition), Tree.Switches.Count > 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

        Event.menu.AppendAction("Set/Variable", A => CreateNode<SetVariable>(MousePosition), Tree.Variables.Count > 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
        Event.menu.AppendAction("Set/Switch", A => CreateNode<SetSwitch>(MousePosition), Tree.Switches.Count > 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

        Event.menu.AppendAction("Logic/Compare", A => CreateNode<CompareNode>(MousePosition));
        Event.menu.AppendAction("Logic/User Choice", A => CreateNode<Branch>(MousePosition));
        Event.menu.AppendAction("Logic/Conditional Branch", A => CreateNode<ConditionalBranch>(MousePosition));
        Event.menu.AppendAction("Logic/Random", A => CreateNode<RandomBranch>(MousePosition));

        Event.menu.AppendAction("Dialogue/Show Speech", A => CreateNode<ShowText>(MousePosition));
        Event.menu.AppendAction("Dialogue/Move Entity", A => CreateNode<MoveEntity>(MousePosition));

        Event.menu.AppendSeparator();

        TypeCache.TypeCollection Types = TypeCache.GetTypesDerivedFrom<Node>();
            
        foreach (Type T in Types)
        {
            if (!DefaultTypes.Contains(T))
            {
                if (T.Namespace == typeof(Node).Namespace)
                {
                    Event.menu.AppendAction($"Experimental/{T.Name}", A => CreateNode(T, MousePosition));
                }
            }
        }

        foreach (Type T in Types)
        {
            if (!DefaultTypes.Contains(T))
            {
                if (T.Namespace != typeof(Node).Namespace)
                {
                    Event.menu.AppendAction($"Custom/{T.Name}", A => CreateNode(T, MousePosition));
                }
            }
        }

        Event.menu.AppendSeparator();

        Event.menu.AppendAction("End", A => CreateNode<EndDialogue>(MousePosition));
    }

    private T CreateNode<T>(Vector2 MousePosition) where T : Node
    {
        return CreateNode(typeof(T), MousePosition) as T;
    }

    private Node CreateNode(Type Type, Vector2 MousePosition)
    {
        Node Node = EventTreeEditor.CreateNode(Type, Tree);
        Node.ParentTree = Tree;
        Node.Position = MousePosition;
        CreateNodeView(Node);

        if (Type == typeof(StartNode))
        {
            Tree.RootNode = Node;
        }

        return Node;
    }

    private NodeView CreateNodeView(Node Node)
    {
        NodeView NodeView = new NodeView(Node);
        NodeView.OnNodeSelected += OnNodeSelected;
        AddElement(NodeView);

        return NodeView;
    }

    #region Events
    [EventInterest(typeof(KeyUpEvent))]
    private void KeyUp(KeyUpEvent Event)
    {
        Vector2 MoveEvent = Vector2.zero;

        if (Event.keyCode == KeyCode.LeftArrow)
        {
            MoveEvent.x = -10;
        }
        else if (Event.keyCode == KeyCode.RightArrow)
        {
            MoveEvent.x = 10;
        }
        else if (Event.keyCode == KeyCode.UpArrow)
        {
            MoveEvent.y = -10;
        }
        else if (Event.keyCode == KeyCode.DownArrow)
        {
            MoveEvent.y = 10;
        }

        if (MoveEvent != Vector2.zero)
        {
            foreach (NodeView Node in selection)
            {
                Node.SetPosition(new Rect(Node.GetPosition().position + MoveEvent * 10, Node.GetPosition().size));
            }
        }
    }
    #endregion
}
