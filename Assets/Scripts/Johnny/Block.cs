using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Block", fileName = "new Block")]
public class Block : ScriptableObject
{
    public string BlockName;
    public GameObject BlockObject;
    public string ItemsNeededForBuildingBlock;
    public int AmountOfItemNeeded;
    public bool isSideMountable;
    public float blockHealth;
}