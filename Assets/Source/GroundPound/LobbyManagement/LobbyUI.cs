using Discord;
using Fusion;
using SkySoft;
using SkySoft.Discord;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public InputField CodeField;

        public async void CopyCode(Text Display)
        {
            GUIUtility.systemCopyBuffer = MatchMaker.LobbyCode;
            Display.text = "Copied!";
            await Task.Delay(1500);
            Display.text = "Copy Code";
            
        }

        private void Awake()
        {
            // Because the MatchMaker is required for the setup process, we should make sure it exists before we setup

            if (!MatchMaker.Instance)
                MatchMaker.OnPostAwake += Setup;
            else
                Setup();
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_SendMessage(string Sender, string Message)
        {
            string Colour = Sender == "|System|" ? "magenta" : "yellow";
            Log.text += $"{(string.IsNullOrEmpty(Log.text) ? "" : "\n")}<<color={Colour}>{Sender.Replace("|", "")}</color>> - {Message}";
        }

        private void Setup()
        {
            MatchMaker.OnPlayerCountChanged += UpdateLobbyView;
            MatchMaker.OnPlayerHasJoined += Value => { LobbyCountUpdate(Value, false); };
            MatchMaker.OnPlayerHasLeft += Value => { LobbyCountUpdate(Value, true); };
        }

        public void SendChatMessage(InputField MessageBox)
        {
            RPC_SendMessage(DiscordAPI.UserManager.GetCurrentUser().Username, MessageBox.text);
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

        public GameObject InviteOverlayBase;
        public GameObject UserInviteWidget;
        public RectTransform UserInviteRoot;

        public Sprite OnlineIndicator, IdleIndicator, DoNotDisturbIndicator;

        private List<Status> OnlineStates = new List<Status>
        {
            Status.Online,
            Status.Idle,
            Status.DoNotDisturb
        };

        public bool FilterOnline(ref Relationship Relationship)
        {
            return OnlineStates.Contains(Relationship.Presence.Status);
        }

        public void OpenInviteOverlay()
        {
            DiscordAPI.OverlayManager.OpenActivityInvite(ActivityActionType.Join, (Result) =>
            {
                Debug.Log("Inviting Users");
            });
        }

        public Text Log;

        public string JoinLeave;

        private void OnGotUser(Result Result, ref User User)
        {
            string Starter = "";

            if (!string.IsNullOrEmpty(Log.text))
                Starter = "\n";

            Log.text += $"{Starter}<<color=magenta>System</color>> - '<color=yellow>{User.Username}</color>' has {JoinLeave} the game!";
        }

        public void UpdateLobbyView()
        {
            CodeField.text = MatchMaker.LobbyCode;

            foreach (Transform T in DisplayRoot)
                Destroy(T.gameObject);

            int PlayerCount = 0;

            for (int I = 0; I < Game.MaxRoomSize; I++)
            {
                GenerateUser(I);
            }

            SkyEngine.SetLobbyDetails(MatchMaker.LobbyCode, (int)NetworkedGameProperties.Instance.JoinType, PlayerCount, NetworkedGameProperties.Instance.TeamCount);
        }

        private void LobbyCountUpdate(long Player, bool Left)
        {
            JoinLeave = Left ? "left" : "joined";
            DiscordAPI.UserManager.GetUser(Player, OnGotUser);

            UpdateLobbyView();
        }
    }
}