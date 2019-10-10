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


public class ViewBodyAngles : MonoBehaviour
{
    public GameObject bodySensor;       /*!< Reference to body sensor. */
    public GameObject marker;           /*!< Reference to the solid used as marker (a box for instance). */
    public GameObject textBox;

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

        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            GameObject joint = (GameObject)Object.Instantiate(marker);
            joint.name = jt.ToString();
            joint.transform.parent = body.transform;
        }

        // Text boxes for some body joints
        // ---> Spine
        GameObject spineBox = (GameObject)Object.Instantiate(textBox);
        spineBox.name = "Spine angles";
        spineBox.transform.parent = body.transform;

        // ---> Right shoulder
        GameObject rightShoulderBox = (GameObject)Object.Instantiate(textBox);
        rightShoulderBox.name = "Right shoulder angles";
        rightShoulderBox.transform.parent = body.transform;

        return body;
    }

    // Update the information of every body joint for a given body
    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        BodyAnalysis.SetMapper(bodyReader.GetMapper());

        // Get the body joints and convert them to a 2D representation using the scale of the 
        // current object. For AR purposes
        Vector2[] bodyJoints2D = BodyAnalysis.convertToUnity2DPosition(body.Joints, scale, position);
        // Get the body joint orientation and convert them to a Unity Quaternion type
        Quaternion[] bodyOrientation = BodyAnalysis.convertToUnityOrientation(body.JointOrientations);

        int i = 0;
        foreach (Transform child in bodyObject.transform)
        {
            // If the current joint is not being tracked propperly (i.e. infinity as position),
            // ignore it
            if ((bodyJoints2D[i].x == Mathf.Infinity || bodyJoints2D[i].x == Mathf.NegativeInfinity) ||
                (bodyJoints2D[i].y == Mathf.Infinity || bodyJoints2D[i].y == Mathf.NegativeInfinity))
            {
                i++;
                if (i >= bodyJoints2D.Length) break;
                continue;
            }
            // Update the current joint position and rotation.
            child.transform.position = new Vector3(bodyJoints2D[i].x, bodyJoints2D[i].y, position.z);
            child.transform.localRotation = bodyOrientation[i];

            i++;
            if (i >= bodyJoints2D.Length) break;
        }

        Transform spineBox = bodyObject.transform.Find("Spine angles");
        spineBox.position = new Vector3(bodyJoints2D[(int)Kinect.JointType.Head].x, bodyJoints2D[(int)Kinect.JointType.Head].y, position.z-0.5f);
        int spineFlex1 = (int)BodyAnalysis.FlexionSpineAngle(body, true);  // coronal
        int spineFlex2 = (int)BodyAnalysis.FlexionSpineAngle(body, false); // sagital
        spineBox.GetChild(0).GetComponent<TextMesh>().text = "Coronal: " + spineFlex1.ToString() + "\nSagital:" + spineFlex2.ToString();

        Transform rightShoulderBox = bodyObject.transform.Find("Right shoulder angles");
        rightShoulderBox.position = new Vector3(bodyJoints2D[(int)Kinect.JointType.ShoulderRight].x, bodyJoints2D[(int)Kinect.JointType.ShoulderRight].y, position.z - 0.5f);
        int rShoulderAngle1 = (int)BodyAnalysis.FlexionShoulderAngle(body, true);
        int rShoulderAngle2 = (int)BodyAnalysis.AbductionShoulderAngle(body, true);
        rightShoulderBox.GetChild(0).GetComponent<TextMesh>().text = "Flexion: " + rShoulderAngle1.ToString() + "\nAbduction: " + rShoulderAngle2.ToString();
    }
}
