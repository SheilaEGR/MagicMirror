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
using Kinect = Windows.Kinect;

/** Brief Abduction/Adduction movement represented in 2D.
 */
public class AbductionMove : MonoBehaviour
{
    public GameObject movingMarker;     /*<! Marker representing the target of the movement. */
    public GameObject pivotMarker;      /*<! Marker representing the pivot of movement (for example, shoulder). */
    public Vector3 pivotJointPosition;  /*<! Position of pivot. */
    public float minAngleInDegrees;     /*<! Minimum angle to perform the movement (in degrees). */
    public float maxAngleInDegrees;     /*<! Maximum angle at which perform the movement (in degrees). */
    public float radius = 1f;           /*<! Radius of abduction/adduction movement (for example, arm length). */
    public float speed = 1f;            /*<! Speed at which make the repetitions */
    public int numRepetitions = 3;      /*<! Amount of repetitions to be performed by the user. */

    private Transform pivot;
    private Transform marker;
    private float minAngle, maxAngle;
    private float angle;
    private float dAngle;
    private int repetitions;
    private bool doneRepetitions = false;

    /*!
     * \brief Check if the exercise has been performed a given amount of times.
     * \return True when all repetitions are completed, False otherwise.
     */
    public bool AreRepetitionsDone()
    {
        return doneRepetitions;
    }

    private void Start()
    {
        // All the game object will be translated to the pivot position
        transform.position = pivotJointPosition;

        // Convert angles in degrees to radians
        minAngle = minAngleInDegrees * Mathf.PI / 180.0f;
        maxAngle = maxAngleInDegrees * Mathf.PI / 180.0f;
        angle = minAngle;                               // The movement starts at this position
        dAngle = (minAngle < maxAngle) ? 0.1f : -0.1f;  // Angle step between two consecutive frames

        GameObject mm = Instantiate(movingMarker);
        mm.name = "Moving marker";
        mm.transform.parent = transform;
        marker = mm.transform;
        float x = radius * Mathf.Cos(angle);
        float y = radius * Mathf.Sin(angle);
        marker.position = new Vector3(x, y, transform.position.z);

        GameObject pm = Instantiate(pivotMarker);
        pm.name = "Pivot marker";
        pm.transform.parent = transform;
        pivot = pm.transform;
        pivot.position = transform.position;

        repetitions = numRepetitions * 2;
    }

    private void Update()
    {
        if (repetitions <= 0)
        {
            doneRepetitions = true;
            return;
        }

        angle += dAngle * speed * Time.deltaTime;
        if (angle < minAngle || angle > maxAngle)
        {
            dAngle = -dAngle;
            repetitions--;
        }

        float x = radius * Mathf.Cos(angle);
        float y = radius * Mathf.Sin(angle);
        marker.position = new Vector3(x, y, transform.position.z);
    }
}
