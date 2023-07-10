using Fusion;
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
                    return;
                }
            }
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