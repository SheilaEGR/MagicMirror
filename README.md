# Magic Mirror Project

---

__Prof. Pascal Fallavollita__

*University of Ottawa*

www.metrics-lab.ca

---

## Contents

### 1. MM_unity

Content designed to run on Unity. The codes where designed and tested on unity 2018.4.5f1 but the Scripts can run on Unity 2017 too.

To run the demos, you need the following:

- [Kinect for Windows SDK 2.0](https://www.microsoft.com/en-ca/download/details.aspx?id=44561)
- [Kinect v2 Unity plugin](https://go.microsoft.com/fwlink/?LinkID=513177)

There is a set of sample scenes to help you get familiar with the scripts provided.

#### 1.1 Simple RGB scene

This is [the simples scene](Scenes/SimpleRGB.md) we can build. A cube with aspect ratio of 16:9 displays the color frames acquired by the Kinecv v2 sensor.

#### 1.2 Simple Body scene

In [this scene](Scenes/SimpleBody.md), the sensor detects and tracks one body, then it displays every body joint position and orientation using a box as marker.

#### 1.3 Body and RGB scene

In [this scene](Scenes/BodyAndRGB.md), the sensor acquires both RGB frames and body information, and displays all that information superimposed. This is the first step towards __Augmented Reality__.