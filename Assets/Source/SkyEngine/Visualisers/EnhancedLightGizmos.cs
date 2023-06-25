using UnityEngine;

namespace SkySoft
{
    public class EnhancedLightGizmos : MonoBehaviour
    {
        Light Light => GetComponent<Light>();

        Mesh SpotlightMesh;
        Mesh ArrowMesh;

        private void OnDrawGizmos()
        {
            if (!SpotlightMesh)
                SpotlightMesh = Resources.Load<Mesh>("SkyEngine/Models/Gizmos/Spotlight");
            if (!ArrowMesh)
                ArrowMesh = Resources.Load<Mesh>("SkyEngine/Models/Gizmos/Camera_Facing");

            SkyEngine.Gizmos.Colour = Light.color * new Color(1, 1, 1, 0.7f);
            SkyEngine.Gizmos.Matrix = Matrix4x4.TRS(transform.position, transform.rotation, (Light.type == LightType.Point || Light.type == LightType.Directional) ? Vector3.one : (Vector3.one * 0.25f));
            switch (Light.type)
            {
                case LightType.Spot:
                    SkyEngine.Gizmos.DrawMesh(SpotlightMesh, Vector3.zero, Quaternion.identity, Vector3.one);
                    SkyEngine.Gizmos.Colour = new Color(1, 1, 1, 0.8f);
                    SkyEngine.Gizmos.DrawArrow();
                    break;
                case LightType.Directional:
                    SkyEngine.Gizmos.DrawSphere(Vector3.zero, 0.25f);
                    SkyEngine.Gizmos.DrawArrow();
                    break;
                case LightType.Point:
                    SkyEngine.Gizmos.DrawSphere(Vector3.zero, 0.25f);
                    SkyEngine.Gizmos.DrawWireSphere(Vector3.zero, Light.range);
                    break;
            }
        }

        private void OnDrawGizmosSelected()
        {
            SkyEngine.Gizmos.Colour = Light.color * new Color(1, 1, 1, 0.3f);
            SkyEngine.Gizmos.Matrix = Matrix4x4.TRS(transform.position, transform.rotation, (Light.type == LightType.Point || Light.type == LightType.Directional) ? Vector3.one : (Vector3.one * 0.25f));
            if (Light.type == LightType.Point)
            {
                SkyEngine.Gizmos.DrawSphere(Vector3.zero, Light.range);
            }
        }
    }
}