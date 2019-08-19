/*  *************************************************************************************
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


/** Brief Attach this class to the body controller. This is useful for projects that include a hand tracker.
 * 
 * This script is part of the "Simple hand tracking" scene.
 */
public class HandTracker : MonoBehaviour
{
    public GameObject bodySensor;           /*!< Reference to body sensor. */
    public GameObject marker;               /*!< Reference to the solid used as marker (a box for instance). */
    public bool trackRightHand = true;      /*!< True if tracking right hand. */
    public bool trackLeftHand = false;      /*!< True if tracking left hand. */

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

    // Create a new game object containing a marker for every hand as children
    private GameObject CreateBody(ulong id)
    {
        GameObject body = new GameObject();
        body.name = "Body:" + id;
        body.transform.parent = transform;

        // HAND INSTANTIATION

        // ---> Right hand
        GameObject rHand = (GameObject)Object.Instantiate(marker);
        rHand.name = "Right hand";
        rHand.transform.parent = body.transform;
        rHand.SetActive(trackRightHand);
       
        // ---> Left hand
        GameObject lHand = (GameObject)Object.Instantiate(marker);
        lHand.name = "Left hand";
        lHand.transform.parent = body.transform;
        lHand.SetActive(trackLeftHand);
        
        return body;
    }

    // Update the information regarding one body
    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        BodyAnalysis.SetMapper(bodyReader.GetMapper());

        Vector2[] bodyJoints2D = BodyAnalysis.convertToUnity2DPosition(body.Joints, scale, position);
        Quaternion[] bodyOrientation = BodyAnalysis.convertToUnityOrientation(body.JointOrientations);

        // ---------------------------------------------------------------------------------
        // Show/hide the marker for each hand. These two lines are added to this function
        // only for testing (debugging) puposes. Remove for deployment
        bodyObject.transform.GetChild(0).gameObject.SetActive(trackRightHand);
        bodyObject.transform.GetChild(1).gameObject.SetActive(trackLeftHand);
        // ---------------------------------------------------------------------------------

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
