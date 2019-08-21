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
using Windows.Kinect;

public class BodyAnalysis 
{
    private static CoordinateMapper _Mapper;       // Maps from body to color space
    

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


    /*! \brief Convert Kinect's 3D position to Unity's Vector3
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


    /*! \brief Convert Kinect's orientation to Unity's Quaternion 
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


    /*! \brief Compute arm length in 3D space.
     * 
     * \param body Reference to a body detected and tracked by the kinect sensor.
     * \param rightArm True for right arm, False for left arm.
     * \return Arm length in 3D space.
     */
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


    /*! \brief Compute leg length in 3D scene.
     * 
     * \param body Reference to a body detected and tracked by the kinect sensor.
     * \param rightLeg True for right leg, False for left leg.
     * \return Leg length in 3D space.
     */
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


    /*! \brief Angle of the spine in flexion movement.
     * 
     * \param body Reference to a body detected and tracked by the kinect sensor.
     * \param coronalPlane true for coronal plane, false for sagital plane.
     * \return Angle of spine in flexion movement.
     */
    public static float FlexionSpineAngle(Body body, bool coronalPlane)
    {
        UnityEngine.Vector3[] jointPos = convertToUnityPosition(body.Joints);

        UnityEngine.Vector3 spineVec = jointPos[(int)JointType.SpineBase] - jointPos[(int)JointType.SpineShoulder];
        UnityEngine.Vector2 spineVec2D;
        if (coronalPlane)
        {
            spineVec2D = new UnityEngine.Vector2(spineVec.x, spineVec.y);
        }
        else
        {
            spineVec2D = new UnityEngine.Vector2(spineVec.z, spineVec.y);
        }

        return UnityEngine.Vector2.Angle(UnityEngine.Vector2.up, spineVec2D);
    }


    /*! \brief Angle of the shoulder in abduction movement.
     * 
     * \param Reference to a body detected and tracked by the kinect sensor.
     * \param rightArm true for right arm, false for left arm.
     * \return Angle of shoulder in abduction movement.
     */
    public static float AbductionShoulderAngle(Body body, bool rightArm)
    {
        UnityEngine.Vector3[] jointPos = convertToUnityPosition(body.Joints);

        UnityEngine.Vector3 spineVec = jointPos[(int)JointType.SpineBase] - jointPos[(int)JointType.SpineShoulder];
        UnityEngine.Vector3 armVec;
        if (rightArm)
        {
            armVec = jointPos[(int)JointType.ShoulderRight] - jointPos[(int)JointType.ElbowRight];
        }
        else
        {
            armVec = jointPos[(int)JointType.ShoulderLeft] - jointPos[(int)JointType.ElbowLeft];
        }

        UnityEngine.Vector2 spineVec2D = new UnityEngine.Vector2(spineVec.x, spineVec.y);
        UnityEngine.Vector2 armVec2D = new UnityEngine.Vector2(armVec.x, armVec.y);

        return UnityEngine.Vector2.Angle(spineVec2D, armVec2D);
    }


    /*! \brief Angle of the leg in abduction movement (taking hip as rotation pivot).
     * 
     * \param body Reference to a body detected and tracked by the kinect sensor.
     * \param rightLeg true for right leg, false for left leg.
     * \return Angle of leg in abduction movement.
     */
    public static float AbductionLegAngle(Body body, bool rightLeg)
    {
        UnityEngine.Vector3[] jointPos = convertToUnityPosition(body.Joints);

        UnityEngine.Vector3 spineVec = jointPos[(int)JointType.SpineBase] - jointPos[(int)JointType.SpineShoulder];
        UnityEngine.Vector3 legVec;
        if (rightLeg)
        {
            legVec = jointPos[(int)JointType.ShoulderRight] - jointPos[(int)JointType.ElbowRight];
        }
        else
        {
            legVec = jointPos[(int)JointType.ShoulderLeft] - jointPos[(int)JointType.ElbowLeft];
        }

        UnityEngine.Vector2 spineVec2D = new UnityEngine.Vector2(spineVec.x, spineVec.y);
        UnityEngine.Vector2 legVec2D = new UnityEngine.Vector2(legVec.x, legVec.y);

        return UnityEngine.Vector2.Angle(spineVec2D, legVec2D);
    }


