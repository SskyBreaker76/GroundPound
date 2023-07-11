using Fusion;
using SkySoft;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sky.GroundPound
{
    [AddComponentMenu("Ground Pound/Networked Game Properties")]
    public class NetworkedGameProperties : NetworkBehaviour
    {
        [Networked, Capacity(Game.MaxRoomSize)]
        public NetworkArray<PlayerInformation> Players => default;

        private static NetworkedGameProperties m_Instance;
        public static NetworkedGameProperties Instance
        {
            get
            {
                if (!m_Instance)
                    m_Instance = FindFirstObjectByType<NetworkedGameProperties>();

                return m_Instance;
            }
        }

        [Networked] public int TeamCount { get; set; }
        List<int> UniqueTeams = new List<int>();

        [Networked] public int m_JoinType { get; set; }
        public MatchMaker.LobbyPublicity JoinType
        {
            get
            {
                return (MatchMaker.LobbyPublicity)m_JoinType;
            }
            set
            {
                if (!Object.HasStateAuthority)
                    return;

                m_JoinType = (int)value;
            }
        }

        public void TryRegisterPlayer(PlayerRef Player, long DiscordID)
        {
            RPC_TryRegisterPlayer(new PlayerInformation(Player, DiscordID));
        }

        public void TryRegisterPlayer(PlayerInformation Player)
        {
            RPC_TryRegisterPlayer(Player);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        protected void RPC_TryRegisterPlayer(PlayerInformation Player)
        {
            if (!Object.HasStateAuthority) // Just in-case someone somehow bypasses the Targeter
                return;

            for (int I = 0; I < Game.MaxRoomSize; I++)
            {
                if (Players.Get(I).DiscordUserID == 0) 
                {
                    Players.Set(I, Player);
                    RPC_SendJoinLeaveMessage(Player.DiscordUserID, false);
                    return;
                }
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        protected void RPC_SendJoinLeaveMessage(long PlayerID, bool Left)
        {
            if (Left)
                MatchMaker.OnPlayerHasLeft(PlayerID);
            else
                MatchMaker.OnPlayerHasJoined(PlayerID);

            MatchMaker.OnPlayerCountChanged();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_MatchStartInitiated()
        {
            MatchMaker.OnMatchStartInitiated();
        }
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_MatchStartCancelled()
        {
            MatchMaker.OnMatchStartCancelled();
        }
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_MatchStartTick(int Tick)
        {
            MatchMaker.OnMatchStartTick(Tick);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_DisableMatchJoining()
        {
            SkyEngine.SetRichPresence("Online", "In a game...");
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            if (Object.HasStateAuthority)
            {
                UniqueTeams.Clear();

                foreach (PlayerRef Player in GameManager.Teams.Keys)
                {
                    if (!UniqueTeams.Contains(GameManager.Teams[Player]))
                        UniqueTeams.Add(GameManager.Teams[Player]);
                }
                TeamCount = UniqueTeams.Count;
            }
        }
    }
}