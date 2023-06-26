/*
    Developed by Sky MacLennan
 */

using SkySoft;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sky.GroundPound
{
    [AddComponentMenu("Ground Pound/Ability Manager")]
    public class AbilityManager : MonoBehaviour
    {
        public AudioSource PlayerAudio;
        public NetworkedEntity Parent;

        [Header("Footsteps")]
        public AudioClip[] FootstepSounds;

        [Header("Ground Pound")]
        public AudioClip GroundPoundStart;
        public AudioClip GroundPoundEnd;
        [Space]
        public float GroundPoundRadius;
        public float GroundPoundFalloff;
        public float GroundPoundForce;

        private void OnDrawGizmosSelected()
        {
            SkyEngine.Gizmos.Colour = Color.red;
            SkyEngine.Gizmos.DrawCircle(Parent.transform.position, GroundPoundRadius);
            SkyEngine.Gizmos.Colour = Color.yellow;
            SkyEngine.Gizmos.DrawCircle(Parent.transform.position, GroundPoundFalloff);
        }

        public void PlayFootstep()
        {
            AudioClip Footstep = FootstepSounds[Random.Range(0, FootstepSounds.Length)];
            if (PlayerAudio && Footstep)
                PlayerAudio.PlayOneShot(Footstep, 0.6f);
        }

        public void BeginGroundPound()
        {
            if (PlayerAudio && GroundPoundStart)
                PlayerAudio.PlayOneShot(GroundPoundStart);
        }

        public void DoGroundPound()
        {
            if (PlayerAudio && GroundPoundEnd)
                PlayerAudio.PlayOneShot(GroundPoundEnd);

            foreach (NetworkedEntity Entity in Parent.GetEntities(GroundPoundRadius))
            {
                Vector2 Direction = Entity.transform.position - Parent.transform.position;
                float Distance = Direction.magnitude;
                if (Distance <= GroundPoundRadius)
                {
                    Vector2 V = Direction.normalized * GroundPoundForce;
                    Entity.RPC_ApplyForce(V.x, V.y);
                }
                else
                {
                    float Force = Mathf.Lerp(GroundPoundForce, 0, Distance /  GroundPoundRadius);
                    Vector2 V = Direction.normalized * Force;
                    Entity.RPC_ApplyForce(V.x, V.y);
                }
            }
        }
    }
}