    /*! \brief Angle of shoulder in Flexion movement.
     * 
     * \param body Reference to a body detected and tracked by the kinect sensor.
     * \param rightArm true for right arm, false for left arm.
     * \return Angle of shoulder in flexion movement.
     */
    public static float FlexionShoulderAngle(Body body, bool rightArm)
    {
        UnityEngine.Vector3[] jointPos = convertToUnityPosition(body.Joints);
 
        UnityEngine.Vector3 armVec;
        if (rightArm)
        {
            armVec = jointPos[(int)JointType.ShoulderRight] - jointPos[(int)JointType.ElbowRight];
        }
        else
        {
            armVec = jointPos[(int)JointType.ShoulderLeft] - jointPos[(int)JointType.ElbowLeft];
        }

        UnityEngine.Vector2 armVec2D = new UnityEngine.Vector2(armVec.z, armVec.y);

        return UnityEngine.Vector2.Angle(UnityEngine.Vector2.down, armVec2D);
    }


    /*! \brief Angle of elbow in flexion movement.
     * 
     * \param body Reference to a body detected and tracked by the kinect sensor.
     * \param rightArm true for right arm, false for left arm.
     * \return angle of elbow in flexion movement.
     */
    public static float FlexionElbowAngle(Body body, bool rightArm)
    {
        UnityEngine.Vector3[] jointPos = convertToUnityPosition(body.Joints);

        UnityEngine.Vector3 shoulderElbow, elbowWrist;
        if(rightArm)
        {
            shoulderElbow = jointPos[(int)JointType.ShoulderRight] - jointPos[(int)JointType.ElbowRight];
            elbowWrist = jointPos[(int)JointType.ElbowRight] - jointPos[(int)JointType.WristRight];
        } else
        {
            shoulderElbow = jointPos[(int)JointType.ShoulderLeft] - jointPos[(int)JointType.ElbowLeft];
            elbowWrist = jointPos[(int)JointType.ElbowLeft] - jointPos[(int)JointType.WristLeft];
        }

        float argument = UnityEngine.Vector3.Dot(shoulderElbow, elbowWrist) / (UnityEngine.Vector3.Magnitude(shoulderElbow) * UnityEngine.Vector3.Magnitude(elbowWrist));
        return Mathf.Acos(argument);
    }


    /*! \brief Angle of leg in Flexion movement.
     * 
     * \param body Reference to a body detected and tracked by the kinect sensor.
     * \param rightLeg true for right leg, false for left leg.
     * \return Angle of leg in flexion movement.
     */
    public static float FlexionLegAngle(Body body, bool rightLeg)
    {
        UnityEngine.Vector3[] jointPos = convertToUnityPosition(body.Joints);

        UnityEngine.Vector3 legVec;
        if (rightLeg)
        {
            legVec = jointPos[(int)JointType.HipRight] - jointPos[(int)JointType.KneeRight];
        }
        else
        {
            legVec = jointPos[(int)JointType.HipLeft] - jointPos[(int)JointType.KneeLeft];
        }

        UnityEngine.Vector2 legVec2D = new UnityEngine.Vector2(legVec.z, legVec.y);

        return UnityEngine.Vector2.Angle(UnityEngine.Vector2.down, legVec);
    }


    /*! \brief Angle of knee in flexion movement.
     * 
     * \param body Reference to a body detected and tracked by the kinect sensor.
     * \param rightLeg true for right leg, false for left leg.
     * \return Angle of knee in flexion movement.
     */
    public static float FlexionKneeAngle(Body body, bool rightLeg)
    {
        UnityEngine.Vector3[] jointPos = convertToUnityPosition(body.Joints);

        UnityEngine.Vector3 hipKnee, kneeAnkle;
        if (rightLeg)
        {
            hipKnee = jointPos[(int)JointType.HipRight] - jointPos[(int)JointType.KneeRight];
            kneeAnkle = jointPos[(int)JointType.KneeRight] - jointPos[(int)JointType.AnkleRight];
        }
        else
        {
            hipKnee = jointPos[(int)JointType.HipLeft] - jointPos[(int)JointType.KneeLeft];
            kneeAnkle = jointPos[(int)JointType.KneeLeft] - jointPos[(int)JointType.AnkleLeft];
        }

        float argument = UnityEngine.Vector3.Dot(hipKnee, kneeAnkle) / (UnityEngine.Vector3.Magnitude(hipKnee) * UnityEngine.Vector3.Magnitude(kneeAnkle));
        return Mathf.Acos(argument);
    }


