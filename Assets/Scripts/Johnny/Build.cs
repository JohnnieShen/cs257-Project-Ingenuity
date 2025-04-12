using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class BuildSystem : MonoBehaviour
{
    // public Block[] availableBuildingBlocks;
    int currentBlockIndex = 0;
 
    Block currentBlock;
    public TMP_Text blockNameText;
    public Image blockUIImage;
    public int breakForce;
 
    public Transform shootingPoint;
    GameObject blockObject;
 
    public Transform parent;
 
    public Color normalColor;
    public Color highlightedColor;
 
    GameObject lastHightlightedBlock;
    public LayerMask rayCastLayers;
    public LayerMask shieldLayer;
    public LayerMask previewIgnoreLayers;
    
    public Material previewMaterial;
    private GameObject previewBlock;
    private Quaternion previewBlockOriginalRotation;
    private int rotationOffsetCount = 0;
    // public BlockInventory[] availableBuildingBlocks;
    // private Dictionary<Block, BlockInventory> blockInventory = new Dictionary<Block, BlockInventory>();
    private List<Block> availableBlocksList = new List<Block>();

    [Header("References")]
    [SerializeField] private Transform commandModule;
    private Transform referenceTransform;
 
    private void OnEnable()
    {
        var allBlocks = BlockInventoryManager.instance.availableBuildingBlocks;
        if (allBlocks != null && allBlocks.Length > 0)
        {
            currentBlockIndex = 0;
            currentBlock = allBlocks[currentBlockIndex].Block;
            SetText();
        }
        if (InputManager.instance != null)
        {
            InputManager.instance.GetBuildBuildAction().performed += OnBuildPerformed;
            InputManager.instance.GetBuildRemoveAction().performed += OnRemovePerformed;
            InputManager.instance.GetBuildScrollAction().performed += OnScrollPerformed;
            InputManager.instance.GetBuildRotateAction().performed += OnRotatePerformed;
        }
        if (commandModule != null)
        {
            referenceTransform.position = commandModule.position;
            referenceTransform.rotation = commandModule.rotation;
        }

        ReparentAllBlocksToReference();
        referenceTransform.rotation = Quaternion.identity;
        TransferBlocksToParent();
    }

    private void OnDisable()
    {
        if (InputManager.instance != null)
        {
            InputManager.instance.GetBuildBuildAction().performed -= OnBuildPerformed;
            InputManager.instance.GetBuildRemoveAction().performed -= OnRemovePerformed;
            InputManager.instance.GetBuildScrollAction().performed -= OnScrollPerformed;
            InputManager.instance.GetBuildRotateAction().performed -= OnRotatePerformed;
        }
    }
    private void Awake()
    {
        // InitializeInventory();
        var allBlocks = BlockInventoryManager.instance.availableBuildingBlocks;
        if (allBlocks != null && allBlocks.Length > 0)
        {
            currentBlockIndex = 0;
            currentBlock = allBlocks[currentBlockIndex].Block;
            SetText();
        }
        GameObject dummy = new GameObject("ReferenceTransform");
        referenceTransform = dummy.transform;
    }
    // void InitializeInventory()
    // {
    //     foreach (BlockInventory ib in availableBuildingBlocks)
    //     {
    //         blockInventory[ib.Block] = ib;
    //         if (ib.CurrentCount > 0)
    //         {
    //             availableBlocksList.Add(ib.Block);
    //         }
    //     }
    //     UpdateAvailableBlocks();
    // }

    // void UpdateAvailableBlocks()
    // {
    //     availableBlocksList.Clear();
    //     foreach (BlockInventory ib in availableBuildingBlocks)
    //     {
    //         if (ib.CurrentCount > 0)
    //         {
    //             availableBlocksList.Add(ib.Block);
    //         }
    //     }
        
    //     if (availableBlocksList.Count > 0)
    //     {
    //         currentBlockIndex = Mathf.Clamp(currentBlockIndex, 0, availableBlocksList.Count - 1);
    //         currentBlock = availableBlocksList[currentBlockIndex];
    //     }
    //     else
    //     {
    //         currentBlock = null;
    //     }
    //     SetText();
    // }

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

    private void Update()
    {
        if (commandModule != null && referenceTransform != null)
        {
            referenceTransform.position = commandModule.position;
            referenceTransform.rotation = commandModule.rotation;
        }
        UpdatePreview();
    }
    public void destroyPreviewBlock()
    {
        if (previewBlock != null)
        {
            Destroy(previewBlock);
            previewBlock = null;
        }
    }
 
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

        var allBlocks = BlockInventoryManager.instance.availableBuildingBlocks;
        if (allBlocks == null || allBlocks.Length == 0) return;

        if (scroll > 0)
        {
            currentBlockIndex++;
            if (currentBlockIndex >= allBlocks.Length)
                currentBlockIndex = 0;
        }
        else if (scroll < 0)
        {
            currentBlockIndex--;
            if (currentBlockIndex < 0)
                currentBlockIndex = allBlocks.Length - 1;
        }

        currentBlock = allBlocks[currentBlockIndex].Block;
        SetText();

        if (previewBlock != null)
        {
            Destroy(previewBlock);
            previewBlock = null;
        }
    }

    private void OnRotatePerformed(InputAction.CallbackContext ctx)
    {
        rotationOffsetCount = (rotationOffsetCount + 1) % 4;
    }

    void SetText()
    {
        if (blockNameText != null && currentBlock != null)
        {
            int currentCount = BlockInventoryManager.instance.GetBlockCount(currentBlock);

            blockNameText.text = $"{currentBlock.BlockName} ({currentCount})\n";
            if (blockUIImage != null)
                blockUIImage.sprite = currentBlock.uiSprite;
        }
        else if (blockNameText != null)
        {
            blockNameText.text = "No Block Selected";
            if (blockUIImage != null)
                blockUIImage.sprite = null;
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
        int count = BlockInventoryManager.instance.GetBlockCount(currentBlock);
        if (currentBlock == null || count <= 0) // No blocks remaining of this type
        {
            Debug.LogWarning("No blocks remaining of this type!");
            return;
        }
        LayerMask combinedMask = rayCastLayers & ~shieldLayer;
        LayerMask combinedMask_preview = combinedMask & ~previewIgnoreLayers;
        if (Physics.Raycast(shootingPoint.position, shootingPoint.forward, out RaycastHit hitInfo, combinedMask_preview)) // Raycast hit something that is a block
        {
            // Debug.Log("raycast hitting"+hitInfo.collider.gameObject.name);

            // 1. Instantiating the block

            if (hitInfo.collider.gameObject.layer == 6) // Hit a block
            {
                Vector3 localPoint = referenceTransform.InverseTransformPoint(hitInfo.point);
                Vector3 localNormal = referenceTransform.InverseTransformDirection(hitInfo.normal);
                float angleWithUp = Vector3.Angle(hitInfo.normal, referenceTransform.TransformDirection(Vector3.up));
                bool isTopSurface = (angleWithUp < 30f);
                bool isBottomSurface = (angleWithUp > 150f);
                bool isSideSurface = (!isTopSurface && !isBottomSurface);
                if (isTopSurface && !currentBlock.isTopMountable)
                {
                    Debug.LogWarning("This block cannot be top-mounted!");
                    return;
                }
                if (isBottomSurface && !currentBlock.isBottomMountable)
                {
                    Debug.LogWarning("This block cannot be bottom-mounted!");
                    return;
                }
                if (isSideSurface && !currentBlock.isSideMountable)
                {
                    Debug.LogWarning("This block cannot be side-mounted!");
                    return;
                }
                Vector3 localSpawn = new Vector3( // Calculate spawn position
                    Mathf.RoundToInt(localPoint.x + localNormal.x / 2f),
                    Mathf.RoundToInt(localPoint.y + localNormal.y / 2f),
                    Mathf.RoundToInt(localPoint.z + localNormal.z / 2f)
                );
                Vector3Int spawnPosInt = Vector3Int.RoundToInt(localSpawn);
                Vector3 spawnWorldPos = referenceTransform.TransformPoint(localSpawn);
                Vector3 spawnInParentSpace = parent.InverseTransformPoint(spawnWorldPos);
                // Vector3 spawnInParentSpace = parent.InverseTransformPoint(commandModule.TransformPoint(localSpawn));
                GameObject newBlock = Instantiate(blockPrefab, parent); // Instantiate new block as child of command module
                newBlock.transform.localPosition = spawnInParentSpace; // Set local position of new block
                Quaternion newBlockWorldRotation = Quaternion.identity;
                if (isSideSurface)
                {
                    Vector3 hitNormal = hitInfo.normal;
                    // Debug.Log($"[BuildBlock] Raw hitNormal: {hitNormal}");

                    Vector3 snappedNormal = new Vector3(
                        Mathf.Abs(hitNormal.x) > 0.9f ? Mathf.Sign(hitNormal.x) : 0f,
                        Mathf.Abs(hitNormal.y) > 0.9f ? Mathf.Sign(hitNormal.y) : 0f,
                        Mathf.Abs(hitNormal.z) > 0.9f ? Mathf.Sign(hitNormal.z) : 0f
                    );
                    snappedNormal.Normalize();
                    // Debug.Log($"[BuildBlock] Snapped hitNormal: {snappedNormal}");

                    Vector3 blockAttachDir_world = referenceTransform.TransformDirection(currentBlock.attachDirection);
                    // Debug.Log($"[BuildBlock] currentBlock.attachDirection: {currentBlock.attachDirection} -> blockAttachDir_world: {blockAttachDir_world}");

                    Quaternion toNormal_world = Quaternion.FromToRotation(blockAttachDir_world, snappedNormal);
                    // Debug.Log($"[BuildBlock] toNormal_world (Euler): {toNormal_world.eulerAngles}");

                    newBlockWorldRotation = toNormal_world * referenceTransform.rotation;
                    // Debug.Log($"[BuildBlock] Intermediate newBlockWorldRotation (Euler): {newBlockWorldRotation.eulerAngles}");

                    newBlockWorldRotation *= Quaternion.Euler(0f, 180f, 0f);
                    // Debug.Log($"[BuildBlock] Final newBlockWorldRotation (Euler): {newBlockWorldRotation.eulerAngles}");
                }

                else
                {
                    newBlockWorldRotation = referenceTransform.rotation;
                    if (isBottomSurface)
                    {
                        newBlockWorldRotation *= Quaternion.Euler(180f, 0f, 0f);
                    }
                }
                if (currentBlock.isRotatable) // If block is rotatable, apply rotation offset
                {
                    newBlockWorldRotation *= Quaternion.Euler(0f, 90f * rotationOffsetCount, 0f);
                }
                // newBlock.transform.localRotation *= Quaternion.Euler(0, 90f * rotationOffsetCount, 0);
                Quaternion finalLocalRotation = Quaternion.Inverse(parent.rotation) * newBlockWorldRotation;
                newBlock.transform.localRotation = finalLocalRotation;
                Rigidbody newBlockRb = newBlock.GetComponent<Rigidbody>();
                
                // Vector3Int spawnPosInt = Vector3Int.RoundToInt(localSpawn);
                // Debug.Log("Spawn grid coordinate: " + spawnPosInt);
                
                // 2. Creating joints and connections

                Hull newHull = newBlock.GetComponent<Hull>();
                if(newHull != null)
                {
                    newHull.sourceBlock = currentBlock; // Set the source block of the new block to the current selected block
                    foreach (Vector3Int offset in newHull.validConnectionOffsets) // Iterate over valid connection offsets of the new block as set in the block scriptable object
                    {
                        Vector3 offsetLocal = offset;
                        Vector3 offsetWorld = newBlock.transform.TransformDirection(offsetLocal);
                        Vector3 offsetInModule = referenceTransform.InverseTransformDirection(offsetWorld);
                        Vector3Int offsetInModuleInt = new Vector3Int(
                            Mathf.RoundToInt(offsetInModule.x),
                            Mathf.RoundToInt(offsetInModule.y),
                            Mathf.RoundToInt(offsetInModule.z)
                        );
                        Vector3Int neighborPos = spawnPosInt + offsetInModuleInt; // Calculate the position of the neighbor block
                        // Debug.Log("Checking neighbor at: " + neighborPos + " for connection offset: " + offset);
                        if (BlockManager.instance != null && BlockManager.instance.TryGetBlockAt(neighborPos, out Rigidbody neighborRb)) // If neighbor exist
                        {
                            if (neighborRb != null) {
                                Hull neighborHull = neighborRb.GetComponent<Hull>();
                                if (neighborHull != null)
                                {
                                    Vector3Int gridOffset = spawnPosInt - neighborPos;

                                    Vector3 gridOffsetVec = gridOffset;
                                    Quaternion neighborLocalRot = neighborRb.transform.localRotation;
                                    Vector3 neighborLocalOffset = Quaternion.Inverse(neighborLocalRot) * gridOffsetVec;
                                    Vector3Int neighborLocalOffsetInt = Vector3Int.RoundToInt(neighborLocalOffset);

                                    if (neighborHull.validConnectionOffsets.Contains(neighborLocalOffsetInt))
                                    {
                                        var joint = newBlock.AddComponent<FixedJoint>();
                                        joint.connectedBody = neighborRb;
                                        joint.breakForce = breakForce;

                                        BlockManager.instance.AddConnection(spawnPosInt, neighborPos);
                                        
                                        
                                        // Debug.Log($"Connected new block at {spawnPosInt} to neighbor at {neighborPos} (offset {offset}, opposite {oppositeOffset}).");
                                    }
                                    else
                                    {
                                        // Debug.Log($"Neighbor at {neighborPos} does not allow connection at offset {oppositeOffset}.");
                                    }
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
                    BlockInventoryManager.instance.TryConsumeBlock(currentBlock, 1);
                    SetText();
                }
                // else
                // {
                //     Debug.LogWarning("New block has no Hull component. Skipping connection logic.");
                // }
                
                if (BlockManager.instance != null)
                {
                    BlockManager.instance.AddBlock(spawnPosInt, newBlockRb); // Register the new block in the block manager
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
            if (hitInfo.transform.CompareTag("Block") || ((hitInfo.transform.CompareTag("EnemyBlock") && hitInfo.transform.GetComponent<Hull>().canPickup)))
            {
                Hull hull = hitInfo.transform.GetComponent<Hull>();
                if (hull != null && hull.sourceBlock != null)
                {
                    BlockInventoryManager.instance.AddBlock(hull.sourceBlock, 1);
                    SetText();
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
        // We'll do the same conversion steps for the preview
        LayerMask effectiveMask = rayCastLayers & ~shieldLayer;
        effectiveMask = effectiveMask & ~previewIgnoreLayers;

        if (Physics.Raycast(shootingPoint.position, shootingPoint.forward, out RaycastHit hitInfo, Mathf.Infinity, effectiveMask))
        {
            // Debug.Log("Raycast hit: " + hitInfo.collider.gameObject.name);
            // Debug.Log("Raycast hit object has parent: " + hitInfo.collider.gameObject.transform.parent?.gameObject.name);
            if (hitInfo.collider.gameObject.layer == 6 || (hitInfo.collider.gameObject.transform.parent != null && hitInfo.collider.gameObject.transform.parent.gameObject.layer == 6))
            {
                // Convert to commandModule space
                Vector3 localPoint = commandModule.InverseTransformPoint(hitInfo.point);
                Vector3 localNormal = commandModule.InverseTransformDirection(hitInfo.normal);

                // Round spawn
                Vector3 localSpawn = new Vector3(
                    Mathf.RoundToInt(localPoint.x + localNormal.x / 2f),
                    Mathf.RoundToInt(localPoint.y + localNormal.y / 2f),
                    Mathf.RoundToInt(localPoint.z + localNormal.z / 2f)
                );

                // If we haven't created the preview object yet, do it
                if (previewBlock == null)
                {
                    previewBlock = Instantiate(currentBlock.PreviewObject, parent);
                    previewBlockOriginalRotation = previewBlock.transform.rotation;
                    RemovePhysicsComponents(previewBlock);
                }

                // Convert to world, then parent-local
                Vector3 worldSpawn = commandModule.TransformPoint(localSpawn);
                Vector3 spawnInParentSpace = parent.InverseTransformPoint(worldSpawn);
                previewBlock.transform.localPosition = spawnInParentSpace;

                // Figure out orientation (top / bottom / side)
                float angleWithUp = Vector3.Angle(hitInfo.normal, commandModule.TransformDirection(Vector3.up));
                bool isTopSurface = angleWithUp < 30f;
                bool isBottomSurface = angleWithUp > 150f;
                bool isSideSurface = !isTopSurface && !isBottomSurface;

                // We'll build the final world rotation
                Quaternion newBlockWorldRotation = commandModule.rotation;

                if (isSideSurface)
                {
                    Vector3 hitNormal = hitInfo.normal;
                    Vector3 snappedNormal = new Vector3(
                        Mathf.Abs(hitNormal.x) > 0.9f ? Mathf.Sign(hitNormal.x) : 0f,
                        Mathf.Abs(hitNormal.y) > 0.9f ? Mathf.Sign(hitNormal.y) : 0f,
                        Mathf.Abs(hitNormal.z) > 0.9f ? Mathf.Sign(hitNormal.z) : 0f
                    );
                    snappedNormal.Normalize();

                    Vector3 blockAttachDir_world = referenceTransform.TransformDirection(currentBlock.attachDirection);

                    Quaternion toNormal_world = Quaternion.FromToRotation(blockAttachDir_world, snappedNormal);

                    newBlockWorldRotation = toNormal_world * referenceTransform.rotation;

                    newBlockWorldRotation *= Quaternion.Euler(0f, 180f, 0f);
                }
                else
                {
                    // top or bottom
                    if (isBottomSurface)
                    {
                        newBlockWorldRotation *= Quaternion.Euler(180f, 0f, 0f);
                    }
                }

                // Add user rotation offset
                if (currentBlock.isRotatable){ // If block is rotatable, apply rotation offset
                    newBlockWorldRotation *= Quaternion.Euler(0f, 90f * rotationOffsetCount, 0f);
                }

                // Convert world rotation -> parent local
                Quaternion finalLocalRotation = Quaternion.Inverse(parent.rotation) * newBlockWorldRotation;
                previewBlock.transform.localRotation = finalLocalRotation;
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
    private void ReparentAllBlocksToReference()
    {
        List<Transform> blocksToMove = new List<Transform>();
        foreach (Transform child in parent)
        {
            blocksToMove.Add(child);
        }
        foreach (Transform child in blocksToMove)
        {
            child.SetParent(referenceTransform, true);
        }
    }

    private void TransferBlocksToParent()
    {
        List<Transform> blocksToTransfer = new List<Transform>();
        foreach (Transform child in referenceTransform)
        {
            blocksToTransfer.Add(child);
        }
        foreach (Transform child in blocksToTransfer)
        {
            child.SetParent(parent, true);
        }
    }
}

// [System.Serializable]
// public class BlockInventory
// {
//     public Block Block;
//     public int CurrentCount;
//     public int MaxStack = 99;
// }