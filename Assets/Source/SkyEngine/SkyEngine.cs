using SkySoft.Inventory;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using SkySoft.Interaction;
using SkySoft.IO;
using System.IO;
using UnityEngine.InputSystem;
using Steamworks;
using UnityEngine.Internal;
using SkySoft.Entities;
using SkySoft.Generated;
using SkySoft.Objects;
using UnityEngine.Rendering.Universal;
using System.Linq;

namespace SkySoft.Interaction
{
    public enum InteractionType
    {
        Talk,
        Use,
        Inspect,
        Shop
    }
}

namespace SkySoft
{
    public enum E_CharacterBackground
    {
        Ranger,
        Archer,
        Rogue,
        Agent,
        Knight
    }

    public enum E_CharacterBirthplace
    {
        Namsy,
        Rhonda,
        Crestia,
        Forengard,
        Jorvitch
    }

    [Serializable]
    public class PlayerFile
    {
        public float SaveAge;
        [Space]
        public static bool NewCharacter = false;
        public Currencies Currency;
        public ItemStack[] Items;
        public float TimeOfDay = 0.015f;
        public string Head,
            Shoulders,
            Chest,
            Back,
            Hands,
            Legs,
            Feet,
            Weapon,
            Accessory;
        public string CurrentArea;
        public E_CharacterBackground Background;
        public E_CharacterBirthplace Birthplace;
        public SceneDefinitionFile ActiveScene;
        public int Level;
        public E_Race Race;
        public EyeColour EyeColour;
        public EyeColour SecondEyeColour;
        public float SkinTone;
        public float HairColour;
        public EntityProperties Properties = new EntityProperties();
        public int[] Followers;
    }

    public static class SkyEngine
    {
        public static LocalisationFile ActiveLocalisation { get; private set; }
        private static Dictionary<string, string> m_CommonTexts = new Dictionary<string, string>();
        public static Dictionary<string, string> CommonTexts 
        {  
            get
            {
                if (ActiveLocalisation == null || ActiveLocalisation.Strings.Count == 0)
                {
                    InitializeLanguages();
                    UpdateLocalisation();
                }

                return m_CommonTexts;
            }
        }

        public static void SetLanguage(int Language)
        {
            SetLanguage(Localisation.Languages[Language]);
        }

        public static LocalisationFile[] Languages;
        private static bool HasLoadedLanguages;

        private static void InitializeLanguages()
        {
            if (!HasLoadedLanguages)
            {
                List<LocalisationFile> Languages = new List<LocalisationFile>();

                foreach (LocalisationFile Language in Resources.LoadAll<LocalisationFile>("SkyEngine/Localisations/"))
                {
                    Languages.Add(Language);
                }

                SkyEngine.Languages = Languages.ToArray();
                HasLoadedLanguages = true;
            }
        }

        public static void SetLanguage(string Language)
        {
            InitializeLanguages();

            foreach (LocalisationFile Localisation in Languages)
            {
                if (Localisation.Language.ToLower() == Language.ToLower())
                {
                    PlayerPrefs.SetString("Language", Language.ToLower());
                    UpdateLocalisation();
                    break;
                }
            }
        }

        public static void UpdateLocalisation(Action Complete = null)
        {
            InitializeLanguages();
            
            string Lang = Localisation.English;

            if (!Localisation.Languages.ToList().Contains(PlayerPrefs.GetString("Language", Lang)))
                Lang = Localisation.English;

            string Language = PlayerPrefs.GetString("Language", Lang);

#if UNITY_EDITOR // This fixes the editor showing the wrong language. I might end-up implementing translations for SkyEngine but for now that's not required
            if (!Application.isPlaying)
                Language = Localisation.English;
#endif

            if (!Localisation.CustomLanguages.Contains(Language))
            {
                foreach (LocalisationFile Localisation in Languages)
                {
                    if (Localisation.Language.ToLower() == Language.ToLower())
                    {
                        ActiveLocalisation = Localisation;
                        m_CommonTexts = Localisation.CommonTexts;
                        break;
                    }
                }
            }
            else
            {
                try
                {
                    LocalisationFile File = LocalisationFile.ReadFromDisc(Language);
                    ActiveLocalisation = File;
                    m_CommonTexts = File.CommonTexts;
                }
                catch
                {
                    PlayerPrefs.SetString("Language", Localisation.English);
                    ConfigManager.SetOption("LanguageInt", 0, "System");
                    ActiveLocalisation = Languages[0];
                    m_CommonTexts = Languages[0].CommonTexts;
                }
            }

            if (Complete != null)
                Complete();
        }

