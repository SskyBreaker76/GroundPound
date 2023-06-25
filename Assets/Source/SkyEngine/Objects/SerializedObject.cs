using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.Objects
{
    [AddComponentMenu("SkyEngine/Serialized Object")]
    public class SerializedObject : MonoBehaviour
    {
        [DisplayOnly]
        public string InstanceID = "";
        public bool RefreshID = false;

        public virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(InstanceID))
                InstanceID = Guid.NewGuid().ToString();

            if (FindObjectsOfType<SerializedObject>().Length > 0)
            {
                foreach (SerializedObject Obj in FindObjectsOfType<SerializedObject>())
                {
                    if (Obj.InstanceID == InstanceID && Obj != this)
                    {
                        RefreshID = true;
                        break;
                    }
                }
            }

            if (RefreshID)
            {
                InstanceID = SkyEngine.CreateID();
                RefreshID = false;
            }
        }

        /// <summary>
        /// This fuction needs to be over-written before it actual does anything
        /// </summary>
        /// <returns></returns>
        public virtual string Serialize() { return ""; }
    }
}