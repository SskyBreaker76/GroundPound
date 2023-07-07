using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sky.GroundPound
{
    [AddComponentMenu("Ground Pound/Networked Game Properties")]
    public class NetworkedGameProperties : NetworkBehaviour
    {
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