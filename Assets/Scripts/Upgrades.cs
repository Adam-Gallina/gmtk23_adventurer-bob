using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upgrades : MonoBehaviour
{
    public static Upgrades Instance;

    private void Awake()
    {
        Instance = this;
    }

    [Header("Player Unlocks")]
    public Upgrade damageMod;
    public Upgrade healthTotal;
    public Upgrade backpackSize;
}

[System.Serializable]
public class Upgrade
{
    public float Val { get { return val[level]; } }
    public int Price { get { return price[level]; } }

    public float[] val;
    public int[] price;
    [HideInInspector] public int level = 0;
    [HideInInspector] public bool Max { get { return level >= price.Length - 1; } }

    public bool CanUpgrade()
    {
        if (level >= price.Length - 1)
            return false;

        return ShopController.Instance.totalGold >= price[level];
    }

    public void DoUpgrade()
    {
        if (level < val.Length - 1)
        {
            ShopController.Instance.totalGold -= price[level];
            level++;
        }
    }
}