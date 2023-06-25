using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SkySoft.UI
{
    public enum StatusBarMode
    {
        SliderBar,
        TextBar,
        NumberLabel,
        NumeralLabel,
        ProgressGraphic,
        FilledBar
    }
    
    [System.Serializable]
    public class StatusEvent
    {
        public string DefaultState;
        public string AnimationName;
        public Animator Anim;
        public float StatePercentage;

        public void Check(float Percent)
        {
            if (Percent <= StatePercentage)
            {
                Anim.Play(AnimationName);
            }
            else
            {
                Anim.Play(DefaultState);
            }
        }
    }

    [AddComponentMenu("SkyEngine/UI/Status Bar")]
    public class StatusBar : MonoBehaviour
    {
        public StatusBarMode Mode;
        public Image.FillMethod FillMethod;
        [Tooltip("This only effects Sliders")]
        public bool FlipDirection;
        [SerializeField][Range(0, 1)] private float m_Value;
        public string PreLabel = "";
        public string PostLabel = "";
        public float MaxValue;
        [Space]
        public RectTransform TargetGraphic;
        public StatusEvent[] Events;

        public float Value
        {
            get
            {
                return m_Value * MaxValue;
            }
            set
            {
                m_Value = value / MaxValue;
                UpdateGraphic();
            }
        }

        private void OnValidate()
        {
            UpdateGraphic();
        }

        private void UpdateGraphic()
        {
            Text Txt;
            Image Img;

            switch (Mode)
            {
                case StatusBarMode.SliderBar:
                    {
                        TargetGraphic.anchoredPosition = new Vector2(Mathf.Lerp(GetComponent<RectTransform>().rect.width * (FlipDirection ? 1 : -1), 0, m_Value), 0);
                    }
                    break;
                case StatusBarMode.TextBar:
                    {
                        if (Txt = TargetGraphic.GetComponent<Text>())
                        {
                            // TODO[Sky] Implement TextBar mode to the StatusBar
                        }
                    }
                    break;
                case StatusBarMode.NumberLabel:
                    {
                        if (Txt = TargetGraphic.GetComponent<Text>())
                        {
                            Txt.text = PreLabel + Mathf.RoundToInt(m_Value * MaxValue).ToString() + PostLabel;
                        }
                    }
                    break;
                case StatusBarMode.NumeralLabel:
                    {
                        if (Txt = TargetGraphic.GetComponent<Text>())
                        {
                            string Numeral = SkyEngine.ToNumeral(Mathf.RoundToInt(m_Value * MaxValue));
                            Txt.text = PreLabel + (string.IsNullOrEmpty(Numeral) ? "0" : Numeral) + PostLabel;
                        }
                    }
                    break;
                case StatusBarMode.FilledBar:
                    {
                        if (Img = TargetGraphic.GetComponent<Image>())
                        {
                            Img.type = Image.Type.Filled;
                            Img.fillMethod = FillMethod;

                            if (FillMethod == Image.FillMethod.Horizontal || FillMethod == Image.FillMethod.Vertical)
                            {
                                Img.fillAmount = m_Value;
                                Img.fillOrigin = FlipDirection ? 1 : 0;
                            }
                        }
                    }
                    break;
            }

            foreach (StatusEvent EV in Events)
            {
                EV.Check(m_Value);
            }
        }
    }
}