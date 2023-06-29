/*
    Developed by Sky MacLennan
 */

using SkySoft;
using SkySoft.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedElement : MonoBehaviour
{
    private Image m_Graphic;
    private Image Graphic
    {
        get
        {
            if (!m_Graphic)
                m_Graphic = GetComponent<Image>();

            return m_Graphic;
        }
    }

    private SpriteRenderer m_Sprite;
    private SpriteRenderer Sprite
    {
        get
        {
            if (!m_Sprite)
                m_Sprite = GetComponent<SpriteRenderer>();

            return m_Sprite;
        }
    }

    public Sprite[] Frames;
    private int m_CurrentFrame;

    public float FrameDuration = 0.5f;

    float TimeLastFrame = 0;
    bool DoneFrameSwap;

    [DisplayOnly] public float Time;

    public void SetFrame(int Frame)
    {
        m_CurrentFrame = Mathf.Clamp(Frame, 0, Frames.Length - 1);
    }

    private void FixedUpdate()
    {
        AudioSource S;

        if (S = BGM.ActiveAudioSource)
        {
            Time = S.time - Mathf.Floor(S.time);
            float UpscaledTime = Time * 10;
            Time = Mathf.Floor(UpscaledTime) / 10f;

            if (Time % FrameDuration == 0)
            {
                if (!DoneFrameSwap)
                {
                    m_CurrentFrame++;
                    if (m_CurrentFrame >= Frames.Length)
                        m_CurrentFrame = 0;
                    DoneFrameSwap = true;
                }
            }
            else
            {
                DoneFrameSwap = false;
            }

            if (Graphic)
                Graphic.sprite = Frames[m_CurrentFrame];
            if (Sprite)
                Sprite.sprite = Frames[m_CurrentFrame];
        }
    }
}