        public static LocalisationFile GetLocalisation(string Language)
        {
            foreach (LocalisationFile Localisation in Resources.FindObjectsOfTypeAll<LocalisationFile>())
            {
                if (Localisation.Language.ToLower() == Language.ToLower())
                    return Localisation;
            }

            return null;
        }

        public static bool NextWriteAsTmp;
        public static PlayerFile LoadedFile;

        public static Action<EquipmentManager> OnEquipmentUpdated = new Action<EquipmentManager>(Value => { });

        public static bool InDialogue;

        public static class Gizmos
        {
            public static Color Colour { get => UnityEngine.Gizmos.color; set => UnityEngine.Gizmos.color = value; }
            public static Matrix4x4 Matrix { get => UnityEngine.Gizmos.matrix; set => UnityEngine.Gizmos.matrix = value; }
            
            public static void DrawLine(Vector3 From, Vector3 To) => UnityEngine.Gizmos.DrawLine(From, To);
            public static void DrawWireSphere(Vector3 Centre, float Radius) => UnityEngine.Gizmos.DrawWireSphere(Centre, Radius);
            public static void DrawSphere(Vector3 Centre, float Radius) => UnityEngine.Gizmos.DrawSphere(Centre, Radius);
            public static void DrawWireCube(Vector3 Centre, Vector3 Size) => UnityEngine.Gizmos.DrawWireCube(Centre, Size);
            public static void DrawCube(Vector3 Centre, Vector3 Size) => UnityEngine.Gizmos.DrawCube(Centre, Size);
            public static void DrawWireMesh(Mesh Mesh, [DefaultValue("Vector3.zero")] Vector3 Position, [DefaultValue("Quaternion.identity")] Quaternion Rotation, [DefaultValue("Vector3.one")] Vector3 Scale) => UnityEngine.Gizmos.DrawWireMesh(Mesh, Position, Rotation, Scale);
            public static void DrawMesh(Mesh Mesh, [DefaultValue("Vector3.zero")] Vector3 Position, [DefaultValue("Quaternion.identity")] Quaternion Rotation, [DefaultValue("Vector3.one")] Vector3 Scale) => UnityEngine.Gizmos.DrawMesh(Mesh, Position, Rotation, Scale);

            private static Mesh ArrowGizmos;

            public static void DrawArrow(Vector3 Position, Quaternion Rotation, Vector3 Scale)
            {
                if (!ArrowGizmos)
                    ArrowGizmos = Resources.Load<Mesh>("SkyEngine/Models/Gizmos/Camera_Facing");

                if (ArrowGizmos)
                {
                    DrawMesh(ArrowGizmos, Position, Rotation, Scale);
                }
            }

            public static void DrawArrow()
            {
                DrawArrow(Vector3.zero, Quaternion.identity, Vector3.one);
            }

            public static void DrawArrow(Vector3 Position)
            {
                DrawArrow(Position, Quaternion.identity, Vector3.one);
            }

            public static void DrawArrow(Vector3 Position, Quaternion Rotation)
            {
                DrawArrow(Position, Rotation, Vector3.one);
            }
            public static void DrawCircle(Vector3 Position, float Radius)
            {
                float Theta = 0f;
                float X = Radius * Mathf.Cos(Theta);
                float Y = Radius * Mathf.Sin(Theta);

                Vector3 P = Position + new Vector3(X, Y, 0);

                for (int I = 0; I < 360; I++)
                {
                    Theta = I * Mathf.PI / 180f;
                    X = Radius * Mathf.Cos(Theta);
                    Y = Radius * Mathf.Sin(Theta);
                    Vector3 NewP = Position + new Vector3(X, Y, 0);
                    Gizmos.DrawLine(P, NewP);
                    P = NewP;
                }
            }
        }

