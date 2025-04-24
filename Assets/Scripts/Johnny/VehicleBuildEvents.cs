using System;
using UnityEngine;

public static class VehicleBuildEvents
{
    public static event Action<Block, Vector3Int> OnBlockAdded;
    public static event Action<Block, Vector3Int> OnBlockRemoved;

    public static void RaiseBlockAdded(Block block, Vector3Int pos)
        => OnBlockAdded?.Invoke(block, pos);

    public static void RaiseBlockRemoved(Block block, Vector3Int pos)
        => OnBlockRemoved?.Invoke(block, pos);
}
