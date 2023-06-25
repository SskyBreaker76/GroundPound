using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkySoft.Entities;

namespace SkySoft.World.Weather
{
    public enum WeatherTemplate
    {
        Clear,
        Overcast,
        LightCloud,
        Spitting,
        DarkCloud,
        Rain,
        Storm,
        Cyclone
    }

    public enum LightningFrequency
    {
        None,
        VeryRare,
        Rare,
        Common,
        Constant
    }

    [AddComponentMenu("SkyEngine/Weather/Weather Manager")]
    public class WeatherManager : MonoBehaviour
    {
        public WeatherTemplate Weather;
        [Tooltip("This only effects anything during the Storm and Cyclone weather types")]
        public LightningFrequency LightningMode;
        [Range(0, 24)] public float CurrentTime;
        [Space]
        public Light Sun;
        public Light Lightning;
        public float LightningFlashDuration;
        public Renderer CloudRenderer;
        public ParticleSystem Rain;
        [Space]
        public AudioClip[] CloseLightningSounds;
        public AudioClip[] DistantLightningSounds;
        public float ThunderVolume = 0.75f;
        public float ThunderDistance = 10000;
        public AudioClip LightRain;
        public float LightRainVolume = 0.5f;
        public float LightRainFadeDuration = 3;
        public AudioClip HeavyRain;
        public float HeavyRainVolume = 0.5f;
        public float HeavyRainFadeDuration = 3;
        public AudioClip LightWind;
        public float LightWindVolume = 0.5f;
        public float LightWindFadeDuration = 6;
        public AudioClip HeavyWind;
        public float HeavyWindVolume = 0.5f;
        public float HeavyWindFadeDuration = 5;

        private List<AudioSource> LightningSources = new List<AudioSource>();
        
        private AudioSource LightRainSource;
        private AudioSource HeavyRainSource;

        private AudioSource LightWindSource;
        private AudioSource HeavyWindSource;

