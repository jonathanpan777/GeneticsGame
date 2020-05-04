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
    public Text pointsText;

    private int la = 0;
    private int lb = 0;
    private int d = 0;
    private int a = 0;
    private int s = 0;

    void Start()
    {
        title.text = "Stats";
        displayStats();
    }

    void Update()
    {
        pointsText.text = Animal.points.ToString();
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
        string dsr = "* ";
        string atk = "* ";
        string dfs = "* ";

        for (int i = 0; i < s; i++)
            spd += "* ";

        for (int i = 0; i < lb; i++)
            dsr += "* ";

        for (int i = 0; i < a; i++)
            atk += "* ";

        for (int i = 0; i < d; i++)
            dfs += "* ";

        text.text = "Desire : " + dsr + '\n' +
                    "Defens : " + dfs + '\n' +
                    "Attack : " + atk + '\n' +
                    "Speed  : " + spd;
    }

    public void showButton(string str)
    {
        button.SetActive(true);
        string type = str.Substring(0, 1);
        int level = System.Convert.ToInt32(str.Substring(1, 1));
        int cost = System.Convert.ToInt32(text.text.Substring(6, 2));
        switch (type)
        {
            case "l":
                string type2 = str.Substring(2, 1);
                if (type2 == "a")
                    if (level == la)
                    {
                        if (cost <= Animal.points)
                        {
                            buttonText.text = "Upgrade";
                            buttonText.color = Color.white;
                        }
                        else
                        {
                            buttonText.text = "Not Enough Points!";
                            buttonText.color = Color.red;
                        }
                    }
                    else if (level > la)
                    {
                        buttonText.text = "LOCKED!";
                        buttonText.color = Color.grey;
                    }
                    else
                    {
                        buttonText.text = "Already Upgraded";
                        buttonText.color = Color.grey;
                    }
                else
                    if (level == lb)
                    {
                        if (cost <= Animal.points)
                        {
                            buttonText.text = "Upgrade";
                            buttonText.color = Color.white;
                        }
                        else
                        {
                            buttonText.text = "Not Enough Points!";
                            buttonText.color = Color.red;
                        }
                    }
                    else if (level > lb)
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
                    if (cost <= Animal.points)
                    {
                        buttonText.text = "Upgrade";
                        buttonText.color = Color.white;
                    }
                    else
                    {
                        buttonText.text = "Not Enough Points!";
                        buttonText.color = Color.red;
                    }
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
                    if (cost <= Animal.points)
                    {
                        buttonText.text = "Upgrade";
                        buttonText.color = Color.white;
                    }
                    else
                    {
                        buttonText.text = "Not Enough Points!";
                        buttonText.color = Color.red;
                    }
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
                    if (cost <= Animal.points)
                    {
                        buttonText.text = "Upgrade";
                        buttonText.color = Color.white;
                    }
                    else
                    {
                        buttonText.text = "Not Enough Points!";
                        buttonText.color = Color.red;
                    }
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
            Animal.points -= System.Convert.ToInt32(text.text.Substring(6, 2));
            buttonText.text = "Already Upgraded";
            buttonText.color = Color.grey;

            // which one are we upgrading
            if (title.text.Substring(0, 5) == "Quick")
            {
                Animal.incrementSpeed();
                s += 1;
            }
            // sexxxx
            else if (title.text.Substring(0, 5) == "Desir")
            {
                Animal.incrementSex();
                lb += 1;
                if (title.text.Length == 7)
                    la += 1;
            }
            // def
            else if (title.text.Substring(0, 5) == "Tough")
            {
                Animal.incrementDefense();
                d += 1;
            }
            // atk
            else if (title.text.Substring(0, 5) == "Stron")
            {
                Animal.incrementAttack();
                a += 1;
            }
            else if (title.text.Substring(0, 5) == "Lucky")
            {
                Animal.luckyMate2 = true;
                la += 1;
            }
            else if (title.text.Substring(0, 5) == "The M")
            {
                Animal.luckyMate1 = true;
                la += 1;
            }
            else if (title.text.Substring(0, 5) == "Fight")
            {
                Animal.fightBackEvolve();
                a += 1;
            }
        }
    }

    public void hideButton()
    {
        button.SetActive(false);
    }
}
