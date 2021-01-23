using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuNavigation : MonoBehaviour
{
    private SettingsClass settings;
    public int navigationID;
    private int ButtonCount;
    public SoundManager sfxManager;
    public int LevelCount;
    // Start is called before the first frame update
    void Start()
    {
        ButtonCount = GameObject.FindGameObjectsWithTag("Button").Length-1;
        Select();
        if(SceneManager.GetActiveScene().buildIndex == 0)
        {
            settings = GameObject.Find("SettingsManager").GetComponent<SettingsClass>();
            settings.Load();
        }
    }
    public void SetIndex()
    {
        int i = 0;
        foreach (var item in GameObject.FindGameObjectsWithTag("Button"))
        {
            item.GetComponent<ButtonIndex>().index = i;
            i++;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.S))
        {
            navigationID++;
            clamp();
            Select();
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            navigationID--;
            clamp();
            Select();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            foreach (var item in GameObject.FindGameObjectsWithTag("Button"))
            {
                var itemMenu = item.GetComponent<ButtonIndex>();
                if (itemMenu.isSelected())
                {
                    if (itemMenu.action == ButtonIndex.menuAction.levelSelect)
                    {
                        int parsed = int.Parse(itemMenu.text.text) - 1;
                        if (parsed < 1)
                        {
                            parsed = LevelCount;
                        }
                        itemMenu.text.text = $"{parsed}";
                        
                        break;
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            foreach (var item in GameObject.FindGameObjectsWithTag("Button"))
            {
                var itemMenu = item.GetComponent<ButtonIndex>();
                if (itemMenu.isSelected())
                {
                    if (itemMenu.action == ButtonIndex.menuAction.levelSelect)
                    {
                        int parsed = int.Parse(itemMenu.text.text) + 1;
                        if(parsed > LevelCount)
                        {
                            parsed = 1;
                        }
                        itemMenu.text.text = $"{parsed}";
                        break;
                    }
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            foreach (var item in GameObject.FindGameObjectsWithTag("Button"))
            {
                var itemMenu = item.GetComponent<ButtonIndex>();
                if (itemMenu.isSelected())
                {
                    itemMenu.DoAction();
                    break;
                }
            }
        }
    }
    private void clamp()
    {
        if(navigationID > ButtonCount)
        {
            navigationID = 0;
        }
        if(navigationID < 0)
        {
            navigationID = ButtonCount;
        }
    }
    private void Select()
    {
        foreach (var item in GameObject.FindGameObjectsWithTag("Button"))
        {
            var itemMenu = item.GetComponent<ButtonIndex>();
            itemMenu.Select();
        }
    }
}
