using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
 
public class BuildSystem : MonoBehaviour
{
    public Block[] availableBuildingBlocks;
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
    
    public Material previewMaterial;
    private GameObject previewBlock;

    [Header("References")]
    [SerializeField] private Transform commandModule;
 
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            BuildBlock(currentBlock.BlockObject);
        }
        if (Input.GetMouseButtonDown(1))
        {
            DestroyBlock();
        }
        ChangeCurrentBlock();
        // UpdatePreview();
    }
 
    void ChangeCurrentBlock()
    {
        float scroll = Input.mouseScrollDelta.y;
        if(scroll > 0)
        {
            currentBlockIndex++;
            if (currentBlockIndex > availableBuildingBlocks.Length - 1)
            {
                currentBlockIndex = 0;
            }
        } else if(scroll < 0)
        {
            currentBlockIndex--;
            if (currentBlockIndex < 0)
            {
                currentBlockIndex = availableBuildingBlocks.Length - 1;
            }
        }
        currentBlock = availableBuildingBlocks[currentBlockIndex];
        SetText();
        if (previewBlock != null)
        {
            Destroy(previewBlock);
            previewBlock = null;
        }
    }
 
    void SetText()
    {
        blockNameText.text = currentBlock.BlockName + "\n" + currentBlock.AmountOfItemNeeded + " x " + currentBlock.ItemsNeededForBuildingBlock;
    }
   
 
    void BuildBlock(GameObject blockPrefab)
    {
        if (Physics.Raycast(shootingPoint.position, shootingPoint.forward, out RaycastHit hitInfo, rayCastLayers))
        {
            if (hitInfo.collider.gameObject.layer == 6)
            {
                Vector3 localPoint  = commandModule.InverseTransformPoint(hitInfo.point);
                Vector3 localNormal = commandModule.InverseTransformDirection(hitInfo.normal);
                
                Vector3 localSpawn = new Vector3(
                    Mathf.RoundToInt(localPoint.x + localNormal.x / 2f),
                    Mathf.RoundToInt(localPoint.y + localNormal.y / 2f),
                    Mathf.RoundToInt(localPoint.z + localNormal.z / 2f)
                );
                
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
                
                Vector3Int spawnPosInt = Vector3Int.RoundToInt(localSpawn);
                // Debug.Log("Spawn grid coordinate: " + spawnPosInt);
                
                Hull newHull = newBlock.GetComponent<Hull>();
                if(newHull != null)
                {
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
                            Hull neighborHull = neighborRb.GetComponent<Hull>();
                            if (neighborHull != null)
                            {
                                Vector3Int oppositeOffset = -rotatedOffsetInt;
                                if (neighborHull.validConnectionOffsets.Contains(oppositeOffset))
                                {
                                    newBlock.AddComponent<FixedJoint>().connectedBody = neighborRb;
                                    // Debug.Log($"Connected new block at {spawnPosInt} to neighbor at {neighborPos} (offset {offset}, opposite {oppositeOffset}).");
                                }
                                // else
                                // {
                                //     Debug.Log($"Neighbor at {neighborPos} does not allow connection at offset {oppositeOffset}.");
                                // }
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
        if (Physics.Raycast(shootingPoint.position, shootingPoint.forward, out RaycastHit hitInfo))
        {
            if (hitInfo.transform.CompareTag("Block"))
            {
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
 