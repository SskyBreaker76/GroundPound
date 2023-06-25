using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkySoft
{
    public class EnhancedCameraGizmos : MonoBehaviour
    {
#if UNITY_EDITOR
        private Camera Camera => GetComponent<Camera>();

        private Mesh CameraGizmos;
        private Mesh CameraFacingGizmos;

        private void OnDrawGizmos()
        {
            if (!CameraGizmos)
                CameraGizmos = Resources.Load<Mesh>("SkyEngine/Models/Gizmos/Camera");
            if (!CameraFacingGizmos)
                CameraFacingGizmos = Resources.Load<Mesh>("SkyEngine/Models/Gizmos/Camera_Facing");

            if (CameraGizmos && CameraFacingGizmos)
            {
                SkyEngine.Gizmos.Matrix = Matrix4x4.TRS(Camera.transform.position, Camera.transform.rotation, Vector3.one * 0.25f);
                SkyEngine.Gizmos.Colour = (Camera.enabled ? (Camera == Camera.main ? Color.yellow : Color.green) : Color.red) * new Color(1, 1, 1, 0.8f);
                SkyEngine.Gizmos.DrawMesh(CameraGizmos, Vector3.zero, Quaternion.identity, Vector3.one);
                SkyEngine.Gizmos.Colour = new Color(1, 1, 1, 0.8f);
                SkyEngine.Gizmos.DrawMesh(CameraFacingGizmos, Vector3.zero, Quaternion.identity, Vector3.one);
                SkyEngine.Gizmos.Colour = new Color(0.9f, 0.9f, 0.9f, 0.8f);
                SkyEngine.Gizmos.DrawMesh(CameraFacingGizmos, Vector3.zero, Quaternion.Euler(-90, 0, 0), Vector3.one * 0.75f);
                SkyEngine.Gizmos.Matrix = Matrix4x4.identity;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (CameraGizmos && CameraFacingGizmos)
            {
                SkyEngine.Gizmos.Matrix = Matrix4x4.TRS(Camera.transform.position, Camera.transform.rotation, Vector3.one * 0.25f);
                SkyEngine.Gizmos.Colour = UnityEditor.Selection.gameObjects.ToList().Contains(Camera.gameObject) ? Color.white : new Color(1, 1, 1, 0.3f);
                Gizmos.DrawFrustum(Vector3.zero, Camera.fieldOfView, Camera.farClipPlane, Camera.nearClipPlane, Camera.aspect);
                SkyEngine.Gizmos.Matrix = Matrix4x4.identity;
            }
        }
#endif
    }
}