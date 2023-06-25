using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using SkySoft.IO;
using static Unity.Burst.Intrinsics.X86.Avx;
using SkySoft.Steam;

namespace SkySoft.UI
{
    [RequireComponent(typeof(Text)), AddComponentMenu("SkyEngine/UI/Message Box")]
    public class MessageBox : MonoBehaviour
    {
        [System.Serializable]
        public class Message
        {
            [TextArea] public string Text;
            public int MinDelay, MaxDelay;
            public int EndDelay;
            [Space]
            public MessageBox Target;
            [Space]
            public UnityEvent OnMessageStart;
            public UnityEvent OnMessageEnd;
            public UnityEvent OnMessageExit;
        }

        public Text Display => GetComponent<Text>();

        [SerializeField] private GameObject m_Parent;
        public GameObject Parent 
        { 
            get
            {
                if (m_Parent)
                    return m_Parent;

                return gameObject;
            }
            set
            {
                m_Parent = value;
            }
        }

        public int DefaultMinDelay = 5;
        public int DefaultMaxDelay = 15;

        private Queue<Message> Messages = new Queue<Message>();
        public GameObject NextIndicator;

        public void QueueMessage(string Text, int MinDelay = -1, int MaxDelay = -1)
        {
            if (MinDelay == -1)
                MinDelay = DefaultMinDelay;
            if (MaxDelay == -1)
                MaxDelay = DefaultMaxDelay;

            if (MinDelay < 0)
                MinDelay = 1;
            if (MaxDelay < MinDelay)
                MaxDelay = MinDelay + 1;

            QueueMessage(new Message()
            {
               Text = Text,
               MinDelay = MinDelay,
               MaxDelay = MaxDelay
            });
        }

        public void QueueMessage(Message Message)
        {
            if (Message.MinDelay == -1)
                Message.MinDelay = DefaultMinDelay;
            if (Message.MaxDelay == -1)
                Message.MaxDelay = DefaultMaxDelay;

            Messages.Enqueue(Message);
        }

        private Message CurrentMessage;

        public async void ShowMessage(Message Message)
        {
            string Formatted = Message.Text.Replace("[N]", SkyEngine.PlayerEntity.Properties.Name.ToUpper());

            CurrentMessage = Message;
            ShowingMessage = true;

            Message.OnMessageStart.Invoke();
            Display.text = ""; // Clear the Display

            for (int I = 0; I < Formatted.Length; I++)
            {
                Display.text += Formatted[I];
                await Task.Delay(Random.Range(Message.MinDelay, Message.MaxDelay));
            }

            if (Message.EndDelay > 0)
                await Task.Delay(Message.EndDelay);

            Message.OnMessageEnd.Invoke();

            ShowingMessage = false;
        }

        private static bool ShowingMessage = false;

        private void Update()
        {
            if (!ShowingMessage)
            {
                if (Messages.Count > 0)
                {
                    if (string.IsNullOrEmpty(Display.text))
                    {
                        if (CurrentMessage != null)
                            CurrentMessage.OnMessageExit.Invoke();

                        ShowMessage(Messages.Dequeue());
                    }
                    else
                    {
                        NextIndicator.SetActive(true);

                        if (SkyEngine.Input.Menus.Confirm.WasPressedThisFrame())
                        {
                            if (CurrentMessage != null)
                                CurrentMessage.OnMessageExit.Invoke();

                            ShowMessage(Messages.Dequeue());
                        }
                    }
                }
                else
                {
                    NextIndicator.SetActive(true);

                    if (SkyEngine.Input.Menus.Confirm.WasPressedThisFrame())
                    {
                        if (CurrentMessage != null)
                            CurrentMessage.OnMessageExit.Invoke();

                        Parent.SetActive(false);
                    }
                }
            }
            else
            {
                NextIndicator.SetActive(false);
            }
        }
    }
}