using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Math;
public class FPSCounter : MonoBehaviour
{
    private SettingsClass settings;
    int fps_Amount;
    float fps_Sum;
    private void Start()
    {
        settings = GameObject.Find("SettingsManager").GetComponent<SettingsClass>();
    }
    void OnGUI()
    {
        if (settings?.settings?.ShowFPS ?? false)
        {
            fps_Sum += (1.0f / Time.smoothDeltaTime);
            fps_Amount++;
            GUI.Label(new Rect(0, 70, 100, 100), $"{ (int)(1.0f / Time.smoothDeltaTime)} FPS \n" +
                $"{(int)fps_Sum / fps_Amount} Average");
        }
    }

}