    // ===============================================================================================
    // ==========                            2D ANALYSIS                                    ==========
    // ===============================================================================================


    /*! \brief Set Kinect's CoordinateMapper for conversion from 3D to screen coordinates.
     * 
     * \param mapper Reference to a CoordinateMapper.
     */
    public static void SetMapper(CoordinateMapper mapper)
    {
        _Mapper = mapper;
    }


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

        float legLength;
        if (trackedJointsLeft > trackedJointsRight)
        {
            legLength = ComputeLegLength2D(body, scale, position, false);
        }
        else
        {
            legLength = ComputeLegLength2D(body, scale, position, true);
        }

        return (dHead2Neck + dNeck2SpineShoulder + dSpine + legLength + 0.08f);
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


    /*! \brief Compute arm length in screen coordinates.
     * 
     * \param kinectJoint Position of the body joints in Kinect's Joint structure.
     * \param scale Size of the screen where the model will be placed.
     * \param position Position of the screen where the model will be placed.
     * \param rightArm True for right arm, False for left arm.
     * \return Arm length in screen coordinates.
     */
    public static float ComputeArmLength2D(Body body, UnityEngine.Vector2 scale, UnityEngine.Vector2 position, bool rightArm)
    {
        UnityEngine.Vector2[] jointPos = convertToUnity2DPosition(body.Joints, scale, position);

        float length = 0f;
        if (rightArm)
        {
            length += UnityEngine.Vector2.Distance(jointPos[(int)JointType.ShoulderRight], jointPos[(int)JointType.ElbowRight]);
            length += UnityEngine.Vector2.Distance(jointPos[(int)JointType.ElbowRight], jointPos[(int)JointType.WristRight]);
            length += UnityEngine.Vector2.Distance(jointPos[(int)JointType.WristRight], jointPos[(int)JointType.HandRight]);
        } else
        {
            length += UnityEngine.Vector2.Distance(jointPos[(int)JointType.ShoulderLeft], jointPos[(int)JointType.ElbowLeft]);
            length += UnityEngine.Vector2.Distance(jointPos[(int)JointType.ElbowLeft], jointPos[(int)JointType.WristLeft]);
            length += UnityEngine.Vector2.Distance(jointPos[(int)JointType.WristLeft], jointPos[(int)JointType.HandLeft]);
        }

        return length;
    }


    /*! \brief Compute leg length in screen coordinates.
     * 
     * \param kinectJoint Position of the body joints in Kinect's Joint structure.
     * \param scale Size of the screen where the model will be placed.
     * \param position Position of the screen where the model will be placed.
     * \param rightLeg True for right leg, False for left leg.
     * \return Leg length in screen coordinates.
     */
    public static float ComputeLegLength2D(Body body, UnityEngine.Vector2 scale, UnityEngine.Vector2 position, bool rightLeg)
    {
        UnityEngine.Vector2[] jointPos = convertToUnity2DPosition(body.Joints, scale, position);

        float length = 0f;
        if (rightLeg)
        {
            length += UnityEngine.Vector2.Distance(jointPos[(int)JointType.HipRight], jointPos[(int)JointType.KneeRight]);
            length += UnityEngine.Vector2.Distance(jointPos[(int)JointType.KneeRight], jointPos[(int)JointType.AnkleRight]);
            length += UnityEngine.Vector2.Distance(jointPos[(int)JointType.AnkleRight], jointPos[(int)JointType.FootRight]);
        }
        else
        {
            length += UnityEngine.Vector2.Distance(jointPos[(int)JointType.HipLeft], jointPos[(int)JointType.KneeLeft]);
            length += UnityEngine.Vector2.Distance(jointPos[(int)JointType.KneeLeft], jointPos[(int)JointType.AnkleLeft]);
            length += UnityEngine.Vector2.Distance(jointPos[(int)JointType.AnkleLeft], jointPos[(int)JointType.FootLeft]);
        }

        return length;
    }
}
