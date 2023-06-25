using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using SkySoft.LevelManagement;
using SkySoft.Steam;
using SkySoft.Discord;
using SkySoft.IO;
using System.IO;

namespace SkySoft.UI
{
    [AddComponentMenu("SkyEngine/UI/Helpers/Title Function")]
    public class TitleFunctions : MonoBehaviour
    {
        public Text Logo;
        public Text PressStartWidget;
        private bool PressedStart;
        private bool WasFailure;

        public UnityEvent OnStartPressed;

        public AudioSource StartPressedSound;
        public AudioSource TransitionSound;

        public Button ContinueButton;

        private void Awake()
        {
            AudioListener.pause = false;
        }

        public void ResetMenu()
        {
            PressedStart = false;
        }

        private void Update()
        {
            SkyEngine.LoadPositions = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            try
            {
                Logo.text = SkyEngine.ActiveLocalisation.GameName;
            } catch { }

            if (!PressedStart)
            {                
                if (SkyEngine.Input.Menus.Menu.WasPressedThisFrame())
                {
                    StartPressedSound.Play();

                    OnStartPressed.Invoke();
                    PressedStart = true;
                }
            }

            if (Steamhook.HasSteam && !SkyEngine.RunInDemoMode) // We don't want players loading saves on a demo version of Erinheim
            {
                /*
                ContinueButton.GetComponentInChildren<Text>().text = $"Continue <size=12><color=grey>( Slot {SkyEngine.ActiveSaveIndex.ToString("00")} )</color></size>";
                ContinueButton.interactable = FileManager.GetSaveCount > SkyEngine.ActiveSaveIndex;
                */
            }
        }

        [SceneReference]
        public string FirstLevel = "Introduction";

        public void StartGame()
        {
            LoadTrigger.ComingFrom = "";

            FileManager.ReadFile<PlayerFile>("Entities", $"_Player_Local", Plr => 
            {
                SkyEngine.LoadPositions = true;

                if (!string.IsNullOrEmpty(Plr.CurrentArea))
                {
                    LevelManager.LoadLevel(Plr.CurrentArea, FadeColour.White, () => { }, Plr);
                }
                else
                {
                    LevelManager.LoadLevel(FirstLevel, FadeColour.White, () => { }, Plr);
                }
            }, ".entity");
        }

        public void ContinueGame()
        {
            LoadTrigger.ComingFrom = "";

            FileManager.GetSave(SkyEngine.ActiveSaveIndex);

            FileManager.ReadFile<PlayerFile>("Entities", $"_Player_Local", Plr =>
            {
                SkyEngine.LoadPositions = true;

                if (!string.IsNullOrEmpty(Plr.CurrentArea))
                {
                    LevelManager.LoadLevel(Plr.CurrentArea, FadeColour.White, () => { }, Plr);
                }
                else
                {
                    LevelManager.LoadLevel("Introduction", FadeColour.White, () => { }, Plr);
                }
            }, ".entity");
        }

        public void LoadGame()
        {
            SaveMenu.SummonMenu(SkyEngine.Properties.SaveMenu, () => { }, true);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        } 
    }
}