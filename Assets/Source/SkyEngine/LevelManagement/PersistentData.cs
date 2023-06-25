using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft
{
    [Serializable]
    public class PersistentInfo
    {
        public string Tag;
        public string Information;

        public static PersistentInfo Create<T>(T Object, string Tag = "None") => new PersistentInfo
        {
            Information = JsonUtility.ToJson(Object),
            Tag = Tag
        };

        public T Convert<T>()
        {
            return JsonUtility.FromJson<T>(Information);
        }
    }

    public static class PersistentData
    {
        private static Dictionary<string, PersistentInfo> m_Data = new Dictionary<string, PersistentInfo>();

        public static PersistentInfo PersistObject<T>(string Key, T Object, string Tag="None")
        {
            PersistentInfo I = PersistentInfo.Create(Object, Tag);

            if (!m_Data.ContainsKey(Key))
                m_Data.Add(Key, I);
            else
                m_Data[Key] = I;

            return I;
        }

        public static void ForgetObject(string Key)
        {
            if (m_Data.ContainsKey(Key))
                m_Data.Remove(Key);
        }

        public static T RecallObject<T>(string Key)
        {
            if (!m_Data.ContainsKey(Key))
                return default;

            return m_Data[Key].Convert<T>();
        }

        public static void ForgetEverything(string Tag = "All")
        {
            if (Tag == "All")
                m_Data.Clear();
            else
            {
                Dictionary<string, PersistentInfo> m_Temp = new Dictionary<string, PersistentInfo>();

                foreach (string Key in m_Data.Keys)
                {
                    if (m_Data[Key].Tag != Tag)
                    {
                        m_Temp.Add(Key, m_Data[Key]);
                    }
                }

                m_Data = m_Temp;
            }
        }
    }
}
