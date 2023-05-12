using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UR3Terminal : MonoBehaviour
{
    [SerializeField] private GameObject robotArm;
    [SerializeField] private bool isTerminal;

    private void OnTriggerEnter(Collider other)
    {
        if (isTerminal)
        {
            if (other.tag == "target")
            {
                Debug.Log("Hit");
                robotArm.GetComponent<UR3>().Terminal();
            }
        }
    }
}
