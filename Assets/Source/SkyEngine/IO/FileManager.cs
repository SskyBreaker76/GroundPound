using IniParser;
using IniParser.Model;
using IniParser.Parser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using UnityEngine;

namespace SkySoft.IO
{
    [Serializable]
    public struct FileCreationTime
    {
        public int Day, Month, Year, Hour, Minute, Second, Millisecond;

        public FileCreationTime(DateTime Time)
        {
            Day = Time.Day;
            Month = Time.Month;
            Year = Time.Year;
            Hour = Time.Hour;
            Minute = Time.Minute;
            Second = Time.Second;
            Millisecond = Time.Millisecond;
        }

        public DateTime ToDateTime
        {
            get
            {
                return new DateTime(Year, Month, Day, Hour, Minute, Second, Millisecond);
            }
        }

        public TimeSpan Age
        {
            get
            {
                return DateTime.Now - ToDateTime;
            }
        }

        public static string FormatTimeSpan(TimeSpan Input)
        {
            return $"{(Input.Days > 0 ? $"{Input.Days} and " : "")}{Input.Hours.ToString("00")}:{Input.Minutes.ToString("00")}:{Input.Seconds.ToString("00")}.{Input.Milliseconds.ToString("000")}";
        }
    }

    [System.Serializable]
    public class EventDataFile
    {
        public string EventName;

        internal class DialogueVariable
        {
            public string Key;
            public string Value;
        }

        [SerializeField] private List<DialogueVariable> DialogueVariables = new List<DialogueVariable>();
        private Dictionary<string, string> Variables = new Dictionary<string, string>();

        public void SetVariable(string Key, string Value)
        {
            if (!Variables.ContainsKey(Key))
                Variables.Add(Key, Value);

            Variables[Key] = Value;
        }

        public string GetVariable(string Key, string DefaultValue = "")
        {
            if (!Variables.ContainsKey(Key))
                Variables.Add(Key, DefaultValue);

            return Variables[Key];
        }

        public void WriteFile(Action OnComplete)
        {
            // Rather than load the file from Disc and potentially cause NPCs to forget things that've
            // happened between now and the last save, first check SkyEngine's bank of DirtyEvents to
            // see if this one is in there
            foreach (EventDataFile DirtyEvent in SkyEngine.DirtyEvents)
            {
                if (DirtyEvent.EventName == EventName)
                {
                    DirtyEvent.Variables = Variables;
                    OnComplete();
                    return;
                }
            }

            SkyEngine.DirtyEvents.Add(this);
            OnComplete();
        }

        public void ReadFile(Action OnComplete)
        {
            // Rather than load the file from Disc and potentially cause NPCs to forget things that've
            // happened between now and the last save, first check SkyEngine's bank of DirtyEvents to
            // see if this one is in there
            foreach (EventDataFile DirtyEvent in SkyEngine.DirtyEvents)
            {
                if (DirtyEvent.EventName == EventName)
                {
                    DialogueVariables = DirtyEvent.DialogueVariables;
                    OnComplete();
                    return;
                }
            }

            if (FileManager.FileExists<EventDataFile>("Events", EventName))
                DialogueVariables = FileManager.ReadFile<EventDataFile>("Events", EventName, V => OnComplete()).DialogueVariables;
            else
                OnComplete();
        }
    }

    /// <summary>
    /// This is the base class in-charge of any read/write operations SkyEngine needs.
    /// </summary>
    public static class FileManager
    {
        public static string RootPath { get; private set; } = Application.persistentDataPath;

        public static string ArchivedSavesPath => $"{RootPath}\\Saves";
        public static string BaseWritePath => $"{RootPath}\\ActiveData";
        public static string ConfigPath => $"{RootPath}\\Config.ini";

        public static IniData ReadConfigFile()
        {
            if (!File.Exists(ConfigPath))
                File.WriteAllText(ConfigPath, "");

            var Parser = new FileIniDataParser();
            return Parser.ReadFile(ConfigPath);
        }

        public static void WriteConfigFile(IniData Data)
        {
            var Parser = new FileIniDataParser();
            Parser.WriteFile(ConfigPath, Data);
        }