        public static int ActiveSaveIndex
        {
            get => ConfigManager.GetOption("ActiveSaveIndex", 0, "System");
            set => ConfigManager.SetOption("ActiveSaveIndex", value, "System");
        }

        public static bool PlayerOwnsThisGame => SteamApps.IsSubscribed;
        public static bool RunInDemoMode => SteamApps.AppOwner != SteamClient.SteamId;
        public static bool IsPiratedCopy => !Application.genuine;

        public static bool LoadPositions = false;
        public static Entities.Entity PlayerEntity;
        public static bool IsOnline => Steam.Steamhook.LobbyMemberCount > 0;
        private static SkyEngineProperties m_Properties;
        public static SkyEngineProperties Properties => m_Properties == null ? (m_Properties = Resources.Load<SkyEngineProperties>("SkyEngine")) : m_Properties;
        private static Audio.BGM m_BGM;
        public static Audio.BGM BGM => m_BGM == null ? (m_BGM = Resources.Load<Audio.BGM>("BGM")) : m_BGM;
        private static LevelManagement.LevelManagement m_Levels;
        public static LevelManagement.LevelManagement Levels => m_Levels == null ? (m_Levels = Resources.Load<LevelManagement.LevelManagement>("Levels")) : m_Levels;

        public const string ItemCountText = "<COUNT>x";
        [SerializeReference] private static Dictionary<string, Item> m_Items;

        /// <summary>
        /// Gets whether the Player owns the chosen DLC
        /// </summary>
        /// <param name="DLC"></param>
        /// <returns></returns>
        public static bool OwnsDLC(string DLC)
        {
            if (Properties.DLCs.ContainsKey(DLC))
            {
                return Properties.DLCs[DLC].IsOwned;
            }

            return false;
        }

        public static Dictionary<string, Item> Items => Properties.Items;

        public const int EquipmentStackSize = 16;
        public const int GeneralStackSize = 128;

        /// <summary>
        /// With Projectile Weapons, Speed determines the ReloadSpeed of the weapon. Every other weapon Speed determines their SwingSpeed
        /// </summary>
        public static Dictionary<WeaponType, float> WeaponSpeeds => new Dictionary<WeaponType, float>
        {
            { WeaponType.BattleAxe, 1 },
            { WeaponType.Bow, 0.9f },
            { WeaponType.Crossbow, 1.5f },
            { WeaponType.Dagger, 0.3f },
            { WeaponType.GreatBlade, 1.2f },
            { WeaponType.HandAxe, 0.8f },
            { WeaponType.LargeFirearm, 3.6f },
            { WeaponType.LongBlade, 1.15f },
            { WeaponType.Rapier, 0.65f },
            { WeaponType.ShortBlade, 0.45f },
            { WeaponType.SmallFirearm, 2 },
            { WeaponType.Spear, 1.1f },
            { WeaponType.ThrowingWeapon, 0.35f }
        };

        public static bool IsHandSlot(EquipmentSlot Slot)
        {
            return Slot == EquipmentSlot.RightHand || Slot == EquipmentSlot.LeftHand;
        }

        private static Input.Input m_Input;
        private static bool m_InputSet = false;
        public static Input.Input Input
        {
            get
            {
                if (!m_InputSet)
                {
                    m_Input = new Input.Input();
                    m_Input.Enable();
                    m_InputSet = true;
                }

                return m_Input;
            }
        }

        [Serializable]
        internal class RumbleClass
        {
            public float LowFrequency, HighFrequency;
            public float Duration;
            private float StartTime;
            public bool ShouldEnd => Time.time - StartTime > Duration;

            public RumbleClass(float LowFrequency, float HighFrequency, float Duration)
            {
                this.LowFrequency = LowFrequency;
                this.HighFrequency = HighFrequency;
                this.Duration = Duration;
                StartTime = Time.time;
            }
        }

        private static Dictionary<string, RumbleClass> Rumblers = new Dictionary<string, RumbleClass>();
        
