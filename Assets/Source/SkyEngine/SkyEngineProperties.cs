using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using SkySoft.Inventory;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using SkySoft.Interaction;
using SkySoft;
using Steamworks;
using SkySoft.IO;
using JetBrains.Annotations;

[System.Serializable]
public class DLCObject
{
    [Tooltip("This is used in-code as a lookup")]
    public string ID;
    [Tooltip("This is used by Steam to identify the DLC")]
    public int SteamID;
    public string Name;
    public bool IsOwned
    {
        get
        {
            AppId ID = new AppId { Value = (uint)SteamID };
            return SteamApps.IsDlcInstalled(ID);
        }
    }
}

public enum GamepadButton
{
    None, LeftStick, LeftStickDown, RightStick, RightStickDown, LeftBumper, RightBumper, LeftTrigger, RightTrigger, DPadUp, DPadLeft, DPadDown, DPadRight, North, East, South, West,
    DPad, Start, Select
}

[System.Serializable]
public class SkyEngineGlyphPack
{
    public string Label;
    public Steamworks.InputType InputType = Steamworks.InputType.Unknown;
    public Sprite LeftStick;
    public Sprite LeftStickDown, RightStick, RightStickDown, LeftBumper, RightBumper, LeftTrigger, RightTrigger, DPad, DPadUp, DPadLeft, DPadDown, DPadRight, North, East, South, West, Start, Select;

    public void ResetGlyphs()
    {
        if (InstancedButtonGlyphs != null)
            InstancedButtonGlyphs.Clear();
    }

    private Dictionary<GamepadButton, Sprite> InstancedButtonGlyphs;
    public Dictionary<GamepadButton, Sprite> Buttons
    {
        get
        {
            if (InstancedButtonGlyphs == null || InstancedButtonGlyphs.Count == 0)
            {
                InstancedButtonGlyphs = new Dictionary<GamepadButton, Sprite>
                {
                    { GamepadButton.LeftStick, LeftStick },
                    { GamepadButton.LeftStickDown, LeftStickDown != null ? LeftStickDown : LeftStick },
                    { GamepadButton.RightStick, RightStick },
                    { GamepadButton.RightStickDown, RightStickDown != null ? RightStickDown : RightStick },
                    { GamepadButton.LeftBumper, LeftBumper },
                    { GamepadButton.RightBumper, RightBumper },
                    { GamepadButton.LeftTrigger, LeftTrigger },
                    { GamepadButton.RightTrigger, RightTrigger },
                    { GamepadButton.DPad, DPad },
                    { GamepadButton.DPadUp, DPadUp },
                    { GamepadButton.DPadLeft, DPadLeft },
                    { GamepadButton.DPadDown, DPadDown },
                    { GamepadButton.DPadRight, DPadRight },
                    { GamepadButton.North, North },
                    { GamepadButton.East, East },
                    { GamepadButton.South, South },
                    { GamepadButton.West, West },
                    { GamepadButton.Start, Start },
                    { GamepadButton.Select, Select }
                };
            }

            return InstancedButtonGlyphs;
        }
    }
}

[System.Serializable]
public class DiscordProperties
{
    [SerializeField] private string m_ClientID;
    public long ClientID => long.Parse(m_ClientID);
}

[System.Serializable]
public class SteamProperties 
{
    public int AppID;
}

[CreateAssetMenu(fileName = "SkyEngine", menuName = "SkyEngine/Properties")]
public class SkyEngineProperties : ScriptableObject
{
    public GameObject SaveMenu;

    public string InitialStatus = "Logging In";
    public bool EnableSteam;
    public SteamProperties SteamProperties;
    public bool EnableDiscord;
    public DiscordProperties DiscordProperties;

    private Steamworks.InputType PreviousDefaultGlyphs;
    public Steamworks.InputType DefaultGlyphPack;

    [Header("Gamepad Glyphs")]
    public Sprite LeftStick; 
    public Sprite RightStick, LeftBumper, RightBumper, LeftTrigger, RightTrigger, DPad, DPadUp, DPadLeft, DPadDown, DPadRight, North, East, South, West;

