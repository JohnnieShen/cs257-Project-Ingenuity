using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
public class BuildSystem : MonoBehaviour
{
    // public Block[] availableBuildingBlocks;
    int currentBlockIndex = 0;
 
    Block currentBlock;
    public TMP_Text blockNameText;
 
    public Transform shootingPoint;
    GameObject blockObject;
 
    public Transform parent;
 
    public Color normalColor;
    public Color highlightedColor;
 
    GameObject lastHightlightedBlock;
    public LayerMask rayCastLayers;
    public LayerMask shieldLayer;
    
    public Material previewMaterial;
    private GameObject previewBlock;
    public BlockInventory[] availableBuildingBlocks;
    private Dictionary<Block, BlockInventory> blockInventory = new Dictionary<Block, BlockInventory>();
    private List<Block> availableBlocksList = new List<Block>();

    [Header("References")]
    [SerializeField] private Transform commandModule;
 
    private void OnEnable()
    {
        if (InputManager.instance != null)
        {
            InputManager.instance.GetBuildBuildAction().performed += OnBuildPerformed;
            InputManager.instance.GetBuildRemoveAction().performed += OnRemovePerformed;
            InputManager.instance.GetBuildScrollAction().performed += OnScrollPerformed;
        }
    }

    private void OnDisable()
    {
        if (InputManager.instance != null)
        {
            InputManager.instance.GetBuildBuildAction().performed -= OnBuildPerformed;
            InputManager.instance.GetBuildRemoveAction().performed -= OnRemovePerformed;
            InputManager.instance.GetBuildScrollAction().performed -= OnScrollPerformed;
        }
    }
    private void Start()
    {
        InitializeInventory();
        if (availableBuildingBlocks != null && availableBuildingBlocks.Length > 0)
        {
            currentBlockIndex = 0;
            currentBlock = availableBuildingBlocks[currentBlockIndex].Block;
            SetText();
        }
    }

    void InitializeInventory()
    {
        foreach (BlockInventory ib in availableBuildingBlocks)
        {
            blockInventory[ib.Block] = ib;
            if (ib.CurrentCount > 0)
            {
                availableBlocksList.Add(ib.Block);
            }
        }
        UpdateAvailableBlocks();
    }

    void UpdateAvailableBlocks()
    {
        availableBlocksList.Clear();
        foreach (BlockInventory ib in availableBuildingBlocks)
        {
            if (ib.CurrentCount > 0)
            {
                availableBlocksList.Add(ib.Block);
            }
        }
        
        if (availableBlocksList.Count > 0)
        {
            currentBlockIndex = Mathf.Clamp(currentBlockIndex, 0, availableBlocksList.Count - 1);
            currentBlock = availableBlocksList[currentBlockIndex];
        }
        else
        {
            currentBlock = null;
        }
        SetText();
    }

    // private void Update()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         BuildBlock(currentBlock.BlockObject);
    //     }
    //     if (Input.GetMouseButtonDown(1))
    //     {
    //         DestroyBlock();
    //     }
    //     ChangeCurrentBlock();
    //     // UpdatePreview();
    // }
 
    private void OnBuildPerformed(InputAction.CallbackContext ctx)
    {
        if (currentBlock == null || currentBlock.BlockObject == null)
        {
            Debug.LogWarning("No block selected yet!");
            return;
        }
        BuildBlock(currentBlock.BlockObject);
    }

    private void OnRemovePerformed(InputAction.CallbackContext ctx)
    {
        DestroyBlock();
    }

    private void OnScrollPerformed(InputAction.CallbackContext ctx)
    {
        Vector2 scrollValue = ctx.ReadValue<Vector2>();
        float scroll = scrollValue.y;

        if (scroll > 0)
        {
            currentBlockIndex++;
            if (currentBlockIndex >= availableBuildingBlocks.Length)
                currentBlockIndex = 0;
        }
        else if (scroll < 0)
        {
            currentBlockIndex--;
            if (currentBlockIndex < 0)
                currentBlockIndex = availableBuildingBlocks.Length - 1;
        }

        currentBlock = availableBuildingBlocks[currentBlockIndex].Block;
        SetText();

        if (previewBlock != null)
        {
            Destroy(previewBlock);
            previewBlock = null;
        }
    }

    void SetText()
    {
        if (blockNameText != null && currentBlock != null)
        {
            blockNameText.text = $"{currentBlock.BlockName} ({blockInventory[currentBlock].CurrentCount}/{blockInventory[currentBlock].MaxStack})\n" ;
        }
    }
    // void ChangeCurrentBlock()
    // {
    //     float scroll = Input.mouseScrollDelta.y;
    //     if(scroll > 0)
    //     {
    //         currentBlockIndex++;
    //         if (currentBlockIndex > availableBuildingBlocks.Length - 1)
    //         {
    //             currentBlockIndex = 0;
    //         }
    //     } else if(scroll < 0)
    //     {
    //         currentBlockIndex--;
    //         if (currentBlockIndex < 0)
    //         {
    //             currentBlockIndex = availableBuildingBlocks.Length - 1;
    //         }
    //     }
    //     currentBlock = availableBuildingBlocks[currentBlockIndex];
    //     SetText();
    //     if (previewBlock != null)
    //     {
    //         Destroy(previewBlock);
    //         previewBlock = null;
    //     }
    // }
 
   
 
