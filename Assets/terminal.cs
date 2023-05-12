using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class terminal : MonoBehaviour
{
    [SerializeField] private GameObject robotArm;

    [SerializeField] private bool isTerminal;

    private void OnTriggerEnter(Collider other)
    {
        if (isTerminal)
        {
            if(other.tag == "target")
            {
                Debug.Log("Hit");
                robotArm.GetComponent<Catch>().Terminal();
            }
        }
        else
        {
            if(other.tag == "target")
            {
                Debug.Log("Box");
                robotArm.GetComponent<Catch>().HitGrap();
            }
        }
        if(other.tag == "wall")
        {
            Debug.Log("Wall");
            robotArm.GetComponent<Catch>().HitWall();
        }
    }
}
