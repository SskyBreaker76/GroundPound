using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.Events.Graph
{
    public abstract class CompositeNode : Node
    {
        public List<Node> Children = new List<Node>();
    }
}