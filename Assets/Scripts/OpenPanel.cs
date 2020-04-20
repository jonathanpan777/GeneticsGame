using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenPanel : MonoBehaviour
{
    public GameObject Panel;

    public void Open()
    {
        if (Panel != null)
        {
            if (!Panel.active)
            {
                Panel.SetActive(true);
            }
            else
            {
                Panel.SetActive(false);
            }
        }
    }
}
