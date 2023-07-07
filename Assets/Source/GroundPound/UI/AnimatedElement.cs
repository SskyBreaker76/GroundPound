/*
    Developed by Sky MacLennan
 */

using Sky.GroundPound;
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
    public float FrameDuration;

    private float TimeLastFrame = 0;
    private bool DoneFrameSwap;

    [DisplayOnly] public float Time;

    public void SetFrame(int Frame)
    {
        m_CurrentFrame = Mathf.Clamp(Frame, 0, Frames.Length - 1);
    }

    private float Addition = 0;

    private void StepFrame()
    {
        m_CurrentFrame++;
        if (m_CurrentFrame >= Frames.Length)
            m_CurrentFrame = 0;
        DoneFrameSwap = true;
        Addition += Game.BeatLength * FrameDuration;
    }

    private void Update()
    {
        AudioSource S;

        if (S = BGM.ActiveAudioSource)
        {
            Time = S.time - Mathf.Floor(S.time);
            bool ResetV = Time < TimeLastFrame;
            TimeLastFrame = Time;
            float UpscaledTime = Time * 100;
            Time = Mathf.Floor(UpscaledTime) / 100f;

            if (ResetV)
            {
                if (Game.BeatLength * FrameDuration % 1 == 0)
                {
                    StepFrame();
                }

                Addition = Time * Game.BeatLength;
            }

            if (Time > ((Game.BeatLength * FrameDuration) + Addition))
            {
                if (!DoneFrameSwap)
                {
                    StepFrame();
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
