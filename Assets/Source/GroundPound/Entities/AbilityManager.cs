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
        public float GroundPoundUpwardForce = 3;

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

            foreach (Collider2D Collider in Parent.GetCollisions(GroundPoundRadius))
            {
                NetworkedEntity Entity;
                Rigidbody2D Rigidbody;

                Vector2 Direction = Collider.transform.position - Parent.transform.position;
                float Distance = Direction.magnitude;

                if (Rigidbody = Collider.GetComponent<Rigidbody2D>())
                {
                    if (Collider.transform.position.y + 0.25f >= Parent.transform.position.y)
                    {
                        Vector2 V = Vector2.zero;

                        if (Distance <= GroundPoundRadius)
                        {
                            V = Direction.normalized * GroundPoundForce;
                        }
                        else
                        {
                            float Force = Mathf.Lerp(GroundPoundForce, 0, Distance / GroundPoundRadius);
                            V = Direction.normalized * Force;
                        }

                        if (Entity = Collider.GetComponent<NetworkedEntity>())
                        {
                            Entity.ModifyHitpoints(-1);
                            Entity.RPC_ApplyForce(V.x, V.y + GroundPoundUpwardForce);
                        }
                        else
                        {
                            Rigidbody.AddForce(V + Vector2.up * GroundPoundUpwardForce, ForceMode2D.Impulse);
                        }
                    }
                }
            }
        }
    }
}