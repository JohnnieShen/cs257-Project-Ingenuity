using UnityEngine;

[System.Serializable]
public class BlockInventoryRow
{
    public BlockInventory[] columns;
}

public class BlockInventoryMatrix : MonoBehaviour
{
    [Header("Matrix dimensions")]
    [Min(1)] public int rowsCount = 3;
    [Min(1)] public int columnsCount = 4;

    [Header("Block inventory rows x levels")]
    public BlockInventoryRow[] rows;

    void OnValidate()
    {
        if (rows == null || rows.Length != rowsCount)
            rows = new BlockInventoryRow[rowsCount];

        for (int r = 0; r < rowsCount; r++)
        {
            if (rows[r] == null)
                rows[r] = new BlockInventoryRow();

            if (rows[r].columns == null || rows[r].columns.Length != columnsCount)
                rows[r].columns = new BlockInventory[columnsCount];
        }
    }

    public BlockInventory GetEntry(int r, int c) => rows[r].columns[c];
    public void SetEntry(int r, int c, BlockInventory bi) => rows[r].columns[c] = bi;
}
