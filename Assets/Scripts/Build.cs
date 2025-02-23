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
        }else if(scroll < 0)
        {
            currentBlockIndex--;
            if (currentBlockIndex < 0)
            {
                currentBlockIndex = availableBuildingBlocks.Length - 1;
            }
        }
        currentBlock = availableBuildingBlocks[currentBlockIndex];
        SetText();
    }
 
    void SetText()
    {
        blockNameText.text = currentBlock.BlockName + "\n" + currentBlock.AmountOfItemNeeded + " x " + currentBlock.ItemsNeededForBuildingBlock;
    }
   
 
    void BuildBlock(GameObject block)
    {
        if(Physics.Raycast(shootingPoint.position, shootingPoint.forward, out RaycastHit hitInfo, rayCastLayers))
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
                
                GameObject newBlock = Instantiate(block, parent);
                newBlock.transform.localPosition = localSpawn;
                newBlock.transform.localRotation = Quaternion.identity;
    
                // ConnectionPoint cp = hitInfo.collider.GetComponent<ConnectionPoint>();
                // if (cp != null)
                // {
                //     FixedJoint cpJoint = newBlock.AddComponent<FixedJoint>();
                //     cpJoint.connectedBody = cp.body;
                //     Debug.Log("Connected new block with ConnectionPoint from hit block.");
                // }
    
                Rigidbody newBlockRb = newBlock.GetComponent<Rigidbody>();
                Vector3Int spawnPosInt = Vector3Int.RoundToInt(localSpawn);
                Vector3Int[] neighborOffsets = new Vector3Int[]
                {
                    new Vector3Int( 1, 0, 0),
                    new Vector3Int(-1, 0, 0),
                    new Vector3Int( 0, 1, 0),
                    new Vector3Int( 0,-1, 0),
                    new Vector3Int( 0, 0, 1),
                    new Vector3Int( 0, 0,-1)
                };
    
                foreach (var offset in neighborOffsets)
                {
                    Vector3Int neighborPos = spawnPosInt + offset;
                    // if (ShouldConnect(neighborPos, spawnPosInt))
                    // {
                    if (BlockManager.instance != null && BlockManager.instance.TryGetBlockAt(neighborPos, out Rigidbody neighborRb))
                    {
                        newBlock.AddComponent<FixedJoint>().connectedBody = neighborRb;
                    }
                }
    
                if (BlockManager.instance != null)
                {
                    BlockManager.instance.AddBlock(spawnPosInt, newBlockRb);
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
}
 