using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Build : MonoBehaviour
{
    public Transform shootingPoint;
    public GameObject blockObject;

    public Transform vehicle;

    public Color normalColor;
    public Color highlightedColor;

    GameObject lastHighlightedBlock;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Place(blockObject);
        }
        if (Input.GetMouseButtonDown(1))
        {
            Remove();
        }
        // Highlight(); // just copy pasted from youtube, not working
    }

    void Place(GameObject block)
    {
        if (Physics.Raycast(shootingPoint.position, shootingPoint.forward, out RaycastHit hit))
        {
            Vector3 normal = hit.transform.position - hit.point;
            Vector3 snappedNormal = new Vector3(Mathf.RoundToInt(hit.normal.x), Mathf.RoundToInt(hit.normal.y), Mathf.RoundToInt(hit.normal.z));
            Vector3 spawnPosition = hit.transform.position + snappedNormal;
            GameObject newBlock = Instantiate(block, spawnPosition, hit.transform.rotation, vehicle);
            if (hit.transform.tag == "Block")
            {
                hit.collider.AddComponent<FixedJoint>().connectedBody = newBlock.GetComponent<Rigidbody>();
            }
        }
    }

    void Remove()
    {
        if (Physics.Raycast(shootingPoint.position, shootingPoint.forward, out RaycastHit hit))
        {
            if (hit.transform.tag == "Block")
            {
                Destroy(hit.transform.gameObject);
            }
        }
    }

    void Highlight()
    {
        if (Physics.Raycast(shootingPoint.position, shootingPoint.forward, out RaycastHit hitInfo))
        {
            if (hitInfo.transform.tag == "Block")
            {
                if (lastHighlightedBlock == null)
                {
                    lastHighlightedBlock = hitInfo.transform.gameObject;
                    hitInfo.transform.gameObject.GetComponent<Renderer>().material.color = highlightedColor;
                }
                else if (lastHighlightedBlock != hitInfo.transform.gameObject)
                {
                    lastHighlightedBlock.GetComponent<Renderer>().material.color = normalColor;
                    hitInfo.transform.gameObject.GetComponent<Renderer>().material.color = highlightedColor;
                    lastHighlightedBlock = hitInfo.transform.gameObject;
                }
            }
        }
    }
}