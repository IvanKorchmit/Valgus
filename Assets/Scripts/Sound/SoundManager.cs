using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private SettingsClass settings;
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
                if (settings != null)
                {
                    AudioSource sound = item.Play(settings.settings);
                    Destroy(sound.gameObject, sound.clip.length + 1);
                }
                break;
            }   
        }
    }
    private void Start()
    {
        settings = GameObject.Find("SettingsManager").GetComponent<SettingsClass>();
        musicObject = GameObject.Find("Music");
        if(musicObject == null)
        {
            musicObject = new GameObject("Music");
            AudioSource music = musicObject.AddComponent<AudioSource>();
            music.GetComponent<AudioSource>().loop = true;
            music.GetComponent<AudioSource>().clip = this.music;
            music.volume = settings.musVolume * settings.settings.MasterVolume;
            music.GetComponent<AudioSource>().Play();
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
    public AudioSource Play(Settings settings)
    {
        if (settings != null)
        {
            GameObject sound = new GameObject("Sound", typeof(AudioSource));
            AudioSource SoundEffect = sound.GetComponent<AudioSource>();
            sound.transform.parent = Camera.main.transform;
            SoundEffect.clip = Sounds[Random.Range(0, Sounds.Length)];
            SoundEffect.Play();
            SoundEffect.volume = settings.MasterVolume * settings.soundVolume;
            return SoundEffect;
        }
        else
        {
            GameObject sound = new GameObject("Sound", typeof(AudioSource));
            AudioSource SoundEffect = sound.GetComponent<AudioSource>();
            sound.transform.parent = Camera.main.transform;
            SoundEffect.clip = Sounds[Random.Range(0, Sounds.Length)];
            SoundEffect.Play();
            return SoundEffect;
        }
    }
    

}
