using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlot : MonoBehaviour
{
    [HideInInspector] public ItemBase currItem;
    public bool open { get { return currItem == null; } }
    [HideInInspector] public Vector2Int pos;

    public bool AddItem(ItemBase item)
    {
        if (currItem != null)
            return false;

        currItem = item;
        return true;
    }

    public bool RemoveItem(ItemBase item)
    {
        if (currItem != item)
            return true;

        currItem = null;
        return true;
    }
}
