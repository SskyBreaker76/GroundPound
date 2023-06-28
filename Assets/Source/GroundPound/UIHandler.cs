/*
    Developed by Sky MacLennan
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sky.GroundPound
{
    [AddComponentMenu("Ground Pound/Canvas Scaling Handler"), ExecuteInEditMode, RequireComponent(typeof(Canvas))]
    public class UIHandler : MonoBehaviour
    {
        private CanvasScaler m_Target;
        protected CanvasScaler Target
        {
            get
            {
                if (!m_Target)
                    m_Target = GetComponent<CanvasScaler>();

                return m_Target;
            }
        }

        private void Update()
        {
            Target.matchWidthOrHeight = Screen.width > Screen.height ? 1 : 0;
        }
    }
}