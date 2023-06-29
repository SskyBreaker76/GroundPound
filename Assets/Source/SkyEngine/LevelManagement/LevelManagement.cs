using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using SkySoft.Objects;
using SkySoft.IO;
using System.IO;
using SkySoft.Interaction;
using SkySoft.Audio;
using SkySoft.Generated;

namespace SkySoft.LevelManagement
{
    public enum FadeColour
    {
        White,
        Black,
        Custom
    }

    [System.Serializable]
    public class LevelDefinition
    {
        public string DisplayName = "Namsan Coastline";
        public string ShortKey = "Map000";
        public string Scene = "Map000_NamsanCoastline";
    }

    [CreateAssetMenu(fileName = "Levels", menuName = "SkyEngine/Levels")]
    public class LevelManagement : ScriptableObject
    {
        [SerializeField] private LevelDefinition[] m_Levels;

        public LevelDefinition[] GetLevels { get => m_Levels; }

        private Dictionary<string, string> ScenesCache;
        public Dictionary<string, string> Scenes
        {
            get
            {
                if (ScenesCache == null || ScenesCache.Count == 0)
                {
                    ScenesCache = new Dictionary<string, string>();

                    foreach (LevelDefinition Level in m_Levels)
                    {
                        ScenesCache.Add(Level.ShortKey, Level.Scene);
                    }
                }

                return ScenesCache;
            }
        }

        public string GetShortKey(string LevelName)
        {
            foreach (LevelDefinition Level in m_Levels)
            {
                if (Level != null)
                {
                    if (Level.Scene.ToLower() == LevelName.ToLower())
                        return Level.ShortKey;
                }
            }

            return "";
        }

        public string GetShortKey(int Index)
        {
            if (Index < m_Levels.Length)
                return m_Levels[Index].ShortKey;

            return "";
        }

        public string GetSceneAt(int Index)
        {
            if (Index < m_Levels.Length)
                return m_Levels[Index].Scene;

            return "";
        }

        public string GetDisplayName(int Index)
        {
            if (Index < m_Levels.Length)
                return m_Levels[Index].DisplayName;

            return "";
        }

        public string GetDisplayName(string ID, bool AllowLocalisation = true)
        {
            foreach (LevelDefinition Level in m_Levels)
            {
                if (Level.ShortKey.ToLower() == ID.ToLower())
                {
                    if (AllowLocalisation)
                        if (SkyEngine.CommonTexts.ContainsKey($"level.{Level.ShortKey.ToLower()}"))
                            return SkyEngine.CommonTexts[$"level.{Level.ShortKey.ToLower()}"];

                    return Level.DisplayName;
                }
            }

            return "";
        }

        public string GetSceneName(string ID)
        {
            foreach (LevelDefinition Level in m_Levels)
            {
                if (Level.ShortKey.ToLower() == ID.ToLower())
                {
                    return Level.Scene;
                }
            }

            return "";
        }
    }

    public static class LevelManager
    {
        public static bool IsLoadingLevel { get; private set; }
        public static Image Fader;
        public static float LoadStartTime { get; private set; }

        /// <summary>
        /// Fadeout the screen. OnComplete will run once the fadeout is finished
        /// </summary>
        /// <param name="Colour"></param>
        /// <param name="OnComplete"</param>
        public static async void FadeoutScreen(FadeColour Colour, Action OnComplete, float Duration = 1)
        {
            FadeoutScreen(Colour, Color.white, OnComplete, Duration);
            while (Fader.color.a < 1) await Task.Yield();
        }

        /// <summary>
        /// Fadeout the screen. OnComplete will run once the fadeout is finished
        /// </summary>
        /// <param name="Colour"></param>
        /// <param name="CustomColour"></param>
        /// <param name="OnComplete"></param>
        public static void FadeoutScreen(FadeColour Colour, Color CustomColour, Action OnComplete, float Duration = 1)
        {
            if (Colour == FadeColour.White)
                CustomColour = Color.white;
            else if (Colour == FadeColour.Black)
                CustomColour = Color.black;

            Color TargetColour = CustomColour;
            TargetColour.a = 1;

            Fader.color = TargetColour;

            LevelFader.Instance.FadeAlpha(1, Duration, OnComplete);
        }

