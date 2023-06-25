using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public class DisplayOnlyAttribute : PropertyAttribute
    {
        public DisplayOnlyAttribute() { }
    }
}