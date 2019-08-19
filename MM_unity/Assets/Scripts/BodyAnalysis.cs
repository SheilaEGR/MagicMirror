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
using Windows.Kinect;

public class BodyAnalysis : MonoBehaviour
{
    private static CoordinateMapper _Mapper;       // Maps from body to color space

    public static void SetMapper(CoordinateMapper mapper)
    {
        _Mapper = mapper;
    }

    // ===============================================================================================
    // ==========                            3D ANALYSIS                                    ==========
    // ===============================================================================================

    /*! \brief Compute the height of the currently tracked body in 3D space, for scaling purposes.
     Methodology obtained from https://pterneas.com/kinect/

     \param body Reference to a body detected and tracked by the kinect sensor.
     \param scale The current size of the "screen" where the 3D model will be drawn.
     \return Height of the tracked body in 3D space.
     */
    public static float ComputeBodyHeight(Body body)
    {
        UnityEngine.Vector3[] jointPos = convertToUnityPosition(body.Joints);
        float dHead2Neck = UnityEngine.Vector3.Distance(jointPos[(int)JointType.Head], jointPos[(int)JointType.Neck]);
        float dNeck2SpineShoulder = UnityEngine.Vector3.Distance(jointPos[(int)JointType.Neck], jointPos[(int)JointType.SpineShoulder]);
        float dSpine = UnityEngine.Vector3.Distance(jointPos[(int)JointType.SpineShoulder], jointPos[(int)JointType.SpineBase]);

        int trackedJointsLeft = 0;
        if (body.Joints[JointType.HipLeft].TrackingState == TrackingState.Tracked) trackedJointsLeft++;
        if (body.Joints[JointType.KneeLeft].TrackingState == TrackingState.Tracked) trackedJointsLeft++;
        if (body.Joints[JointType.AnkleLeft].TrackingState == TrackingState.Tracked) trackedJointsLeft++;
        if (body.Joints[JointType.FootLeft].TrackingState == TrackingState.Tracked) trackedJointsLeft++;

        int trackedJointsRight = 0;
        if (body.Joints[JointType.HipRight].TrackingState == TrackingState.Tracked) trackedJointsRight++;
        if (body.Joints[JointType.KneeRight].TrackingState == TrackingState.Tracked) trackedJointsRight++;
        if (body.Joints[JointType.AnkleRight].TrackingState == TrackingState.Tracked) trackedJointsRight++;
        if (body.Joints[JointType.FootRight].TrackingState == TrackingState.Tracked) trackedJointsRight++;

        float dLeg;
        if (trackedJointsLeft > trackedJointsRight)
            dLeg = ComputeLegLength(body, false);
        else
            dLeg = ComputeLegLength(body, false);

        return (dHead2Neck + dNeck2SpineShoulder + dSpine + dLeg + 0.08f);
    }


    /*! Convert Kinect's 3D position to Unity's Vector3
     Kinect reads and stores body joints using its own "Joint" structure. For displaying and further analysis purposes, 
     we need to convert to Unity's Vector3 structure.
     
     \param kinectJoint Position of the body joints in Kinect's Joint structure.
     \param scale The size of the screen where the model will be placed.
     \return Position of the body joints in Unity's Vector3 structure, more suitable for further analysis.
         */
    public static UnityEngine.Vector3[] convertToUnityPosition(Dictionary<JointType, Windows.Kinect.Joint> kinectJoint)
    {
        UnityEngine.Vector3[] unityJoint = new UnityEngine.Vector3[kinectJoint.Count];
        for (int i = 0; i < kinectJoint.Count; i++)
        {
            unityJoint[i] = new UnityEngine.Vector3(kinectJoint[(JointType)i].Position.X,
                                        kinectJoint[(JointType)i].Position.Y,
                                        kinectJoint[(JointType)i].Position.Z);
        }
        return unityJoint;
    }


    /*! Convert Kinect's orientation to Unity's Quaternion 
     Kinect has its own Quaternion representation for body joint's orientation.
     This function converts that representation to Unity's Quaternion for further analysis.

     \param kinectJoint Position of thbody joints in Kinect's Joint structure.
     \return Vector of Unity's Quaternion representing each body joint's orientation.
     */
    public static UnityEngine.Quaternion[] convertToUnityOrientation(Dictionary<JointType, JointOrientation> kinectJoint)
    {
        UnityEngine.Quaternion[] unityJoint = new UnityEngine.Quaternion[kinectJoint.Count];
        for (int i = 0; i < kinectJoint.Count; i++)
        {
            unityJoint[i] = new UnityEngine.Quaternion(kinectJoint[(JointType)i].Orientation.X,
                kinectJoint[(JointType)i].Orientation.Y,
                kinectJoint[(JointType)i].Orientation.Z,
                kinectJoint[(JointType)i].Orientation.W);
        }
        return unityJoint;
    }


