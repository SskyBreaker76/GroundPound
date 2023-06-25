using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using SkySoft.Events.Graph;
using Node = SkySoft.Events.Graph.Node;
using UnityEngine.Tilemaps;
using Codice.CM.SEIDInfo;
using SkySoft.Entities;

public class Speaker { }
public class ImaginarySpeaker { }
public class Location { }
public class Switch { }
public class Number { }
public class Flow { }

public class NodeView : UnityEditor.Experimental.GraphView.Node
{
    public static Color DefaultNodeColour = Color.white;
    public static Color DefaultVariableColour = Color.grey;
    public static Dictionary<Type, Color> NodeColours { get; private set; } = new Dictionary<Type, Color>
    {
        { typeof(Speaker), Color.cyan },
        { typeof(ImaginarySpeaker), Color.magenta },
        { typeof(Location), Color.yellow },
        { typeof(Switch), Color.red },
        { typeof(Number), Color.blue }
    };
    public static Dictionary<Type, Color> SpecialNodeColours { get; private set; } = new Dictionary<Type, Color>
    {

    };

    public static Type ConvertType<T>()
    {
        return ConvertType(typeof(T));
    }

    public static Type ConvertType(Type Input)
    {
        if (Input == typeof(Node))
            return typeof(Flow);
        if (Input == typeof(string))
            return typeof(ImaginarySpeaker);
        if (Input == typeof(Vector3))
            return typeof(Location);
        if (Input == typeof(Entity))
            return typeof(Speaker);
        if (Input == typeof(bool))
            return typeof(Switch);
        if (Input == typeof(int) || Input == typeof(float))
            return typeof(Number);

        return Input;
    }

    public Action<NodeView> OnNodeSelected;

    public Node Node;

    public Port Input { get { return Inputs[0]; } set { Inputs[0] = value; } }
    public List<Port> Inputs = new List<Port>();
    public Port Output { get { return Outputs[0]; } set { Outputs[0] = value; } }
    public List<Port> Outputs = new List<Port>();

    public static Color BaseColour => new Color(0.3f, 0.3f, 0.3f, 1);

    public NodeView(Node Node)
    {
        if (Node)
        {
            this.Node = Node;
            Node.Setup(); // Call this when we create the NodeView so that the node can setup its variables and outputs
            viewDataKey = Node.GUID;

            style.left = Node.Position.x;
            style.top = Node.Position.y;

            StyleColor BG = new StyleColor(BaseColour * Node.NodeTint);
            titleContainer.style.backgroundColor = BG;

            inputContainer.style.backgroundColor = new StyleColor(BaseColour * 0.9f);
            outputContainer.style.backgroundColor = new StyleColor(BaseColour * 0.7f);

            CreateInputPorts();

            title = Node.DecorativeName;

            Label DisplayLabel = new Label(Node.Description);
            DisplayLabel.style.paddingBottom = 8;
            DisplayLabel.style.paddingLeft = 8;
            DisplayLabel.style.paddingRight = 8;
            DisplayLabel.style.paddingTop = 8;
            DisplayLabel.style.fontSize = 12;
            DisplayLabel.style.unityFontStyleAndWeight = FontStyle.Normal;
            DisplayLabel.style.backgroundColor = new StyleColor(BaseColour * Color.grey);
            mainContainer.Add(DisplayLabel);

            CreateOutputPorts();
        }
    }

    private void CreateInputPorts()
    {
        Inputs = new List<Port>();
        for (int I = 0; I < Node.Variables.Count; I++)
        {
            NodeVariable Var = Node.Variables[I];

            if (Var.ValueType != null)
            {
                Type ConvertedType = ConvertType(Var.VariableType);
                Port VarInput = InstantiatePort(Orientation.Horizontal, Direction.Input, ConvertedType == typeof(Flow) ? Port.Capacity.Multi : Port.Capacity.Single, ConvertedType);
                if (NodeColours.ContainsKey(ConvertedType))
                {
                    VarInput.portColor = NodeColours[ConvertedType];
                }
                else
                {
                    if (SpecialNodeColours.ContainsKey(Node.GetType()))
                        VarInput.portColor = SpecialNodeColours[Node.GetType()];
                    else
                        VarInput.portColor = ConvertedType == typeof(Flow) ? DefaultNodeColour : DefaultVariableColour;
                }
                VarInput.portName = Var.Key;
                VarInput.tabIndex = I;
                Inputs.Add(VarInput);
            }
            else
            {
                inputContainer.Add(new Label(Var.Key));
            }
        }

        if (Inputs.Count > 0)
        {
            foreach (Port Input in Inputs)
            {
                inputContainer.Add(Input);
            }
        }
    }

    private void CreateOutputPorts()
    {
        Outputs = new List<Port>();

        if (Node is LoadLevel)
            return;

        for (int I = 0; I < Node.Returns.Count; I++)
        {
            NodeVariable Out = Node.Returns[I];

            Type ConvertedType = ConvertType(Out.VariableType);
            
            Port VarOutput = InstantiatePort(Orientation.Horizontal, Direction.Output, ConvertedType == typeof(Flow) ? Port.Capacity.Single : Port.Capacity.Multi, ConvertedType);
            if (NodeColours.ContainsKey(ConvertedType))
            {
                VarOutput.portColor = NodeColours[ConvertedType];
            }
            else
            {
                if (SpecialNodeColours.ContainsKey(Node.GetType()))
                    VarOutput.portColor = SpecialNodeColours[Node.GetType()];
                else
                    VarOutput.portColor = ConvertedType == typeof(Flow) ? DefaultNodeColour : DefaultVariableColour;
            }
            VarOutput.portName = Out.Key;
            VarOutput.tabIndex = I;
            Outputs.Add(VarOutput);
        }

        if (Outputs.Count > 0)
        {
            foreach (Port Output in Outputs)
            {
                outputContainer.Add(Output);
            }
        }    
    }

    public override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);
        Node.Position.x = newPos.xMin;
        Node.Position.y = newPos.yMin;
        EventTreeEditor.ActiveEditor.MarkDirty();
    }

    public override void OnSelected()
    {
        base.OnSelected();
        if (OnNodeSelected != null)
        {
            OnNodeSelected.Invoke(this);
        }
    }
}