using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDoor : MonoBehaviour
{
    /*
    Author: Jay
    Summary: Script that lowers the door leading to boss base into the ground
    */
    // Start is called before the first frame update
    public Transform door;
    public float lowerDistance = 5f;
    public float duration = 2f;

    public void LowerDoor()
    {
        StartCoroutine(LowerDoorRoutine());
    }

    IEnumerator LowerDoorRoutine()
    {
        Vector3 startPos = door.position;
        Vector3 endPos = startPos - new Vector3(0, lowerDistance, 0);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            door.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        door.position = endPos;
    }
}
