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

public class ArmRoutine : MonoBehaviour
{
    public GameObject bodySensor;           /*!< Reference to body sensor. */
    public GameObject abductionObject;      /*<! Prefab object containing the AbductionMove script. */

    private Dictionary<ulong, GameObject> bodyDict = new Dictionary<ulong, GameObject>();
    private SensorBody bodyReader;
    private Vector3 scale, position;

    private void Start()
    {
        scale = transform.localScale;
        position = transform.position;
    }

    private void Update()
    {
        // Check if there is a sensor available
        if (bodySensor == null) return;

        // Acquire the sensor, cast it to SensorBody
        bodyReader = bodySensor.GetComponent<SensorBody>();
        if (bodyReader == null) return;

        // Find the information about found bodies
        Kinect.Body[] data = bodyReader.GetBodies();
        if (data == null) return;

        // Assign a tracking ID to every found body
        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in data)
        {
            if (body == null) continue;
            if (body.IsTracked)
                trackedIds.Add(body.TrackingId);
        }

        // Remove bodies that are not being tracked anymore
        List<ulong> knownIds = new List<ulong>(bodyDict.Keys);
        foreach (ulong trackingId in knownIds)
        {
            if (!trackedIds.Contains(trackingId))
            {
                Destroy(bodyDict[trackingId]);
                bodyDict.Remove(trackingId);
            }
        }

        foreach (var body in data)
        {
            if (body == null) continue;
            if (body.IsTracked)
            {

                // If no virtual object associated to this body, create one
                if (!bodyDict.ContainsKey(body.TrackingId))
                    bodyDict[body.TrackingId] = CreateBody(body.TrackingId);
                // Perform operations related to body tracking
                RefreshBodyObject(body, bodyDict[body.TrackingId]);
                break;  // <--- Track only  one body
            }
        }
    }

    private GameObject CreateBody(ulong id)
    {
        GameObject body = new GameObject();
        body.name = "Body: " + id.ToString();
        body.transform.parent = transform;



        return body;
    }

    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {

    }
}
