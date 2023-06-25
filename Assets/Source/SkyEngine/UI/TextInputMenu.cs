using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using SkySoft.Events;

namespace SkySoft.UI
{
    [System.Serializable]
    public class SubList<T>
    {
        public List<T> Values = new List<T>();
    }

    [AddComponentMenu("SkyEngine/UI/Text Input")]
    public class TextInputMenu : MonoBehaviour
    {
        public bool SkipValidation = false;
        public static List<string> Swears
        {
            get
            {
                return new List<string>
                {
                    "fuck",
                    "shit",
                    "cock",
                    "cunt",
                    "bitch",
                    "slag",
                    "chav",
                    "slut",
                    "nigger",
                    "cracker",
                    "asshole",
                    "arsehole",
                    "cotton picker",
                    "cottonpicker",
                    "whore",
                    "prick",
                    "nazi"
                };
            }
        }

        public string Text;
        [Space]
        private Vector3 CursorPosLastFrame = Vector3.zero;
        [Space]
        public float CursorMoveSpeed = 1000;
        public Transform Selection;
        [Space]
        public List<SubList<Transform>> Buttons = new List<SubList<Transform>>();
        public Vector2Int m_Selection;
        public int m_Cursor;
        public Text[] Letters;
        public Text[] Underscores;
        [Space]
        public UnityEvent OnCancel;
        public UnityEvent OnNavigate;
        public UnityEvent OnConfirm;
        public UnityEvent<string> OnFinished;
        public Action<string> OnComplete;
        [Space]
        public GameObject ErrorDialogue;
        public Text ErrorText;
        [Space]
        public GameObject ConfirmDialogue;
        public Text ConfirmText;

        protected Vector2Int Selected
        {
            get
            {
                return m_Selection;
            }
            set
            {
                m_Selection = new Vector2Int(Mathf.Clamp(value.x, 0, Buttons.Count - 1), Mathf.Clamp(value.y, 0, Buttons[value.x].Values.Count - 1));
            }
        }

        protected int Cursor
        {
            get
            {
                return m_Cursor;
            }
            set
            {
                m_Cursor = Mathf.Clamp(value, 0, Underscores.Length);
            }
        }

        private Transform SelectedTransform
        {
            get
            {
                ObjectEventHandler EventHandler;

                if (EventHandler = Buttons[Selected.x].Values[Selected.y].GetComponent<ObjectEventHandler>())
                {
                    if (EventHandler.CursorPosition)
                        return EventHandler.CursorPosition;
                }

                Button Btn;

                if (Btn = Buttons[Selected.x].Values[Selected.y].GetComponent<Button>())
                {
                    return Btn.transform.GetChild(0);
                }

                return null;
            }
        }

        private bool UpdatedSelection = false;

        private void OnEnable()
        {
            Selected = Vector2Int.zero;
            Cursor = 0;
        }

        protected virtual void HandleInput(Vector2 Scroll)
        {
            Debug.Log(Scroll);

            float V = Scroll.y;
            float H = Scroll.x;

            if (H > 0.1f)
            {
                if (!UpdatedSelection)
                {
                    OnNavigate.Invoke();
                    Vector2Int S = Selected;
                    S.y--;
                    Selected = S;
                    UpdatedSelection = true;
                }
            }
            else if (H < -0.1f)
            {
                if (!UpdatedSelection)
                {
                    OnNavigate.Invoke();
                    Vector2Int S = Selected;
                    S.y++;
                    Selected = S;
                    UpdatedSelection = true;
                }
            }
            else if (V > 0.1f)
            {
                if (!UpdatedSelection)
                {
                    OnNavigate.Invoke();
                    Vector2Int S = Selected;
                    S.x--;
                    Selected = S;
                    UpdatedSelection = true;
                }
            }
            else if (V < -0.1f)
            {
                if (!UpdatedSelection)
                {
                    OnNavigate.Invoke();
                    Vector2Int S = Selected;
                    S.x++;
                    Selected = S;
                    UpdatedSelection = true;
                }
            }
            else
            {
                UpdatedSelection = false;
            }
        }

