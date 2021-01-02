using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Step : MonoBehaviour
{
    private SoundManager sfxManager;
    private void Start()
    {
        sfxManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
    }
    public void StepSound()
    {
        sfxManager.PlaySound(SoundEffect.SoundEvent.onStep);
    }
}
