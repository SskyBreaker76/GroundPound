using SkySoft;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.Objects
{
    [AddComponentMenu("SkyEngine/Objects/Mimic Transform")]
    public class TransformMimic : MonoBehaviour
    {
        [Tooltip("Runs the script in edit-mode")]
        public bool Run = false;
        [Space]
        public bool StickToGrid;
        public Transform Target;
        public bool TrackPlayer;
        [Space]
        public Vector3 Offset;

        private void OnValidate()
        {
            if (Run)
            {
                Update();
            }
        }

        private void Update()
        {
            if (TrackPlayer)
            {
                if (SkyEngine.PlayerEntity)
                    Target = SkyEngine.PlayerEntity.transform;
            }

            Vector3 T = Target.position + Offset;

            if (StickToGrid)
                T = new Vector3((int)T.x, (int)T.y, (int)T.z);

            if (Target)
            {
                transform.position = T;
            }
        }
    }
}