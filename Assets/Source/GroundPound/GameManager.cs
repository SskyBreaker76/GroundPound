/*
    Developed by Sky MacLennan
 */

using Fusion;
using Fusion.Sockets;
using SkySoft;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public class GameManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        private Dictionary<PlayerRef, NetworkObject> m_LocalSpawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();
        private Dictionary<PlayerRef, NetworkObject> m_SpawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();
        public static GameManager Instance;

        private NetworkRunner Runner;
        public int PlayerCount = 0;
        public bool EnableGUI = true;

        [Header("General Settings")]
        public Camera MainCamera;
        public NetworkPrefabRef PlayerPrefab;
        public Transform[] Spawns;

        [Header("Session Settings")]
        public GameMode ActiveGameMode;
        public Vector2 CameraMin, CameraMax;
        public float DeathDistance = 12;

        public Vector2 MapCentre
        {
            get
            {
                return Vector2.Lerp(CameraMin, CameraMax, 0.5f);
            }
        }

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

        public void GetViewableArea(out Vector2 Minimum, out Vector2 Maximum)
        {
            float OrthoSize = MainCamera.orthographicSize * (SkyEngine.AspectRatio * 1.215f); // It just works

            Minimum = new Vector2(CameraMin.x - OrthoSize, CameraMin.y - (OrthoSize / 1.775f)); // IT JUST WORKS
            Maximum = new Vector2(CameraMax.x + OrthoSize, CameraMax.y + (OrthoSize / 1.775f)); // I T   J U S T   W O R K S
        }

        public void GetPlayableArea(out Vector2 Minimum, out Vector2 Maximum)
        {
            GetViewableArea(out Vector2 Min, out Vector2 Max);
            Min -= Vector2.one * DeathDistance;
            Max += Vector2.one * DeathDistance;
            Minimum = Min;
            Maximum = Max;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;

            if (Spawns.Length > 0)
                foreach (Transform Spawn in Spawns)
                    SkyEngine.Gizmos.DrawCircle(Spawn.position, 0.25f);

            Gizmos.color = Color.blue;
            DrawBounds(CameraMin, CameraMax);

            if (MainCamera)
            {
                GetViewableArea(out Vector2 ViewMin, out Vector2 ViewMax);

                Gizmos.color = Color.cyan;
                DrawBounds(ViewMin, ViewMax);

                GetPlayableArea(out ViewMin, out ViewMax);

                Gizmos.color = Color.red;
                DrawBounds(ViewMin, ViewMax);
            }
        }

        private void OnDrawGizmosSelected()
        {
            SkyEngine.Gizmos.Colour = Color.white;
            SkyEngine.Gizmos.DrawCircle(MapCentre, 0.25f);
        }

        public bool IsPlayerInBounds(NetworkObject Player)
        {
            Player Plr;

            if (Plr = Player.GetComponent<Player>())
            {
                return IsPlayerInBounds(Plr);
            }

            return true;
        }

        public bool IsPlayerInBounds(Player Plr)
        {
            GetPlayableArea(out Vector2 Min, out Vector2 Max);

            Vector2 MinNearest = Plr.NearestPoint(Min);
            Vector2 MaxNearest = Plr.NearestPoint(Max);

            return (MinNearest.x > Min.x && MaxNearest.x < Max.x &&
                MinNearest.y > Min.y && MaxNearest.y < Max.y);
        }

        public bool IsPlayerInBounds(NetworkObject Player, out Player PlayerComponent)
        {
            Player Plr;
            if (Plr = Player.GetComponent<Player>())
            {
                PlayerComponent = Plr;
                return IsPlayerInBounds(Plr);
            }

            PlayerComponent = null;
            return true;
        }

        private void Update()
        {
            if (!Runner) return;

            if (Runner.IsServer)
            {
                foreach (NetworkObject Player in m_SpawnedPlayers.Values)
                {
                    if (!IsPlayerInBounds(Player, out Player Plr))
                    {
                        if (Plr.Hitpoints > 0)
                            Plr.RPC_Kill();
                    }
                }
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

        private void OnGUI()
        {
            if (!Runner && EnableGUI)
            {
                if (GUI.Button(new Rect(0, 0, 200, 40), "Host"))
                {
                    StartGame(Fusion.GameMode.Host);
                }
                if (GUI.Button(new Rect(0, 40, 200, 40), "Join"))
                {
                    StartGame(Fusion.GameMode.Client);
                }
            }
        }
        
        public void StartHost()
        {
            StartGame(Fusion.GameMode.Host);
        }

        public void StartClient()
        {
            StartGame(Fusion.GameMode.Client);
        }

        public void ResetLevel()
        {
            // TODO[Sky] Implement ResetLevel behaviour
        }

        protected async void StartGame(Fusion.GameMode Mode, string LobbyCode = "Default")
        {
            Runner = gameObject.GetComponent<NetworkRunner>();
            Runner.ProvideInput = true;

            await Runner.StartGame(new StartGameArgs()
            {
                GameMode = Mode,
                SessionName = LobbyCode,
                Scene = SceneManager.GetActiveScene().buildIndex,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });
        }

        public void PlayerJoined(PlayerRef Player)
        {
            if (Player == Runner.LocalPlayer)
                RPC_RequestServerInf(Player);
        }

        public void OnPlayerJoined(NetworkRunner Runner, PlayerRef Player)
        {
            if (Runner.IsServer)
            {
                if (m_SpawnedPlayers.Count < Spawns.Length)
                {
                    Vector3 SpawnPosition = Spawns[m_SpawnedPlayers.Count].position;
                    NetworkObject PlayerObject = Runner.Spawn(PlayerPrefab, SpawnPosition, inputAuthority: Player);
                    m_SpawnedPlayers.Add(Player, PlayerObject);
                }
            }
        }

        public void OnPlayerLeft(NetworkRunner Runner, PlayerRef Player)
        {
            if (m_SpawnedPlayers.TryGetValue(Player, out NetworkObject PlayerObject))
            {
                Runner.Despawn(PlayerObject);
                m_SpawnedPlayers.Remove(Player);
            }
        }

        public void OnInput(NetworkRunner Runner, NetworkInput Input)
        {
            GroundPoundInputData Data = new GroundPoundInputData();

            Vector2 I = SkyEngine.Input.Gameplay.Move.ReadValue<Vector2>();
            float Horizontal = I.x;

            Data.Direction = Horizontal;
            
            Data.mJump = SkyEngine.Input.Gameplay.Jump.IsPressed() || I.y > 0 ? Player.BUTTON_ON : Player.BUTTON_OFF;
            Data.mGroundPound = I.y < 0 ? Player.BUTTON_ON : Player.BUTTON_OFF;
            Data.mDash = SkyEngine.Input.Gameplay.Dash.IsPressed() ? Player.BUTTON_ON : Player.BUTTON_OFF;

            // TODO[Sky] Implement the "force bolt" ability

            Input.Set(Data);
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {

        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {

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

        public void OnSceneLoadStart(NetworkRunner run)
        {
            
        }
    }
}