using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sky.GroundPound
{
    [AddComponentMenu("Ground Pound/Game Manager UI")]
    public class GameManagerUI : MonoBehaviour
    {
        private GameManager GameManager => GameManager.Instance;

        [Header("Pip UI")]
        public GameObject PipUI_Root;
        public Image PipUI_Crown;
        public GameObject PipUI_Pip;
        public RectTransform PipUI_LeftScores;
        public RectTransform PipUI_RightScores;

        [Header("Numbers UI")]
        public GameObject NumbersUI_Root;
        public Text NumbersUI_Display;

        public void UpdateUI()
        {
            PipUI_Root.SetActive(GameManager.GameSettings.ScoringMode == ScoringSystem.Pips);
            NumbersUI_Root.SetActive(GameManager.GameSettings.ScoringMode == ScoringSystem.Number);

            switch (GameManager.GameSettings.ScoringMode)
            {
                case ScoringSystem.Pips:
                    {
                        foreach (Transform T in PipUI_LeftScores)
                        {
                            Destroy(T.gameObject);
                        }
                        foreach (Transform T in PipUI_RightScores)
                        {
                            Destroy(T.gameObject);
                        }

                        for (int I = 0; I < GameManager.GameSettings.RequiredWins; I++)
                        {
                            RectTransform Root = Instantiate(PipUI_Pip, PipUI_LeftScores).transform as RectTransform;
                            Root.GetChild(0).GetComponent<Image>().color = I < GameManager.Scores.GetScore(0) ? GameManager.GetTeamColour(0) : Color.clear;

                            Root = Instantiate(PipUI_Pip, PipUI_RightScores).transform as RectTransform;
                            Root.GetChild(0).GetComponent<Image>().color = I < GameManager.Scores.GetScore(1) ? GameManager.GetTeamColour(1) : Color.clear;
                        }
                    }
                    break;
                case ScoringSystem.Number:
                    {
                        int WinningTeamIndex = 0;
                        int WinningTeamScore = 0;

                        for (int I = 0; I < NetworkedGameProperties.Instance.TeamCount; I++)
                        {
                            int Score;
                            if ((Score = GameManager.Scores.GetScore(I)) > WinningTeamScore)
                            {
                                WinningTeamIndex = I;
                                WinningTeamScore = Score;
                            }
                        }

                        if (WinningTeamScore > 0)
                        {
                            NumbersUI_Display.color = GameManager.GetTeamColour(WinningTeamIndex);
                            NumbersUI_Display.text = GameManager.Scores.GetScore(WinningTeamIndex).ToString();
                        }
                        else
                        {
                            NumbersUI_Display.color = Color.white;
                            NumbersUI_Display.text = "0";
                        }
                    }
                    break;
            }
        }
    }
}