    void BuildBlock(GameObject blockPrefab)
    {
        if (currentBlock == null || blockInventory[currentBlock].CurrentCount <= 0)
        {
            Debug.LogWarning("No blocks remaining of this type!");
            return;
        }
        LayerMask combinedMask = rayCastLayers & ~shieldLayer;
        if (Physics.Raycast(shootingPoint.position, shootingPoint.forward, out RaycastHit hitInfo, combinedMask))
        {
            // Debug.Log("raycast hitting"+hitInfo.collider.gameObject.name);
            if (hitInfo.collider.gameObject.layer == 6)
            {
                Vector3 localPoint  = commandModule.InverseTransformPoint(hitInfo.point);
                Vector3 localNormal = commandModule.InverseTransformDirection(hitInfo.normal);
                
                Vector3 localSpawn = new Vector3(
                    Mathf.RoundToInt(localPoint.x + localNormal.x / 2f),
                    Mathf.RoundToInt(localPoint.y + localNormal.y / 2f),
                    Mathf.RoundToInt(localPoint.z + localNormal.z / 2f)
                );
                Vector3Int spawnPosInt = Vector3Int.RoundToInt(localSpawn);
                if (BlockManager.instance != null && BlockManager.instance.TryGetBlockAt(spawnPosInt, out _))
                {
                    Debug.LogWarning("Cannot build - grid position already occupied!");
                    return;
                }
                GameObject newBlock = Instantiate(blockPrefab, commandModule);
                newBlock.transform.localPosition = localSpawn;
                if (currentBlock.isSideMountable)
                {
                    if (Vector3.Angle(hitInfo.normal, commandModule.TransformDirection(Vector3.up)) > 10f)
                    {
                        Quaternion adjustment = Quaternion.FromToRotation(newBlock.transform.up, hitInfo.normal);
                        newBlock.transform.rotation = adjustment * newBlock.transform.rotation;
                        newBlock.transform.localRotation = Quaternion.Inverse(commandModule.rotation) * newBlock.transform.rotation;
                    }
                    else
                    {
                        newBlock.transform.localRotation = Quaternion.identity;
                    }
                }
                else
                {
                    newBlock.transform.localRotation = Quaternion.identity;
                }
                
                Rigidbody newBlockRb = newBlock.GetComponent<Rigidbody>();
                
                // Vector3Int spawnPosInt = Vector3Int.RoundToInt(localSpawn);
                // Debug.Log("Spawn grid coordinate: " + spawnPosInt);
                
                Hull newHull = newBlock.GetComponent<Hull>();
                if(newHull != null)
                {
                    newHull.sourceBlock = currentBlock;
                    foreach (Vector3Int offset in newHull.validConnectionOffsets)
                    {
                        Vector3 rotatedOffset = newBlock.transform.localRotation * (Vector3)offset;
                        Vector3Int rotatedOffsetInt = new Vector3Int(
                            Mathf.RoundToInt(rotatedOffset.x),
                            Mathf.RoundToInt(rotatedOffset.y),
                            Mathf.RoundToInt(rotatedOffset.z)
                        );
                        Vector3Int neighborPos = spawnPosInt + rotatedOffsetInt;
                        // Debug.Log("Checking neighbor at: " + neighborPos + " for connection offset: " + offset);
                        if (BlockManager.instance != null && BlockManager.instance.TryGetBlockAt(neighborPos, out Rigidbody neighborRb))
                        {
                            if (neighborRb != null) {
                                Hull neighborHull = neighborRb.GetComponent<Hull>();
                                if (neighborHull != null)
                                {
                                    Vector3Int oppositeOffset = -rotatedOffsetInt;
                                    if (neighborHull.validConnectionOffsets.Contains(oppositeOffset))
                                    {
                                        newBlock.AddComponent<FixedJoint>().connectedBody = neighborRb;
                                        Vector3Int newBlockPos = Vector3Int.RoundToInt(localSpawn);
                                        BlockManager.instance.AddConnection(newBlockPos, neighborPos);
                                        blockInventory[currentBlock].CurrentCount--;
                                        UpdateAvailableBlocks();
                                        
                                        // Debug.Log($"Connected new block at {spawnPosInt} to neighbor at {neighborPos} (offset {offset}, opposite {oppositeOffset}).");
                                    }
                                    // else
                                    // {
                                    //     Debug.Log($"Neighbor at {neighborPos} does not allow connection at offset {oppositeOffset}.");
                                    // }
                                }
                            }
                            
                            // else
                            // {
                            //     Debug.LogWarning("Neighbor found at " + neighborPos + " has no Hull component.");
                            // }
                        }
                        // else
                        // {
                        //     Debug.Log("No neighbor found at " + neighborPos + ".");
                        // }
                    }
                }
                // else
                // {
                //     Debug.LogWarning("New block has no Hull component. Skipping connection logic.");
                // }
                
                if (BlockManager.instance != null)
                {
                    BlockManager.instance.AddBlock(spawnPosInt, newBlockRb);
                    // Debug.Log("Registered new block at grid coordinate: " + spawnPosInt);
                }
            }
        }
    }


 
    bool ShouldConnect(Vector3Int neighborPos, Vector3Int spawnPos)
    {
        if(neighborPos.x < spawnPos.x) return true;
        if(neighborPos.x > spawnPos.x) return false;
        if(neighborPos.y < spawnPos.y) return true;
        if(neighborPos.y > spawnPos.y) return false;
        return neighborPos.z < spawnPos.z;
    }
 
