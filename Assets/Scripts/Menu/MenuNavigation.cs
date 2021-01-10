using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuNavigation : MonoBehaviour
{
    public int navigationID;
    private int ButtonCount;
    public SoundManager sfxManager;
    // Start is called before the first frame update
    void Start()
    {
        ButtonCount = GameObject.FindGameObjectsWithTag("Button").Length-1;
        Select();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.W))
        {
            navigationID++;
            clamp();
            Select();
        }
        else if (Input.GetKeyDown(KeyCode.S))
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
                        itemMenu.text.text = $"{int.Parse(itemMenu.text.text) - 1}";
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
                        itemMenu.text.text = $"{int.Parse(itemMenu.text.text) + 1}";
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
