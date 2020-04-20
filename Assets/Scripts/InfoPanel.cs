using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour
{
    public Text title;
    public Text text;
    public GameObject button;
    public Text buttonText;

    private int l = 0;
    private int d = 0;
    private int a = 0;
    private int s = 0;

    void Start()
    {
        title.text = "Stats";
        displayStats();
    }

    public void setTitle(string t)
    {
        title.text = t;
    }

    public void setText(string t)
    {
        text.text = t;
    }

    public void displayStats()
    {
        string spd = "* ";
        for (int i = 0; i < s; i++)
        {
            spd += "* ";
        }
        text.text = "Speed: " + spd;
    }

    public void showButton(string str)
    {
        button.SetActive(true);
        string type = str.Substring(0, 1);
        int level = System.Convert.ToInt32(str.Substring(1, 1));
        switch (type)
        {
            case "l":
                if (level == l)
                {
                    buttonText.text = "Upgrade";
                    buttonText.color = Color.white;
                }
                else if (level > l)
                {
                    buttonText.text = "LOCKED!";
                    buttonText.color = Color.grey;
                }
                else
                {
                    buttonText.text = "Already Upgraded";
                    buttonText.color = Color.grey;
                }
                break;
            case "d":
                if (level == d)
                {
                    buttonText.text = "Upgrade";
                    buttonText.color = Color.white;
                }
                else if (level > d)
                {
                    buttonText.text = "LOCKED!";
                    buttonText.color = Color.grey;
                }
                else
                {
                    buttonText.text = "Already Upgraded";
                    buttonText.color = Color.grey;
                }
                break;
            case "a":
                if (level == a)
                {
                    buttonText.text = "Upgrade";
                    buttonText.color = Color.white;
                }
                else if (level > a)
                {
                    buttonText.text = "LOCKED!";
                    buttonText.color = Color.grey;
                }
                else
                {
                    buttonText.text = "Already Upgraded";
                    buttonText.color = Color.grey;
                }
                break;
            case "s":
                if (level == s)
                {
                    buttonText.text = "Upgrade";
                    buttonText.color = Color.white;
                }
                else if (level > s)
                {
                    buttonText.text = "LOCKED!";
                    buttonText.color = Color.grey;
                }
                else
                {
                    buttonText.text = "Already Upgraded";
                    buttonText.color = Color.grey;
                }
                break;
        }
    }

    public void Upgrade()
    {
        // if we can upgrade
        if (buttonText.text == "Upgrade")
        {

            buttonText.text = "Already Upgraded";
            buttonText.color = Color.grey;

            // which one are we upgrading
            if (title.text.Substring(0, 5) == "Quick")
            {
                Animal.incrementSpeed();
                s += 1;
            }
        }
    }

    public void hideButton()
    {
        button.SetActive(false);
    }
}
