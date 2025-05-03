using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Block", fileName = "new Block")]
public class Block : ScriptableObject
{
    /*
    * Author: IDK
    * Summary: This script defines a Block class that inherits from ScriptableObject.
    * It contains various properties and attributes related to the block, such as its name, object reference, mountability,
    * rotatability, craftability, cost, recycle bonus, health, and UI representation.
    * The Block class is used to create and manage different types of blocks in the game.
    */

    [Header("Block Settings")]
    public string BlockName;
    public GameObject BlockObject;
    public GameObject PreviewObject;
    // public string ItemsNeededForBuildingBlock;
    // public int AmountOfItemNeeded;
    [Header("Attributes")]
    public bool isTopMountable;
    public bool isSideMountable;
    public bool isBottomMountable;
    public bool isRotatable;
    public bool isCraftable;
    public int cost;
    public int recycleBonus;
    public Vector3 attachDirection = Vector3.down;
    public float blockHealth;
    [Header("UI")]
    public Sprite uiSprite;
    public bool onlyInlinePlacement = false;
    public int connectionStrength = 0;
    public string description;
}