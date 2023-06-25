using Steamworks;
using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkySoft.Interaction;
using SkySoft.IO;
using System.IO;

namespace SkySoft.Steam
{
    [AddComponentMenu("SkyEngine/Steam/Steam Hook")]
    public class Steamhook : MonoBehaviour
    {
        public static Steamhook Instance;
        public const uint AppID = 1637020;
        public static bool IsOnline { get; private set; }

        /// <summary>
        /// Returns 0 if the user is offline
        /// </summary>
        public static int LobbyMemberCount
        {
            get
            {
                return IsOnline ? CurrentLobby.MemberCount : 0;
            }
        }

        public static Steamworks.Data.Lobby CurrentLobby { get; private set; }
        public static Dictionary<string, Texture2D> CachedTextures = new Dictionary<string, Texture2D>();
        public static bool Initialized { get; private set; } = false;

        private void OnDestroy()
        {
            SteamClient.Shutdown();

            foreach (FileInfo Inf in FileManager.GetAllFiles("Loot"))
            {
                FileManager.ReadFile<LootBoxSave>("Loot", Inf.Name.Replace(".loot", "").Replace("loot", ""), Save =>
                {
                    if (Save.Temp)
                    {
                        File.Delete(Inf.FullName);
                    }
                }, ".loot");
            }
        }

        public static void Dispose()
        {
            SteamClient.Shutdown();
        }

        public static bool HasSteam { get; private set; }

        public static void Initialize(Action OnFailedToConnect = null, Action OnSuccess = null, UnityEngine.UI.Text Display = null, Action Finished = null)
        {
            if (!Initialized)
            {
                Instance = new GameObject("SteamhookInstance").AddComponent<Steamhook>();
                DontDestroyOnLoad(Instance.gameObject);

                SteamClient.Init(AppID, true);

                HasSteam = true;

                SteamMatchmaking.OnLobbyMemberJoined += (Lobby, Client) =>
                {
                };

                SteamMatchmaking.OnLobbyCreated += (Result, Lobby) =>
                {
                    if (Result == Result.OK)
                    {
                        CurrentLobby = Lobby;
                        SteamNetworking.AllowP2PPacketRelay(true);
                        CurrentLobby.SetPublic();
                        CurrentLobby.SetJoinable(true);
                        CurrentLobby.Join();
                        IsOnline = true;

                        if (Display)
                        {
                            Display.text = "All Connected!";
                            OnSuccess();
                        }
                    }
                    else
                    {
                        Debug.Log($"Couldn't start internal Server. Reason: {Result}");
                        IsOnline = false;

                        if (Display)
                        {
                            Display.text = "Failed to start internal server!\nPress START to continue...";
                            OnFailedToConnect();
                        }
                    }
                };

                if (Display)
                    Display.text = "Starting internal server...";

                CreateLobby(() =>
                {
                }, true);

                Initialized = true;
            }

            if (Finished != null)
                Finished();
        }

        public static void CreateLobby(Action OnComplete, bool SkipInit = false)
        {
            if (!SkipInit)
                Initialize();

            Debug.Log("Creating Lobby!");
            SteamMatchmaking.CreateLobbyAsync(4);
        }

        public static async void JoinLobby(SteamId LobbyID, Action<bool> OnComplete)
        {
            Initialize();

            Steamworks.Data.Lobby? Lobby = await SteamMatchmaking.JoinLobbyAsync(LobbyID);

            if (Lobby.HasValue)
            {
                CurrentLobby = Lobby.Value;
                OnComplete(true);
            }

            OnComplete(false);
        }

        public static async Task<Texture2D> GetProfilePicture(SteamId UserID, Action<Texture2D> OnComplete)
        {
            Initialize();

            if (CachedTextures.ContainsKey($"{UserID}.Avatar"))
            {
                OnComplete(CachedTextures[$"{UserID}.Avatar"]);
                return CachedTextures[$"{UserID}.Avatar"];
            }
            else
            {
                Friend F = new Friend(UserID);

                Steamworks.Data.Image? Image = await F.GetLargeAvatarAsync();

                if (Image.HasValue)
                {
                    Texture2D T2D = new Texture2D((int)Image.Value.Width, (int)Image.Value.Height);

                    for (int X = 0; X < Image.Value.Width; X++)
                    {
                        for (int Y = 0; Y < Image.Value.Height; Y++)
                        {
                            Steamworks.Data.Color Colour = Image.Value.GetPixel(X, Y);
                            T2D.SetPixel(X, Y, new Color(Colour.r / 255f, Colour.g / 255f, Colour.b / 255f, Colour.a / 255f));
                        }
                    }

                    T2D.filterMode = FilterMode.Point;
                    T2D.Apply();

                    CachedTextures.Add($"{UserID}.Avatar", T2D);
                    OnComplete(T2D);
                    return T2D;
                }
            }

            return null;
        }
        public static string LocalUsername => SteamClient.Name;
        public static SteamId LocalUserID => SteamClient.SteamId;

        public static Friend[] GetFriends()
        {
            List<Friend> Friends = new List<Friend>();

            foreach (Friend F in SteamFriends.GetFriends())
            {
                Friends.Add(F);
            }

            return Friends.ToArray();
        }

        public static bool SetRichPresence(string Text)
        {
            if (SteamFriends.SetRichPresence("Status", Text))
            {
                SteamFriends.SetRichPresence("steam_display", "#MainStatus");

                SteamFriends.SetRichPresence("steam_player_group", CurrentLobby.Id.ToString());
                SteamFriends.SetRichPresence("steam_player_group_size", CurrentLobby.MemberCount.ToString());
                return true;
            }

            return false;
        }
    }
}