using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using SkySoft.Objects;
using SkySoft.IO;

namespace SkySoft.Audio
{
    [Serializable]
    public class AreaMusic
    {
        public string Key;
        public AudioClip BGM;
        [Range(0, 1)] public float BGMVolume = 1;
        [Range(0.5f, 1.5f)] public float BGMPitch = 1;
        [Space]
        public AudioClip BattleBGM;
        [Range(0, 1)] public float BattleBGMVolume = 1;
        [Range(0.5f, 1.5f)] public float BattleBGMPitch = 1;
    }

    [CreateAssetMenu(fileName = "BGM", menuName = "SkyEngine/BGM Handler")]
    public class BGM : ScriptableObject
    {
        public static Transform Parent
        {
            get
            {
                if (!GameObject.Find("[ BGM ]"))
                {
                    DontDestroyOnLoad(new GameObject("[ BGM ]").transform);
                    if (Main)
                    {
                        GameInstance.Main.OnUpdate -= Main.Update;
                        GameInstance.Main.OnUpdate += Main.Update;
                    }
                }

                return GameObject.Find("[ BGM ]").transform;
            }
        }
        public static BGM Main => SkyEngine.BGM;

        public float CurrentTrackVolumeMultiplier = 1;
        public float VolumeMultiplier = 1;

        public AudioClip BattleBGM;
        [Range(0, 1)] public float BattleBGMVolume;
        [Range(0.5f, 1.5f)] public float BattleBGMPitch;
        [Space]
        [SerializeField] private AreaMusic[] m_Areas;
        private Dictionary<string, AreaMusic> AreasCache;
        public Dictionary<string, AreaMusic> Areas
        {
            get
            {
                if (AreasCache == null || AreasCache.Count == 0)
                {
                    AreasCache = new Dictionary<string, AreaMusic>();

                    foreach (AreaMusic Mus in m_Areas)
                    {
                        AreasCache.Add(Mus.Key, Mus);
                    }
                }

                return AreasCache;
            }
        }

        public static AudioSource ActiveAudioSource;

        [Button]
        public bool GenerateScript;
        private static bool PlayingCombatMus;

        private void OnValidate()
        {
            if (GenerateScript)
            {
#if UNITY_EDITOR
                string Path = UnityEditor.EditorUtility.SaveFilePanel("Generate Script", Application.dataPath + "Assets\\Scripts\\", "BGMs", "cs");
                try
                {
                    string Values = "";

                    Values += "None,\n\t\tRetain,";

                    for (int I = 0; I < m_Areas.Length; I++)
                    {
                        if (m_Areas[I].Key != "None" && m_Areas[I].Key != "Retain")
                        {
                            Values += "\n\t\t";

                            Values += m_Areas[I].Key;

                            if (I < m_Areas.Length - 1)
                            {
                                Values += ",";
                            }
                        }
                        else
                        {
                            UnityEditor.EditorUtility.DisplayDialog("Generate Script", $"{m_Areas[I].Key} is reserved, and so will be ignored!", "Continue");
                        }
                    }

                    string Script =
                        "namespace SkySoft.Generated\n{\n\tpublic enum E_BGMs\n\t{\n\t\tVAL\n\t}\n}".Replace("VAL", Values);

                    System.IO.File.WriteAllText(Path, Script);
                    UnityEditor.AssetDatabase.Refresh();
                }
                catch (UnityException E)
                {
                    UnityEditor.EditorUtility.DisplayDialog("Generate Script", $"Ran into error\n{E.Message}\n{E.StackTrace}", "Okay");
                }
#endif
            }

            GenerateScript = false;
        }

        /// <summary>
        /// Fadeout the current playing BGM. If used in front of await it will wait until the BGM is done fading out
        /// </summary>
        /// <param name="FadeDuration"></param>
        /// <returns></returns>
        public void FadeoutBGM(float FadeDuration, Action OnComplete)
        {
            LeanTween.value(((ConfigManager.GetOption("MusicVolume", 10, "Audio") / 10f) * VolumeMultiplier) * CurrentTrackVolumeMultiplier, 0, FadeDuration).setOnUpdate(Value =>
            {
                ActiveAudioSource.volume = Value;
            }).setOnComplete(OnComplete);
        }

