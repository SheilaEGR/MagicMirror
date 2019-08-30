# Simple RGB scene

To acquire and display RGB images from the Kinect v2 sensor, there are two important components to be added to the scene. 

1. __RGB Sensor__. This is an empty object containing the __SensorRGB__ script.
2. __Display__. In this scene, a cube was created to display the RGB image, however, any solid can do the job. Add the __DisplayRGB__ script and remember to parse the __Sensor RGB__ as a reference to it.

![SimpleRGB scene](Images/BasicRGB.png)

To keep the image in the right proportions, the cube should have an aspect ratio of 16:9 in one of its faces. Recall that kinect v2 sensor delivers a color image with 1920 pixels width and 1080 pixels height.

## Sensor RGB script

This script was taken from the original Kinect SDK for Unity documentation. The only method you are interested in when using this script is `GetColorTexture`. Call this method from the displaying object to attach the last acquired frame as a texture to it.

## Display RGB script

In this script we receive the __SensorRGB__ object as a public attribute.

```csharp
using UnityEngine;

public class DisplayRGB : MonoBehaviour
{
    public GameObject sensorRGB;       
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
```

If you will use this object superimposing virtual elements (for Augmented Reality), disable _Cast shadows_ and _receive shadows_ parameters from the __Mesh renderer__ component. Otherwise, the virtual object will cast a shadow on the display object, resulting in a non pleasant experience.

[Back to README](../README.md)