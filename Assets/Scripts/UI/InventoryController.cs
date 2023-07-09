using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private InventorySlot inventorySlotPrefab;
    [SerializeField] private Vector2 inventoryTop;
    [SerializeField] private Vector2Int inventorySize;
    [SerializeField] private float slotPadding;
    [SerializeField] private float slotSize = 1;
    private float slotScale;

    protected InventorySlot[][] inventorySlots;
    [SerializeField] protected bool useUpgradeCount = false;
    protected int lastUpgradeSlots;

    private void OnDrawGizmos()
    {
        slotScale = slotSize - slotPadding;

        float xMin = inventoryTop.x - slotSize * (inventorySize.x / 2f - 0.5f);
        float yMin = inventoryTop.y - slotSize / 2;

        Gizmos.color = Color.white;
        for (int y = 0; y < inventorySize.y; y++)
        {
            for (int x = 0; x < inventorySize.x; x++)
            {
                Gizmos.DrawWireCube(transform.position + new Vector3(xMin + slotSize * x, yMin - slotSize * y), new Vector2(slotScale, slotScale));
            }
        }
    }

    protected virtual void Start()
    {
        GenInventory();
    }

    private void Update()
    {
        if (useUpgradeCount && lastUpgradeSlots != Upgrades.Instance.backpackSize.Val)
        {
            lastUpgradeSlots = (int)Upgrades.Instance.backpackSize.Val;

            for (int y = 0; y < lastUpgradeSlots; y++)
            {
                for (int x = 0; x < inventorySize.x; x++)
                {
                    inventorySlots[y][x].gameObject.SetActive(true);
                }
            }
        }
    }

    private void GenInventory()
    {
        slotScale = slotSize - slotPadding;
        inventorySlots = new InventorySlot[inventorySize.y][];

        float xMin = inventoryTop.x - slotSize * (inventorySize.x / 2f - 0.5f);
        float yMin = inventoryTop.y - slotSize / 2;

        for (int y = 0; y < inventorySize.y; y++)
        {
            inventorySlots[y] = new InventorySlot[inventorySize.x];
            for (int x = 0; x < inventorySize.x; x++)
            {
                InventorySlot t = Instantiate(inventorySlotPrefab, transform);
                t.transform.position = transform.position + new Vector3(xMin + slotSize * x, yMin - slotSize * y);
                t.transform.localScale = new Vector2(slotScale, slotScale);

                t.pos = new Vector2Int(x, y);
                inventorySlots[y][x] = t;

                if (useUpgradeCount && y >= Upgrades.Instance.backpackSize.Val)
                    t.gameObject.SetActive(false);
            }
        }

        lastUpgradeSlots = (int)Upgrades.Instance.backpackSize.Val;
    }

    public InventorySlot V3toInv(Vector3 pos)
    {
        float closestDist = Mathf.Infinity;
        InventorySlot closest = inventorySlots[0][0];
        for (int y = 0; y < (useUpgradeCount ? lastUpgradeSlots : inventorySlots.Length); y++)
        {
            foreach (InventorySlot t in inventorySlots[y])
            {
                float d = Vector3.Distance(t.transform.position, pos);
                if (t.open && d < closestDist)
                {
                    closestDist = d;
                    closest = t;
                }
            }
        }

        return closest;
    }

    public InventorySlot GetInv(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= inventorySize.x
         || pos.y < 0 || pos.y >= inventorySize.y)
            return null;

        return inventorySlots[pos.y][pos.x];
    }
}