        private void Update()
        {
            if (Sun)
            {
                Sun.transform.eulerAngles = new Vector3((CurrentTime / 24) * 360, 45, 15);
            }

            if (Lightning.intensity > 0)
            {
                Lightning.intensity -= Time.deltaTime / (LightningFlashDuration / 1);
            }

            for (int I = 0; I < LightningSources.Count; I++)
            {
                AudioSource Src = LightningSources[I];

                if (Src != null)
                {
                    if (Src.isPlaying == false)
                        Destroy(Src.gameObject);
                }
                else
                {
                    LightningSources.RemoveAt(I);
                    break;
                }
            }

            foreach (AudioSource Src in LightningSources)
            {
                if (Src != null)
                {
                    if (Src.isPlaying == false)
                        Destroy(Src.gameObject);
                }
            }

            if (LightRainSource)
                if (LightRainSource.volume <= 0)
                    Destroy(LightRainSource.gameObject);
            if (HeavyRainSource)
                if (HeavyRainSource.volume <= 0)
                    Destroy(HeavyRainSource.gameObject);
            if (LightWindSource)
                if (LightWindSource.volume <= 0)
                    Destroy(LightWindSource.gameObject);
            if (HeavyWindSource)
                if (HeavyWindSource.volume <= 0)
                    Destroy(HeavyWindSource.gameObject);

            bool SpawnLightning = false;
            bool LightRain = false;
            bool HeavyRain = false;
            bool LightWind = false;
            bool HeavyWind = false;

            switch (Weather)
            {
                case WeatherTemplate.Clear:
                    if (Rain)
                        Rain.emissionRate = 0;
                    break;
                case WeatherTemplate.Overcast:
                    if (Rain)
                        Rain.emissionRate = 0;
                    break;
                case WeatherTemplate.LightCloud:
                    LightWind = true;
                    if (Rain)
                        Rain.emissionRate = 0;
                    break;
                case WeatherTemplate.Spitting:
                    LightWind = true;
                    if (Rain)
                        Rain.emissionRate = 10;
                    break;
                case WeatherTemplate.DarkCloud:
                    LightWind = true;
                    LightRain = true;
                    if (Rain)
                        Rain.emissionRate = 25;
                    break;
                case WeatherTemplate.Rain:
                    LightWind = true;
                    HeavyRain = true;
                    if (Rain)
                        Rain.emissionRate = 50;
                    break;
                case WeatherTemplate.Storm:
                    HeavyWind = true;
                    HeavyRain = true;
                    if (Rain)
                        Rain.emissionRate = 100;

                    float LightningRand = Random.value;

                    switch (LightningMode)
                    {
                        case LightningFrequency.VeryRare:
                            if (LightningRand < 0.001f)
                                SpawnLightning = true;
                            break;
                        case LightningFrequency.Rare:
                            if (LightningRand < 0.0025f)
                                SpawnLightning = true;
                            break;
                        case LightningFrequency.Common:
                            if (LightningRand < 0.0025f)
                                SpawnLightning = true;
                            break;
                        case LightningFrequency.Constant:
                            if (LightningRand < 0.003f)
                                SpawnLightning = true;
                            break;
                    }

                    break;
                case WeatherTemplate.Cyclone:
                    HeavyWind = true;
                    HeavyRain = true;
                    if (Rain)
                        Rain.emissionRate = 150;
                    break;
            }

            if (SpawnLightning)
            {
                Lightning.intensity = 1;
                Vector3 Position = Camera.main.transform.position + new Vector3(Random.Range(-ThunderDistance, ThunderDistance), Random.Range(-ThunderDistance, ThunderDistance), Random.Range(-ThunderDistance, ThunderDistance));

                Debug.Log(Vector3.Distance(Camera.main.transform.position, Position));

                AudioClip Clip;
                if (Vector3.Distance(Camera.main.transform.position, Position) < 5000)
                {
                    Clip = CloseLightningSounds[Random.Range(0, CloseLightningSounds.Length)];
                }
                else
                {
                    Clip = DistantLightningSounds[Random.Range(0, DistantLightningSounds.Length)];
                }

                AudioSource Src = new GameObject(Clip.name).AddComponent<AudioSource>();
                Src.clip = Clip;
                Src.minDistance = 100000;
                Src.maxDistance = 1000000;
                Src.rolloffMode = AudioRolloffMode.Linear;
                Src.spatialBlend = 1;
                Src.PlayDelayed(Vector3.Distance(Camera.main.transform.position, Position) / 1000);

                LightningSources.Add(Src);
            }

            HandleAudioSource(LightWind, LightWindFadeDuration, LightWindSource, out LightWindSource, this.LightWind, LightWindVolume);
            HandleAudioSource(HeavyWind, HeavyWindFadeDuration, HeavyWindSource, out HeavyWindSource, this.HeavyWind, HeavyWindVolume);
            HandleAudioSource(LightRain, LightRainFadeDuration, LightRainSource, out LightRainSource, this.LightRain, LightRainVolume);
            HandleAudioSource(HeavyRain, HeavyRainFadeDuration, HeavyRainSource, out HeavyRainSource, this.HeavyRain, HeavyRainVolume);
        }

        private void HandleAudioSource(bool Enable, float FadeDuration, AudioSource InSrc, out AudioSource OutSrc, AudioClip Clip, float TargetVolume)
        {
            if (Enable)
            {
                if (InSrc)
                {
                    InSrc.volume += Time.deltaTime / (FadeDuration * TargetVolume);
                    InSrc.volume = Mathf.Clamp(InSrc.volume, 0, TargetVolume);

                    OutSrc = InSrc;
                }
                else
                {
                    OutSrc = new GameObject(Clip.name).AddComponent<AudioSource>();
                    OutSrc.clip = Clip;
                    OutSrc.loop = true;
                    OutSrc.volume = 0.001f;
                    OutSrc.Play();

                    InSrc = OutSrc;
                }
            }
            else
            {
                if (InSrc)
                {
                    InSrc.volume -= Time.deltaTime / LightRainFadeDuration;
                }
            }

            OutSrc = InSrc;
        }
    }
}