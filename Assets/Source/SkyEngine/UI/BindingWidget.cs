using UnityEngine;
using UnityEngine.UI;

namespace SkySoft.UI
{
    [RequireComponent(typeof(Image)), AddComponentMenu("SkyEngine/UI/Binding")]
    public class BindingWidget : MonoBehaviour
    {
        private Image TargetGraphic => GetComponent<Image>();
        public string ActionKey;
        public float Alpha = 1;

        private void OnValidate()
        {
            Update();
        }

        private void OnEnable()
        {
            TargetGraphic.color = Color.clear;
        }

        private void Update()
        {
            GamepadButton Btn = SkyEngine.Properties.GetBinding(ActionKey);

            if (Btn != GamepadButton.None)
            {
                TargetGraphic.color = Color.white * new Color(1, 1, 1, Alpha);
                TargetGraphic.sprite = SkyEngine.Properties.Glyphs.Buttons[Btn];
            }
            else
            {
                TargetGraphic.color = Color.magenta * new Color(1, 1, 1, Alpha); // Colour the graphic magenta and remove its image to make the error more visible
                TargetGraphic.sprite = null;
            }
        }
    }
}