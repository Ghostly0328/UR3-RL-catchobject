using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCollider : MonoBehaviour
{
    public UR3 brain;
    [SerializeField] private bool OnColSwitchOn = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (OnColSwitchOn)
        {
            Debug.Log("UR3 Hit Target");
            brain.AddReward(-1f);
        }
    }
}
