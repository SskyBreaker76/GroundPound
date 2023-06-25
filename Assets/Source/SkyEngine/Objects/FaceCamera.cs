using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("SkyEngine/Objects/Face Camera")]
public class FaceCamera : MonoBehaviour 
{ 
    private void Update()
    {
        transform.LookAt(Camera.main.transform);
    }
}
