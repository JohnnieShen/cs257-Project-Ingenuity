using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CutsceneManager : MonoBehaviour
{
    public CinemachineVirtualCamera playerCam;
    public CinemachineVirtualCamera cutsceneCam;
    public float cutsceneDuration = 5f;

    public void StartCutscene()
    {
        cutsceneCam.Priority = 20; // Take over
        playerCam.Priority = 10;

        StartCoroutine(EndCutsceneAfterDelay());
    }

    IEnumerator EndCutsceneAfterDelay()
    {
        yield return new WaitForSeconds(cutsceneDuration);
        cutsceneCam.Priority = 5; // Lower it
        playerCam.Priority = 15;  // Take control back
    }
}
