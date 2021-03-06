﻿/*  *************************************************************************************
*  Copyright (c) 2017 - onward
*  MEDICAL EDUCATION, TRAINING, AND COMPUTER ASSISTED
*  INTERVENTIONS (METRICS) LAB (www.metrics-lab.ca)
*  UNIVERSITY OF OTTAWA
*  
*  Prof.  Pascal Fallavollita           (pfallavo@uottawa.ca)
*  Dr. Sheila Esmeralda Gonzalez Reyna  (sheila.esmeralda.gonzalez@gmail.com)
************************************************************************************* */

using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;

/*! Brief Display the body joints as boxes or any given marker (sphere, capsule, etc.).
 * 
 * This script is part of the "Simple Body" scene.
 */
public class BoxSkeleton : MonoBehaviour
{
    public GameObject BodySourceManager;    /*!< Reference to SensorBody. */
    public GameObject Marker;               /*!< Reference to the solid used as marker (a box for instance). */
    public float scale = 1.0f;              /*!< The scale at which the marker will be drawn on each body joint. */

    private Dictionary<ulong, GameObject> bodyDict = new Dictionary<ulong, GameObject>();
    private SensorBody bodyReader;

    void Update()
    {
        // Check if there is a sensor available
        if (BodySourceManager == null) return;

        // Acquire the sensor, cast it to SensorBody
        bodyReader = BodySourceManager.GetComponent<SensorBody>();
        if (bodyReader == null) return;

        // Find the information about found bodies
        Kinect.Body[] data = bodyReader.GetBodies();
        if (data == null) return;

        // Assign a tracking ID to every found body
        List<ulong> trackedIds = new List<ulong>();
        foreach(var body in data)
        {
            if (body == null) continue;
            if (body.IsTracked)
                trackedIds.Add(body.TrackingId);
        }

        // Remove bodies that are not being tracked anymore
        List<ulong> knownIds = new List<ulong>(bodyDict.Keys);
        foreach(ulong trackingId in knownIds)
        {
            if(!trackedIds.Contains(trackingId))
            {
                Destroy(bodyDict[trackingId]);
                bodyDict.Remove(trackingId);
            }
        }

        foreach(var body in data)
        {
            if (body == null) continue;
            if(body.IsTracked)
            {
                // If no virtual object associated to this body, create one
                if(!bodyDict.ContainsKey(body.TrackingId))
                    bodyDict[body.TrackingId] = CreateBody(body.TrackingId);
                // Perform operations related to body tracking
                RefreshBodyObject(body, bodyDict[body.TrackingId]);

                break;  // <--- Track only one body
            }            
        }
    }

    // Create a new game object containing a marker for every joint as children
    private GameObject CreateBody(ulong id)
    {
        GameObject body = new GameObject();
        body.name = "Body:" + id;
        body.transform.parent = transform;

        // Create one marker for every body joint, every joint will be a child of "body" Game Object
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            GameObject joint = (GameObject)Object.Instantiate(Marker);
            joint.name = jt.ToString();
            joint.transform.parent = body.transform;
        }

        return body;
    }

    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        // Get position and rotation of every body joint. 
        // Position will be computed as 2D vector for displaying purposes
        Vector3[] bodyJoints = BodyAnalysis.convertToUnityPosition(body.Joints);
        Quaternion[] bodyOrientation = BodyAnalysis.convertToUnityOrientation(body.JointOrientations);

        int i = 0;
        foreach(Transform child in bodyObject.transform)
        {
            // If the current body joint is not being tracked (infinity), ignore
            if ((bodyJoints[i].x == Mathf.Infinity || bodyJoints[i].x == Mathf.NegativeInfinity) ||
                (bodyJoints[i].y == Mathf.Infinity || bodyJoints[i].y == Mathf.NegativeInfinity))
            {
                i++;
                if (i >= bodyJoints.Length) break;
                continue;
            }

            // Place the current body joint on the desired location
            child.transform.position = bodyJoints[i] * scale;
            child.transform.localRotation = bodyOrientation[i];

            i++;
            if (i >= bodyJoints.Length) break;
        }
    }
}
