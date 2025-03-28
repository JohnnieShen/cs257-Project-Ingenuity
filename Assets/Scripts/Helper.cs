using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helper : MonoBehaviour
{
    public static Vector3Int ToInt(Vector3 vector)
    {
        return new Vector3Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
    }

    public static Vector3 ToFloat(Vector3Int vector)
    {
        return new Vector3(vector.x, vector.y, vector.z);
    }

    public static int Mod(int x, int y)
    {
        while (x < 0)
        {
            x += y;
        }
        while (x >= y)
        {
            x -= y;
        }
        return x;
    }
}
