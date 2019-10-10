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

/*! \brief Supervises Abduction/Adduction movement on both shoulders. If the elbow is bent, a sign is displayed.
 */
public class ShoulderAbduction : MonoBehaviour
{
    public GameObject bodySensor;       /*!< Reference to body sensor. */
    public GameObject shoulderMarker;   /*!< Reference to the solid used as marker (a box for instance). */
    public GameObject elbowMarker;      /*!< Reference to the solid used as marker for the elbow. */
    public GameObject handMarker;       /*!< Reference to the solid used as marker for the hand. */
    public GameObject textBox;          /*!< Reference to the textbox where the angles will be displayed*/

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
                break;  // <----- Track only one body
            }
        }
    }

    // Create a new game object containing a marker for every joint as children
    private GameObject CreateBody(ulong id)
    {
        GameObject body = new GameObject();
        body.name = "Body:" + id;
        body.transform.parent = transform;

        // LEFT
        // ---> Left shoulder
        GameObject leftShoulder = (GameObject)Object.Instantiate(shoulderMarker);
        leftShoulder.name = "Left shoulder";
        leftShoulder.transform.parent = body.transform;

        // ---> Left elbow
        GameObject leftElbow = (GameObject)Object.Instantiate(elbowMarker);
        leftElbow.name = "Left elbow";
        leftElbow.transform.parent = body.transform;
        leftElbow.gameObject.SetActive(false);

        // ---> Left hand
        GameObject leftHand = (GameObject)Object.Instantiate(handMarker);
        leftHand.name = "Left hand";
        leftHand.transform.parent = body.transform;

        // ---> Text box for left shoulder
        GameObject leftText = (GameObject)Object.Instantiate(textBox);
        leftText.name = "Left text";
        leftText.transform.parent = body.transform;

        // RIGHT
        // ---> Right shoulder
        GameObject rightShoulder = (GameObject)Object.Instantiate(shoulderMarker);
        rightShoulder.name = "Right shoulder";
        rightShoulder.transform.parent = body.transform;

        // ---> Right elbow
        GameObject rightElbow = (GameObject)Object.Instantiate(elbowMarker);
        rightElbow.name = "Right elbow";
        rightElbow.transform.parent = body.transform;
        rightElbow.gameObject.SetActive(false);

        // ---> Right hand
        GameObject rightHand = (GameObject)Object.Instantiate(handMarker);
        rightHand.name = "Right hand";
        rightHand.transform.parent = body.transform;

        // ---> Text box for right shoulder
        GameObject rightText = (GameObject)Object.Instantiate(textBox);
        rightText.name = "Right text";
        rightText.transform.parent = body.transform;

        return body;
    }

    // Update the information of every body joint for a given body
    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        BodyAnalysis.SetMapper(bodyReader.GetMapper());

        // Get the body joints and convert them to a 2D representation using the scale of the 
        // current object. For AR purposes
        Vector2[] bodyJoints2D = BodyAnalysis.convertToUnity2DPosition(body.Joints, scale, position);

        // Get the Abduction/Adduction angle of the shoulders.
        Quaternion[] jointOrientation = BodyAnalysis.convertToUnityOrientation(body.JointOrientations);
        float leftShoulderAngle = BodyAnalysis.AbductionShoulderAngle(body, false);
        float rightShoulderAngle = BodyAnalysis.AbductionShoulderAngle(body, true);

        // Get the flexion of the elbows. The shoulder Abduction/Adduction should be performed
        // without bending the arm.
        float leftElbowAngle = BodyAnalysis.FlexionElbowAngle(body, false) * 180.0f / Mathf.PI;
        float rightElbowAngle = BodyAnalysis.FlexionElbowAngle(body, true) * 180.0f / Mathf.PI;
        const float minAngle = 30;

        // For each desired joint, find its position in 2D space, check if that position
        // is valid (not infinity), and update the position of the corresponding marker.
        // The marker on the elbows is only visible when the angle is higher than a
        // threshold (minAngle)

        // ---> LEFT
        // Left shoulder
        Vector2 pos = bodyJoints2D[(int)Kinect.JointType.ShoulderLeft];
        Transform joint = bodyObject.transform.Find("Left shoulder");
        if (pos.x != Mathf.Infinity && pos.x != Mathf.NegativeInfinity &&
            pos.y != Mathf.Infinity && pos.y != Mathf.NegativeInfinity)
        {
            joint.position = new Vector3(pos.x, pos.y, position.z - 0.5f);
        }

        // Left text box
        Transform box = bodyObject.transform.Find("Left text");
        box.transform.position = new Vector3(pos.x - 2, pos.y, position.x - 0.5f);
        box.GetChild(0).GetComponent<TextMesh>().text = "Angle: " + leftShoulderAngle.ToString();

        // Left elbow
        pos = bodyJoints2D[(int)Kinect.JointType.ElbowLeft];
        joint = bodyObject.transform.Find("Left elbow");
        if (pos.x != Mathf.Infinity && pos.x != Mathf.NegativeInfinity &&
            pos.y != Mathf.Infinity && pos.y != Mathf.NegativeInfinity)
        {
            joint.gameObject.SetActive(leftElbowAngle > minAngle);
            joint.position = new Vector3(pos.x, pos.y, position.z - 0.5f);
        }

        // Left hand
        pos = bodyJoints2D[(int)Kinect.JointType.HandLeft];
        joint = bodyObject.transform.Find("Left hand");
        if (pos.x != Mathf.Infinity && pos.x != Mathf.NegativeInfinity &&
            pos.y != Mathf.Infinity && pos.y != Mathf.NegativeInfinity)
        {
            joint.position = new Vector3(pos.x, pos.y, position.z - 0.5f);
        }

        // ---> RIGHT
        // Right shoulder
        pos = bodyJoints2D[(int)Kinect.JointType.ShoulderRight];
        joint = bodyObject.transform.Find("Right shoulder");
        if (pos.x != Mathf.Infinity && pos.x != Mathf.NegativeInfinity &&
            pos.y != Mathf.Infinity && pos.y != Mathf.NegativeInfinity)
        {
            joint.position = new Vector3(pos.x, pos.y, position.z - 0.5f);
        }

        // Right text box
        box = bodyObject.transform.Find("Right text");
        box.transform.position = new Vector3(pos.x + 2, pos.y, position.x - 0.5f);
        box.GetChild(0).GetComponent<TextMesh>().text = "Angle: " + rightShoulderAngle.ToString();

        // Right elbow
        pos = bodyJoints2D[(int)Kinect.JointType.ElbowRight];
        joint = bodyObject.transform.Find("Right elbow");
        if (pos.x != Mathf.Infinity && pos.x != Mathf.NegativeInfinity &&
            pos.y != Mathf.Infinity && pos.y != Mathf.NegativeInfinity)
        {
            joint.gameObject.SetActive(rightElbowAngle > minAngle);
            joint.position = new Vector3(pos.x, pos.y, position.z - 0.5f);
        }

        // Right hand
        pos = bodyJoints2D[(int)Kinect.JointType.HandRight];
        joint = bodyObject.transform.Find("Right hand");
        if (pos.x != Mathf.Infinity && pos.x != Mathf.NegativeInfinity &&
            pos.y != Mathf.Infinity && pos.y != Mathf.NegativeInfinity)
        {
            joint.position = new Vector3(pos.x, pos.y, position.z - 0.5f);
        }
    }
}
