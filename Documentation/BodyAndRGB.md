# Body and RGB scene

This is the first example of an __Augmented Reality__ application using the Kinect v2 sensor. There are four important components to add:

1. __RGB sensor__. Just as in the "Simple RGB" scene, this is an empty object containing the __SensorRGB__ script.

2. __RGB display__. Same as in "Simple RGB" scene, this is a cube with aspect ratio of 16:9, the __DisplayRGB__ script and a reference to the __RGB sensor__ on it.

3. __Body sensor__. This is an empty object containing a __SensorBody__ script.

4. __Body display__. This is a cube object that does not display its mesh. The size of this cube is adjusted to fill the size of the camera at a distance of 2 units in front of the __RGB display__. This object uses the __SimpleBodyTracker__ script, which takes a reference to the __Body sensor__ and a __marker__ as inputs.

![Body and RGB scene](Images/BodyAndRGB.png)

The marker can be any solid, and it was stored as a prefab.

## Simple body tracker script

This script is very similar to the one used in the [__Simple body scene__](./SimpleBody.md), however there are some differences due the need to superimpose the virtual model to the RGB frame.

As mentioned before, the size of the invisible cube using this script has to be adjusted to fill the camera at a certain distance from the RGB display. In this very case, that size is (11.3098, 6.361763, 0.1) and the position is (0, 0, -2). The RGB display size is (16, 9, 0.1) and its position is (0, 0, 0). This step is important to make an accurate Augmented Reality representation.


```csharp
using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;

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
        BodyAnalysis.SetMapper(bodyReader.GetMapper());

        Vector2[] bodyJoints2D = BodyAnalysis.convertToUnity2DPosition(body.Joints, scale, position);
        Quaternion[] bodyOrientation = BodyAnalysis.convertToUnityOrientation(body.JointOrientations);

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
```

[Back to README](../README.md)