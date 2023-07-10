/*
    Developed by Sky MacLennan
 */

using Fusion;
using Fusion.Sockets;
using SkySoft.Discord;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Sky.GroundPound
{
    [System.Serializable]
    public struct PlayerInformation : INetworkStruct
    {
        [Networked]
        public PlayerRef PlayerReference { get => default; set { } }
        [Networked]
        public long DiscordUserID { get => default; set { } }

        public PlayerInformation(PlayerRef PlayerReference, long DiscordUserID)
        {
            this.PlayerReference = PlayerReference;
            this.DiscordUserID = DiscordUserID;
        }
    }

    [AddComponentMenu("Ground Pound/Matchmaker")]
    public class MatchMaker : SimulationBehaviour, INetworkRunnerCallbacks
    {
        public static Action OnPlayerCountChanged { get; set; } = delegate
        {
            if (Instance)
                Instance.m_OnPlayerCountChanged.Invoke();
        };
        public static Action OnPostAwake { get; set; } = delegate { };

        public static string LobbyCode { get; protected set; }
        public static NetworkRunner MainRunner { get; private set; }
        public static MatchMaker Instance { get; private set; }
        public UnityEvent m_OnPlayerCountChanged;

        private void Awake()
        {
            Instance = this;
            OnPostAwake();
        }

        public async void StartGame(Fusion.GameMode LobbyType, Action OnStarted = null)
        {
            LobbyCode = new System.Guid().ToString();

            MainRunner = gameObject.GetComponent<NetworkRunner>();
            if (!MainRunner)
                MainRunner = gameObject.AddComponent<NetworkRunner>();
            MainRunner.ProvideInput = true;
            Runner = MainRunner;

            INetworkSceneManager SceneManager;

            if ((SceneManager = GetComponent<INetworkSceneManager>()) == null)
            {
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
            }

            await Runner.StartGame(new StartGameArgs
            {
                GameMode = LobbyType,
                SessionName = LobbyCode,
                Scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex,
                SceneManager = SceneManager
            });

            OnStarted();
        }

        public void OnPlayerJoined(NetworkRunner Runner, PlayerRef Player)
        {
            if (Player == Runner.LocalPlayer)
                NetworkedGameProperties.Instance.TryRegisterPlayer(Player, DiscordAPI.LocalUserID);
            
            OnPlayerCountChanged();
        }

        public void OnPlayerLeft(NetworkRunner Runner, PlayerRef Player)
        {
            for (int I = 0; I < Game.MaxRoomSize; I++)
                if (NetworkedGameProperties.Instance.Players.Get(I).PlayerReference == Player)
                    NetworkedGameProperties.Instance.Players.Set(I, default);

            OnPlayerCountChanged();
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {;
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
        }

        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
        {
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
        }
    }
}
