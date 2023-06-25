using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Discord;
using SkySoft.Steam;
using SkySoft.Audio;
using UnityEngine.Rendering.Universal;
using SkySoft.IO;

namespace SkySoft.Discord
{
    [AddComponentMenu("SkyEngine/Discord API/Discord Hook")]
    public class DiscordAPI : MonoBehaviour
    {
        public static DiscordAPI Instance { get; private set; }
        private const long ClientID = 1116935427203547177;

        private static global::Discord.Discord Discord;
        private static ActivityManager ActivityManager;

        public static bool Initialized { get; private set; }

        private static void Dispose()
        {
            try
            {
                if (HasDiscord && Discord != null)
                {
                    Discord.Dispose();
                }
            }
            catch
            {

            }
        }

        private void OnDestroy()
        {
            Dispose();
        }

        private static bool HasDiscord;

        public static void Initialize()
        {
            if (!Initialized)
            {
                try
                {
                    Discord = new global::Discord.Discord(ClientID, (ulong)CreateFlags.Default);

                    ActivityManager = Discord.GetActivityManager();

                    ActivityManager.RegisterSteam(Steamhook.AppID);

                    HasDiscord = true;
                }
                catch (System.Exception Ex) 
                {
                    Debug.LogError(Ex);

#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPaused = true;
#endif

                    HasDiscord = false;
                }

                GameObject Obj = new GameObject("[ DISCORD ]");
                DontDestroyOnLoad(Obj);
                Instance = Obj.AddComponent<DiscordAPI>();

                Initialized = true;

                SkyEngine.SetRichPresence("Gearing Up", "");
            }
        }

        public static void UpdateActivity(string State, string Details, bool SkipInit = false, string PlayerDetails = "")
        {
            if (!SkipInit)
            {
                Initialize();
            }

            if (HasDiscord)
            {
                Activity NewActivity = new Activity
                {
                    State = State,
                    Details = Details,
                    Assets =
                {
                    LargeImage = "iconlarge",
                    LargeText = PlayerDetails
                }
                };

                try
                {
                    ActivityManager.UpdateActivity(NewActivity, Result =>
                    {
                        Debug.Log($"UpdateActivity() returned with result: {Result}");
                    });
                }
                catch { }
            }
        }

        private void Update()
        {
            if (HasDiscord)
                Discord.RunCallbacks();

            SkyEngine.RunRumbleTick();
            Camera.main.GetComponent<UniversalAdditionalCameraData>().renderPostProcessing = ConfigManager.GetOption("PostEffects", 1, "Graphics") == 1;
            
            if (QualitySettings.GetQualityLevel() != ConfigManager.GetOption("Quality", 5, "Graphics"))
            {
                QualitySettings.SetQualityLevel(ConfigManager.GetOption("Quality", 5, "Graphics"), true);
            }
        }
    }
}