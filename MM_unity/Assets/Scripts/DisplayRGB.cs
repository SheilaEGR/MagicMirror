/*  *************************************************************************************
*  Copyright (c) 2017 - onward
*  MEDICAL EDUCATION, TRAINING, AND COMPUTER ASSISTED
*  INTERVENTIONS (METRICS) LAB (www.metrics-lab.ca)
*  UNIVERSITY OF OTTAWA
*  
*  Prof.  Pascal Fallavollita           (pfallavo@uottawa.ca)
*  Dr. Sheila Esmeralda Gonzalez Reyna  (sheila.esmeralda.gonzalez@gmail.com)
************************************************************************************* */

using UnityEngine;

/** Brief Display Kinect v2 RGB image on a solid.
 * 
 * Attach this script to a cube with aspect ratio of 16:9 (same as RGB image retrieved by the sensor) in one of their faces.
 * Drag the object containing the SensorRGB script to the cube containing this script.
 */
public class DisplayRGB : MonoBehaviour
{
    public GameObject sensorRGB;        /*!< Game object containing a  SensorRGB script. */
    private SensorRGB colorCamera;

    void Start()
    {
        gameObject.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
    }

    void Update()
    {
        if (sensorRGB == null)
        {
            return;
        }

        colorCamera = sensorRGB.GetComponent<SensorRGB>();
        if (colorCamera == null)
        {
            return;
        }

        gameObject.GetComponent<Renderer>().material.mainTexture = colorCamera.GetColorTexture();
    }
}
