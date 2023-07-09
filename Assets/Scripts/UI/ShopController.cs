using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopController : InventoryController
{
    public static ShopController Instance;

    [HideInInspector] public int totalGold = 0;

    private void Awake()
    {
        Instance = this;
    }

    protected override void Start()
    {
        base.Start();
        GameUI.Instance.SetGoldText(totalGold);
    }

    private void Update()
    {
        if (!gameObject.activeSelf)
            return;

        GameUI.Instance.SetPriceText(CalcSellAmount());
    }

    public int CalcSellAmount()
    {
        List<ItemBase> counted = new List<ItemBase>();
        int total = 0;

        if (inventorySlots != null)
        {
            for (int y = 0; y < inventorySlots.Length; y++)
            {
                for (int x = 0; x < inventorySlots[0].Length; x++)
                {
                    ItemBase item = inventorySlots[y][x].currItem;
                    if (item && !counted.Contains(item))
                    {
                        total += item.sellValue;
                        counted.Add(item);
                    }
                }
            }
        }

        return total;
    }

    public void SellAll()
    {
        List<ItemBase> counted = new List<ItemBase>();

        if (inventorySlots != null)
        {
            for (int y = 0; y < inventorySlots.Length; y++)
            {
                for (int x = 0; x < inventorySlots[0].Length; x++)
                {
                    ItemBase item = inventorySlots[y][x].currItem;
                    if (item && !counted.Contains(item))
                    {
                        inventorySlots[y][x].RemoveItem(item);
                        totalGold += item.sellValue;
                        Destroy(item.gameObject);
                        counted.Add(item);
                    }
                }
            }
        }

        GameUI.Instance.SetGoldText(totalGold);
    }
}
