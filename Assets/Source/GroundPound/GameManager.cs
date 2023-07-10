/*
    Developed by Sky MacLennan
 */

// TODO[Sky]: Please convert the GameManager so it ONLY manages gameplay, no Network Lobby code >:{

using Discord;
using Fusion;
using Fusion.Sockets;
using SkySoft;
using SkySoft.LevelManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sky.GroundPound
{
    /// <summary>
    /// Basic GameMode flags. We can add more later
    /// </summary>
    public enum GameMode
    {
        Standard,
        ThreeInARow,
        SuperSaiyan,
        Custom
    }

    public enum ScoringSystem
    {
        Pips,
        Number
    }

    public enum LevelGainMode
    {
        Reset,
        Lose,
        None,
        Gain
    }

    public enum WinMode
    {
        Total,
        Sequential,
        BestOf
    }

    public enum ThreeWayMode
    {
        WinnerStaysTagged,
        LoserStaysTagged
    }

    [System.Serializable]
    public class GameModeSettings
    {
        public GameMode BaseGameMode;
        public WinMode WinningMode;
        public ScoringSystem ScoringMode;
        public int BaseLevel;
        public int RequiredWins;
        public LevelGainMode LevellingOnLose;
        public LevelGainMode LevellingOnWin;
        public float GravityScale;
        public float GlobalForceBuff;
        public float GlobalRangeBuff;
        public float GlobalMoveSpeedBuff;
        public float GlobalJumpForceBuff;
        public float AbilityCooldownMultiplier;
        public float StunDurationMultiplier;
        public float UltimateWindUpStageMultiplier;
        public float GlobalUltimatePowerBuff;
        public float GlobalUltimateRangeBuff;
        public int UltimateCharges;
        public int PlayerCount;
        public bool ThreeWay;
        public ThreeWayMode ThreeWayMode;

        protected static GameModeSettings m_StandardMode = new GameModeSettings
        {
            BaseGameMode = GameMode.Standard,
            WinningMode = WinMode.Total,
            ScoringMode = ScoringSystem.Pips,
            BaseLevel = 0,
            RequiredWins = 5,
            LevellingOnLose = LevelGainMode.Gain,
            LevellingOnWin = LevelGainMode.Reset,
            GravityScale = 1,
            GlobalForceBuff = 0, 
            GlobalRangeBuff = 0,
            GlobalMoveSpeedBuff = 0,
            GlobalJumpForceBuff = 0,
            AbilityCooldownMultiplier = 1,
            StunDurationMultiplier = 1,

            PlayerCount = 2,
            ThreeWay = false
        };
        public static GameModeSettings StandardMode => m_StandardMode;

        protected static GameModeSettings m_ThreeInARow = new GameModeSettings
        {
            BaseGameMode = GameMode.ThreeInARow,
            WinningMode = WinMode.Sequential,
            ScoringMode = ScoringSystem.Number,
            BaseLevel = 1,
            RequiredWins = 3,
            LevellingOnLose = LevelGainMode.Reset,
            LevellingOnWin = LevelGainMode.Gain,
            GravityScale = 1,
            GlobalForceBuff = 0,
            GlobalRangeBuff = 0,
            PlayerCount = 2,
            ThreeWay = false
        };
        public static GameModeSettings ThreeInARow => m_ThreeInARow;

        protected static GameModeSettings m_SuperSaiyan = new GameModeSettings
        {
            BaseGameMode = GameMode.SuperSaiyan,
            WinningMode = WinMode.Total,
            ScoringMode = ScoringSystem.Pips,
            BaseLevel = 4,
            RequiredWins = 3,
            LevellingOnLose = LevelGainMode.None,
            LevellingOnWin = LevelGainMode.None,
            GravityScale = 1,
            GlobalForceBuff = 0,
            GlobalRangeBuff = 0,
            PlayerCount = 2,
            ThreeWay = false
        };
        public static GameModeSettings SuperSaiyan => m_SuperSaiyan;
    }

    [System.Serializable]
    public class MatchInformation
    {
        [Obsolete("Use ModifyScore, SetScore, ClearScore and GetScore")]
        public int Team0_Wins, Team1_Wins;
        [Obsolete("Use ModifyLevel, SetLevel, ClearLevel and GetLevel")]
        public int Team0_Level, Team1_Level;

        private Dictionary<int, int> Scoreboard = new Dictionary<int, int>();

        /// <summary>
        /// Modifies a given team's score by value
        /// </summary>
        /// <param name="Team"></param>
        /// <param name="Value"></param>
        public void ModifyScore(int Team, int Value)
        {
            if (!Scoreboard.ContainsKey(Team)) // Checks if a team exists. If not, we'll add them.
                SetScore(Team, Value); // Why create, when you can recycle :D
            else
                Scoreboard[Team] += Value; // If team exists, it's faster to directly modify the score than to use SetScore(Team, GetScore(Team) + Value);
        }

        /// <summary>
        /// Sets a given team's score to the provided value
        /// </summary>
        /// <param name="Team"></param>
        /// <param name="Score"></param>
        public void SetScore(int Team, int Score)
        {
            if (!Scoreboard.ContainsKey(Team)) // Check if the team exists. If not, we'll add them.
                Scoreboard.Add(Team, Score);
            else // Team exists, just set their score
                Scoreboard[Team] = Score;
        }

        /// <summary>
        /// Clears the score of a given team
        /// </summary>
        /// <param name="Team"></param>
        public void ClearScore(int Team)
        {
            /*
            if (Scoreboard.ContainsKey(Team)) // Saves about 8 bytes of RAM until the player gets a score
                Scoreboard[Team] = 0;
            */

            SetScore(Team, 0); // Oh no, the poor 8 bytes of RAM I saved! But woah, more recycling!
        }

        public int GetScore(int Team)
        {
            if (!Scoreboard.ContainsKey(Team))
                return 0; // No team found, just return 0 to save memory

            return Scoreboard[Team]; // Because the if statement above checks if the scoreboard hasn't got a team and returns 0 when true, we can assume that the Team exists in the scoreboard and not nullcheck here.
                                     // Saved like, a microsecond at most xD
        }

        private Dictionary<int, int> Levels = new Dictionary<int, int>();

        public void ModifyLevel(int Team, int Value)
        {
            if (!Levels.ContainsKey(Team))
                SetLevel(Team, GameManager.GameSettings.BaseLevel + Value);
            else
                Levels[Team] += Value;
        }

        public void SetLevel(int Team, int Level)
        {
            if (!Levels.ContainsKey(Team))
                Levels.Add(Team, Level);
            else
                Levels[Team] = Level;
        }

        public void ResetLevel(int Team)
        {
            SetLevel(Team, GameManager.GameSettings.BaseLevel);
        }

        public int GetLevel(int Team)
        {
            if (!Levels.ContainsKey(Team))
                return GameManager.GameSettings.BaseLevel;
            else
                return Levels[Team];
        }
    }

    [AddComponentMenu("Ground Pound/Game Manager")]
    public class GameManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        public static Dictionary<PlayerRef, int> Teams = new Dictionary<PlayerRef, int>();

        public static List<Color> BaseColours = new List<Color>()
        {
            new Color(1, 0.360784313725f, 0),
            new Color(0, 0.611764705882f, 1)
        };
        public static List<Color> SuperColours = new List<Color>()
        {
            new Color(1, 0, 0),
            new Color(0.235294117647f, 0.062745098039f, 1)
        };

        public static Color GetTeamColour(int Team)
        {
            int PlayerLevel = GameManager.Instance.Scores.GetLevel(Team);
            float PlayerLevel01 = PlayerLevel / 4f;
            float Time = PlayerLevel01 + (PlayerLevel <= 1 ? 0.25f : 0);
            return Color.Lerp(BaseColours[Team], SuperColours[Team], Time);
        }

        private static bool ReceivedSignal = false;
        private static int ReceivedIndex = 0;
        private static int ReceivedTeam = 0;
        private static int ReceivedScore = 0;

        public static async void GetPlayerInformation(PlayerRef Player, Action<int, int, int> OnComplete)
        {
            RPC_RequestPlayerIndex(Player, GroundPound.Player.LocalPlayer.Object.InputAuthority);
            while (!ReceivedSignal)
                await Task.Yield();
            OnComplete(ReceivedIndex, ReceivedTeam, ReceivedScore);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private static void RPC_RequestPlayerIndex(PlayerRef Player, PlayerRef Requester)
        {
            for (int I = 0; I < Teams.Count; I++)
            {
                if (Teams.Keys.ToArray()[I] == Player)
                {
                    RPC_ReceivePlayerIndex(Requester, I, Teams[Player], Instance.Scores.GetScore(Teams[Player]));
                }
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private static void RPC_ReceivePlayerIndex(PlayerRef Target, int Index, int Team, int Score)
        {
            if (Target == Player.LocalPlayer.Object.InputAuthority)
            {
                ReceivedIndex = Index;
                ReceivedTeam = Team;
                ReceivedScore = Score;
                ReceivedSignal = true;
            }
        }


        public static GameModeSettings GameSettings { get; private set; } = GameModeSettings.StandardMode;
        public MatchInformation Scores = new MatchInformation();

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_SyncScoring(int Team, int Score)
        {
            Scores.SetScore(Team, Score);
        }

        private Dictionary<PlayerRef, NetworkObject> m_LocalSpawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();
        private Dictionary<PlayerRef, NetworkObject> m_SpawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();
        public static GameManager Instance;

        public int PlayerCount { get; private set;  }

        private NetworkRunner Runner;
        public bool EnableGUI = true;

        [Header("General Settings")]
        public Camera MainCamera;
        public NetworkPrefabRef PlayerPrefab;
        public Transform[] Spawns;
        public float ReloadDelay = 4;
        public float FadeoutLength = 0.5f;
        public float StartDelay = 3; // Classic

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

            if (ActiveGameMode != GameMode.Custom)
            {
                switch (ActiveGameMode)
                {
                    case GameMode.Standard:
                        GameSettings = GameModeSettings.StandardMode;
                        break;
                    case GameMode.SuperSaiyan:
                        GameSettings = GameModeSettings.SuperSaiyan;
                        break;
                    case GameMode.ThreeInARow:
                        GameSettings = GameModeSettings.ThreeInARow;
                        break;
                }    
            }
        }

        private enum Side
        {
            Top,
            Bottom,
            Left,
            Right
        }

        private void DrawBounds(Vector2 Min, Vector2 Max, params Side[] IgnoreSides)
        {
            List<Side> Ignored = IgnoreSides.ToList();

            if (!Ignored.Contains(Side.Bottom))
                Gizmos.DrawLine(new Vector3(Min.x, Min.y), new Vector3(Max.x, Min.y));

            if (!Ignored.Contains(Side.Top))
                Gizmos.DrawLine(new Vector3(Min.x, Max.y), new Vector3(Max.x, Max.y));

            if (!Ignored.Contains(Side.Left))
                Gizmos.DrawLine(new Vector3(Min.x, Min.y), new Vector3(Min.x, Max.y));

            if (!Ignored.Contains(Side.Right))
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
                DrawBounds(ViewMin, ViewMax, Side.Top);
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

        public bool IsSpriteInBounds(SpriteRenderer Sprite)
        {
            GetPlayableArea(out _, out Vector2 Max);

            Vector3 MaxNearest = Sprite.transform.position + Sprite.bounds.max;

            return (MaxNearest.x < Max.x &&
                MaxNearest.y < Max.y);
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

        public static bool m_PauseMatch;
        public static bool PauseMatch
        {
            get
            {
                return m_PauseMatch;
            }
            private set
            {
                if (Instance.Runner.IsServer)
                {
                    m_PauseMatch = value;
                    RPC_BeginPauseSync(m_PauseMatch);
                }
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private static void RPC_BeginPauseSync(bool Pause)
        {
            m_PauseMatch = Pause;
        }

        private void Update()
        {
            if (!Runner) return;
            if (PauseMatch) return;

            if (Runner.IsServer)
            {
                foreach (NetworkObject Player in m_SpawnedPlayers.Values)
                {
                    if (!IsPlayerInBounds(Player, out Player Plr))
                    {
                        Plr.RPC_Kill();

                        HandlePlayerDeath();
                        PauseMatch = true;
                    }
                }
            }
        }

        private async void HandlePlayerDeath()
        {
            await Task.Delay(Mathf.RoundToInt(ReloadDelay * 1000));

            LevelManager.FadeoutScreen(FadeColour.Black, async () =>
            {
                ResetLevel();

                await Task.Delay(1); // Simulate loading time. Game looks weird otherwise

                LevelManager.FadeinScreen(FadeColour.Black, async () =>
                {
                    await Task.Delay(Mathf.RoundToInt(StartDelay * 1000));
                    PauseMatch = false;
                }, FadeoutLength);
            }, FadeoutLength);
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
                if (GUI.Button(new Rect(0, 0, 200, 40), "Offline (TESTING ONLY)"))
                {
                    StartSingle();
                }
                if (GUI.Button(new Rect(0, 40, 200, 40), "Host"))
                {
                    StartHost();
                }
                if (GUI.Button(new Rect(0, 80, 200, 40), "Join"))
                {
                    StartClient();
                }
            }
        }
        
        /// <summary>
        /// You should only use this for testing!
        /// </summary>
        public void StartSingle()
        {
            StartGame(Fusion.GameMode.Single);
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
            if (Runner.IsServer)
            {
                foreach (NetworkObject Player in m_SpawnedPlayers.Values)
                {
                    GroundPound.Player Plr;
                    if (Plr = Player.GetComponent<GroundPound.Player>())
                    {
                        Plr.Rigidbody.Rigidbody.simulated = false;
                        Plr.Hitpoints = Plr.StartHitpoints;
                        Plr.transform.position = Spawns[Plr.Team].position;
                        Plr.Rigidbody.Rigidbody.simulated = true;
                    }
                }
            }
        }

        protected async void StartGame(Fusion.GameMode Mode, string LobbyCode = "Default")
        {
            Runner = MatchMaker.MainRunner;

            if (!Runner)
            {
                Runner = gameObject.AddComponent<NetworkRunner>();
                Runner.ProvideInput = true;
            }

            await Runner.StartGame(new StartGameArgs()
            {
                GameMode = Mode,
                SessionName = LobbyCode,
                Scene = SceneManager.GetActiveScene().buildIndex,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });
        }


        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        protected virtual void RPC_RequestServerInf(PlayerRef Requester)
        {
            PlayerCount = m_SpawnedPlayers.Count;
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

        private void SpawnAllPlayers()
        {
            if (Runner.IsServer)
            {
                foreach (NetworkObject Value in  m_SpawnedPlayers.Values)
                {
                    Runner.Despawn(Value);
                }

                m_SpawnedPlayers.Clear();

                foreach (PlayerRef Player in Teams.Keys)
                {
                    if (!m_SpawnedPlayers.ContainsKey(Player))
                    {
                        Vector3 SpawnPosition = Spawns[m_SpawnedPlayers.Count].position;
                        NetworkObject PlayerObject = Runner.Spawn(PlayerPrefab, SpawnPosition, inputAuthority: Player);

                        GroundPound.Player Plr;
                        if (Plr = PlayerObject.GetComponent<Player>())
                        {
                            if (Teams.ContainsKey(Player))
                            {
                                Plr.Team = Teams[Player];
                            }
                            else
                            {
                                int TeamACount = 0;
                                int TeamBCount = 0;
                                for (int I = 0; I < NetworkedGameProperties.Instance.TeamCount; I++)
                                {
                                    if (I == 0)
                                        TeamACount++;
                                    else // It's safe to assume if someone's not on Team 0, they're on Team 1
                                        TeamBCount++;
                                }

                                if (TeamACount < TeamBCount) // If there are less players on Team A (Orange), put the player there
                                    Plr.Team = 0;
                                else if (TeamBCount < TeamACount) // If there are less players on Team B (Blue), put the player there
                                    Plr.Team = 1;
                                else
                                    Plr.Team = UnityEngine.Random.Range(0, 2); // If both teams are equal size, chuck the player on a random one
                            }
                        }

                        m_SpawnedPlayers.Add(Player, PlayerObject);
                    }
                }
            }
        }

        public void OnPlayerJoined(NetworkRunner Runner, PlayerRef Player)
        {
            RPC_RequestServerInf(Player);
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