        /// <summary>
        /// Starts a rumble
        /// </summary>
        /// <param name="LowFrequency"></param>
        /// <param name="HighFrequency"></param>
        /// <param name="ID"></param>
        /// <returns>Returns the ID used for the rumble in case one was generated</returns>
        public static string StartRumble(float LowFrequency, float HighFrequency, string ID = "", float Duration = Mathf.Infinity)
        {
            if (string.IsNullOrEmpty(ID))
            {
                ID = Guid.NewGuid().ToString();
            }

            Rumblers.Add(ID, new RumbleClass(LowFrequency, HighFrequency, Duration));

            return ID;
        }

        public static void StopRumble(string ID)
        {
            if (Rumblers.ContainsKey(ID))
            {
                Rumblers.Remove(ID);
            }
        }

        public static void Rumble(float LowFrequency, float HighFrequency, float Duration = 0.25f)
        {
            StartRumble(LowFrequency, HighFrequency, "", Duration);
        }

        public static void RunRumbleTick()
        {
            float LowFrequency = 0, HighFrequency = 0;

            List<string> DeathList = new List<string>();
            
            foreach (string Key in Rumblers.Keys)
            {
                if (Rumblers[Key].ShouldEnd)
                    DeathList.Add(Key);
                else if (Rumblers[Key].LowFrequency > LowFrequency && Rumblers[Key].HighFrequency > HighFrequency)
                {
                    LowFrequency = Rumblers[Key].LowFrequency;
                    HighFrequency = Rumblers[Key].HighFrequency;
                }
            }

            foreach (string Key in DeathList)
            {
                Rumblers.Remove(Key);
            }

            if (Gamepad.current != null)
            {
                Gamepad.current.SetMotorSpeeds(LowFrequency, HighFrequency);
            }
        }

        private static Vector2 m_Resoultion = new Vector2(1920, 1080);
        public static Vector2 Resolution
        {
            get
            {
                return m_Resoultion;
            }
            set
            {
                int ClosestIndex = -1;
                float ClosestPoint = 1000000000;
                for (int I = 0; I < Screen.resolutions.Length; I++)
                {
                    Resolution R = Screen.resolutions[I];
                    if (Vector2.Distance(value, new Vector2(R.width, R.height)) < ClosestPoint)
                    {
                        ClosestIndex = I;
                        ClosestPoint = Vector2.Distance(value, new Vector2(R.width, R.height));
                    }
                }
                if (ClosestIndex != -1)
                {
                    Resolution R = Screen.resolutions[ClosestIndex];
                    Screen.SetResolution(R.width, R.height, true);

                    m_Resoultion = value;
                }
            }
        }

        public static string ToNumeral(int number)
        {
            if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("insert value betwheen 1 and 3999");
            if (number < 1) return "";
            if (number >= 1000) return "M" + ToNumeral(number - 1000);
            if (number >= 900) return "CM" + ToNumeral(number - 900);
            if (number >= 500) return "D" + ToNumeral(number - 500);
            if (number >= 400) return "CD" + ToNumeral(number - 400);
            if (number >= 100) return "C" + ToNumeral(number - 100);
            if (number >= 90) return "XC" + ToNumeral(number - 90);
            if (number >= 50) return "L" + ToNumeral(number - 50);
            if (number >= 40) return "XL" + ToNumeral(number - 40);
            if (number >= 10) return "X" + ToNumeral(number - 10);
            if (number >= 9) return "IX" + ToNumeral(number - 9);
            if (number >= 5) return "V" + ToNumeral(number - 5);
            if (number >= 4) return "IV" + ToNumeral(number - 4);
            if (number >= 1) return "I" + ToNumeral(number - 1);
            throw new UnityException("Impossible state reached");
        }

        public static List<EventDataFile> DirtyEvents = new List<EventDataFile>();

        public static void SaveGame(Action OnComplete, Action<float> OnProgressTick = null)
        {
            SaveGame(OnComplete, 0, true, OnProgressTick);
        }