    public static float ComputeArmLength(Body body, bool rightArm)
    {
        float length = 0;
        UnityEngine.Vector3[] jointPos = convertToUnityPosition(body.Joints);

        if (rightArm)
        {
            length = UnityEngine.Vector3.Distance(jointPos[(int)JointType.ShoulderRight], jointPos[(int)JointType.ElbowRight]);
            length += UnityEngine.Vector3.Distance(jointPos[(int)JointType.ElbowRight], jointPos[(int)JointType.WristRight]);
            length += UnityEngine.Vector3.Distance(jointPos[(int)JointType.WristRight], jointPos[(int)JointType.HandRight]);
        }
        else
        {
            length = UnityEngine.Vector3.Distance(jointPos[(int)JointType.ShoulderLeft], jointPos[(int)JointType.ElbowLeft]);
            length += UnityEngine.Vector3.Distance(jointPos[(int)JointType.ElbowLeft], jointPos[(int)JointType.WristLeft]);
            length += UnityEngine.Vector3.Distance(jointPos[(int)JointType.WristLeft], jointPos[(int)JointType.HandLeft]);
        }

        return length;
    }

    public static float ComputeLegLength(Body body, bool rightLeg)
    {
        float length = 0;
        UnityEngine.Vector3[] jointPos = convertToUnityPosition(body.Joints);

        if (rightLeg)
        {
            length = UnityEngine.Vector3.Distance(jointPos[(int)JointType.HipRight], jointPos[(int)JointType.KneeRight]);
            length += UnityEngine.Vector3.Distance(jointPos[(int)JointType.KneeRight], jointPos[(int)JointType.AnkleRight]);
            length += UnityEngine.Vector3.Distance(jointPos[(int)JointType.AnkleRight], jointPos[(int)JointType.FootRight]);
        }
        else
        {
            length = UnityEngine.Vector3.Distance(jointPos[(int)JointType.HipLeft], jointPos[(int)JointType.KneeLeft]);
            length += UnityEngine.Vector3.Distance(jointPos[(int)JointType.KneeLeft], jointPos[(int)JointType.AnkleLeft]);
            length += UnityEngine.Vector3.Distance(jointPos[(int)JointType.AnkleLeft], jointPos[(int)JointType.FootLeft]);
        }

        return length;
    }




    // ===============================================================================================
    // ==========                            2D ANALYSIS                                    ==========
    // ===============================================================================================

