using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Math;
public class FPSCounter : MonoBehaviour
{
    int fps_Amount;
    float fps_Sum;
    float average;
    void OnGUI()
    {
        fps_Sum += (1.0f / Time.smoothDeltaTime);
        fps_Amount++;
        GUI.Label(new Rect(0, 0, 100, 100),$"{ (int)(1.0f / Time.smoothDeltaTime)} FPS \n" +
            $"{(int)fps_Sum/fps_Amount} Average");
    }

}
