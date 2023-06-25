using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer)), AddComponentMenu("SkyEngine/Objects/Line Helper")]
public class LineHelper : MonoBehaviour
{
    private LineRenderer R => GetComponent<LineRenderer>();
    public Transform Target;
    public int Index;

    private void Update()
    {
        R.SetPosition(Index, Target.localPosition);
    }
}