    void DestroyBlock()
    {
        LayerMask combinedMask = rayCastLayers & ~shieldLayer;
        if (Physics.Raycast(shootingPoint.position, shootingPoint.forward, out RaycastHit hitInfo, combinedMask))
        {
            if (hitInfo.transform.CompareTag("Block"))
            {
                Hull hull = hitInfo.transform.GetComponent<Hull>();
                if (hull != null && hull.sourceBlock != null)
                {
                    if (blockInventory.ContainsKey(hull.sourceBlock))
                    {
                        blockInventory[hull.sourceBlock].CurrentCount++;
                        blockInventory[hull.sourceBlock].CurrentCount = Mathf.Min(
                            blockInventory[hull.sourceBlock].CurrentCount,
                            blockInventory[hull.sourceBlock].MaxStack
                        );
                        UpdateAvailableBlocks();
                    }
                }
                Vector3 localPos = commandModule.InverseTransformPoint(hitInfo.transform.position);
                Vector3Int localPosInt = Vector3Int.RoundToInt(localPos);
                Destroy(hitInfo.transform.gameObject);
                if (BlockManager.instance != null)
                {
                    BlockManager.instance.RemoveBlock(localPosInt);
                }
            }
        }
    }
 
    void HighlightBlock()
    {
        if (Physics.Raycast(shootingPoint.position, shootingPoint.forward, out RaycastHit hitInfo))
        {
            if (hitInfo.transform.tag == "Block")
            {
                if(lastHightlightedBlock == null)
                {
                    lastHightlightedBlock = hitInfo.transform.gameObject;
                    hitInfo.transform.gameObject.GetComponent<Renderer>().material.color = highlightedColor;
                }
                else if (lastHightlightedBlock != hitInfo.transform.gameObject)
                {
                    lastHightlightedBlock.GetComponent<Renderer>().material.color = normalColor;
                    hitInfo.transform.gameObject.GetComponent<Renderer>().material.color = highlightedColor;
                    lastHightlightedBlock = hitInfo.transform.gameObject;
                }
            }
            else if(lastHightlightedBlock != null)
            {
                lastHightlightedBlock.GetComponent<Renderer>().material.color = normalColor;
                lastHightlightedBlock = null;
            }
        }
    }
     void UpdatePreview()
    {
        if (Physics.Raycast(shootingPoint.position, shootingPoint.forward, out RaycastHit hitInfo, Mathf.Infinity, rayCastLayers))
        {
            if (hitInfo.collider.gameObject.layer == 6)
            {
                Vector3 localPoint = commandModule.InverseTransformPoint(hitInfo.point);
                Vector3 localNormal = commandModule.InverseTransformDirection(hitInfo.normal);

                Vector3 localSpawn = new Vector3(
                    Mathf.RoundToInt(localPoint.x + localNormal.x / 2f),
                    Mathf.RoundToInt(localPoint.y + localNormal.y / 2f),
                    Mathf.RoundToInt(localPoint.z + localNormal.z / 2f)
                );

                if (previewBlock == null)
                {
                    previewBlock = Instantiate(currentBlock.BlockObject, parent);
                    RemovePhysicsComponents(previewBlock);
                    ApplyPreviewMaterial(previewBlock);
                }
                previewBlock.transform.localPosition = localSpawn;
                previewBlock.transform.localRotation = Quaternion.identity;
                previewBlock.SetActive(true);
            }
            else
            {
                if (previewBlock != null)
                {
                    previewBlock.SetActive(false);
                }
            }
        }
        else
        {
            if (previewBlock != null)
            {
                previewBlock.SetActive(false);
            }
        }
    }

    void RemovePhysicsComponents(GameObject obj)
    {
        foreach (Collider col in obj.GetComponentsInChildren<Collider>())
        {
            Destroy(col);
        }
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Destroy(rb);
        }
        foreach (FixedJoint joint in obj.GetComponents<FixedJoint>())
        {
            Destroy(joint);
        }
    }

    void ApplyPreviewMaterial(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            rend.material = previewMaterial;
        }
    }
}

[System.Serializable]
public class BlockInventory
{
    public Block Block;
    public int CurrentCount;
    public int MaxStack = 99;
}
 