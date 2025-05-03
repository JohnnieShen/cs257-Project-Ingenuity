using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTriggerZoneEnter : MonoBehaviour
{
    /**
    *Author: Jay
    *Summary: when the player enters a certain range within the boss base,
    *a cutscene will play.
    **/
    public BossDoor door;
    public GameObject playerCamera;
    public GameObject cutsceneCam;
    private bool cutsceneTriggered = false;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Core") && !cutsceneTriggered)
        {
            cutsceneTriggered = true;
            Debug.Log("Entered Boss Zone");
            door.LowerDoor();

            cutsceneCam.SetActive(true);
            playerCamera.SetActive(false);

            StartCoroutine(EndCutscene());
        }
    }
    IEnumerator EndCutscene()
    {
        yield return new WaitForSeconds(5f);
        cutsceneCam.SetActive(false);
        playerCamera.SetActive(true);
    }
}
