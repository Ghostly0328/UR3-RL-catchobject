using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class check : MonoBehaviour
{
    public Bounds bounds1;
    private void Start()
    {
         bounds1 = GetComponent<BoxCollider>().bounds;
}

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "target")
        {
            Bounds bounds2 = other.GetComponent<BoxCollider>().bounds;

            // ?取??Bounds相交的部分
            Bounds overlapBounds = bounds2;
            overlapBounds.Intersects(bounds1);

            float overlapArea = overlapBounds.size.x * overlapBounds.size.y * overlapBounds.size.z;
            Debug.Log("OverlabArea : " + overlapArea);
        }

    }
}
