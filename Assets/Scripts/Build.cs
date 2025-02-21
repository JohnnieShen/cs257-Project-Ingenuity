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
 
    private void Start()
    {
        SetText();
    }
 
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
        // HighlightBlock();
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
        Debug.Log(currentBlockIndex);
        currentBlock = availableBuildingBlocks[currentBlockIndex];
        SetText();
    }
 
    void SetText()
    {
        blockNameText.text = currentBlock.BlockName + "\n" + currentBlock.AmountOfItemNeeded + " x " + currentBlock.ItemsNeededForBuildingBlock;
    }
   
 
    void BuildBlock(GameObject block)
    {
        if(Physics.Raycast(shootingPoint.position, shootingPoint.forward, out RaycastHit hitInfo))
        {
 
            if(hitInfo.transform.tag == "Block")
            {
                Vector3 spawnPosition = new Vector3(Mathf.RoundToInt(hitInfo.point.x + hitInfo.normal.x/2), Mathf.RoundToInt(hitInfo.point.y + hitInfo.normal.y / 2), Mathf.RoundToInt(hitInfo.point.z + hitInfo.normal.z /2));
                GameObject newBlock = Instantiate(block, spawnPosition, Quaternion.identity, parent);
                newBlock.AddComponent<FixedJoint>().connectedBody = hitInfo.collider.GetComponent<Rigidbody>();
            }
            else
            {
                Vector3 spawnPosition = new Vector3(Mathf.RoundToInt(hitInfo.point.x), Mathf.RoundToInt(hitInfo.point.y), Mathf.RoundToInt(hitInfo.point.z));
                Instantiate(block, spawnPosition, Quaternion.identity, parent);
            }
        }
    }
 
    void DestroyBlock()
    {
        if (Physics.Raycast(shootingPoint.position, shootingPoint.forward, out RaycastHit hitInfo))
        {
            if (hitInfo.transform.tag == "Block")
            {
                Destroy(hitInfo.transform.gameObject);
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
 