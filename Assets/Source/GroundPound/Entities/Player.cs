/*
    Developed by Sky MacLennan
 */

using UnityEngine;
using SkySoft;

namespace Sky.GroundPound
{
    public class Player : NetworkedEntity
    {
        protected override void OnNetworkUpdate()
        {
            Vector2 Movement = Velocity;
            Movement.x = SkyEngine.Input.Gameplay.Move.ReadValue<Vector2>().x * m_BaseMoveSpeed;

            if (SkyEngine.Input.Gameplay.Jump.triggered)
                Jump();
        }
    }
}
