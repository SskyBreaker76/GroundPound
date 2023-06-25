using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SkySoft
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ButtonAttribute : PropertyAttribute
    {
        public string Label;
        public string Tooltip;

        public ButtonAttribute(string Label = "", string Tooltip = "")
        {
            this.Label = Label;
            this.Tooltip = Tooltip;
        }
    }
}