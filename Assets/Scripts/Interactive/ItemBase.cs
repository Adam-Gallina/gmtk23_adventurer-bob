using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemBase : GrabbableBase
{
    protected InventorySlot currSlot;
    protected InventoryController storedInv;

    [SerializeField] protected Vector2Int[] itemTileOffsets = new Vector2Int[] { Vector2Int.zero };

    public int sellValue = 1;

    protected InventoryController touchingInv;

    protected Rigidbody2D rb;

    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody2D>();
    }

    public override bool GrabStart()
    {
        bool ret = true;
        if (currSlot)
        {
            foreach (Vector2Int i in itemTileOffsets)
            {
                InventorySlot inv = storedInv.GetInv(currSlot.pos + i);
                ret = inv && !inv.open;
                if (!ret)
                    return false;
            }

            foreach (Vector2Int i in itemTileOffsets)
            {
                InventorySlot inv = storedInv.GetInv(currSlot.pos + i);
                inv.RemoveItem(this);
            }

            currSlot = null;
        }

        return base.GrabStart() && ret;
    }

    public override void GrabUpdate(Vector3 pos)
    {
        Collider2D[] colls = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), 1 << Constants.UiLayer);
        bool foundInv = false;
        foreach (Collider2D coll in colls)
        {
            InventoryController inv = coll.gameObject.GetComponent<InventoryController>();
            if (inv)
            {
                foundInv = true;
                touchingInv = inv;
                break;
            }
        }
        if (!foundInv)
            touchingInv = null;

        if (touchingInv)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            transform.localEulerAngles = Vector3.zero;

            currSlot = touchingInv.V3toInv(pos);
            transform.position = currSlot.transform.position;
        }
        else
        {
            rb.constraints = RigidbodyConstraints2D.None;
            transform.position = pos;
            currSlot = null;
        }
    }

    public override bool GrabEnd()
    {
        bool ret = true;
        if (currSlot)
        {
            foreach (Vector2Int i in itemTileOffsets)
            {
                InventorySlot inv = touchingInv.GetInv(currSlot.pos + i);
                ret = inv && inv.open;
                if (!ret)
                    return false;
            }

            foreach (Vector2Int i in itemTileOffsets)
            {
                InventorySlot inv = touchingInv.GetInv(currSlot.pos + i);
                inv.AddItem(this);
            }

            transform.parent = null;
            storedInv = touchingInv;
        }
        else
        {
            transform.parent = RoomGenerator.Instance.CurrRoom.transform;
        }

        return base.GrabEnd() && ret;
    }
}
