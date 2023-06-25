using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class CommonTextKeyAttribute : PropertyAttribute
    {
        public bool AllowStringInput = false;
    }
}
