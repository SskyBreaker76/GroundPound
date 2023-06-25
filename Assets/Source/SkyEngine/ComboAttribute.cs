using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class ComboAttribute : PropertyAttribute
    {
        public string Label;
        public string[] Options;

        public ComboAttribute(string Label = "", params string[] Values)
        {
            this.Label = Label;
            Options = Values;
        }

        public ComboAttribute(string True = "", string False = "", string Label = "", bool UseSpecialColours = true)
        {
            this.Label = Label;
            Options = new string[] { False, True };
        }
    }
}