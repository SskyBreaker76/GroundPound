using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class ToastHandler : MonoBehaviour
{
    public static ToastHandler Instance;

    private Animator Anim => GetComponent<Animator>();
    private Queue<string> Toasts = new Queue<string>();

    public Text Display;
    public float ToastDuration = 5;
    private float ToastCounter = 0;
    public string 
        ShowAnimation   =   "ToastShow",
        HideAnimation   =   "ToastHide",
        NextAnimation   =   "ToastNext",
        BasisAnimation  =   "ToastBasis";

    private bool IsOpen = false;

    public void UpdateToastText()
    {
        Display.text = Toasts.Dequeue();
        ToastCounter = ToastDuration;
    }

    private void NextMessage()
    {
        if (Toasts.Count > 0) 
        {
            ToastCounter = ToastDuration;
            Anim.Play(NextAnimation);
        }
        else
        {
            Anim.Play(HideAnimation);
            IsOpen = false;
        }
    }

    public void ShowToast(string Message)
    {
        Toasts.Enqueue(Message);

        if (!IsOpen)
        {
            Anim.Play(ShowAnimation);
            IsOpen = true;
        }
    }

    private void Update()
    {
        ToastCounter -= Time.unscaledDeltaTime;

        if (Instance == null || Instance != this)
        {
            Instance = this;
        }

        if (IsOpen)
        {
            if (ToastCounter <= 0)
            {
                ToastCounter = ToastDuration;
                NextMessage();
            }
        }
    }
}
