using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Block", fileName = "new Block")]
public class Block : ScriptableObject
{
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
}