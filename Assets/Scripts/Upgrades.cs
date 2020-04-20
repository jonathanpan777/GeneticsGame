using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upgrades : MonoBehaviour
{
    public GameObject Love;
    public GameObject Defense;
    public GameObject Attack;
    public GameObject Speed;

    public void OpenLove()
    {
        Love.SetActive(true);
        Defense.SetActive(false);
        Attack.SetActive(false);
        Speed.SetActive(false);
    }

    public void OpenDefense()
    {
        Love.SetActive(false);
        Defense.SetActive(true);
        Attack.SetActive(false);
        Speed.SetActive(false);
    }

    public void OpenAttack()
    {
        Love.SetActive(false);
        Defense.SetActive(false);
        Attack.SetActive(true);
        Speed.SetActive(false);
    }

    public void OpenSpeed()
    {
        Love.SetActive(false);
        Defense.SetActive(false);
        Attack.SetActive(false);
        Speed.SetActive(true);
    }
}
