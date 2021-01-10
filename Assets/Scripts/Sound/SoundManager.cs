using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public SoundEffect[] sounds;
    public GameObject musicObject;
    public AudioClip music;
    public void PlaySound(SoundEffect.SoundEvent e)
    {
        foreach (SoundEffect item in sounds)
        {
            if (item.soundEvent != e) continue;
            if(item.soundEvent == e)
            {
                var sound = item.Play();
                Destroy(sound.gameObject, sound.clip.length + 1);
                break;
            }   
        }
    }
    private void Start()
    {
        musicObject = GameObject.Find("Music");
        if(musicObject == null)
        {
            musicObject = new GameObject("Music", typeof(AudioSource));
            musicObject.GetComponent<AudioSource>().loop = true;
            musicObject.GetComponent<AudioSource>().clip = music;
            musicObject.GetComponent<AudioSource>().Play();
            DontDestroyOnLoad(musicObject);
        }
    }
}
[System.Serializable]
public class SoundEffect
{
    public enum SoundEvent
    {
        onWaterEnter,
        onWaterExit,
        onWaterMove,
        onSwimming,
        onOxygenOut,
        Death,
        onBodyImpact,
        onPickup,
        onJumpImpact,
        onJump,
        onStep,
        ButtonPush,
        Explosion
    }
    public AudioClip[] Sounds; // List of sounds. If there're more than one sound. It will pick randomly
    public SoundEvent soundEvent;
    public AudioSource Play()
    {
        GameObject sound = new GameObject("Sound", typeof(AudioSource));
        AudioSource SoundEffect = sound.GetComponent<AudioSource>();
        sound.transform.parent = Camera.main.transform;
        SoundEffect.clip = Sounds[Random.Range(0,Sounds.Length)];
        SoundEffect.Play();
        SoundEffect.volume = 0.35f;
        return SoundEffect;
    }
    

}
