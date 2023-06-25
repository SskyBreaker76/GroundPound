using SkySoft.Generated;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.Objects
{
    [System.Serializable]
    public class CharacterBlend
    {
        public string Label;
        public int Index;
        [Range(0, 100)] public float Value;
        public GameObject[] ObjectsToToggle;
        public bool OverrideFace;
        public Texture2D Neutral;
        public Texture2D Happy, SuperHappy, Sad, Angry, SuperAngry, Damaged;
        public Texture2D Iris, Iris2;
        public bool OverrideSkinTones;
        public Gradient SkinTones;
    }

    public enum Emotion
    {
        Neutral,
        Happy,
        SuperHappy,
        Sad,
        Angry,
        SuperAngry,
        Damaged
    }

    public enum EyeColour
    {
        Brown,
        CrystalBlue,
        Blue,
        Green,
        Purple,
        Magenta,
        Red,
        Grey,
        White,
        Gold,
        Orange
    }

    [AddComponentMenu("SkyEngine/Objects/Character Manager")]
    public class Charactermanager : MonoBehaviour
    {
        public bool EasterEggMode = false;
        public bool ForceCombatStance;
        protected Dictionary<E_Race, CharacterBlend> m_SpecialRaceBlends;
        public Dictionary<E_Race, CharacterBlend> SpecialBlends
        {
            get
            {
                if (m_SpecialRaceBlends == null || m_SpecialRaceBlends.Count == 0)
                {
                    m_SpecialRaceBlends = new Dictionary<E_Race, CharacterBlend>();

                    foreach (CharacterBlend Blend in Blends)
                    {
                        for (int I = 0; I < System.Enum.GetNames(typeof(E_Race)).Length; I++)
                        {
                            if (Blend.Label == $"E_Race.{(E_Race)I}")
                            {
                                if (!m_SpecialRaceBlends.ContainsKey((E_Race)I))
                                {
                                    m_SpecialRaceBlends.Add((E_Race)I, Blend);
                                }
                            }
                        }
                    }
                }

                return m_SpecialRaceBlends;
            }
        }

        [Tooltip("Only enable this when needed, as it creates Instanced materials!")]
        public bool EnableRefresh = false;
        public bool WalkDoll = false;
        public CharacterBlend[] Blends;
        public virtual Animator TargetAnimator => Anim;
        public Animator Anim;
        public Animator Anim2;
        public Animator Anim3;
        public SkinnedMeshRenderer Renderer;

        [Header("Random Idles")]
        public float MinimumWait = 6;
        public float MaximumWait = 24;
        protected float RandomIdleCounter;
        public int RandomIdleCount = 1;

        [Header("Face Controls")]
        public int FaceMaterialIndex = 1;
        [Range(0, 1)] public float SkinColour;
        [Range(0, 1)] public float HairColour;
        [Space]
        public Emotion CurrentEmotion;
        public bool EnableBlinking = true;
        [Tooltip("This should only be used by animations")]
        public bool ForceBlink = false;
        public float BlinkMinWait = 2;
        public float BlinkMaxWait = 16;
        public float BlinkDuration = 0.1f;

        /// <summary>
        /// This is for the actual blink animation
        /// </summary>
        protected float BlinkTimer;
        /// <summary>
        /// This is for the time between the blink animation
        /// </summary>
        protected float BlinkCounter;
        [Space]
        public E_Race Race;
        public Gradient SkinColours;
        public bool HasHeterochromia;
        public EyeColour EyesColour = EyeColour.CrystalBlue;
        public EyeColour LeftEyeColour = EyeColour.CrystalBlue;
        public Color EyesBrown,
            EyesCrystalBlue,
            EyesBlue,
            EyesGreen,
            EyesPurple,
            EyesMagenta,
            EyesRed,
            EyesGrey,
            EyesWhite,
            EyesGold = new Color(1, 0.85f, 0.4f),
            EyesOrange = new Color(1, 0.4431372549019608f, 0);
        public Gradient HairColours;

        public Gradient OSkinColours;
        [Space]
        public Texture2D Neutral;
        public Texture2D Happy, SuperHappy, Sad, Angry, SuperAngry, Damaged;
        public Texture2D Iris, Iris2;

        protected Texture2D ONeutral;
        protected Texture2D OHappy, OSuperHappy, OSad, OAngry, OSuperAngry, ODamaged;
        protected Texture2D OIris, OIris2;

        [Header("Equipment")]
        [SerializeField] private WeaponModel m_WeaponModel;
        bool RunningRefresh = false;
        public WeaponModel WeaponModel
        {
            get { return m_WeaponModel; }
            set { bool Refresh = m_WeaponModel != value;  m_WeaponModel = value; if (!RunningRefresh && Refresh) RefreshWeapons(); }
        }

        public bool ForceWeaponRefresh;

        protected GameObject SpawnedModel;
        public Transform WeaponRoot;

        public void RefreshWeapons()
        {
            RunningRefresh = true;
            if (SpawnedModel != null)
            {
                Destroy(SpawnedModel);
            }

            try
            {
                SpawnedModel = Instantiate(WeaponModel.Prefab, WeaponRoot);
                SpawnedModel.transform.localPosition = WeaponModel.PositionOffset;
                SpawnedModel.transform.localEulerAngles = WeaponModel.RotationOffset;
                SpawnedModel.layer = WeaponRoot.gameObject.layer;

                if (SpawnedModel.transform.childCount > 0)
                {
                    foreach (Transform T in SpawnedModel.transform)
                        T.gameObject.layer = WeaponRoot.gameObject.layer;
                }
            }
            catch { }

            OnWeaponsRefreshed();
            RunningRefresh = false;
        }

        protected virtual void OnWeaponsRefreshed()
        {

        }

        public void ResetAttackState()
        {
            if (TargetAnimator)
                TargetAnimator.SetBool("Attack", false);
        }

        public void SetBlend(string Label, float Value)
        {
            foreach (CharacterBlend Blend in Blends)
            {
                if (Blend.Label == Label)
                {
                    Blend.Value = Value;
                }
            }
        }

        public void SetBlend(int Index, float Value)
        {
            Blends[Index].Value = Value;
        }

        protected void OnDrawGizmos()
        {
            if (EnableRefresh)
            {
                CheapUpdate();
            }
        }

        /// <summary>
        /// This is mostly used for the Editor. It doesn't update based off of race nor contain the blink counter
        /// </summary>
        protected void CheapUpdate()
        {
            Texture2D TargetFace = Neutral;
            switch (CurrentEmotion)
            {
                case Emotion.Happy:
                    TargetFace = Happy;
                    break;
                case Emotion.SuperHappy:
                    TargetFace = SuperHappy;
                    break;
                case Emotion.Sad:
                    TargetFace = Sad;
                    break;
                case Emotion.Angry:
                    TargetFace = Angry;
                    break;
                case Emotion.SuperAngry:
                    TargetFace = SuperAngry;
                    break;
                case Emotion.Damaged:
                    TargetFace = Damaged;
                    break;
            }

            bool IsBlinking = BlinkTimer > 0;

            if (EnableBlinking)
            {
                Renderer.sharedMaterials[FaceMaterialIndex].SetFloat("_Blink", IsBlinking || ForceBlink ? 1 : 0);
            }
            else
            {
                Renderer.sharedMaterials[FaceMaterialIndex].SetFloat("_Blink", ForceBlink ? 1 : 0);
            }

            Color TargetEyeColour = EyesBrown;

            switch (EyesColour)
            {
                case EyeColour.Brown:
                    TargetEyeColour = EyesBrown;
                    break;
                case EyeColour.CrystalBlue:
                    TargetEyeColour = EyesCrystalBlue;
                    break;
                case EyeColour.Blue:
                    TargetEyeColour = EyesBlue;
                    break;
                case EyeColour.Green:
                    TargetEyeColour = EyesGreen;
                    break;
                case EyeColour.Purple:
                    TargetEyeColour = EyesPurple;
                    break;
                case EyeColour.Magenta:
                    TargetEyeColour = EyesMagenta;
                    break;
                case EyeColour.Red:
                    TargetEyeColour = EyesRed;
                    break;
                case EyeColour.Grey:
                    TargetEyeColour = EyesGrey;
                    break;
                case EyeColour.White:
                    TargetEyeColour = EyesWhite;
                    break;
                case EyeColour.Gold:
                    TargetEyeColour = EyesGold;
                    break;
            }


            foreach (SkinnedMeshRenderer Renderer in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                for (int I = 0; I < Renderer.materials.Length; I++)
                {
                    if (Renderer.sharedMaterials[I].HasColor("_IrisColour"))
                        Renderer.sharedMaterials[I].SetColor("_IrisColour", TargetEyeColour);
                    if (Renderer.sharedMaterials[I].HasTexture("_Base") && Renderer.sharedMaterials[I].GetTexture("_Base") != null)
                        Renderer.sharedMaterials[I].SetTexture("_Base", TargetFace);
                    if (Renderer.sharedMaterials[I].HasTexture("_IrisMask") && Renderer.materials[I].GetTexture("_IrisMask") != null)
                        Renderer.sharedMaterials[I].SetTexture("_IrisMask", Iris);
                    if (Renderer.sharedMaterials[I].HasColor("_SkinColour"))
                        Renderer.sharedMaterials[I].SetColor("_SkinColour", SkinColours.Evaluate(SkinColour));
                }
            }
        }

        protected void Awake()
        {
            RefreshWeapons();
        }

        protected Color GetEyeColour(EyeColour Colour)
        {
            switch (Colour)
            {
                case EyeColour.Brown:
                    return EyesBrown;
                case EyeColour.CrystalBlue:
                    return EyesCrystalBlue;
                case EyeColour.Blue:
                    return EyesBlue;
                case EyeColour.Green:
                    return EyesGreen;
                case EyeColour.Purple:
                    return EyesPurple;
                case EyeColour.Magenta:
                    return EyesMagenta;
                case EyeColour.Red:
                    return EyesRed;
                case EyeColour.Grey:
                    return EyesGrey;
                case EyeColour.White:
                    return EyesWhite;
                case EyeColour.Gold:
                    return EyesGold;
                case EyeColour.Orange:
                    return EyesOrange;
            }

            return EyesCrystalBlue;
        }

        public void ForceUpdate()
        {
            Update();
        }

        protected virtual void Update()
        {
            if (ForceWeaponRefresh)
            {
                RefreshWeapons();
                ForceWeaponRefresh = false;
            }

            bool OverrideFace = false;
            bool OverrideSkin = false;

            foreach (CharacterBlend Blend in Blends)
            {
                if (Blend.Index >= 0)
                {
                    Renderer.SetBlendShapeWeight(Blend.Index, Blend.Value);
                    foreach (GameObject G in Blend.ObjectsToToggle)
                    {
                        G.SetActive(!EasterEggMode && Blend.Value > 50);
                    }

                    if (Blend.Value > 50)
                    {
                        OverrideSkin = Blend.OverrideSkinTones;
                        OSkinColours = Blend.SkinTones;

                        OverrideFace = Blend.OverrideFace;
                        ONeutral = Blend.Neutral;
                        OHappy = Blend.Happy;
                        OSuperHappy = Blend.SuperHappy;
                        OSad = Blend.Sad;
                        OAngry = Blend.Angry;
                        OSuperAngry = Blend.SuperAngry;
                        ODamaged = Blend.Damaged;
                        OIris = Blend.Iris;
                        OIris2 = Blend.Iris2;
                    }
                }
            }

            if (SpecialBlends.ContainsKey(Race))
            {
                if (SpecialBlends[Race].OverrideSkinTones)
                {
                    OverrideSkin = true;
                    OSkinColours = SpecialBlends[Race].SkinTones;
                }

                if (SpecialBlends[Race].OverrideFace)
                {
                    OverrideFace = true;
                    ONeutral = SpecialBlends[Race].Neutral;
                    OHappy = SpecialBlends[Race].Happy;
                    OSuperHappy = SpecialBlends[Race].SuperHappy;
                    OSad = SpecialBlends[Race].Sad;
                    OAngry = SpecialBlends[Race].Angry;
                    OSuperAngry = SpecialBlends[Race].SuperAngry;
                    ODamaged = SpecialBlends[Race].Damaged;
                    OIris = SpecialBlends[Race].Iris;
                    OIris2 = SpecialBlends[Race].Iris2;
                }
            }

            #region FaceControl
            bool Unscaled = Anim.updateMode == AnimatorUpdateMode.UnscaledTime;

            BlinkTimer -= Unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            BlinkCounter -= Unscaled ? Time.unscaledDeltaTime : Time.deltaTime;

            Texture2D TargetFace = OverrideFace ? ONeutral : Neutral;
            switch (CurrentEmotion)
            {
                case Emotion.Happy:
                    TargetFace = OverrideFace ? OHappy : Happy;
                    break;
                case Emotion.SuperHappy:
                    TargetFace = OverrideFace ? OSuperHappy : SuperHappy;
                    break;
                case Emotion.Sad:
                    TargetFace = OverrideFace ? OSad : Sad;
                    break;
                case Emotion.Angry:
                    TargetFace = OverrideFace ? OAngry : Angry;
                    break;
                case Emotion.SuperAngry:
                    TargetFace = OverrideFace ? OSuperAngry : SuperAngry;
                    break;
                case Emotion.Damaged:
                    TargetFace = OverrideFace ? ODamaged : Damaged;
                    break;
            }

            bool IsBlinking = BlinkTimer > 0;

            if (EnableBlinking)
            {
                Renderer.materials[FaceMaterialIndex].SetFloat("_Blink", IsBlinking || ForceBlink ? 1 : 0);
            }
            else
            {
                Renderer.materials[FaceMaterialIndex].SetFloat("_Blink", ForceBlink ? 1 : 0);
            }

            if (BlinkCounter <= 0)
            {
                BlinkTimer = BlinkDuration;
                BlinkCounter = Random.Range(BlinkMinWait, BlinkMaxWait);
            }

            Color TargetEyeColour = EyesBrown;

            switch (EyesColour)
            {
                case EyeColour.Brown:
                    TargetEyeColour = EyesBrown;
                    break;
                case EyeColour.CrystalBlue:
                    TargetEyeColour = EyesCrystalBlue;
                    break;
                case EyeColour.Blue:
                    TargetEyeColour = EyesBlue;
                    break;
                case EyeColour.Green:
                    TargetEyeColour = EyesGreen;
                    break;
                case EyeColour.Purple:
                    TargetEyeColour = EyesPurple;
                    break;
                case EyeColour.Magenta:
                    TargetEyeColour = EyesMagenta;
                    break;
                case EyeColour.Red:
                    TargetEyeColour = EyesRed;
                    break;
                case EyeColour.Grey:
                    TargetEyeColour = EyesGrey;
                    break;
                case EyeColour.White:
                    TargetEyeColour = EyesWhite;
                    break;
                case EyeColour.Gold:
                    TargetEyeColour = EyesGold;
                    break;
                case EyeColour.Orange:
                    TargetEyeColour = EyesOrange;
                    break;
            }

            Color SecondEyeColour = TargetEyeColour;

            if (HasHeterochromia)
            {
                switch (LeftEyeColour)
                {
                    case EyeColour.Brown:
                        SecondEyeColour = EyesBrown;
                        break;
                    case EyeColour.CrystalBlue:
                        SecondEyeColour = EyesCrystalBlue;
                        break;
                    case EyeColour.Blue:
                        SecondEyeColour = EyesBlue;
                        break;
                    case EyeColour.Green:
                        SecondEyeColour = EyesGreen;
                        break;
                    case EyeColour.Purple:
                        SecondEyeColour = EyesPurple;
                        break;
                    case EyeColour.Magenta:
                        SecondEyeColour = EyesMagenta;
                        break;
                    case EyeColour.Red:
                        SecondEyeColour = EyesRed;
                        break;
                    case EyeColour.Grey:
                        SecondEyeColour = EyesGrey;
                        break;
                    case EyeColour.White:
                        SecondEyeColour = EyesWhite;
                        break;
                    case EyeColour.Gold:
                        SecondEyeColour = EyesGold;
                        break;
                    case EyeColour.Orange:
                        SecondEyeColour = EyesOrange;
                        break;
                }
            }

            foreach (SkinnedMeshRenderer Renderer in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                for (int I = 0; I < Renderer.materials.Length; I++)
                {
                    if (Renderer.materials[I].HasColor("_IrisColour"))
                        Renderer.materials[I].SetColor("_IrisColour", TargetEyeColour);
                    if (Renderer.materials[I].HasColor("_IrisColour2"))
                        Renderer.materials[I].SetColor("_IrisColour2", SecondEyeColour);
                    if (Renderer.materials[I].HasTexture("_Base") && Renderer.materials[I].GetTexture("_Base") != null)
                        Renderer.materials[I].SetTexture("_Base", TargetFace);
                    if (Renderer.materials[I].HasTexture("_IrisMask") && Renderer.materials[I].GetTexture("_IrisMask") != null)
                        Renderer.materials[I].SetTexture("_IrisMask", OverrideFace ? OIris : Iris);
                    if (Renderer.materials[I].HasTexture("_IrisMask2") && Renderer.materials[I].GetTexture("_IrisMask2") != null)
                        Renderer.materials[I].SetTexture("_IrisMask2", OverrideFace ? OIris2 : Iris2);
                    if (Renderer.materials[I].HasColor("_SkinColour"))
                        Renderer.materials[I].SetColor("_SkinColour", OverrideSkin ? OSkinColours.Evaluate(SkinColour) : SkinColours.Evaluate(SkinColour));
                }
            }
            #endregion

            #region IdleControl
            if (Anim.GetBool("Run") == false)
            {
                RandomIdleCounter -= Unscaled ? Time.unscaledDeltaTime : Time.deltaTime;

                if (RandomIdleCounter <= 0)
                {
                    int Value = Random.Range(0, RandomIdleCount + 1);

                    Anim.SetTrigger($"Idle{Value}");
                    if (Anim2)
                        Anim2.SetTrigger($"Idle{Value}");
                    if (Anim3)
                        Anim3.SetTrigger($"Idle{Value}");
                    RandomIdleCounter = Random.Range(MinimumWait, MaximumWait);
                }
            }
            #endregion

            Anim.SetBool("Walk", WalkDoll);
            if (Anim2)
                Anim2.SetBool("Walk", WalkDoll);
            if (Anim3)
                Anim3.SetBool($"Walk", WalkDoll);

            if (ForceCombatStance)
            {
                Anim.SetBool("InCombat", true);
                if (Anim2)
                    Anim2.SetBool("InCombat", true);
                if (Anim3)
                    Anim3.SetBool("InCombat", true);
            }
        }
    }
}