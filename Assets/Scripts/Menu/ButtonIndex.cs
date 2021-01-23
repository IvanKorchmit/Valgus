using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class ButtonIndex : MonoBehaviour
{
    private SettingsClass settings;
    public TextMeshProUGUI text;
    public AudioSource audioMusic;
    public int index;
    public int newGameScene;
    public MenuNavigation menuNav;
    public Color selectedColor;
    public float FloatOpt;
    public int IntOpt;
    public bool BoolOpt;
    public menuAction action;
    public Option option;
    public enum Option
    {
        enableFPS,
        targetFPS,
        masterVolume,
        musicVolume,
        soundVolume
    }
    public enum menuAction
    {
        newGame,
        levelSelect,
        Options
    }
    private void Start()
    {
        settings = GameObject.Find("SettingsManager").GetComponent<SettingsClass>();
        IntializeOptions();
        UpdateOptions();
    }
    public void DoAction()
    {
        if (isSelected())
        {
            switch (action)
            {
                case menuAction.newGame:
                    SceneManager.LoadSceneAsync(newGameScene);
                    break;
                case menuAction.levelSelect:
                    SceneManager.LoadSceneAsync(int.Parse(text.text));
                    break;
                case menuAction.Options:
                    MakeOption();
                    break;
            }
        }
    }
    private void IntializeOptions()
    {
        if(action == menuAction.Options)
        {
            switch (option)
            {
                case Option.enableFPS:
                    BoolOpt = settings.settings.ShowFPS;
                    break;
                case Option.targetFPS:
                    IntOpt = settings.settings.TargetFPS;
                    break;
                case Option.masterVolume:
                    FloatOpt = settings.settings.MasterVolume;
                    break;
                case Option.musicVolume:
                    FloatOpt = settings.musVolume;
                    break;
                case Option.soundVolume:
                    FloatOpt = settings.settings.soundVolume;
                    break;
                default:
                    break;
            }
        }
    }
    public void Select()
    {
        if(isSelected())
        {
            GetComponent<TextMeshProUGUI>().color = selectedColor;
            menuNav.sfxManager.PlaySound(SoundEffect.SoundEvent.ButtonPush);
        }
        else
        {
            GetComponent<TextMeshProUGUI>().color = Color.gray;
        }
    }
    public bool isSelected()
    {
        return index == menuNav.navigationID;
    }
    private void MakeOption()
    {
        switch (option)
        {
            case Option.enableFPS:
                BoolOpt = !BoolOpt;
                settings.settings.ShowFPS = BoolOpt;
                break;
            case Option.targetFPS:
                IntOpt += 30;
                if (IntOpt > 300)
                {
                    IntOpt = 30;
                }
                settings.settings.TargetFPS = IntOpt;
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = settings.settings.TargetFPS;
                break;
            case Option.masterVolume:
                FloatOpt += 0.05f;
                audioMusic = GameObject.Find("Music").GetComponent<AudioSource>();
                if (FloatOpt > 1.05f)
                {
                    FloatOpt = 0;
                }
                settings.settings.MasterVolume = FloatOpt;
                settings.musVolume = settings.musVolume;
                break;
            case Option.musicVolume:
                FloatOpt += 0.05f;
                if (FloatOpt > 1.05f)
                {
                    FloatOpt = 0;
                }
                settings.musVolume = FloatOpt;
                break;
            case Option.soundVolume:
                FloatOpt += 0.05f;
                if (FloatOpt > 1.05f)
                {
                    FloatOpt = 0;
                }
                settings.settings.soundVolume = FloatOpt;
                menuNav.sfxManager.PlaySound(SoundEffect.SoundEvent.ButtonPush);
                break;
            default:
                break;
        }
        settings.Save();
        UpdateOptions();
    }
    private void UpdateOptions()
    {
        TextMeshProUGUI textM = GetComponent<TextMeshProUGUI>();
        if (action == menuAction.Options)
        {
            switch (option)
            {
                case Option.enableFPS:
                    textM.text = BoolOpt ? "Show FPS" : "Don't show FPS";
                    break;
                case Option.targetFPS:
                    textM.text = $"Target FPS {IntOpt}";
                    break;
                case Option.masterVolume:
                    textM.text = $"Master Volume: {(int)(FloatOpt * 100)}%";
                    break;
                case Option.musicVolume:
                    textM.text = $"Music Volume: {(int)(FloatOpt * 100)}%";
                    break;
                case Option.soundVolume:
                    textM.text = $"Sound Volume: {(int)(FloatOpt * 100)}%";
                    break;
                default:
                    break;
            }
        }
    }
}
