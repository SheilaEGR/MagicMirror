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

public class HandTracker : MonoBehaviour
{
    public GameObject bodySensor;
    public GameObject marker;
    public bool trackRightHand = true;
    public bool trackLeftHand = false;

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
                break;  // <--- Track only  one body
            }
        }
    }

    private GameObject CreateBody(ulong id)
    {
        GameObject body = new GameObject();
        body.name = "Body:" + id;
        body.transform.parent = transform;

        // HAND INSTANTIATION
        GameObject rHand = (GameObject)Object.Instantiate(marker);
        rHand.name = "Right hand";
        rHand.transform.parent = body.transform;
        rHand.SetActive(trackRightHand);
       
        GameObject lHand = (GameObject)Object.Instantiate(marker);
        lHand.name = "Left hand";
        lHand.transform.parent = body.transform;
        lHand.SetActive(trackLeftHand);
        
        return body;
    }

    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        Vector2[] bodyJoints2D = bodyReader.convertToUnity2DPosition(body.Joints, scale, position);
        Quaternion[] bodyOrientation = bodyReader.convertToUnityOrientation(body.JointOrientations);

        bodyObject.transform.GetChild(0).gameObject.SetActive(trackRightHand);
        bodyObject.transform.GetChild(1).gameObject.SetActive(trackRightHand);

        // HAND TRACKING
        if (trackRightHand)
        {
            Vector2 rightHandPos = bodyJoints2D[(int)Kinect.JointType.HandRight];
            if ((rightHandPos.x != Mathf.Infinity && rightHandPos.x != Mathf.NegativeInfinity) &&
                (rightHandPos.y != Mathf.Infinity && rightHandPos.y != Mathf.NegativeInfinity))
            {
                Transform hand = bodyObject.transform.GetChild(0).transform;
                hand.position = new Vector3(rightHandPos.x, rightHandPos.y, position.z);
            }
        }
        if(trackLeftHand)
        {
            Vector2 leftHandPos = bodyJoints2D[(int)Kinect.JointType.HandLeft];
            if ((leftHandPos.x != Mathf.Infinity && leftHandPos.x != Mathf.NegativeInfinity) &&
                (leftHandPos.y != Mathf.Infinity && leftHandPos.y != Mathf.NegativeInfinity))
            {
                Transform hand = bodyObject.transform.GetChild(1).transform;
                hand.position = new Vector3(leftHandPos.x, leftHandPos.y, position.z);
            }
        }
    }
}
