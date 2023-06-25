using SkySoft.Discord;
using SkySoft.Steam;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SkySoft
{
    public class SplashManager : MonoBehaviour
    {
        [SerializeField, Combo("Allow Skipping", "Disallow Skipping", Label = "Skipping")]
        private bool CanLoadNextScene = false;

        [SceneReference] public string TargetScene;

        private void StartDiscord()
        {
            try
            {
                Discord.DiscordAPI.Initialize();
            }
            catch (System.Exception E) 
            { 
                Debug.LogError(E);
#if UNITY_EDITOR 
                UnityEditor.EditorApplication.isPaused = true;
#endif 
                }
            }

        public void InitialiseServices()
        {
            if (!Steamhook.Initialized)
            {
                Steamhook.Initialize(null, null, null, StartDiscord);
            }
        }

        public void AllowInput()
        {
            SetCanLoadNextScene(true);
        }

        public void BlockInput()
        {
            SetCanLoadNextScene(false);
        }

        public void SetCanLoadNextScene(bool Allow)
        {
            CanLoadNextScene = Allow;
        }

        public void StartGame()
        {
            SceneManager.LoadScene(SkyEngine.Levels.GetSceneName(TargetScene));
            CanLoadNextScene = false;
        }

        private void Update()
        {
            if (CanLoadNextScene)
            {
                if (SkyEngine.Input.Menus.Menu.WasPressedThisFrame())
                {
                    StartGame();
                }
            }
        }
    }
}
