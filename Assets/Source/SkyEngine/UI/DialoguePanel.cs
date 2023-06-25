using SkySoft;
using SkySoft.Events;
using SkySoft.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[AddComponentMenu("SkyEngine/UI/Dialogue Panel")]
public class DialoguePanel : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueChoice
    {
        public int Index;
        public string Text;
        public UnityAction Chosen;
        public bool Interactable;

        public DialogueChoice(string Text, UnityAction OnChosen, bool Interactable, int Index)
        {
            this.Text = Text;
            this.Chosen = OnChosen;
            this.Interactable = Interactable;
            this.Index = Index;
        }
    }

    public enum PanelLocation
    {
        Top, Centre, Bottom
    }

    public bool Unconfirmed = false;
    private bool HandleChoiceProcessing = false;

    public Animator Anim;

    private Dictionary<PanelLocation, RectTransform> Locations => new Dictionary<PanelLocation, RectTransform>
    {
        { PanelLocation.Top, Top },
        { PanelLocation.Centre, Centre },
        { PanelLocation.Bottom, Bottom }
    };
    public RectTransform Trans;
    public RectTransform Top;
    public RectTransform Centre;
    public RectTransform Bottom;
    [Space]
    public Text TextArea;
    [Space]
    public RectTransform StartNextArea;
    public RectTransform IdleArea;
    public RectTransform DecisionArea;
    public GameObject ChoiceObject;
    [Space]
    public int MaxChoices = 6;
    [Space]
    public AudioClip CharacterSound;
    public AudioClip CursorMoveSound;
    public AudioClip DialogueUpdateSound;
    public AudioSource Source;
    private int SelectedButton = 0;
    private List<ObjectEventHandler> Buttons = new List<ObjectEventHandler>();
    public Transform Cursor;
    public int CursorMoveSpeed = 1000;

    public DialogueChoice CreateChoice(string Text, bool IsEnabled, UnityEvent OnClicked)
    {
        return new DialogueChoice(Text, OnClicked.Invoke, IsEnabled, 0);
    }

    public void ShowText(string Speaker, string Text)
    {
        ShowText(Speaker, Text, PanelLocation.Bottom, 500, new DialogueChoice[] { }, null);
    }

    public void ShowText(string Speaker, string Text, PanelLocation Position, int StartDelay, DialogueChoice[] Choices, Action OnDialogueClosed, int MinCharDelay = 5, int MaxCharDelay = 15, bool SuppressAudio = false)
    {
        Cursor.position = IdleArea.position;
        TextArea.text = "";

        if (!string.IsNullOrEmpty(Speaker))
        {
            TextArea.text = $"<color=yellow><b>{Speaker}</b></color>\n";
        }
           
        Trans.anchorMin = Locations[Position].anchorMin;
        Trans.anchorMax = Locations[Position].anchorMax;
        Trans.anchoredPosition = Locations[Position].anchoredPosition;
        Trans.sizeDelta = Locations[Position].sizeDelta;

        gameObject.SetActive(true);

        DisplayText(Text, StartDelay, MinCharDelay, MaxCharDelay, 10, OnDialogueClosed, SuppressAudio, Choices);
    }

    public void PlaySound(AudioClip Sound, bool DoSoundVariation = false)
    {
        if (Source)
        {
            Source.pitch = DoSoundVariation ? UnityEngine.Random.Range(0.95f, 1.05f) : 1;
            Source.PlayOneShot(Sound);
        }
    }

    public void ClearDecisions()
    {
        if (DecisionArea.childCount > 0)
        {
            foreach (Transform T in DecisionArea)
            {
                Destroy(T.gameObject);
            }
        }
    }

    private async void DisplayText(string Text, int StartDelay, int MinCharacterDelay = 50, int MaxCharacterDelay = 150, int DelayBetweenShownChoice = 100, Action OnDialogueClosed = null, bool SuppressAudio = false, params DialogueChoice[] Choices)
    {
        Anim.SetBool("ShownChoices", false);
        SkyEngine.InDialogue = true;
        CommandMenu.DisableProcessing = true;
        Unconfirmed = true;

        ClearDecisions();

        if (DialogueUpdateSound && !SuppressAudio)
            PlaySound(DialogueUpdateSound);

        await Task.Delay(StartDelay);

        for (int I = 0; I < Text.Length; I++)
        {
            if (Text[I] != '|')
            {
                if (I % 2 == 0) // This prevents ear-rape
                    if (CharacterSound && MinCharacterDelay > 0 && MaxCharacterDelay > 0)
                        PlaySound(CharacterSound, true);

                TextArea.text += Text[I];
                await Task.Delay(UnityEngine.Random.Range(MinCharacterDelay, MaxCharacterDelay) * (SkyEngine.Input.Menus.Confirm.IsPressed() ? 2 : 1));
            }
            else
            {
                while (!SkyEngine.Input.Menus.Confirm.IsPressed())
                    await Task.Delay(10);
                if (DialogueUpdateSound)
                    PlaySound(DialogueUpdateSound);
            }
        }

        if (Choices != null && Choices.Length > 0)
        {
            ShowChoices(DelayBetweenShownChoice, Choices);
        }
        else
        {
            Cursor.position = StartNextArea.position;
            Cursor.GetComponent<Animator>().SetBool("Moving", false);

            while (!SkyEngine.Input.Menus.Confirm.IsPressed())
                await Task.Delay(10);

            OnDialogueClosed();
            Unconfirmed = false;
        }
    }

    public void CloseMenu()
    {
        SkyEngine.InDialogue = false;
        CommandMenu.DisableProcessing = false;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Spawn in all choices and assign their functions
    /// </summary>
    /// <param name="DelayBetweenShownChoice">The Delay (in ms) between each choice being shown. Produces a COOL animation B3</param>
    /// <param name="Choices"></param>
    private async void ShowChoices(int DelayBetweenShownChoice = 100, params DialogueChoice[] Choices)
    {
        Buttons.Clear();

        Anim.SetBool("ShownChoices", true);

        if (Choices.Length > MaxChoices)
        {
            Debug.LogWarning($"Input \"Choices\" has exceeded the maximum length of {MaxChoices}!");
            return;
        }

        foreach (DialogueChoice Choice in Choices)
        {
            ObjectEventHandler Button = Instantiate(ChoiceObject, DecisionArea).GetComponent<ObjectEventHandler>();
            Button.GetComponent<Text>().text = Choice.Text;
            Button.m_Event.AddListener(Choice.Chosen);
            Button.Interactable = Choice.Interactable;
            Buttons.Add(Button);
            await Task.Delay(DelayBetweenShownChoice);
        }

        Cursor.position = Buttons[0].CursorPosition.position;
        if (CursorMoveSound)
            PlaySound(CursorMoveSound);
        SelectedButton = 0; // Always reset this!
        HandleChoiceProcessing = true;
    }

    private bool DoneMenuNavigate;

    private void Update()
    {
        if (HandleChoiceProcessing)
        {
            Animator CursorAnim;

            if (CursorAnim = Cursor.GetComponent<Animator>())
            {
                if (Buttons[SelectedButton] != null)
                {
                    if (Vector3.Distance(Cursor.position, Buttons[SelectedButton].CursorPosition.position) > 1)
                    {
                        CursorAnim.SetBool("Moving", true);
                    }
                    else
                    {
                        CursorAnim.SetBool("Moving", false);
                    }
                }
            }

            if (Buttons[SelectedButton] != null)
                Cursor.position = Vector3.MoveTowards(Cursor.position, Buttons[SelectedButton].CursorPosition.position, (CursorMoveSpeed * Time.unscaledDeltaTime) * (Vector3.Distance(Cursor.position, Buttons[SelectedButton].CursorPosition.position) / (Screen.width / 10)));

            Vector2 MenuNavigation = SkyEngine.Input.Menus.NavigateMenu.ReadValue<Vector2>();

            if (MenuNavigation.y > 0)
            {
                if (!DoneMenuNavigate)
                {
                    if (CursorMoveSound)
                        PlaySound(CursorMoveSound);
                    SelectedButton--;
                    DoneMenuNavigate = true;
                }
            }
            else if (MenuNavigation.y < 0)
            {
                if (!DoneMenuNavigate)
                {
                    if (CursorMoveSound)
                        PlaySound(CursorMoveSound);
                    SelectedButton++;
                    DoneMenuNavigate = true;
                }
            }
            else
            {
                DoneMenuNavigate = false;
            }

            SelectedButton = Mathf.Clamp(SelectedButton, 0, Buttons.Count - 1);

            if (SkyEngine.Input.Menus.Confirm.WasPressedThisFrame())
            {
                Buttons[SelectedButton].Event();
                HandleChoiceProcessing = false;
                Unconfirmed = false;
            }
        }
    }
}