        public static class Rubbish
        {

        }

        public const int MaxSaves = 100;
        public static bool Archiving { get; private set; }

        public static T ReadFromArchive<T>(int SaveSlot, string Folder, string FileName, Action<T> OnComplete, string Extension = DefaultExtension)
        {
            if (File.Exists($"{ArchivedSavesPath}\\File{SaveSlot}.esav"))
            {
                string TmpPath = $"{ArchivedSavesPath}\\Tmp";

                if (Directory.Exists(TmpPath))
                    DeleteAllFiles(new DirectoryInfo(TmpPath));

                Directory.CreateDirectory(TmpPath);

                ZipFile.ExtractToDirectory($"{ArchivedSavesPath}\\File{SaveSlot}.esav", TmpPath);

                T V = JsonUtility.FromJson<T>(File.ReadAllText($"{ArchivedSavesPath}\\Tmp\\{Folder}\\{FileName}{Extension}"));

                OnComplete(V);
                return V;
            }

            return default;
        }

        public static async void ArchiveSave(int SaveSlot, float Prog = 0.92f, Action<float> ReportProgress = null)
        {
            Archiving = true;

            if (SaveSlot >= MaxSaves || SaveSlot < 0)
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayDialog("Failed to archive save!", $"Failed to archive save: SaveSlot[{SaveSlot}] is out of range!", "Shit!");
#endif
                Debug.LogError($"Failed to archive save: SaveSlot[{SaveSlot}] is out of range!");
                Archiving = false;
                return;
            }

            if (!Directory.Exists(ArchivedSavesPath))
                Directory.CreateDirectory(ArchivedSavesPath);

            FileInfo SaveFile = new FileInfo($"{ArchivedSavesPath}\\File{SaveSlot}.esav");

            if (SaveFile.Exists)
                SaveFile.Delete();

            for (int I = 0; I < 10; I++)
            {
                await Task.Delay(100 * UnityEngine.Random.Range(1, 4));
                if (ReportProgress != null)
                {
                    Prog += 0.006f;
                    ReportProgress(Prog);
                }
            }

            ZipFile.CreateFromDirectory(BaseWritePath, $"{ArchivedSavesPath}\\File{SaveSlot}.esav", System.IO.Compression.CompressionLevel.Optimal, false);

            ReportProgress(0.999f);
            await Task.Delay(650);

            SkyEngine.ActiveSaveIndex = SaveSlot;

            Archiving = false;
        }

        public static void GetSave(int SaveSlot, Action OnDone = null)
        {
            if (GetSaveCount == 0)
                return;

            if (SaveSlot >= 0 && SaveSlot < MaxSaves)
            {
                if (!Directory.Exists(ArchivedSavesPath))
                    Directory.CreateDirectory(ArchivedSavesPath);

                DeleteAllFiles(new DirectoryInfo(BaseWritePath));

                Directory.CreateDirectory(BaseWritePath);

                if (File.Exists($"{ArchivedSavesPath}\\File{SaveSlot}.esav"))
                {

                    SkyEngine.ActiveSaveIndex = SaveSlot;

                    ZipFile.ExtractToDirectory($"{ArchivedSavesPath}\\File{SaveSlot}.esav", BaseWritePath);
                }
                else
                {
                    Debug.Log($"No save found on Slot {SaveSlot}");
                }
            }

            if (OnDone != null)
                OnDone();
        }

        public static bool SaveExists(int SaveSlot)
        {
            return File.Exists($"{ArchivedSavesPath}\\File{SaveSlot}.esav");
        }

        public static int GetSaveCount
        {
            get
            {
                int Result = 0;

                if (!Directory.Exists(ArchivedSavesPath))
                    Directory.CreateDirectory(ArchivedSavesPath);

                foreach (FileInfo File in new DirectoryInfo(ArchivedSavesPath).GetFiles())
                {
                    if (File.Extension.ToLower().Contains("esav"))
                    {
                        Result++;
                    }
                }

                return Result;
            }
        }

