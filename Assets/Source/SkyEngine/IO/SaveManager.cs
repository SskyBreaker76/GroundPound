using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SkySoft.IO
{
    [Obsolete("Saving is now handled by your Player Entity using the FileManager class!")]
    [Serializable]
    public class SaveFile
    {
        // SaveFile shit goes here :3
    }

    [Obsolete("Saving is now handled by your Player Entity using the FileManager class!")]
    [Serializable]
    public class SaveInformation
    {
        public Sprite Portrait;
        public int Level;
        public string Class;
        public string Name;
        public DateTime CreationTime;
        public DateTime ModificationTime;
        public long Playtime => (CreationTime - DateTime.Now).Ticks;
        public int FileSize;
    }

    [Obsolete("Saving is now handled by your Player Entity using the FileManager class!")]
    public static class SaveManager
    {

    }
}