        /// <summary>
        /// Fadein the screen. OnComplete will run once the fadein is finished
        /// </summary>
        /// <param name="Colour"></param>
        /// <param name="OnComplete"></param>
        public static void FadeinScreen(FadeColour Colour, Action OnComplete, float Duration = 1)
        {
            FadeinScreen(Colour, Color.white, OnComplete, Duration);
        }

        /// <summary>
        /// Fadein the screen. OnComplete will run once the fadein is finished
        /// </summary>
        /// <param name="Colour"></param>
        /// <param name="CustomColour"></param>
        /// <param name="OnComplete"></param>
        public static void FadeinScreen(FadeColour Colour, Color CustomColour, Action OnComplete, float Duration = 1)
        {
            if (Colour == FadeColour.White)
                CustomColour = Color.white;
            else if (Colour == FadeColour.Black)
                CustomColour = Color.black;

            Color TargetColour = CustomColour;
            TargetColour.a = 1;

            Fader.color = TargetColour;

            LevelFader.Instance.FadeAlpha(0, Duration, OnComplete);
        }

        public static void LoadLevel(string LevelName, FadeColour FadeColour, Action OnComplete, PlayerFile LoadedSave = null, Color CustomColour = default)
        {
            LoadStartTime = Time.time * 10; // Realistically no load should take this long
            IsLoadingLevel = true;

            if (LevelName == "Title")
            {
                LoadTrigger.ComingFrom = ""; // Reset ComingFrom so we don't have any bugs in SaveFiles

                foreach (FileInfo Inf in FileManager.GetAllFiles("Loot"))
                {
                    FileManager.ReadFile<LootBoxSave>("Loot", Inf.Name.Replace(".loot", "").Replace("loot", ""), Save =>
                    {
                        if (Save.Temp)
                        {
                            File.Delete(Inf.FullName);
                        }
                    }, ".loot");
                }
            }
            else
            {
                if (!SkyEngine.LoadPositions && SkyEngine.PlayerEntity)
                {
                    SkyEngine.NextWriteAsTmp = true;
                    (SkyEngine.PlayerEntity).Serialize();
                }
            }

            Debug.Log($"LoadLevel({LevelName}, {FadeColour})");

            FadeoutScreen(FadeColour, async () => 
            {
                Debug.Log("LoadLevel");
                AsyncOperation LoadOperation = SceneManager.LoadSceneAsync(SkyEngine.Levels.Scenes[LevelName]);
                LoadStartTime = Time.time;
                float ProgressLastTick = 0;

                while (LoadOperation != null && LoadOperation.progress < 1)
                {
                    if (ProgressLastTick != LoadOperation.progress)
                    {
                        SkyEngine.OnLoadingProgress(LoadOperation.progress);
                    }
                    await Task.Yield(); // This prevents excessive freezing
                }

                LoadStartTime = Time.time * 1000;

                Time.timeScale = 1; // Unpause the game if it's paused

                SceneDefinition SceneDef = UnityEngine.Object.FindObjectOfType<SceneDefinition>();

                await Task.Delay(500);

                if (SceneDef)
                {
                    await Task.Delay(1000);
                }

                await Task.Delay(500);

                FadeinScreen(FadeColour, () =>
                {
                    IsLoadingLevel = false;
                    OnComplete();
                });
            });
        }

        public static async void LoadLevel(int Level, FadeColour FadeColour, Action OnComplete)
        {
            LoadLevel(SkyEngine.Levels.GetSceneAt(Level), FadeColour, OnComplete);
            await Task.Yield();
        }
    }
}