        public static async void SaveGame(Action OnComplete, int DestinationSlot, bool PS2_Speed = true, Action<float> OnProgressTick = null)
        {
            Objects.SerializedObject[] DirtyObjects = UnityEngine.Object.FindObjectsOfType<Objects.SerializedObject>();
            FileInfo[] DirtyLoot = FileManager.GetAllFiles("Loot");
            int TotalFiles = DirtyEvents.Count + DirtyObjects.Length + DirtyLoot.Length;
            int CurrentFile = 0;

            foreach (EventDataFile File in DirtyEvents)
            {
                string Txt = FileManager.WriteFile<EventDataFile>("Events", File.EventName, File);
                if (OnProgressTick != null)
                {
                    OnProgressTick(Mathf.Lerp(0, 0.9f, CurrentFile / (float)TotalFiles));
                }
                CurrentFile++;
                await Task.Delay(PS2_Speed ? 40 : 10);
            }
            DirtyEvents.Clear();

            foreach (Objects.SerializedObject Obj in DirtyObjects)
            {
                string Txt = Obj.Serialize();
                if (OnProgressTick != null)
                {
                    OnProgressTick(Mathf.Lerp(0, 0.9f, CurrentFile / (float)TotalFiles));
                }
                CurrentFile++;
                await Task.Delay(PS2_Speed ? 40 : 10);
            }

            foreach (FileInfo Inf in DirtyLoot)
            {
                string Txt = "";
                FileManager.ReadFile<LootBoxSave>("Loot", Inf.Name.Replace(".loot", "").Replace("loot", ""), Save =>
                {
                    Save.Temp = false;
                    Txt = FileManager.WriteFile("Loot", Inf.Name.Replace(".loot", "").Replace("loot", ""), Save, Success => { }, ".loot");
                }, ".loot");
                if (OnProgressTick != null)
                {
                    OnProgressTick(Mathf.Lerp(0, 0.9f, CurrentFile / (float)TotalFiles));
                }
                CurrentFile++;
                await Task.Delay(PS2_Speed ? 40 : 10);
            }

            FileManager.ArchiveSave(DestinationSlot, 0.9f, Value => 
            { 
                if (OnProgressTick != null)
                {
                    OnProgressTick(Value);
                }
            });

            while (FileManager.Archiving)
                await Task.Delay(10);

            OnProgressTick(100);

            OnComplete();
        }

        public static void SetRichPresence(string MajorText, string MinorText, bool UseFile = false, PlayerFile File = null)
        {
            try
            {
                Discord.DiscordAPI.UpdateActivity(MinorText, MajorText, true, UseFile ? $"{File.Properties.Name}, Level {File.Level} {File.Race}" : "");
                Steam.Steamhook.SetRichPresence($"{MajorText}{(string.IsNullOrEmpty(MinorText) ? "" : $" - {MinorText}")}");
            } catch (System.Exception Ex) { Debug.LogError(Ex); }
        }

        public static void SpawnLootBag(Vector3 Position, int Currency = 0, params ItemStack[] Items)
        {
            LootBox Bag = UnityEngine.Object.Instantiate(Properties.LootBag.gameObject, Position, Quaternion.identity).GetComponent<LootBox>();
            Bag.Items.AddRange(Items);
            Bag.Currency += Currency;
        }

        private static bool InitializedThisSession = false;

        /// <summary>
        /// This is called before SkyEngine initializes itself
        /// </summary>
        public static Action OnPreInitialize = new Action(() => { });
        /// <summary>
        /// This is called after SkyEngine initializes itself
        /// </summary>
        public static Action OnPostInitialize = new Action(() => { });

        public static void Initialize()
        {
            if (!InitializedThisSession)
            {
                OnPreInitialize();
                UpdateLocalisation(); // Setup the localisation so that the Press START prompt is in the local language

                // Looks messy, just loads the user-preferences right away
                Camera.main.GetComponent<UniversalAdditionalCameraData>().renderPostProcessing = ConfigManager.GetOption("PostEffects", 1, "Graphics") == 1;

                if (QualitySettings.GetQualityLevel() != ConfigManager.GetOption("Quality", 5, "Graphics"))
                {
                    QualitySettings.SetQualityLevel(ConfigManager.GetOption("Quality", 5, "Graphics"), true);
                }

                OnPostInitialize();

                InitializedThisSession = true;
            }
        }

        public static string CreateID()
        {
            return Guid.NewGuid().ToString();
        }
    }
}