using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Block", fileName = "new Block")]
public class Block : ScriptableObject
{
    public string BlockName;
    public GameObject BlockObject;
    public GameObject PreviewObject;
    public string ItemsNeededForBuildingBlock;
    public int AmountOfItemNeeded;
    public bool isTopMountable;
    public bool isSideMountable;
    public bool isBottomMountable;
    public Vector3 attachDirection = Vector3.down;
    public float blockHealth;
    public Sprite uiSprite;
}