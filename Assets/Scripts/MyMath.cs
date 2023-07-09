using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public static class MyMath
{
    public static Vector3 Rotate(Vector3 v, float ang) 
    { 
        float sin = Mathf.Sin(ang * Mathf.Deg2Rad);
        float cos = Mathf.Cos(ang * Mathf.Deg2Rad);

        return new Vector3(cos * v.x - sin * v.z, 0, sin * v.x + cos * v.z);
    }
}


[System.Serializable]
public class RangeI
{
    public int minVal;
    public int maxVal;

    public RangeI(int minVal, int maxVal) { this.minVal = minVal;  this.maxVal = maxVal; }

    public int RandomVal() { return Random.Range(minVal, maxVal); }
    public int PercentVal(float t) { return minVal + (int)((maxVal - minVal) * t); }
    public float PercentOfRange(int val) { return (float)(val - minVal) / (maxVal - minVal); }
}

[System.Serializable]
public class RangeF
{
    public float minVal;
    public float maxVal;
    public float RandomVal { get { return Random.Range(minVal, maxVal); } }

    public RangeF(float min, float max) { minVal = min; maxVal = max; }

    public float PercentVal(float t) { return minVal + (maxVal - minVal) * t; }
    public float PercentOfRange(float val) { return (val - minVal) / (maxVal - minVal); }
}