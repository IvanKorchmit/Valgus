using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
public class SettingsClass : MonoBehaviour
{
    public Settings settings = new Settings();
    public string CONFIG;
    private void Awake()
    {
        CONFIG = Application.dataPath + "/Config/";
    }
    private void Start()
    {
        GameObject go = new GameObject();
        Destroy(go,1);
        DontDestroyOnLoad(go);
        GameObject[] gameobjects = go.scene.GetRootGameObjects();
        foreach (var item in gameobjects)
        {
            if (item.name != "SettingsManager")
            {
                continue;
            }
            else
            {
                return;
            }
        }
        DontDestroyOnLoad(gameObject);
    }
    public float musVolume
    {
        get
        {
            return settings.musicVolume;
        }
        set
        {
            UpdateVolume(value);

        }
    }
    private void UpdateVolume(float value)
    {
        var go = new GameObject();
        DontDestroyOnLoad(go);
        var gameobjects = go.scene.GetRootGameObjects();
        foreach (var item in gameobjects)
        {
            if (item.name != "Music")
            {
                continue;
            }
            else
            {
                item.GetComponent<AudioSource>().volume = value * settings.MasterVolume;
            }
        }
        Destroy(go);
        settings.musicVolume = value;
    }
    public void Save()
    {
        string json = JsonUtility.ToJson(settings);
        if(Directory.Exists(CONFIG))
        {
            File.WriteAllText(CONFIG + "settings.json", json);
        }
        else
        {
            Directory.CreateDirectory(CONFIG);
            File.WriteAllText(CONFIG + "settings.json", json);
        }
    }
    public void Load()
    {
        if (File.Exists(CONFIG + "settings.json"))
        {
            Settings loaded = JsonUtility.FromJson<Settings>(File.ReadAllText(CONFIG + "settings.json"));
            if (loaded != null)
                settings = loaded;
            else
            {
                Debug.LogError("Could not find JSON file or the file is empty!");
            }
        }
        musVolume = musVolume;
    }
}
[System.Serializable]
public class Settings
{
    public bool ShowFPS = false;
    public float MasterVolume = 1f;
    public float musicVolume;
    public float soundVolume = 0.35f;
    public int TargetFPS;
    public Settings()
    {
        ShowFPS = false;
        MasterVolume = 1f;
        musicVolume = 1f;
        soundVolume = 0.35f;
        TargetFPS = 60;

}
}

