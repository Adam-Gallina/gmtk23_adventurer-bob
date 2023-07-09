using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorArrow : GrabbableBase
{
    [HideInInspector] public RoomDoor direction;
    public static bool dirSelected = false;

    public override bool GrabStart()
    {
        if (dirSelected)
            return false;

        dirSelected = true;
        RoomGenerator.Instance.ChangeRoom(direction);
        
        return false;
    }

    public override void GrabUpdate(Vector3 pos)
    {
        return;
    }
}
