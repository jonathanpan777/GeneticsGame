using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour
{
    public Text title;
    public Text text;

    public void setTitle(string t)
    {
        title.text = t;
    }

    public void setText(string t)
    {
        text.text = t;
    }
}
