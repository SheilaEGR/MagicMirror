using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMarker : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Enter:" + collision.transform.tag);
    }

    private void OnCollisionStay(Collision collision)
    {
        Debug.Log("Stay: " + collision.transform.tag);
    }
}
