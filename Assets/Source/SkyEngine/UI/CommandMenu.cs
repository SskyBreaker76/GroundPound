using SkySoft.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SkySoft.UI
{
    public enum NavigationStyle
    {
        Vertical,
        Horizontal
    }

    [AddComponentMenu("SkyEngine/UI/Menu")]
    public class CommandMenu : MonoBehaviour
    {
        public AudioSource ConfirmSound;
        /// <summary>
        /// This will prevent Inputs happening on any CommandMenu. Use wisely!
        /// </summary>
        public static bool DisableProcessing = false;
        private List<GameObject> ValidObjects = new List<GameObject>();

        public bool ExecuteInFreeCam = false;
        public bool ExecuteInDialogue = false;
        public static bool BlockCancel = false;
        private Vector3 CursorPosLastFrame = Vector3.zero;
        public NavigationStyle NavStyle = NavigationStyle.Vertical;
        public bool RetainSelection = true;
        public bool IsInventory = false;
        public Inventory.Inventory Inventory;
        public void SetRetainSelection(bool Value)
        {
            RetainSelection = Value;
        }
        [Space]
        public float CursorMoveSpeed = 1000;
        public Transform Selection;
        [Space]
        public Transform ItemsRoot;
        private int m_Selection { get { return SelectedIndex; } set { SelectedIndex = value; } }
        public int SelectedIndex;
        [Space]
        public UnityEvent OnCancel;
        public UnityEvent OnNavigate;
        public UnityEvent OnConfirm;
        public UnityEvent OnInteractionBlocked;
        public UnityEvent NextBlocked;
        public UnityEvent PreviousBlocked;

        public Image GamepadDebug;

        public void ResetAllValues()
        {
            for (int I = 0; I < ItemsRoot.childCount; I++)
            {
                OptionsEventHandler H;

                if (H = ItemsRoot.GetChild(I).GetComponent<OptionsEventHandler>())
                {
                    H.ResetValue();
                }
            }
        }

        public static bool WasInventoryRefresh = false;

        protected int Selected
        {
            get
            {
                return Mathf.Clamp(m_Selection, 0, ValidObjects.Count - 1);
            }
            set
            {
                m_Selection = GetSelectionValueClamped(value);
                WasInventoryRefresh = false;
            }
        }

        public int GetSelectionValueClamped(int Input)
        {
            return ValidObjects.Count > 0 ? Mathf.Clamp(Input, 0, ValidObjects.Count - 1) : 0;
        }

        private Transform SelectedTransform
        {
            get
            {
                try
                {
                    ObjectEventHandler EventHandler;

                    if (EventHandler = ValidObjects[Selected].GetComponent<ObjectEventHandler>())
                    {
                        if (EventHandler.CursorPosition)
                            return EventHandler.CursorPosition;
                    }

                    return ValidObjects[Selected].transform.GetChild(1);
                }
                catch { }

                return null;
            }
        }

        protected bool UpdatedSelection = false;
        protected bool UpdatedOption = false;

        private void OnEnable()
        {
            if (!RetainSelection)
            {
                Selected = 0;
            }

            UpdateValidObjects();

            Selection.gameObject.SetActive(true);
            Selection.GetComponent<Animator>().SetBool("Select", false);
        }

        public void UpdateValidObjects()
        {
            ValidObjects = new List<GameObject>();
            foreach (Transform T in ItemsRoot)
            {
                if (T.GetComponent<Selectable>() || T.GetComponent<ObjectEventHandler>())
                {
                    ValidObjects.Add(T.gameObject);
                }
            }
        }

        private void OnDisable()
        {
            if (IsInventory)
                Selection.GetComponent<Animator>().SetBool("Select", true);
            else
                Selection.gameObject.SetActive(false);
        }

        protected virtual void HandleInput(Vector2 Scroll)
        {
            float V = NavStyle == NavigationStyle.Vertical ? Scroll.y : Scroll.x;

            if (V > 0.1f)
            {
                if (!UpdatedSelection)
                {
                    bool DontUpdateSelected = false;
                    OnNavigate.Invoke();
                    try
                    {
                        if (Inventory && !WasInventoryRefresh)
                        {
                            Inventory.OnNavigate(this, Selected - 1, out DontUpdateSelected);
                        }
                    }
                    catch (UnityException E)
                    {
                        Debug.Log(E);
                    }
                    if (!DontUpdateSelected)
                    {
                        if (Selected - 1 < 0)
                        {
                            PreviousBlocked.Invoke();
                        }

                        Selected--;
                    }

                    ObjectEventHandler OV;
                    if (OV = ValidObjects[Selected].GetComponent<ObjectEventHandler>())
                        OV.OnHighlightEvent();

                    UpdatedSelection = true;
                }
            }
            else if (V < -0.1f)
            {
                if (!UpdatedSelection)
                {
                    bool DontUpdateSelected = false;
                    OnNavigate.Invoke();
                    try
                    {
                        if (Inventory && !WasInventoryRefresh)
                        {
                            Inventory.OnNavigate(this, Selected + 1, out DontUpdateSelected);
                        }
                    }
                    catch (UnityException E)
                    {
                        Debug.Log(E);
                    }

                    if (!DontUpdateSelected)
                    {
                        if (Selected + 1 >= ValidObjects.Count)
                        {
                            NextBlocked.Invoke();
                        }

                        Selected++;
                    }

                    ObjectEventHandler OV;
                    if (OV = ValidObjects[Selected].GetComponent<ObjectEventHandler>())
                        OV.OnHighlightEvent();

                    UpdatedSelection = true;
                }
            }
            else
            {
                UpdatedSelection = false;
            }
        }

        private void Update()
        {
            if (!DisableProcessing || ExecuteInDialogue)
            {
                if (Selection.gameObject.activeSelf == false)
                    Selection.gameObject.SetActive(true);

                try
                {
                    UpdateValidObjects();

                    if (!FreeCam.Enabled || ExecuteInFreeCam)
                    {
                        if (SelectedTransform)
                            Selection.position = Vector3.MoveTowards(Selection.position, SelectedTransform.position, (CursorMoveSpeed * Time.unscaledDeltaTime) * (Vector3.Distance(Selection.position, SelectedTransform.position) / (Screen.width / 10)));

                        Vector2 Scroll = SkyEngine.Input.Menus.NavigateMenu.ReadValue<Vector2>().normalized;

                        if (GamepadDebug)
                        {
                            if (Scroll == Vector2.zero)
                            {
                                GamepadDebug.color = Color.clear;
                            }
                            else if (Scroll.y > 0.1f)
                            {
                                GamepadDebug.sprite = SkyEngine.Properties.DPadUp;
                                GamepadDebug.color = Color.white;
                            }
                            else if (Scroll.y < -0.1f)
                            {
                                GamepadDebug.sprite = SkyEngine.Properties.DPadDown;
                                GamepadDebug.color = Color.white;
                            }
                            else if (Scroll.x > 0.1f)
                            {
                                GamepadDebug.sprite = SkyEngine.Properties.DPadLeft;
                                GamepadDebug.color = Color.white;
                            }
                            else if (Scroll.x < -0.1f)
                            {
                                GamepadDebug.sprite = SkyEngine.Properties.DPadRight;
                                GamepadDebug.color = Color.white;
                            }
                        }

                        HandleInput(Scroll);

                        for (int I = 0; I < ValidObjects.Count; I++)
                        {
                            ObjectEventHandler EventHandler;

                            if (EventHandler = ValidObjects[I].GetComponent<ObjectEventHandler>())
                            {
                                if (EventHandler.Icon)
                                {
                                    EventHandler.Icon.gameObject.SetActive(I == Selected);
                                }
                            }
                            else
                            {
                                try
                                {
                                    ValidObjects[I].transform.GetChild(0).gameObject.SetActive(I == Selected);
                                }
                                catch { }
                            }
                        }

                        if (SkyEngine.Input.Menus.Confirm.WasPressedThisFrame())
                        {
                            ObjectEventHandler EV;
                            if (EV = ValidObjects[Selected].GetComponent<ObjectEventHandler>())
                            {
                                if (ConfirmSound)
                                    ConfirmSound.mute = !EV.DoSoundInUI;

                                if (EV.Interactable)
                                {
                                    if (ValidObjects[Selected].tag != "NoSound")
                                        OnConfirm.Invoke();
                                    EV.Event();
                                }
                                else
                                {
                                    OnInteractionBlocked.Invoke();
                                }
                            }
                            Button Btn;
                            if (Btn = ValidObjects[Selected].GetComponent<Button>())
                            {
                                if (Btn.interactable)
                                {
                                    if (ValidObjects[Selected].tag != "NoSound")
                                        OnConfirm.Invoke();
                                    Btn.onClick.Invoke();
                                }
                                else
                                {
                                    OnInteractionBlocked.Invoke();
                                }
                            }
                        }
                        if (SkyEngine.Input.Menus.Cancel.WasPressedThisFrame() && !BlockCancel)
                        {
                            OnCancel.Invoke();
                        }

                        if (NavStyle == NavigationStyle.Vertical)
                        {
                            OptionsEventHandler OpHandler;

                            if (SelectedTransform)
                            {
                                if (OpHandler = ValidObjects[Selected].GetComponent<OptionsEventHandler>())
                                {
                                    if (Scroll.x > 0.1f)
                                    {
                                        if (!UpdatedOption)
                                        {
                                            OpHandler.OnLeft();
                                            UpdatedOption = true;
                                            OnConfirm.Invoke();
                                        }
                                    }
                                    else if (Scroll.x < -0.1f)
                                    {
                                        if (!UpdatedOption)
                                        {
                                            OpHandler.OnRight();
                                            UpdatedOption = true;
                                            OnConfirm.Invoke();
                                        }
                                    }
                                    else
                                    {
                                        UpdatedOption = false;
                                    }
                                }
                            }
                        }

                        Animator Cursor;

                        if (Cursor = Selection.GetComponent<Animator>())
                        {
                            if (Vector3.Distance(CursorPosLastFrame, Selection.position) > 1)
                            {
                                Cursor.SetBool("Moving", true);
                            }
                            else
                            {
                                Cursor.SetBool("Moving", false);
                            }
                        }

                        CursorPosLastFrame = Selection.transform.position;
                    }
                }
                catch (UnityException E) { Debug.Log(E); }
            }
        }
    }
}