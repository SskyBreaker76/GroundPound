using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Discord;
using SkySoft.Steam;
using SkySoft.Audio;
using UnityEngine.Rendering.Universal;
using SkySoft.IO;
using System;
using UnityEngine.Networking;

namespace SkySoft.Discord
{
    [AddComponentMenu("SkyEngine/Discord API/Discord Hook")]
    public class DiscordAPI : MonoBehaviour
    {
        public static DiscordAPI Instance { get; private set; }

        private static global::Discord.Discord Discord;
        private static ActivityManager ActivityManager;
        public static UserManager UserManager;

        public static void GetAvatar(User User, Action<Texture2D> OnComplete, Action<string> OnFailed = null, int Resolution = 512)
        {
            Instance.StartCoroutine(Instance.DL_Image(User, OnFailed, OnComplete, Resolution));
        }

        private IEnumerator DL_Image(User User, Action<string> OnFailed = null, Action<Texture2D> OnComplete = null, int Resolution = 512)
        {
            string URL = $"https://cdn.discordapp.com/avatars/{User.Id}/{User.Avatar}.png?size={Resolution}";
            UnityWebRequest WWW = UnityWebRequestTexture.GetTexture(URL);

            yield return WWW.SendWebRequest();

            if (WWW.result != UnityWebRequest.Result.Success)
            {
                if (OnFailed != null)
                    OnFailed(WWW.result.ToString());
            }
            else
            {
                Texture2D LoadedTexture = DownloadHandlerTexture.GetContent(WWW);
                
                if (OnComplete != null)
                    OnComplete(LoadedTexture);
            }
        }

        public static void GetCurrentUserAvatar(Action<Texture2D> OnSuccess, Action<string> OnFailed = null, int Resolution = 512)
        {
            GetAvatar(UserManager.GetCurrentUser(), OnSuccess, OnFailed, Resolution);
        }

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
            if (!SkyEngine.Properties.EnableDiscord)
                return;

            if (!Initialized)
            {
                try
                {
                    Discord = new global::Discord.Discord(SkyEngine.Properties.DiscordProperties.ClientID, (ulong)CreateFlags.Default);

                    ActivityManager = Discord.GetActivityManager();
                    UserManager = Discord.GetUserManager();

                    if (SkyEngine.Properties.EnableSteam)
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

                SkyEngine.SetRichPresence(SkyEngine.Properties.InitialStatus, "");
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

            try
            {
                Camera.main.GetComponent<UniversalAdditionalCameraData>().renderPostProcessing = ConfigManager.GetOption("PostEffects", 1, "Graphics") == 1;
            } catch { }

            if (QualitySettings.GetQualityLevel() != ConfigManager.GetOption("Quality", 5, "Graphics"))
            {
                QualitySettings.SetQualityLevel(ConfigManager.GetOption("Quality", 5, "Graphics"), true);
            }
        }
    }
}