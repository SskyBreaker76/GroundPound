using SkySoft.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.Events.Graph
{
    [System.Serializable]
    public class DialogueSwitch
    {
        public string Key;
        public bool Value;
        public bool Exposed;
    }

    [System.Serializable]
    public class DialogueText
    {
        public string Key;
        public string Value;
        public bool Exposed;
    }

    [CreateAssetMenu(menuName = "SkyEngine/New Dialogue Tree", fileName = "New Event")]
    public class EventTree : ScriptableObject
    {
        public Action OnExit;
        public static bool RequestGraphRefresh = false;
        public Node RootNode;
        public Node.NodeState TreeState = Node.NodeState.Running;
        public List<Node> Nodes = new List<Node>();
        public List<DialogueSwitch> Switches = new List<DialogueSwitch>();
        public List<DialogueText> Variables = new List<DialogueText>();
        public EventDataFile Data = new EventDataFile();

        public void SetSwitchValue(string Key, bool Value)
        {
            foreach (DialogueSwitch Switch in Switches)
            {
                if (Switch.Key == Key)
                {
                    Switch.Value = Value;
                }
            }
        }
        public void SetVariableValue(string Key, string Value)
        {
            foreach (DialogueText Text in Variables)
            {
                if (Text.Key == Key)
                {
                    Text.Value = Value;
                }
            }
        }

        public string CreateID
        {
            get
            {
                return System.Guid.NewGuid().ToString();
            }
        }

        private int CurrentNode;

        public struct ChoiceInf
        {
            public string Text;
            public int ChoiceIndex;
        }

        public struct NextNodeInf
        {
            public string SpeakerName;
            public string Dialogue;
            public AudioClip DialogueSound;
            public bool Exit;
            public bool NextIsChoices;
            public bool IsChoice;
            public ChoiceInf[] Choices;
        }

        public void RestartDialogue()
        {
            CurrentNode = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Output">Returns true when the Dialogue is finished</param>
        /// <returns></returns>
        public NextNodeInf NextNode(int Output = 0)
        {
            Debug.Log($"{Nodes[CurrentNode].name}: {Nodes[CurrentNode].GetType().Name}");

            ActionNode CurrentAsAction = Nodes[CurrentNode] as ActionNode;
            CompositeNode CurrentAsComposite = Nodes[CurrentNode] as CompositeNode;
            StartNode CurrentAsStart = Nodes[CurrentNode] as StartNode;
            NextNodeInf NextInfo = new NextNodeInf();

            if (CurrentAsAction)
            {
                NextInfo.Exit = true;
            }
            if (CurrentAsComposite)
            {
                CurrentNode = Nodes.IndexOf(CurrentAsComposite.Children[Output]);
                ShowText T = CurrentAsComposite as ShowText;
                if (T)
                {
                    NextInfo.SpeakerName = T.Properties.Speaker;
                    NextInfo.Dialogue = T.Properties.Dialogue;
                    NextInfo.DialogueSound = T.Properties.Audio;
                    if (T.Children[0] is Branch)
                    {
                        NextInfo.NextIsChoices = true;
                        List<ChoiceInf> Choices = new List<ChoiceInf>();
                        for (int I = 0; I < (T.Children[0] as Branch).BranchCount; I++)
                        {
                            Choices.Add(new ChoiceInf { ChoiceIndex = I, Text = (T.Children[0] as Branch).Branches[I] });
                        }
                        NextInfo.Choices = Choices.ToArray();
                    }
                }
                Branch B = CurrentAsComposite as Branch;
                if (B)
                {
                    NextInfo.IsChoice = true;
                    List<ChoiceInf> Choices = new List<ChoiceInf>();
                    for (int I = 0; I < (T.Children[0] as Branch).BranchCount; I++)
                    {
                        Choices.Add(new ChoiceInf { ChoiceIndex = I, Text = (T.Children[0] as Branch).Branches[I] });
                    }
                    NextInfo.Choices = Choices.ToArray();

                }
            }
            if (CurrentAsStart)
            {
                CurrentNode = Nodes.IndexOf(CurrentAsStart.Child);
            }

            return NextInfo;
        }

        /// <summary>
        /// Returns -1 if there is no start node
        /// </summary>
        public int StartNodeIndex
        {
            get
            {
                for (int I = 0; I < Nodes.Count; I++)
                {
                    if (Nodes[I] is StartNode)
                        return I;
                }

                return -1;
            }
        }

        public T CreateNode<T>() where T : Node
        {
            T Node = ScriptableObject.CreateInstance(typeof(T)) as T;
            Node.name = typeof(T).Name;
            Node.GUID = CreateID;
            Nodes.Add(Node);

            return Node;
        }

        public Node CreateNode(Type T)
        {
            Node Node = ScriptableObject.CreateInstance(T) as Node;
            Node.name = T.Name;
            Node.GUID = CreateID;
            Nodes.Add(Node);
            return Node;
        }

        public void DeleteNode(Node Node)
        {
            Nodes.Remove(Node);
        }

        public void AddChild(Node Parent, Node Child)
        {
            DefinitionNode DefinitionNode = Parent as DefinitionNode;
            ShowText TextNode = Child as ShowText;

            if (DefinitionNode && TextNode)
            {
                TextNode.InputNodeGUID = DefinitionNode.GUID;

                if (!DefinitionNode.SoftTarget)
                {
                    TextNode.Properties.IsControlledBySoftTarget = false;
                    TextNode.Properties.SpeakerID = DefinitionNode.TargetID;
                }
                else
                {
                    TextNode.Properties.IsControlledBySoftTarget = true;
                    TextNode.Properties.m_Speaker = DefinitionNode.Value;
                }

                RequestGraphRefresh = true;
            }
            else
            {
                StartNode Start = Parent as StartNode;
                if (Start)
                {
                    Start.Child = Child;
                }
                DecoratorNode Decorator = Parent as DecoratorNode;
                if (Decorator)
                {
                    Decorator.Child = Child;
                }
                CompositeNode Composite = Parent as CompositeNode;
                if (Composite)
                {
                    Composite.Children.Add(Child);
                }
            }
        }

        public void RemoveChild(Node Parent, Node Child)
        {
            DefinitionNode DefinitionNode = Parent as DefinitionNode;
            ShowText TextNode = Child as ShowText;

            if (DefinitionNode && TextNode)
            {
                TextNode.InputNodeGUID = "";
                TextNode.Properties.IsControlledBySoftTarget = false;
                TextNode.Properties.SpeakerID = "";
                RequestGraphRefresh = true;
            }
            else
            {
                StartNode Start = Parent as StartNode;
                if (Start)
                {
                    Start.Child = null;
                }
                DecoratorNode Decorator = Parent as DecoratorNode;
                if (Decorator)
                {
                    Decorator.Child = null;
                }
                CompositeNode Composite = Parent as CompositeNode;
                if (Composite)
                {
                    Composite.Children.Remove(Child);
                }
            }
        }

        public List<Node> GetChildren(Node Parent)
        {
            StartNode Start = Parent as StartNode;
            if (Start)
            {
                return new List<Node> { Start.Child };
            }
            DecoratorNode Decorator = Parent as DecoratorNode;
            if (Decorator)
            {
                return new List<Node> { Decorator.Child };
            }
            CompositeNode Composite = Parent as CompositeNode;
            if (Composite)
            {
                return Composite.Children;
            }

            return new List<Node>();
        }

        public bool CanEnd
        {
            get
            {
                foreach (Node N in Nodes)
                {
                    if (N.Connections != null && N.Connections.Count > 0)
                    {
                        foreach (ConnectionInfo Connection in N.Connections)
                        {
                            if (Connection.Target is EndDialogue)
                                return true;
                        }
                    }
                }

                return false;
            }
        }

        public void UpdateFlow()
        {
            foreach (Node N in Nodes)
            {
                N.UpdateVariables();
            }

            foreach (Node N in Nodes)
            {
                foreach (ConnectionInfo C in N.Connections)
                {
                    if (C.Target != null && C.InputIndex > (C.Target.IsPure ? -1 : 0))
                    {
                        if (N is DefinitionNode)
                        {
                            DefinitionNode Def = N as DefinitionNode;
                            if (C.Target is ShowText)
                            {
                                ShowText Targ = C.Target as ShowText;

                                Targ.Properties.m_Speaker = Def.Value;
                            }
                            else if (C.Target is SubEvent)
                            {
                                SubEvent EV = C.Target as SubEvent;
                                if (Def.NodeType == typeof(bool))
                                {
                                    EV.SetSwitchConnection(C.InputIndex, Def);
                                }
                                else
                                {
                                    EV.SetVariableConnection(C.InputIndex, Def);
                                }
                            }
                            else if (C.Target is CompareNode)
                            {
                                CompareNode AsComparison = C.Target as CompareNode;
                                AsComparison.InputType = Def.NodeType;
                                AsComparison.InputValue = Def.Value;
                            }
                            else if (C.Target is WriteVariable)
                            {
                                WriteVariable AsWriteVar = C.Target as WriteVariable;
                                AsWriteVar.Value = Def.Value;
                            }
                            else if (C.Target is LocationDefinition)
                            {
                                LocationDefinition AsLoc = C.Target as LocationDefinition;
                                AsLoc.Location = Def.GetLocation;
                            }
                            else if (C.Target is SpawnObject)
                            {
                                SpawnObject AsSpawned = C.Target as SpawnObject;
                                AsSpawned.Location = Def.GetLocation;
                            }
                            else if (C.Target is ConditionalBranch)
                            {
                                if (Def.NodeType == typeof(bool))
                                {
                                    (C.Target as ConditionalBranch).InputNode = Def;
                                }
                            }
                        }
                        if (N is CompareNode)
                        {
                            CompareNode Comparison = N as CompareNode;
                            if (C.Target is ConditionalBranch)
                            {
                                ((ConditionalBranch)C.Target).InputNode = N;
                            }
                            if (C.Target is SubEvent)
                            {
                                SubEvent EV = C.Target as SubEvent;
                                EV.SetSwitchConnection(C.InputIndex, Comparison);
                            }
                        }
                    }
                }

                N.ParentTree = this;
            }
        }

        public void Run(Action OnDone)
        {
            UpdateFlow();

            RootNode.Run(OnDone);
        }

        private void OnValidate()
        {
        }
    }
}
