using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerminalPoint : MonoBehaviour
{
    [SerializeField] public Transform target;

    private void FixedUpdate()
    {
        float distance = Vector3.Distance(transform.position, target.position);
        Debug.Log(distance);
    }
}
