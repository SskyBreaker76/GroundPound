/*
    Developed by Sky MacLennan
 */

using Fusion;
using SkySoft;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sky.GroundPound
{
    /// <summary>
    /// Basic GameMode flags. We can add more later
    /// </summary>
    public enum GameMode
    {
        Deathmatch,
        TeamDeathmatch,
        CaptureTheFlag,
        KingOfTheHill
    }

    public class GameManager : NetworkBehaviour, IPlayerJoined
    {
        public static GameManager Instance;

        public int PlayerCount = 0;

        [Header("General Settings")]
        public GameObject PlayerPrefab;
        public Transform[] Spawns;

        [Header("Session Settings")]
        public GameMode ActiveGameMode;

        private void Awake()
        {
            Instance = this;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_TickPlayerCount(bool Subtract)
        {
            PlayerCount += Subtract ? -1 : 1;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        protected virtual void RPC_RequestServerInf(PlayerRef Requester)
        {
            RPC_ReceiveServerInf(Requester, (int)ActiveGameMode, PlayerCount);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        protected virtual void RPC_ReceiveServerInf(PlayerRef Target, int GameMode, int PlayerCount)
        {
            if (Runner.LocalPlayer == Target) // We don't want to execute any code on non-target machines
            {
                ActiveGameMode = (GameMode)GameMode;
                this.PlayerCount = PlayerCount;

                // Here's where we spawn our player. Only doing this because we KNOW our player is going to be the right one
                if (PlayerCount < Spawns.Length)
                {
                    Runner.Spawn(PlayerPrefab, Spawns[PlayerCount].position, Quaternion.identity, Target);
                }
                else
                {
                    // Implement spectator spawning
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            if (Spawns.Length > 0)
                foreach (Transform Spawn in Spawns)
                    SkyEngine.Gizmos.DrawCircle(Spawn.position, 1);
        }

        public void PlayerJoined(PlayerRef Player)
        {
            if (Player == Runner.LocalPlayer)
            {
                RPC_RequestServerInf(Player);
            }
        }
    }
}