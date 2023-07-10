using Discord;
using SkySoft;
using SkySoft.Discord;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sky.GroundPound 
{
    [AddComponentMenu("Ground Pound/Lobby Interface")]
    public class LobbyUI : MonoBehaviour
    {
        public GameObject UserWidget;
        public RectTransform DisplayRoot;
        public Color HostColour = Color.green;
        public Color LocalPlayerColour = Color.white;
        public Color NormalColour = Color.grey;

        private Dictionary<long, GameObject> SpawnedWidgets = new Dictionary<long, GameObject>();

        private void Awake()
        {
            // Because the MatchMaker is required for the setup process, we should make sure it exists before we setup

            if (!MatchMaker.Instance)
                MatchMaker.OnPostAwake += Setup;
            else
                Setup();
        }

        private void Setup()
        {
            MatchMaker.OnPlayerCountChanged += LobbyCountUpdate;
        }

        private void GenerateUser(int UserIndex)
        {
            PlayerInformation Target = NetworkedGameProperties.Instance.Players.Get(UserIndex);

            if (Target.DiscordUserID != 0) // To whoever has the Discord ID "0" and can't play this game, I shall give you 50 Australian Cents
            {
                DiscordAPI.UserManager.GetUser(Target.DiscordUserID, new Discord.UserManager.GetUserHandler(OnGotUserForGeneration));
            }
        }

        private void OnGotUserForGeneration(Result Result, ref User User)
        {
            if (Result == Result.Ok)
            {
                GameObject Widget = Instantiate(UserWidget, DisplayRoot);
                DiscordAPI.GetAvatar(User, Tex =>
                {
                    Widget.transform.GetChild(0).GetComponentInChildren<Image>().sprite = Sprite.Create(Tex, new Rect(0, 0, Tex.width, Tex.height), Vector2.one * 0.5f);
                });
                Widget.GetComponentInChildren<Text>().text = User.Username;
                Widget.GetComponentInChildren<Text>().color = User.Id == DiscordAPI.LocalUserID ? LocalPlayerColour : (User.Id == NetworkedGameProperties.Instance.Players.Get(0).DiscordUserID ? HostColour : NormalColour);
            }
        }

        private void LobbyCountUpdate() 
        { 
            for (int I = 0; I < Game.MaxRoomSize; I++)
            {
                GenerateUser(I);
            }
        }
    }
}