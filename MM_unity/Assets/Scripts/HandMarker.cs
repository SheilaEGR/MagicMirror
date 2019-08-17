/*  *************************************************************************************
*  Copyright (c) 2017 - onward
*  MEDICAL EDUCATION, TRAINING, AND COMPUTER ASSISTED
*  INTERVENTIONS (METRICS) LAB (www.metrics-lab.ca)
*  UNIVERSITY OF OTTAWA
*  
*  Prof.  Pascal Fallavollita           (pfallavo@uottawa.ca)
*  Dr. Sheila Esmeralda Gonzalez Reyna  (sheila.esmeralda.gonzalez@gmail.com)
************************************************************************************* */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Brief For projects including a hand tracker, this method will let you know
 * when the marker has collided to some virtual object.
 */
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
