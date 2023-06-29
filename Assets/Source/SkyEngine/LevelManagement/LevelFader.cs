using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SkySoft.LevelManagement
{
    [AddComponentMenu("SkyEngine/Level Management/Level Fader")]
    public class LevelFader : MonoBehaviour
    {
        public static LevelFader Instance;

        public float LoadScreenWait = 1;
        public GameObject LoadingWidget;
        public Image Background;
        public CanvasGroup TargetGroup;

        public void FadeAlpha(float Target, float Duration, Action OnComplete)
        {
            LeanTween.alphaCanvas(TargetGroup, Target, Duration).setOnComplete(OnComplete);
        }

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

            TargetGroup.blocksRaycasts = TargetGroup.alpha > 0;
            TargetGroup.interactable = TargetGroup.alpha > 0;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LevelManager.Fader = Background;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}