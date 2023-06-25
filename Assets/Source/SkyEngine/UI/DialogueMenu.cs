using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SkySoft.UI 
{
    [Serializable]
    public class Dialogue
    {
        [TextArea] public string Text;
        public string Speaker;
        [Space]
        public UnityEvent OnShown;
        public int ShowDuration;
    }

    [RequireComponent(typeof(Animator)), Obsolete("Use the DialoguePanel class instead")]
    public class DialogueMenu : MonoBehaviour
    {
        private Animator Animator => GetComponent<Animator>();
        public static bool IsMenuOpen { get; private set; }
        public static DialogueMenu Instance { get; private set; }
        public Text MessageText;
        [Space]
        public CommandMenu Commands;
        public AudioSource NextSound;
        public float CursorMoveSpeed = 1000;
        public RectTransform Cursor;
        public RectTransform CursorPos;

        private Queue<Dialogue> Dialogues = new Queue<Dialogue>();
        private bool NextDialogue;

        public static void QueueDialogue(string Text, string Speaker, Action OnShown, int ShowDuration = 0)
        {
            Dialogue D = new Dialogue
            {
                Text = Text,
                Speaker = Speaker,
                ShowDuration = ShowDuration,
            };

            D.OnShown = new UnityEvent();
            D.OnShown.AddListener(() => { OnShown(); });

            if (Instance.Dialogues.Count == 0)
                Instance.NextDialogue = true;

            Instance.Dialogues.Enqueue(D);
        }

        private void Awake()
        {
            Instance = this;
        }

        public float ShowCounter;
        public bool RequireConfirm;

        private void Update()
        {
            if (Dialogues.Count > 0 || Animator.GetBool("IsOpen"))
            {
                Cursor.position = Vector3.MoveTowards(Cursor.position, CursorPos.position, (CursorMoveSpeed * Time.unscaledDeltaTime) * (Vector3.Distance(Cursor.position, CursorPos.position) / (Screen.width / 10)));
                
                Commands.enabled = false;

                if (!Animator.GetBool("IsOpen"))
                    NextDialogue = true;

                if (NextDialogue)
                {
                    IsMenuOpen = true;
                    Animator.SetBool("IsOpen", true);

                    Dialogue D = Dialogues.Dequeue();

                    MessageText.text = D.Text;
                    D.OnShown.Invoke();

                    RequireConfirm = D.ShowDuration <= 0;
                    ShowCounter = D.ShowDuration;

                    NextDialogue = false;
                }

                ShowCounter -= Time.deltaTime;

                if (RequireConfirm)
                {
                    if (SkyEngine.Input.Menus.Confirm.WasPressedThisFrame())
                    {
                        NextDialogue = true;
                        NextSound.Play();
                        if (Dialogues.Count <= 0)
                            Animator.SetBool("IsOpen", false);
                    }
                }
                else
                {
                    if (ShowCounter <= 0)
                    {
                        NextDialogue = true;
                        NextSound.Play();
                        if (Dialogues.Count <= 0)
                            Animator.SetBool("IsOpen", false);
                    }
                }
            }
            else
            {
                if (NextDialogue)
                {
                    Commands.enabled = true;

                    IsMenuOpen = false;
                    Animator.SetBool("IsOpen", false);
                    NextDialogue = false;
                }
            }
        }
    }
}