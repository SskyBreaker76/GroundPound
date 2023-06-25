using SkySoft.IO;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SkySoft.Config
{
    public enum ValueType
    {
        String,
        Bool,
        Float,
        Int,
        Comment
    }

    public class InvalidDataTypeException : System.Exception
    {
        private string DType;
        public override string Message => $"InvalidDataType: Value must be of type \"{DType}\"!";

        public void Init(ValueType Type)
        {
            DType = Type.ToString();
        }
    }

    [System.Serializable]
    public class Value
    {
        public ValueType Type;
        public string Key;
        public string V;

        public void CheckType(ValueType T)
        {
            if (Type != T)
            {
                InvalidDataTypeException E = new InvalidDataTypeException();
                E.Init(Type);

                throw E;
            }
        }

        public string GetAsString()
        {
            CheckType(ValueType.String);

            return V;
        }

        public bool GetAsBool()
        {
            CheckType(ValueType.Bool);

            return V == "True";
        }

        public float GetAsFloat()
        {
            CheckType(ValueType.Float);

            return float.Parse(V);
        }

        public int GetAsInt()
        {
            CheckType(ValueType.Int);

            return int.Parse(V);
        }
    }

    [System.Serializable]
    public class ConfigFile
    {
        private Value[] m_Values =
        {
            new Value { Type = ValueType.Comment, V = "Audio" },
            new Value { Type = ValueType.Float, V = "1.0", Key = "MasterVolume" },      // Master Volume
            new Value { Type = ValueType.Float, V = "1.0", Key = "MusicVolume" },       // Music Volume
            new Value { Type = ValueType.Float, V = "1.0", Key = "SoundsVolume" },      // Sounds Volume

            new Value { Type = ValueType.Comment, V = "Graphics" },
            new Value { Type = ValueType.Bool, V = "True", Key = "PostEffects" },       // Post Effects
            new Value { Type = ValueType.Int, V = "5", Key = "Quality" },               // Graphics Quality
            
            new Value { Type = ValueType.Comment, V = "Controls" },
            new Value { Type = ValueType.Float, V = "10" }                              // Camera Sensitivity
        };

        private Dictionary<string, Value> CachedValues;
        public Dictionary<string, Value> Values
        {
            get
            {
                if (CachedValues == null || CachedValues.Count < m_Values.Length)
                {
                    CachedValues = new Dictionary<string, Value>();

                    foreach (Value V in m_Values)
                    {
                        CachedValues.Add(V.Key, V);
                    }
                }

                return CachedValues;
            }
        }

        public void SetValue(string Key, string Value)
        {
            if (Values.ContainsKey(Key))
                Values[Key].V = Value;
        }

        public static void Serialize()
        {
            string FileText = $"";



            File.WriteAllText($"{Application.dataPath}\\Config{FileManager.DefaultExtension}", FileText);
        }

        public static ConfigFile DeSerialize(string Input)
        {
            ConfigFile Config = new ConfigFile
            {

            };

            return Config;
        }
    }

    public static class OptionsManager
    {

    }
}