using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helper : MonoBehaviour
{
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
