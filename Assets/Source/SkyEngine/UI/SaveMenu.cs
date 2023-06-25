using SkySoft;
using SkySoft.Events.Graph;
using SkySoft.IO;
using SkySoft.LevelManagement;
using SkySoft.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SaveMenu : MonoBehaviour
{
    public static Action OnMenuClosed;

    public static SaveMenu Instance;
    private static GameObject InstanceRoot;
    public static bool IsDoingOperation { get; private set; }

    public int Page;

    [DisplayOnly, SerializeField] private int m_Selected = 0;

    [Combo("Load", "Save", Label = "Function")]
    public bool LoadGame;
    public CommandMenu TargetMenu;
    [Space]
    public GameObject WidgetPrefab;
    [Space]
    public GameObject ProgressMenuBase;
    public Text ProgressStatus;
    public StatusBar ProgressBar;

    private static Dictionary<CommandMenu, bool> MenuStates = new Dictionary<CommandMenu, bool>();

    public static void SummonMenu(GameObject MenuPrefab, Action OnMenuClosed = null, bool LoadMode = false)
    {
        MenuStates.Clear();

        foreach (CommandMenu Menu in FindObjectsOfType<CommandMenu>(true))
        {
            MenuStates.Add(Menu, Menu.enabled);
            Menu.enabled = false;
        }

        InstanceRoot = Instantiate(MenuPrefab, Vector3.zero, Quaternion.identity);
        Instance = InstanceRoot.GetComponentInChildren<SaveMenu>();
        Instance.LoadGame = LoadMode;
        SaveMenu.OnMenuClosed = OnMenuClosed == null ? null : OnMenuClosed;
        Instance.Init();
    }

    private static void ExitMenu()
    {
        foreach (CommandMenu Menu in MenuStates.Keys)
        {
            Menu.enabled = MenuStates[Menu];
        }

        if (OnMenuClosed != null)
            OnMenuClosed();

        Destroy(InstanceRoot);
    }

    public void DestroyMenu()
    {
        ExitMenu();
    }

    public static void Close()
    {
        IsBusy = false;
        Instance.GetComponent<Animator>().Play("Close");
    }

    public void Init()
    {
        Page = 0;
        Refresh();
    }

    public static bool IsBusy { get; private set; }

    private void Refresh()
    {
        TargetMenu.enabled = false;

        TargetMenu.NextBlocked.RemoveAllListeners();
        TargetMenu.PreviousBlocked.RemoveAllListeners();

        /*
        TargetMenu.NextBlocked.AddListener(() =>
        {
            if (Page + 3 < FileManager.MaxSaves)
            {
                Page++;
                TargetMenu.SelectedIndex = 3;
            }

            Refresh();
        });
        TargetMenu.PreviousBlocked.AddListener(() =>
        {
            if (Page - 1 >= 0)
            {
                Page--;
                TargetMenu.SelectedIndex = 0;
            }

            Refresh();
        });
        */

        foreach (Transform T in TargetMenu.ItemsRoot)
        {
            Destroy(T.gameObject);
        }

        for (int I = 0; I < 4; I++)
        {
            FileWidget FileButton = Instantiate(WidgetPrefab, TargetMenu.ItemsRoot).GetComponentInChildren<FileWidget>();
            FileButton.Target = Page + I;
            FileButton.m_Event.AddListener(() =>
            {
                OnFileClicked(FileButton.Target);
            });
            FileButton.Refresh();
        }

        TargetMenu.enabled = true;
    }

    public async void OnFileClicked(int Index)
    {
        if (IsBusy)
            return;

        IsBusy = true;
        TargetMenu.enabled = false;

        bool SaveComplete = false;

        if (LoadGame)
        {
            ProgressStatus.text = $"{SkyEngine.CommonTexts["save.status_loading"]}\n{SkyEngine.CommonTexts["save.status_warning"]}";
            ProgressBar.Value = 0;
            ProgressMenuBase.SetActive(true);

            LoadTrigger.ComingFrom = "";

            FileManager.GetSave(Index, async () =>
            {
                while (!FileManager.FileExists<PlayerFile>("Entities", $"_Player_Local", ".entity"))
                    await Task.Delay(10);

                FileManager.ReadFile<PlayerFile>("Entities", $"_Player_Local", Plr =>
                {
                    SkyEngine.LoadPositions = true;

                    if (!string.IsNullOrEmpty(Plr.CurrentArea))
                    {
                        LevelManager.LoadLevel(Plr.CurrentArea, FadeColour.White, () =>
                        {
                            Close();
                        }, Plr);
                    }
                    else
                    {
                        LevelManager.LoadLevel("Introduction", FadeColour.White, () =>
                        {
                            Close();
                        }, Plr);
                    }


                }, ".entity");
            });
        }
        else
        {
            SkyEngine.SaveGame(() => 
            {
                SaveComplete = true;
                Refresh();
            }, Index, true, Value =>
            {
                ProgressStatus.text = $"{SkyEngine.CommonTexts["save.status_saving"]}\n{SkyEngine.CommonTexts["save.status_warning"]}";
                ProgressBar.Value = Value;
                ProgressMenuBase.SetActive(true);
            });
        }

        while (!SaveComplete)
            await Task.Delay(10);

        ProgressStatus.text = SkyEngine.CommonTexts["save.done"];

        while (!SkyEngine.Input.Menus.Confirm.IsPressed())
            await Task.Delay(10);

        Close();
    }

    private void Update()
    {
        if (SkyEngine.Input.Menus.Cancel.WasPressedThisFrame())
        {
            Close();
        }
    }
}
