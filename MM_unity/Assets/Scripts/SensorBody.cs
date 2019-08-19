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

/** Brief Reads body joints from Kinect v2 sensor.
 * 
 * The Kinect v2 sensor can read and track up to six bodies.
 * 
 * Attach this script to an empty object. Make a reference from this object to the one 
 * controlling the display of the body.
 */
public class SensorBody : MonoBehaviour
{
    // ---------- PRIVATE ATTRIBUTES
    private KinectSensor _Sensor;           // Kinect sensor
    private BodyFrameReader _Reader;        // Body joint reader
    private CoordinateMapper _Mapper;       // Maps from body to color space
    private Body[] _Data = null;            // Stores each body detected (Maximum = 6)

    // ---------- PUBLIC METHODS
    /*!
     \brief Get a vector to all bodies read by the sensor.
     \return Vector to all bodies read by the sensor. The class Body is native from Windows.Kinect.
    */
    public Body[] GetBodies()
    {
        return _Data;
    }

    /*!
     \brief Get a mapper to convert between Kinect's 3D and 2D spaces.
    */
    public CoordinateMapper GetMapper()
    {
        return _Mapper;
    }



    // ---------- PRIVATE METHODS
    void Start()
    {
        // Find a Kinect sensor, and connect to it.
        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _Reader = _Sensor.BodyFrameSource.OpenReader();

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }

            _Mapper = _Sensor.CoordinateMapper;
        }
    }

    void Update()
    {
        // If a kinect sensor was found, read the last acquired frame.
        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                if (_Data == null)
                {
                    _Data = new Body[_Sensor.BodyFrameSource.BodyCount];
                }

                frame.GetAndRefreshBodyData(_Data);

                frame.Dispose();
                frame = null;
            }
        }
    }

    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }

        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }

            _Sensor = null;
        }
    }
}
