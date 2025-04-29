using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager1 : MonoBehaviour
{
    /*
    Author: Jay
    Summary: Script for the Main menu, allows for scene transitions when the player
    presses the play button.
    */

    /*
    Uses Unity's build in Scene Management package to call the play scene when the
    play button is pressed on the Main menu.
    */
    public void LoadGameScene()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("Mission Ingenuity 2"); 
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var ms = FindObjectOfType<ModeSwitcher>();
        if (ms != null)
        {
            ms.currentMode = ModeSwitcher.Mode.Build;
            ms.SetMode(ms.currentMode);
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
