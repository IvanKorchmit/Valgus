using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class ButtonIndex : MonoBehaviour
{
    public TextMeshProUGUI text;
    public int index;
    public int newGameScene;
    public MenuNavigation menuNav;
    public Color selectedColor;
    public menuAction action;
    public enum menuAction
    {
        newGame,
        levelSelect,
        Options
    }
    private void Start()
    {
        SetIndex();

    }
    private void SetIndex()
    {
        int i = 0;
        foreach (var item in GameObject.FindGameObjectsWithTag("Button"))
        {
            if(item == gameObject)
            {
                index = i;
                break;
            }
            i++;
        }
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
                    break;
            }
        }
    }
    public void Select()
    {
        if(isSelected())
        {
            GetComponent<SpriteRenderer>().color = selectedColor;
            menuNav.sfxManager.PlaySound(SoundEffect.SoundEvent.ButtonPush);
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
    public bool isSelected()
    {
        return index == menuNav.navigationID;
    }
}
