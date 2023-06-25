using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SkySoft.UI
{
    [AddComponentMenu("SkyEngine/UI/Error Popup")]
    public class ErrorGUI : MonoBehaviour
    {
        public static ErrorGUI Instance;
        public Text Title, Message, Close;
        public Button CloseButton;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
            }

            Instance = this;
        }

        public static void Show(string Title, string Message, string Close, Action OnClose = null)
        {
            Instance.gameObject.SetActive(true);
            Instance.ShowError(Title, Message, Close, OnClose);
        }

        private void ShowError(string Title, string Message, string Close, Action OnClose = null)
        {
            this.Title.text = Title;
            this.Message.text = Message;
            this.Close.text = Close;

            if (OnClose != null)
            {
                CloseButton.onClick.RemoveAllListeners();
                CloseButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => { OnClose(); }));
            }
        }
    }
}