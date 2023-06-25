using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.Physics
{
    [RequireComponent(typeof(BoxCollider)), AddComponentMenu("SkyEngine/Physics/Gizmos/Box")]
    public class BoxTrigger : MonoBehaviour
    {
        private BoxCollider Col => GetComponent<BoxCollider>();

        private void OnDrawGizmos()
        {
            SkyEngine.Gizmos.Colour = Color.green * new Color(1, 1, 1, 0.25F);
            SkyEngine.Gizmos.Matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
           SkyEngine.Gizmos.DrawCube(Col.center, Col.size);
        }
    }
}