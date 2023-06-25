using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SkySoft.Events
{
    [AddComponentMenu("SkyEngine/Events/Object Event")]
    public class ObjectEventHandler : MonoBehaviour
    {
        public bool DoSoundInUI = true;
        private string DefaultText;
        public bool Interactable = true;
        public RectTransform CursorPosition;
        public Image Icon;
        [Tooltip("This Event is only called by specific objects")]
        public UnityEvent m_OnHighlight;
        public UnityEvent m_Event;

        public Color DisabledTint = Color.grey;
        public Color OriginalColour = Color.clear;

        public virtual void OnHighlightEvent()
        {
            m_OnHighlight.Invoke();
        }

        public virtual void Event()
        {
            m_Event.Invoke();
        }

        protected virtual void OnValidate()
        {
            Update();
        }

        private void Awake()
        {
            Text Txt;

            if (Txt = GetComponentInChildren<Text>())
            {
                DefaultText = Txt.text;
            }
        }

        private void Update()
        {
            Text Txt;

            if (Txt = GetComponentInChildren<Text>())
            {
                if (OriginalColour == Color.clear)
                {
                    OriginalColour = Txt.color;
                }

                Txt.color = Interactable ? OriginalColour : (OriginalColour * DisabledTint);
            }
        }
    }
}