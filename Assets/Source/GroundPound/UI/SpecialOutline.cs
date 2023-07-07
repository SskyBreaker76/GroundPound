using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sky.GroundPound.Secret
{
    [RequireComponent(typeof(Outline))]
    public class SpecialOutline : MonoBehaviour
    {
        private Outline m_Outline;
        protected Outline Outline
        {
            get
            {
                if (!m_Outline)
                    m_Outline = GetComponent<Outline>();

                return m_Outline;
            }
        }

        public Gradient Colours;
        public float Speed = 0.1f;

        public List<string> Conditions = new List<string>(){ };

        private void FixedUpdate()
        {
            if (Conditions.Contains(Game.User.Username))
            {
                if (SkySoft.Audio.BGM.ActiveAudioSource)
                {
                    Outline.effectColor = Colours.Evaluate(SkySoft.Audio.BGM.ActiveAudioSource.time * Speed);
                }
            }
        }
    }
}