    [SerializeField] private SkyEngineGlyphPack[] m_Gamepads;
    private Dictionary<Steamworks.InputType, SkyEngineGlyphPack> m_Glyph = new Dictionary<Steamworks.InputType, SkyEngineGlyphPack>();
    public Dictionary<Steamworks.InputType, SkyEngineGlyphPack> Gamepads
    {
        get
        {
            if (m_Glyph.Count < m_Gamepads.Length)
            {
                m_Glyph.Clear();

                foreach (SkyEngineGlyphPack Pack in m_Gamepads)
                {
                    if (!m_Glyph.ContainsKey(Pack.InputType))
                    {
                        m_Glyph.Add(Pack.InputType, Pack);
                    }
                }
            }

            return m_Glyph;
        }
    }

    public SkyEngineGlyphPack Glyphs
    {
        get
        {
            try
            {
                if (Steamworks.SteamInput.Controllers != null && Steamworks.SteamInput.Controllers.Count() > 0)
                {
                    if (Gamepads.ContainsKey(Steamworks.SteamInput.Controllers.ToArray()[0].InputType))
                    {
                        return Gamepads[Steamworks.SteamInput.Controllers.ToArray()[0].InputType];
                    }
                }
            } catch { }

            bool IsEditor = false;

#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                IsEditor = true;
            }
#endif

            return IsEditor ? Gamepads[DefaultGlyphPack] : Gamepads.Values.ToArray()[ConfigManager.GetOption("DefaultGlyphs", 6, "Controls")];
        }
    }

    private Dictionary<GamepadButton, Sprite> InstancedButtonGlyphs;
    public Dictionary<GamepadButton, Sprite> Buttons
    {
        get
        {
            if (InstancedButtonGlyphs == null || InstancedButtonGlyphs.Count == 0)
            {
                InstancedButtonGlyphs = new Dictionary<GamepadButton, Sprite>
                {
                    { GamepadButton.LeftStick, LeftStick },
                    { GamepadButton.RightStick, RightStick },
                    { GamepadButton.LeftBumper, LeftBumper },
                    { GamepadButton.RightBumper, RightBumper },
                    { GamepadButton.LeftTrigger, LeftTrigger },
                    { GamepadButton.RightTrigger, RightTrigger },
                    { GamepadButton.DPad, DPad },
                    { GamepadButton.DPadUp, DPadUp },
                    { GamepadButton.DPadLeft, DPadLeft },
                    { GamepadButton.DPadDown, DPadDown },
                    { GamepadButton.DPadRight, DPadRight },
                    { GamepadButton.North, North },
                    { GamepadButton.East, East },
                    { GamepadButton.South, South },
                    { GamepadButton.West, West }
                };
            }

            return InstancedButtonGlyphs;
        }
    }

    public GamepadButton GetBinding(string Key)
    {
        switch (Key.ToLower())
        {
            default:
                Debug.Log($"Binding \"{Key}\" does not exist!");
                return GamepadButton.None;
            case "move":
                return GamepadButton.LeftStick;
            case "look":
                return GamepadButton.RightStick;
            case "jump":
                return GamepadButton.East;
            case "reaction":
                return GamepadButton.North;
            case "guard":
                return GamepadButton.West;
            case "dodge":
                return GamepadButton.RightStickDown;
            case "lookaround":
                return GamepadButton.RightStick;
            case "lockon":
                return GamepadButton.LeftTrigger;
            case "navigatemenu":
                return GamepadButton.DPad;
            case "navigatemenu.up":
                return GamepadButton.DPadUp;
            case "navigatemenu.left":
                return GamepadButton.DPadLeft;
            case "navigatemenu.down":
                return GamepadButton.DPadDown;
            case "navigatemenu.right":
                return GamepadButton.DPadRight;
            case "confirm":
                return GamepadButton.South;
            case "cancel":
                return GamepadButton.North;
            case "tab.left":
                return GamepadButton.LeftBumper;
            case "tab.right":
                return GamepadButton.RightBumper;
            case "panview":
                return GamepadButton.RightStick;
            case "shortcuts":
                return GamepadButton.RightTrigger;
            case "start":
                return GamepadButton.Start;
        }
    }
    public float VibrationIntensity = 1;

    [Header("DLCs")]
    [SerializeField] private DLCObject[] m_DLCs;

    private Dictionary<string, DLCObject> InstancedDLCs;
    public Dictionary<string, DLCObject> DLCs
    {
        get
        {
            if (InstancedDLCs == null || InstancedDLCs.Count == 0)
            {
                InstancedDLCs = new Dictionary<string, DLCObject>();
                foreach (DLCObject Obj in m_DLCs)
                {
                    InstancedDLCs.Add(Obj.ID, Obj);
                }
            }

            return InstancedDLCs;
        }
    }

    [Header("Interaction")]
    public Sprite TalkIcon;
    public Sprite UseIcon, InspectIcon, ShopIcon;

    [Header("Items")]
    [Button ("Fetch all Items")]public bool QuickGetItems;
    public List<Item> AllItems = new List<Item> { };
    public List<Armour> AllArmours = new List<Armour> { };
    public List<Book> AllBooks = new List<Book> { };
    public List<Consumable> AllConsumables = new List<Consumable> { };
    public List<Equipment> AllEquipment = new List<Equipment> { };
    public List<Weapon> AllWeapons = new List<Weapon> { };

    private Dictionary<string, Item> ItemsCache;
    public Dictionary<string, Item> Items
    {
        get
        {
            if (ItemsCache == null || ItemsCache.Count == 0)
            {
                ItemsCache = new Dictionary<string, Item>();

                foreach (Item Item in AllItems)
                {
                    ItemsCache.Add(Item.ID, Item);
                }
                foreach (Book Item in AllBooks)
                {
                    ItemsCache.Add(Item.ID, Item);
                }
                foreach (Armour Item in AllArmours)
                {
                    ItemsCache.Add(Item.ID, Item);
                }
                foreach (Consumable Item in AllConsumables)
                {
                    ItemsCache.Add(Item.ID, Item);
                }
                foreach (Equipment Item in AllEquipment)
                {
                    ItemsCache.Add(Item.ID, Item);
                } 
                foreach (Weapon Item in AllWeapons)
                {
                    ItemsCache.Add(Item.ID, Item);
                }
            }

            return ItemsCache;
        }
    }

    [Header("Gizmos")]
    public Mesh HumanoidGizmos;
    public Vector3 PositionOffset;
    public Vector3 RotationOffset;
    public float Scale = 1;

    [Header("Other")]
    public LootBox LootBag;

    private void OnValidate()
    {
        List<Steamworks.InputType> RegisteredInputTypes = new List<Steamworks.InputType>();

        foreach (SkyEngineGlyphPack Pack in m_Gamepads)
        {
            Pack.ResetGlyphs();
            Pack.Label = Pack.InputType.ToString();

            RegisteredInputTypes.Add(Pack.InputType);
        }

        if (!RegisteredInputTypes.Contains(DefaultGlyphPack))
        {
            DefaultGlyphPack = PreviousDefaultGlyphs;
        }

        if (QuickGetItems)
        {
            if (!Directory.Exists("Assets\\Resources\\SkyEngine\\Items\\"))
                Directory.CreateDirectory("Assets\\Resources\\SkyEngine\\Items\\");

            AllItems = Resources.FindObjectsOfTypeAll<Item>().ToList();
            List<Item> ToRemove = new List<Item>();

            AllArmours.Clear();
            AllBooks.Clear();
            AllConsumables.Clear();
            AllEquipment.Clear();
            AllWeapons.Clear();

            foreach (Item I in AllItems)
            {
                if (I.GetType() == typeof(Armour))
                {
                    AllArmours.Add(I as Armour);
                    ToRemove.Add(I);
                }
                else if (I.GetType() == typeof(Book))
                {
                    AllBooks.Add(I as Book);
                    ToRemove.Add(I);
                }
                else if (I.GetType() == typeof(Consumable))
                {
                    AllConsumables.Add(I as Consumable);
                    ToRemove.Add(I);
                }
                else if (I.GetType() == typeof(Weapon))
                {
                    AllWeapons.Add(I as Weapon);
                    ToRemove.Add(I);
                }
                else if (I.GetType() == typeof(Equipment))
                {
                    AllEquipment.Add(I as Equipment);
                    ToRemove.Add(I);
                }
            }

            foreach (Item I in ToRemove)
            {
                AllItems.Remove(I);
            }

            QuickGetItems = false;
        }

        PreviousDefaultGlyphs = DefaultGlyphPack;
    }
}