        private static void DeleteAllFiles(DirectoryInfo Inf)
        {
            try
            {
                foreach (FileInfo F in Inf.GetFiles())
                {
                    try
                    {
                        File.Delete(F.FullName);
                    }
                    catch { }
                }

                foreach (DirectoryInfo D in Inf.GetDirectories())
                {
                    try
                    {
                        if (D.GetFiles().Length > 0)
                        {
                            DeleteAllFiles(D);
                        }

                        Directory.Delete(D.FullName);
                    }
                    catch { }
                }
            } catch { }
        }

        public const string DefaultExtension = ".edat";

        private static Dictionary<Type, string> Extensions = new Dictionary<Type, string>();

        public static void RegisterExtension(Type Class, string Extension)
        {
            Extensions.Add(Class, Extension);
        }

        public static bool FileExists<T>(string Folder, string FileName, string Extension = DefaultExtension)
        {
            if (Extension == DefaultExtension)
            {
                if (Extensions.ContainsKey(typeof(T)))
                {
                    Extension = Extensions[typeof(T)];
                }
            }

            if (Directory.Exists($"{BaseWritePath}\\{Folder}\\") && File.Exists($"{BaseWritePath}\\{Folder}\\{FileName}{Extension}"))
                return true;

            return false;
        }

        public static string WriteFile<T>(string Folder, string FileName, T Value, Action<bool> OnComplete = null, string Extension = DefaultExtension)
        {
            if (Extension == DefaultExtension)
            {
                if (Extensions.ContainsKey(typeof(T)))
                {
                    Extension = Extensions[typeof(T)];
                }
            }

            if (!Directory.Exists($"{BaseWritePath}\\{Folder}\\"))
                Directory.CreateDirectory($"{BaseWritePath}\\{Folder}\\");

            try
            {
                string FileText = JsonUtility.ToJson(Value, true);

                File.WriteAllText($"{BaseWritePath}\\{Folder}\\{FileName}{Extension}", FileText);
                if (OnComplete != null)
                    OnComplete(true);

                return FileText;
            }
            catch
            {
                if (OnComplete != null)
                    OnComplete(false);
            }

            if (OnComplete != null)
                OnComplete(false);

            return "";
        }

        public static T ReadFile<T>(string Folder, string FileName, Action<T> OnComplete, string Extension = DefaultExtension)
        {
            if (Extension == DefaultExtension)
            {
                if (Extensions.ContainsKey(typeof(T)))
                {
                    Extension = Extensions[typeof(T)];
                }
            }

            if (!Directory.Exists($"{BaseWritePath}\\{Folder}\\"))
                Directory.CreateDirectory($"{BaseWritePath}\\{Folder}\\");

            try
            {
                T Value = JsonUtility.FromJson<T>(File.ReadAllText($"{BaseWritePath}\\{Folder}\\{FileName}{Extension}"));
                OnComplete(Value);
                return Value;
            }
            catch (UnityException E)
            {
                Debug.Log(E);
            }

            return default;
        }

        public static string FormatFileLength(long Input)
        {
            if (Input >= 1000000000000)
                return $"{Input / 1000000000000} TB";
            else if (Input >= 1000000000)
                return $"{Input / 1000000000} GB";
            else if (Input >= 1000000)
                return $"{Input / 1000000} MB";
            else if (Input >= 1000)
                return $"{Input / 1000} KB";
            else
                return $"{Input} B";
        }

        public static FileInfo GetFile<T>(string Folder, string FileName, string Extension = DefaultExtension)
        {
            if (Extension == DefaultExtension)
            {
                if (Extensions.ContainsKey(typeof(T)))
                {
                    Extension = Extensions[typeof(T)];
                }
            }

            if (File.Exists($"{BaseWritePath}\\{Folder}\\{FileName}{Extension}"))
                return new FileInfo($"{BaseWritePath}\\{Folder}\\{FileName}{Extension}");

            return null;
        }
        
        public static FileInfo[] GetAllFiles(string Folder)
        {
            Directory.CreateDirectory($"{BaseWritePath}\\{Folder}\\");

            return new DirectoryInfo($"{BaseWritePath}\\{Folder}\\").GetFiles();
        }
    }
}