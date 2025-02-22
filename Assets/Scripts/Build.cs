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
        // Debug.Log(currentBlockIndex);
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

                GameObject newBlock = Instantiate(block, commandModule);
                newBlock.transform.localPosition = localSpawn;
                newBlock.transform.localRotation = Quaternion.identity;

                ConnectionPoint cp = hitInfo.collider.GetComponent<ConnectionPoint>();
                if (cp != null)
                {
                    newBlock.AddComponent<FixedJoint>().connectedBody = cp.body;
                }
            }
            else
            {
                // Do nothing
                // Vector3 spawnPosition = new Vector3(Mathf.RoundToInt(hitInfo.point.x), Mathf.RoundToInt(hitInfo.point.y), Mathf.RoundToInt(hitInfo.point.z));
                // Instantiate(block, spawnPosition, Quaternion.identity, parent);
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
 