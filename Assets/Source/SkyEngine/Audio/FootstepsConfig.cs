using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkySoft.Audio
{
    [System.Serializable]
    public class FootstepProfile
    {
        public string Key = "Footsteps";
        public Texture2D[] Textures;
        public string[] Tags;
        public AudioClip[] Sounds;
        [Tooltip("Leave empty if this surface shouldn't have any effects")]
        public GameObject[] DustEffects;

        public FootstepProfile()
        {
            // Sounds = FootstepsConfig.ActiveConfig.DefaultFootsteps;
        }
    }

    [System.Serializable]
    public class FootstepEffect
    {
        public AudioClip Sound;
        public GameObject Particle;
    }

    [CreateAssetMenu(menuName = "SkyEngine/Footsteps Config", fileName = "Footsteps")]
    public class FootstepsConfig : ScriptableObject
    {
        public AudioClip[] DefaultFootsteps;
        public AudioClip[] LightArmourFootsteps;
        public AudioClip[] HeavyArmourFootsteps;
        [Space]
        public FootstepProfile[] FootstepProfiles;
        public LayerMask FootstepLayers;

        private static FootstepsConfig m_CachedConfig;

        public static FootstepsConfig ActiveConfig
        {
            get
            {
                try 
                { 
                    if (!m_CachedConfig)
                        m_CachedConfig = Resources.Load<FootstepsConfig>("Footsteps");

                    return m_CachedConfig;
                } catch { }

                return null;
            }
        }

        public static AudioClip FootstepSound(Vector3 Position, out FootstepEffect Effect)
        {
            AudioClip[] Clips = ActiveConfig.DefaultFootsteps;
            FootstepEffect V = new FootstepEffect();

            if (ActiveConfig.FootstepProfiles.Length > 0)
            {
                RaycastHit H;

                if (UnityEngine.Physics.Raycast(Position + Vector3.up, Vector3.down, out H, 3, ActiveConfig.FootstepLayers))
                {
                    UnityEngine.Terrain T;

                    if (T = H.collider.GetComponent<UnityEngine.Terrain>())
                    {
                        Vector3 LocalPosition = T.transform.InverseTransformPoint(H.point);
                        Vector2 NormalizedPosition = new Vector2(Mathf.InverseLerp(0, T.terrainData.size.x, LocalPosition.x),
                            Mathf.InverseLerp(0.0f, T.terrainData.size.z, LocalPosition.z));
                        Vector2 AlphamapPosition = new Vector2(NormalizedPosition.x * T.terrainData.alphamapWidth,
                            NormalizedPosition.y * T.terrainData.alphamapHeight);

                        float[,,] AlphaMap = T.terrainData.GetAlphamaps(Mathf.RoundToInt(AlphamapPosition.x), Mathf.RoundToInt(AlphamapPosition.y), 1, 1);

                        int TargetIndex = -1;

                        foreach (FootstepProfile Profile in ActiveConfig.FootstepProfiles)
                        {
                            for (int I = 0; I < T.terrainData.terrainLayers.Length; I++)
                            {
                                if (Profile.Textures.ToList().Contains(T.terrainData.terrainLayers[I].diffuseTexture))
                                {
                                    TargetIndex = I;
                                    break;
                                }
                            }

                            if (TargetIndex != -1)
                            {
                                if (AlphaMap[0, 0, TargetIndex] > 0.5f)
                                {
                                    Clips = Profile.Sounds;
                                    if (Profile.DustEffects.Length > 0)
                                        V = new FootstepEffect { Particle = Profile.DustEffects[Random.Range(0, Profile.DustEffects.Length)] };

                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (FootstepProfile Profile in ActiveConfig.FootstepProfiles)
                        {
                            if (Profile.Tags.ToList().Contains(H.collider.tag))
                            {
                                Clips = Profile.Sounds;

                                if (Profile.DustEffects.Length > 0)
                                    V = new FootstepEffect { Particle = Profile.DustEffects[Random.Range(0, Profile.DustEffects.Length)] };
                            }
                        }
                    }
                }
            }

            Effect = V;

            return Clips[Random.Range(0, Clips.Length)];
        }

        public static AudioClip LightArmourSound
        {
            get
            {
                return ActiveConfig.LightArmourFootsteps[Random.Range(0, ActiveConfig.LightArmourFootsteps.Length)];
            }
        }

        public static AudioClip HeavyArmourSound
        {
            get
            {
                return ActiveConfig.HeavyArmourFootsteps[Random.Range(0, ActiveConfig.HeavyArmourFootsteps.Length)];
            }
        }
    }
}