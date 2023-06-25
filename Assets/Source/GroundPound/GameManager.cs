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

    [AddComponentMenu("Ground Pound/Game Manager")]
    public class GameManager : NetworkBehaviour, IPlayerJoined
    {
        public static GameManager Instance;

        public int PlayerCount = 0;

        [Header("General Settings")]
        public Camera MainCamera;
        public GameObject PlayerPrefab;
        public Transform[] Spawns;

        [Header("Session Settings")]
        public GameMode ActiveGameMode;
        public Vector2 CameraMin, CameraMax;

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
                    Runner.Spawn(PlayerPrefab, Spawns[PlayerCount].position, Quaternion.identity, Target);
                else
                    Debug.Log("Spawn Spectator"); // TODO[Sky] Implement spectator system
            }
        }

        private void DrawBounds(Vector2 Min, Vector2 Max)
        {
            Gizmos.DrawLine(new Vector3(Min.x, Min.y), new Vector3(Max.x, Min.y));
            Gizmos.DrawLine(new Vector3(Min.x, Max.y), new Vector3(Max.x, Max.y));
            Gizmos.DrawLine(new Vector3(Min.x, Min.y), new Vector3(Min.x, Max.y));
            Gizmos.DrawLine(new Vector3(Max.x, Max.y), new Vector3(Max.x, Min.y));
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            if (Spawns.Length > 0)
                foreach (Transform Spawn in Spawns)
                    SkyEngine.Gizmos.DrawCircle(Spawn.position, 1);

            if (MainCamera)
            {
                Gizmos.color = Color.red;
                DrawBounds(CameraMin, CameraMax);

                float OrthoSize = MainCamera.orthographicSize * (SkyEngine.AspectRatio * 1.215f); // It just works

                Vector2 ViewMin = new Vector2(CameraMin.x - OrthoSize, CameraMin.y - (OrthoSize / 1.775f)); // IT JUST WORKS
                Vector2 ViewMax = new Vector2(CameraMax.x + OrthoSize, CameraMax.y + (OrthoSize / 1.775f)); // I T   J U S T   W O R K S

                Gizmos.color = Color.yellow;
                DrawBounds(ViewMin, ViewMax);
            }
        }

        [BehaviourButtonAction("Set CameraMin to current Camera position")]
        public void SetMinPosition()
        {
            if (Camera.main)
            {
                CameraMin = Camera.main.transform.position;
            }
        }

        [BehaviourButtonAction("Set CameraMax to current Camera position")]
        public void SetMaxPosition()
        {
            if (Camera.main)
            {
                CameraMax = Camera.main.transform.position;
            }
        }

        public void PlayerJoined(PlayerRef Player)
        {
            if (Player == Runner.LocalPlayer)
                RPC_RequestServerInf(Player);
        }
    }
}