using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.LevelManagement
{
    [AddComponentMenu("SkyEngine/Level Management/Level Fader")]
    public class LevelFader : MonoBehaviour
    {
        public static LevelFader Instance;

        public float LoadScreenWait = 1;
        public GameObject LoadingWidget;

        private void Update()
        {
            if (LevelManager.IsLoadingLevel)
            {
                if (Time.time - LevelManager.LoadStartTime > LoadScreenWait)
                {
                    LoadingWidget.SetActive(true);
                    return;
                }
            }

            LoadingWidget.SetActive(false);
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LevelManager.Fader = GetComponentInChildren<UnityEngine.UI.Image>();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}