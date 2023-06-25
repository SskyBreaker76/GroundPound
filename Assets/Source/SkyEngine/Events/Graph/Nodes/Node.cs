using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkySoft.Events.Graph
{
    public class AnyType { }

    [Serializable]
    public class ConnectionInfo
    {
        public Node Target;
        public int OutputIndex;
        public int InputIndex;
    }

    [Serializable]
    public class NodeVariable
    {
        public string Key;
        public ConnectionInfo Port;
        public object Value;
        public Type VariableType;
        private Type m_ValueType;
        public Type ValueType { get { return HasConnection ? m_ValueType : VariableType; } set { m_ValueType = value; } }
        public bool HasConnection = false;

        public static NodeVariable Create<T>(ConnectionInfo Port, string Key, T DefaultValue = default)
        {
            return new NodeVariable
            {
                Key = Key,
                Value = DefaultValue,
                Port = Port,
                VariableType = typeof(T)
            };
        }
    }

    public abstract class Node : ScriptableObject
    {
        public const bool DebugMode = true;
        public virtual string MenuName => "Custom";

        public virtual Color NodeTint => Color.white;

        /// <summary>
        /// Pure Nodes don't come with any Ports at all, and are executed once at the start of the
        /// Event to initialized their variables. If the value of a Pure node is changed, the node
        /// will update on its own.
        /// </summary>
        public virtual bool IsPure => false;
        public virtual bool UseDefaultOutput => true;
        public EventTree ParentTree;

        public List<ConnectionInfo> Connections = new List<ConnectionInfo>();
        public Dictionary<int, ConnectionInfo> ConnectionDict
        {
            get
            {
                Dictionary<int, ConnectionInfo> C = new Dictionary<int, ConnectionInfo>();

                foreach (ConnectionInfo Connection in Connections)
                {
                    if (!C.ContainsKey(Connection.OutputIndex))
                        C.Add(Connection.OutputIndex, Connection);
                }

                return C;
            }
        }
        private Dictionary<int, List<ConnectionInfo>> PureConnectionDict
        {
            get
            {
                Dictionary<int, List<ConnectionInfo>> C = new Dictionary<int, List<ConnectionInfo>>();

                foreach (ConnectionInfo Connection in Connections)
                {
                    if (!C.ContainsKey(Connection.OutputIndex))
                        C.Add(Connection.OutputIndex, new List<ConnectionInfo> { Connection });
                    else
                        C[Connection.OutputIndex].Add(Connection);
                }

                return C;
            }
        }

        public Action<Node> OnModified = new Action<Node>(OnNodeModified);

        public static void OnNodeModified(Node Node)
        {
            Node.OnNodeWasModified();
            Node.UpdateConnectionCount();
        }
        protected virtual void OnNodeWasModified() { }

        protected virtual void UpdateConnectionCount()
        {

        }

        public List<NodeVariable> Variables = new List<NodeVariable>();
        public List<NodeVariable> Returns = new List<NodeVariable>();

        public void Setup()
        {
            List<NodeVariable> OldVariables = new List<NodeVariable>();
            List<NodeVariable> OldReturns = new List<NodeVariable>();

            Variables.Clear();
            Returns.Clear();

            if (!IsPure && !(this is StartNode))
            {
                Variables.Add(new NodeVariable { Key = "", VariableType = typeof(Node) });
            }
            if (!IsPure && !(this is EndDialogue))
            {
                if (UseDefaultOutput)
                    Returns.Add(new NodeVariable { Key = "", VariableType = typeof(Node), Port = GetPort(0) });
            }

            SetupNode();

            if (OldVariables.Count > 1)
            {
                for (int I = 1; I < OldVariables.Count; I++)
                {
                    Variables[I].HasConnection = OldVariables[I].HasConnection;
                    Variables[I].Value = OldVariables[I].Value;
                    Variables[I].ValueType = OldVariables[I].ValueType;
                }
            }
        }

        /// <summary>
        /// This is where you should setup Variables and Returns
        /// </summary>
        public virtual void SetupNode() { }

        public void UpdateVariables()
        {
            /*
            string VariableDebug = $"{name}'s Variables:\n";

            foreach (NodeVariable Var in Variables)
            {
                VariableDebug += $"{Var.Key} ({Var.ValueType.Name} - {Var.VariableType.Name}) = {Var.Value}";
            }

            Debug.Log(VariableDebug);
            */
            OnVariableUpdate();
        }

        protected virtual void OnVariableUpdate() { }

        public ConnectionInfo GetPort(int OutputIndex)
        {
            Dictionary<int, ConnectionInfo> Cs = ConnectionDict;

            if (Cs.ContainsKey(OutputIndex))
                return Cs[OutputIndex];

            ConnectionInfo C = new ConnectionInfo { OutputIndex = OutputIndex };
            Connections.Add(C);
            return C;
        }

        public void ConnectTo(int OutputIndex, int InputIndex, Node Target)
        {
            // Debug.Log($"ConnectTo({OutputIndex}, {InputIndex}, {Target.GUID}");
            if (!IsPure)
            {
                Dictionary<int, ConnectionInfo> Cs = ConnectionDict;

                if (Cs.ContainsKey(OutputIndex))
                {
                    Cs[OutputIndex].OutputIndex = OutputIndex;
                    Cs[OutputIndex].Target = Target;
                    Cs[OutputIndex].InputIndex = InputIndex;
                }
                else
                {
                    Cs.Add(OutputIndex, new ConnectionInfo { InputIndex = InputIndex, Target = Target, OutputIndex = OutputIndex });
                }

                Connections.Clear();

                foreach (ConnectionInfo Connection in Cs.Values)
                {
                    Connections.Add(Connection);
                }
            }
            else
            {
                Dictionary<int, List<ConnectionInfo>> Cs = PureConnectionDict;

                if (Cs.ContainsKey(OutputIndex))
                {
                    foreach (ConnectionInfo Connection in Cs[OutputIndex])
                    {
                        if (Connection.Target == Target)
                            return;
                    }

                    // Debug.Log("No Target");

                    Cs[OutputIndex].Add(new ConnectionInfo { InputIndex = InputIndex, Target = Target, OutputIndex = OutputIndex });
                }
                else
                {
                    Cs.Add(OutputIndex, new List<ConnectionInfo> { new ConnectionInfo { InputIndex = InputIndex, Target = Target, OutputIndex = OutputIndex } });
                }

                Connections.Clear();

                foreach (List<ConnectionInfo> ConnectionList in Cs.Values)
                {
                    foreach (ConnectionInfo Connection in ConnectionList)
                    {
                        Connections.Add(Connection);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="OutputIndex"></param>
        /// <param name="Target">This is only used for Pure nodes</param>
        public void Disconnect(int OutputIndex, Node Target = null)
        {
            if (!IsPure)
            {
                Dictionary<int, ConnectionInfo> Cs = ConnectionDict;

                if (Cs.ContainsKey(OutputIndex))
                {
                    Cs[OutputIndex].Target = null;
                }

                Connections.Clear();

                foreach (ConnectionInfo Connection in Cs.Values)
                {
                    Connections.Add(Connection);
                }
            }
            else
            {
                Dictionary<int, List<ConnectionInfo>> Cs = PureConnectionDict;

                if (Cs.ContainsKey(OutputIndex))
                {
                    foreach (ConnectionInfo Connection in Cs[OutputIndex])
                    {
                        if (Connection.Target == Target)
                            Connection.Target = null;
                    }
                }

                Connections.Clear();

                foreach (List<ConnectionInfo> ConnectionList in Cs.Values)
                {
                    foreach (ConnectionInfo Connection in ConnectionList)
                    {
                        Connections.Add(Connection);
                    }
                }
            }
        }

        public enum NodeState
        {
            Running,
            Failure,
            Success
        }

        public abstract string DecorativeName { get; }
        public virtual string Description { get; }
        public NodeState State = NodeState.Running;
        public bool Started = false;
        public string GUID;
        public Vector2 Position;

        public NodeState Update()
        {
            if (!Started)
            {
                OnStart();
                Started = true;
            }

            State = OnUpdate();

            if (State == NodeState.Failure || State == NodeState.Success)
            {
                OnStop();
                Started = false;
            }

            return State;
        }

        protected virtual void OnStart() { }
        protected virtual void OnStop() { }
        protected virtual NodeState OnUpdate() { return NodeState.Success; }

        public virtual void Run(Action OnDone) { (Returns[0].Value as Node).Run(OnDone); }

        public virtual void OnStartInspector() { }

        public virtual void DrawInspector()
        {
#if UNITY_EDITOR
            UnityEditor.Editor E = UnityEditor.Editor.CreateEditor(this);
            E.OnInspectorGUI();
#endif
        }
    }
}