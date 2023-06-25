using SkySoft.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IniParser;
using IniParser.Parser;
using IniParser.Model;

namespace SkySoft.IO
{
    public static class ConfigManager
    {
        private static IniData Data = null;

        public static int GetOption(string Key, int DefaultValue = 0, string Section = "Default")
        {
            if (Data == null)
            {
                Data = FileManager.ReadConfigFile();
            }

            if (!Data.Sections.ContainsSection(Section))
                Data.Sections.AddSection(Section);
            if (!Data[Section].ContainsKey(Key))
                Data[Section].AddKey(Key, DefaultValue.ToString());

            return int.Parse(Data[Section][Key]);
        }

        public static void SetOption(string Key, int Value, string Section = "Default")
        {
            if (Data == null)
            {
                Data = FileManager.ReadConfigFile();
            }

            if (!Data.Sections.ContainsSection(Section))
                Data.Sections.AddSection(Section);
            if (!Data[Section].ContainsKey(Key))
                Data[Section].AddKey(Key, Value.ToString());

            Data[Section][Key] = Value.ToString(); // This may be redundent if the section or key doesn't exist,
                                                   // but coding this is the lazy way of having default behaviour
            FileManager.WriteConfigFile(Data);
        }
    }
}
