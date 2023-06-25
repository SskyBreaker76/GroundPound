using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SkySoft.UI
{
    [AddComponentMenu("SkyEngine/UI/Tabbed Menu")]
    public class TabMenu : MonoBehaviour
    {
        public bool StoreTab = true;
        public Transform TabsRoot;
        public Text TabLabel;
        [SerializeField] private int m_Tab;
        public int Tab
        {
            get 
            { 
                return m_Tab; 
            }
            set
            {
                m_Tab = Mathf.Clamp(value, 0, TabsRoot.childCount - 1);
            }
        }

        private void OnEnable()
        {
            if (!StoreTab)
                Tab = 0;
        }

        private void Update()
        {
            for (int I = 0; I < TabsRoot.childCount; I++) 
            {
                TabsRoot.GetChild(I).gameObject.SetActive(I == Tab);
            }

            if (SkyEngine.Input.Menus.TabLeft.WasPressedThisFrame())
            {
                if (Tab > 0)
                {
                    Tab--;
                }
                else
                {
                    Tab = TabsRoot.childCount;
                }
            }
            if (SkyEngine.Input.Menus.TabRight.WasPressedThisFrame())
            {
                if (Tab < TabsRoot.childCount - 1)
                {
                    Tab++;
                }
                else
                {
                    Tab = 0;
                }
            }

            TabLabel.text = TabsRoot.GetChild(Tab).name;
        }
    }
}