        /// <summary>
        /// Fadeout the current playing BGM (if there is any) then play Areas[Key] in its place.
        /// </summary>
        /// <param name="FadeDuration">This is only needed when a BGM is already playing.</param>
        /// <param name="InCombat">Decides whether to play the BGM or BattleBGM.</param>
        /// <param name="Key">Which BGM you want to play.</param>
        /// <returns></returns>
        public void StartBGM(string Key, Action OnComplete, bool InCombat = false, float FadeDuration = 1, bool DoFadeIn = false)
        {
            Debug.Log($"StartBGM({Key})");

            if (Areas.ContainsKey(Key))
            {
                if (ActiveAudioSource)
                {
                    Debug.Log("There is an AudioSource");

                    if (ActiveAudioSource.clip)
                    {
                        if (ActiveAudioSource.clip == (InCombat ? Areas[Key].BattleBGM : Areas[Key].BGM))
                        {
                            OnComplete();
                            return;
                        }
                    }

                    FadeoutBGM(FadeDuration, () => 
                    {
                        StartTheMusic(Key, OnComplete, InCombat, DoFadeIn);
                    });
                }
                else
                {
                    StartTheMusic(Key, OnComplete, InCombat, DoFadeIn);
                }
            }
        }

        private void StartTheMusic(string Key, Action OnComplete, bool InCombat, bool DoFadeIn = false)
        {
            if (!ActiveAudioSource)
            {
                AudioSource Src = new GameObject("MusicPlayer").AddComponent<AudioSource>();
                Src.ignoreListenerPause = true;
                Src.bypassEffects = true;
                Src.bypassListenerEffects = true;
                Src.bypassReverbZones = true;
                Src.transform.parent = Parent;
                ActiveAudioSource = Src;
            }

            CurrentTrackVolumeMultiplier = InCombat ? (Areas[Key].BattleBGM == null ? BattleBGMVolume : Areas[Key].BattleBGMVolume) : Areas[Key].BGMVolume;
            if (!DoFadeIn)
                ActiveAudioSource.volume = ((ConfigManager.GetOption("MusicVolume", 10, "Audio") / 10f) * VolumeMultiplier) * CurrentTrackVolumeMultiplier;
            else
                ActiveAudioSource.volume = 0;

            ActiveAudioSource.pitch = InCombat ? (Areas[Key].BattleBGM == null ? BattleBGMPitch : Areas[Key].BattleBGMPitch) : Areas[Key].BGMPitch;
            ActiveAudioSource.loop = true;
            ActiveAudioSource.clip = InCombat ? (Areas[Key].BattleBGM == null ? BattleBGM : Areas[Key].BattleBGM) : Areas[Key].BGM;
            if (ActiveAudioSource.clip != null)
                ActiveAudioSource.Play();

            OnComplete();
        }

        private void Update()
        {
            if (ActiveAudioSource)
            {
                ActiveAudioSource.volume = Mathf.MoveTowards(ActiveAudioSource.volume, ((ConfigManager.GetOption("MusicVolume", 10, "Audio") / 10f) * VolumeMultiplier) * CurrentTrackVolumeMultiplier, Time.unscaledDeltaTime / 1);
            }

            if (!ActiveAudioSource.isPlaying)
                ActiveAudioSource.Play();
        }

        private static bool AllowCombatSwap = true;

        public async void ToggleCombat(bool InCombat)
        {
            while (!AllowCombatSwap) await Task.Yield();

            if (InCombat != PlayingCombatMus && AllowCombatSwap)
            {
                AllowCombatSwap = false;

                PlayingCombatMus = InCombat;
                StartBGM(SceneDefinition.ActiveDefinition.Area.ToString(), () => { AllowCombatSwap = true; }, InCombat, 1);
            }
        }