        public void EnterCharacter(string Character)
        {
            if (Text.Length < Letters.Length)
            {
                Text += Character;
                Cursor = Text.Length;
            }
        }

        public void EnterCharacter()
        {
            if (Text.Length < Letters.Length)
            {
                Text += Buttons[Selected.x].Values[Selected.y].GetChild(1).GetComponent<Text>().text;
                Cursor = Text.Length;
            }
        }

        public void Backspace()
        {
            if (Text.Length > 0)
            {
                Text = Text.Remove(Cursor - 1, 1);
                Cursor = Text.Length;
            }
        }

        public void Space()
        {
            if (Text.Length < Letters.Length)
            {
                Text += " ";
                Cursor = Text.Length;
            }
        }

        public void Finish()
        {
            for (int I = Text.Length - 1; I >= 0; I--)
            {
                if (Text[I] == ' ')
                {
                    Text = Text.Remove(Text.Length - 1);
                }
                else
                {
                    break;
                }
            }

            bool WasSwear = false;

            foreach (string SwearWord in Swears)
            {
                if (Text.ToLower().Contains(SwearWord.ToLower()))
                {
                    WasSwear = true;
                    break;
                }
            }

            if (SkipValidation)
            {
                if (WasSwear)
                {
                    ErrorText.text = "Your name can't contain foul language!";
                    ErrorDialogue.SetActive(true);
                    enabled = false;
                }
                else if (Text.ToLower() == "skybreaker76")
                {
                    ErrorText.text = "No";
                    ErrorDialogue.SetActive(true);
                    enabled = false;
                }
                else
                {
                    OnFinished.Invoke(Text);
                    OnComplete(Text);
                }
            }
            else
            {
                
                OnFinished.Invoke(Text);
                OnComplete(Text);
            }
        }

        private void Update()
        {
            Selection.position = Vector3.MoveTowards(Selection.position, SelectedTransform.position, (CursorMoveSpeed * Time.deltaTime) * (Vector3.Distance(Selection.position, SelectedTransform.position) / (Screen.width / 10)));

            Vector2 Scroll = SkyEngine.Input.Menus.NavigateMenu.ReadValue<Vector2>().normalized;

            HandleInput(Scroll);

            for (int X = 0; X < Buttons.Count; X++)
            {
                for (int Y = 0; Y < Buttons[X].Values.Count; Y++)
                {
                    ObjectEventHandler EventHandler;

                    if (EventHandler = Buttons[X].Values[Y].GetComponent<ObjectEventHandler>())
                    {
                        if (EventHandler.Icon)
                        {
                            EventHandler.Icon.gameObject.SetActive(new Vector2Int(X, Y) == Selected);
                        }
                    }
                }
            }

            if (SkyEngine.Input.Menus.Confirm.WasPressedThisFrame())
            {
                ObjectEventHandler EV;
                if (EV = Buttons[Selected.x].Values[Selected.y].GetComponent<ObjectEventHandler>())
                {
                    EV.Event();
                    OnConfirm.Invoke();
                }

                Button Btn;

                if (Btn = Buttons[Selected.x].Values[Selected.y].GetComponent<Button>())
                {
                    Btn.onClick.Invoke();
                    OnConfirm.Invoke();
                }
            }
            if (SkyEngine.Input.Menus.Cancel.WasPressedThisFrame())
            {
                OnCancel.Invoke();
            }

            Animator Cursor;

            if (Cursor = Selection.GetComponent<Animator>())
            {
                if (Vector3.Distance(CursorPosLastFrame, Selection.position) > 1)
                {
                    Cursor.speed = 0;
                    Cursor.playbackTime = 0;
                }
                else 
                { 
                    Cursor.speed = 1; 
                }
            }
            CursorPosLastFrame = Selection.transform.position;

            for (int I = 0; I < Letters.Length; I++)
            {
                if (Text.Length >= I + 1)
                {
                    Letters[I].text = Text[I].ToString();
                }
                else
                {
                    Letters[I].text = "";
                }

                if (Underscores.Length >= I + 1)
                {
                    Underscores[I].color = this.Cursor == I ? Color.white : Color.grey;
                }
            }
        }
    }
}