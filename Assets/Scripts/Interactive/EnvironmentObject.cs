using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentObject : GrabbableBase
{
    public override void GrabUpdate(Vector3 pos)
    {
        transform.position = pos;
    }
}