        #region Modding Utils
        /// <summary>
        /// Inject a generated BGM into AreasCache. If AreasCache already has Key, will replace the BGM at AreasCache[Key]
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="BGMName"></param>
        /// <param name="LengthSamples"></param>
        /// <param name="Channels"></param>
        /// <param name="Frequency"></param>
        /// <param name="Data"></param>
        /// <param name="Stream"></param>
        public void InjectBGM(string Key, string BGMName, int LengthSamples, int Channels, int Frequency, float[] Data, bool Stream = true)
        {
            AudioClip Clip = AudioClip.Create(BGMName, LengthSamples, Channels, Frequency, Stream);
            Clip.SetData(Data, 0);

            if (!AreasCache.ContainsKey(Key))
            {
                AreaMusic Mus = new AreaMusic
                {
                    Key = Key,
                    BGM = Clip
                };

                AreasCache.Add(Key, Mus);
            }
            else
            {
                AreasCache[Key].BGM = Clip;
            }
        }
        
        /// <summary>
        /// Load a .wav file from disc and then inject it into AreasCache. If AreasCache already has Key, will replace the BGM at AreasCache[Key]
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="BGMName"></param>
        /// <param name="Path"></param>
        public async void InjectBGM(string Key, string BGMName, string Path)
        {
            AudioClip Clip = null;
            using (UnityWebRequest Request = UnityWebRequestMultimedia.GetAudioClip(Path, AudioType.WAV))
            {
                Request.SendWebRequest();

                try
                {
                    while (!Request.isDone) await System.Threading.Tasks.Task.Delay(5);

                    if (Request.result != UnityWebRequest.Result.Success) Debug.Log($"{Request.error}");
                    else
                        Clip = DownloadHandlerAudioClip.GetContent(Request);
                }
                catch (System.Exception Err)
                {
                    Debug.Log($"{Err.Message}, {Err.StackTrace}");
                }
            }

            if (!AreasCache.ContainsKey(Key))
            {
                AreaMusic Mus = new AreaMusic
                {
                    Key = Key,
                    BGM = Clip
                };

                AreasCache.Add(Key, Mus);
            }
            else
            {
                AreasCache[Key].BGM = Clip;
            }
        }


        /// <summary>
        /// Inject a generated BattleBGM into AreasCache. If AreasCache already has Key, will replace the BattleBGM at AreasCache[Key]
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="BGMName"></param>
        /// <param name="LengthSamples"></param>
        /// <param name="Channels"></param>
        /// <param name="Frequency"></param>
        /// <param name="Data"></param>
        /// <param name="Stream"></param>
        public void InjectBattleBGM(string Key, string BGMName, int LengthSamples, int Channels, int Frequency, float[] Data, bool Stream = true)
        {
            AudioClip Clip = AudioClip.Create(BGMName, LengthSamples, Channels, Frequency, Stream);
            Clip.SetData(Data, 0);

            if (!AreasCache.ContainsKey(Key))
            {
                AreaMusic Mus = new AreaMusic
                {
                    Key = Key,
                    BattleBGM = Clip
                };

                AreasCache.Add(Key, Mus);
            }
            else
            {
                AreasCache[Key].BattleBGM = Clip;
            }
        }

        /// <summary>
        /// Load a .wav file from disc and then inject it into AreasCache. If AreasCache already has Key, will replace the BattleBGM at AreasCache[Key]
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="BGMName"></param>
        /// <param name="Path"></param>
        public async void InjectBattleBGM(string Key, string BGMName, string Path)
        {
            AudioClip Clip = null;
            using (UnityWebRequest Request = UnityWebRequestMultimedia.GetAudioClip(Path, AudioType.WAV))
            {
                Request.SendWebRequest();

                try
                {
                    while (!Request.isDone) await System.Threading.Tasks.Task.Delay(5);

                    if (Request.result != UnityWebRequest.Result.Success) Debug.Log($"{Request.error}");
                    else
                        Clip = DownloadHandlerAudioClip.GetContent(Request);
                }
                catch (System.Exception Err)
                {
                    Debug.Log($"{Err.Message}, {Err.StackTrace}");
                }
            }

            if (!AreasCache.ContainsKey(Key))
            {
                AreaMusic Mus = new AreaMusic
                {
                    Key = Key,
                    BattleBGM = Clip
                };

                AreasCache.Add(Key, Mus);
            }
            else
            {
                AreasCache[Key].BattleBGM = Clip;
            }
        }
        #endregion
    }
}
