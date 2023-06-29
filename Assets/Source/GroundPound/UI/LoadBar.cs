/*
    Developed by Sky MacLennan
 */

using SkySoft;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sky.GroundPound
{
    [AddComponentMenu("Ground Pound/Loading Bar"), RequireComponent(typeof(Image))]
    public class LoadBar : LoadingBar
    {
        private Image m_Bar;
        protected Image Bar
        {
            get
            {
                if (!m_Bar)
                    m_Bar = GetComponent<Image>();

                return m_Bar;
            }
        }

        protected override void OnLoadingProgress(float Progress)
        {
            Bar.fillAmount = Progress;
        }
    }
}