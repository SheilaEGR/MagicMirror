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

/** Brief Display the body joins as boxes (or any given marker) in front of the RGB image.
 * 
 * This script is part of the "Body and RGB" scene.
 */
public class SimpleBodyTracker : MonoBehaviour
{
    public GameObject bodySensor;
    public GameObject marker;

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
        if (bodySensor == null) return;

        bodyReader = bodySensor.GetComponent<SensorBody>();
        if (bodyReader == null) return;

        Kinect.Body[] data = bodyReader.GetBodies();
        if (data == null) return;

        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in data)
        {
            if (body == null) continue;
            if (body.IsTracked)
                trackedIds.Add(body.TrackingId);
        }

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
                if (!bodyDict.ContainsKey(body.TrackingId))
                    bodyDict[body.TrackingId] = CreateBody(body.TrackingId);
                RefreshBodyObject(body, bodyDict[body.TrackingId]);
            }
        }
    }

    private GameObject CreateBody(ulong id)
    {
        GameObject body = new GameObject();
        body.name = "Body:" + id;
        body.transform.parent = transform;

        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            GameObject joint = (GameObject)Object.Instantiate(marker);
            joint.name = jt.ToString();
            joint.transform.parent = body.transform;
        }

        return body;
    }

    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        Vector2[] bodyJoints2D = bodyReader.convertToUnity2DPosition(body.Joints, scale, position);
        Quaternion[] bodyOrientation = bodyReader.convertToUnityOrientation(body.JointOrientations);

        int i = 0;
        foreach (Transform child in bodyObject.transform)
        {
            if((bodyJoints2D[i].x == Mathf.Infinity || bodyJoints2D[i].x == Mathf.NegativeInfinity) ||
                (bodyJoints2D[i].y == Mathf.Infinity || bodyJoints2D[i].y == Mathf.NegativeInfinity))
            {
                i++;
                if (i >= bodyJoints2D.Length) break;
                continue;
            }
            child.transform.position = new Vector3(bodyJoints2D[i].x, bodyJoints2D[i].y, position.z);
            child.transform.localRotation = bodyOrientation[i];

            i++;
            if (i >= bodyJoints2D.Length) break;
        }
    }
}
