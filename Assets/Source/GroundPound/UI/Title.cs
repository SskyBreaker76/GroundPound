/*
    Developed by Sky MacLennan
 */

using SkySoft;
using SkySoft.Discord;
using SkySoft.LevelManagement;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Sky.GroundPound
{
    public class Title : MonoBehaviour
    {
        public CanvasGroup Veil;
        public int LogoDuration = 2;
        [Space]
        public Image UserImage;
        public Text UserName;
        [Space]
        [SceneReference, Tooltip("This is very temporary")] public string StartLevel;

        public void LoadLevel()
        {
            LevelManager.LoadLevel(StartLevel, FadeColour.Black, () => { });
        }

        private async void Awake()
        {
            Veil.alpha = 1;
            DiscordAPI.Initialize();

            await Task.Delay(LogoDuration * 1000);

            while (!DiscordAPI.Initialized)
                await Task.Delay(10);

            Game.Initialize(128, () =>
            {
                LeanTween.alphaCanvas(Veil, 0, 1).setOnComplete(() => { Veil.blocksRaycasts = false; Veil.interactable = false; });

                UserImage.sprite = Game.User.Avatar;
                UserName.text = Game.User.Username;
            });
        }
    }
}