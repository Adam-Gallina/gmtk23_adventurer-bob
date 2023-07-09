using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartRoom : RoomController
{
    public override bool CheckRoomCleared()
    {
        roomCleared = true;
        return false;
    }
}
