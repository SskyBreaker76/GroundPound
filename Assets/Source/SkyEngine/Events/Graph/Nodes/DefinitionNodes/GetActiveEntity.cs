using SkySoft.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.Events.Graph
{
    public class GetActiveEntity : DefinitionNode
    {
        public override string DecorativeName => "Active Entity";
        public override Type NodeType => typeof(Entity);
        public override string Value { get => EventGraphManager.CurrentEvent ? EventGraphManager.CurrentEvent.InstanceID : "NULL"; set => Debug.LogWarning("Set is not available for the GetActiveEntity node!"); }
        public override string Description => "";
    }
}