    /*! \brief Compute the height of the currently tracked body in 2D space, for scaling purposes
     Methodology obtained from https://pterneas.com/kinect/

     \param body Reference to a body detected and tracked by the kinect sensor.
     \param scale The size of the 2D screen where the model will be drawn.
     \param position The position where the screen is placed. Relevant for the conversion between Kinect's Vector2 and Unity's Vector2.
     \return Height of the tracked body in 2D coordinates.
    */
    public static float ComputeBodyHeightInScreenCoordinates(Body body, UnityEngine.Vector2 scale, UnityEngine.Vector2 position)
    {
        UnityEngine.Vector2[] jointPos = convertToUnity2DPosition(body.Joints, scale, position);
        float dHead2Neck = UnityEngine.Vector2.Distance(jointPos[(int)JointType.Head], jointPos[(int)JointType.Neck]);
        float dNeck2SpineShoulder = UnityEngine.Vector2.Distance(jointPos[(int)JointType.Neck], jointPos[(int)JointType.SpineShoulder]);
        float dSpine = UnityEngine.Vector2.Distance(jointPos[(int)JointType.SpineShoulder], jointPos[(int)JointType.SpineBase]);

        int trackedJointsLeft = 0;
        if (body.Joints[JointType.HipLeft].TrackingState == TrackingState.Tracked) trackedJointsLeft++;
        if (body.Joints[JointType.KneeLeft].TrackingState == TrackingState.Tracked) trackedJointsLeft++;
        if (body.Joints[JointType.AnkleLeft].TrackingState == TrackingState.Tracked) trackedJointsLeft++;
        if (body.Joints[JointType.FootLeft].TrackingState == TrackingState.Tracked) trackedJointsLeft++;

        int trackedJointsRight = 0;
        if (body.Joints[JointType.HipRight].TrackingState == TrackingState.Tracked) trackedJointsRight++;
        if (body.Joints[JointType.KneeRight].TrackingState == TrackingState.Tracked) trackedJointsRight++;
        if (body.Joints[JointType.AnkleRight].TrackingState == TrackingState.Tracked) trackedJointsRight++;
        if (body.Joints[JointType.FootRight].TrackingState == TrackingState.Tracked) trackedJointsRight++;

        float dHip2Knee, dKnee2Ankle, dAnkle2Foot;
        if (trackedJointsLeft > trackedJointsRight)
        {
            dHip2Knee = UnityEngine.Vector2.Distance(jointPos[(int)JointType.HipLeft], jointPos[(int)JointType.KneeLeft]);
            dKnee2Ankle = UnityEngine.Vector2.Distance(jointPos[(int)JointType.KneeLeft], jointPos[(int)JointType.AnkleLeft]);
            dAnkle2Foot = UnityEngine.Vector2.Distance(jointPos[(int)JointType.AnkleLeft], jointPos[(int)JointType.FootLeft]);
        }
        else
        {
            dHip2Knee = UnityEngine.Vector2.Distance(jointPos[(int)JointType.HipRight], jointPos[(int)JointType.KneeRight]);
            dKnee2Ankle = UnityEngine.Vector2.Distance(jointPos[(int)JointType.KneeRight], jointPos[(int)JointType.AnkleRight]);
            dAnkle2Foot = UnityEngine.Vector2.Distance(jointPos[(int)JointType.AnkleRight], jointPos[(int)JointType.FootRight]);
        }

        return (dHead2Neck + dNeck2SpineShoulder + dSpine + dHip2Knee + dKnee2Ankle + dAnkle2Foot + 0.08f);
    }


    /*! Convert Kinect's 2D position to Unity's Vector2 
     Kinect reads and stores body joints using its own "Joint" structure. For displaying and further 2D analysis purposes, 
     we need to convert to Unity's Vector2 structure.
     
     \param kinectJoint Position of the body joints in Kinect's Joint structure.
     \param scale Size of the screen where the model will be placed.
     \param position Position of the screen where the model will be placed.
     \return Position of the body joints in Unity's Vector2 structure, more suitable for further analysis.
     */
    public static UnityEngine.Vector2[] convertToUnity2DPosition(Dictionary<JointType, Windows.Kinect.Joint> kinectJoint, UnityEngine.Vector2 scale, UnityEngine.Vector2 position)
    {
        // CameraSpacePoint is a representation of 3D position. The units are in meters.
        // CameraSpacePoint is part of Kinect's Joint structure, so we need to first extract only the vector of 3D positions.
        CameraSpacePoint[] cameraPoints = new CameraSpacePoint[25];
        for (JointType jt = JointType.SpineBase; jt <= JointType.ThumbRight; jt++)
            cameraPoints[(int)jt] = kinectJoint[jt].Position;

        // ColorSpacePoint is a representation of 2D position. 
        // Kinect maps 3D points to 2D points
        ColorSpacePoint[] colorPoints = new ColorSpacePoint[25];
        _Mapper.MapCameraPointsToColorSpace(cameraPoints, colorPoints);

        // Vector2 is Unity's representation of a 2D position.
        // The conversion involves the scaling to the screen where the rendering will happen.
        UnityEngine.Vector2[] colorJointPosition = new UnityEngine.Vector2[25];
        for (JointType jt = JointType.SpineBase; jt <= JointType.ThumbRight; jt++)
        {
            colorJointPosition[(int)jt] = new UnityEngine.Vector2(colorPoints[(int)jt].X, colorPoints[(int)jt].Y);
            colorJointPosition[(int)jt].x = (colorJointPosition[(int)jt].x * scale.x / 1920.0f) - scale.x / 2.0f + position.x;
            colorJointPosition[(int)jt].y = scale.y / 2.0f - (colorJointPosition[(int)jt].y * scale.y / 1080.0f) + position.y;
        }
        return colorJointPosition;
    }


    
}
