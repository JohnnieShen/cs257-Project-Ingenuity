using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WinCondition/Block Checklist", fileName = "new BlockChecklist")]
public class BlockChecklist : ScriptableObject
{
    [Serializable]
    public struct Requirement
    {
        public Block block;
        public int requiredCount;
    }

    public List<Requirement> requirements = new();
}
