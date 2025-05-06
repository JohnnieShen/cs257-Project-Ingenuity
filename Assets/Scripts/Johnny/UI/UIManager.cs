using UnityEngine;

public class UIManager : MonoBehaviour
{
    /*
    * Author: Johnny
    * Summary: This script is responsible for managing the UI elements in the game. It provides methods to show popups and manage UI entries.
    * The UIManager is a singleton, ensuring that only one instance exists throughout the game.
    * It also handles the instantiation of UI elements and their destruction after a specified delay.
    */
    public static UIManager Instance;

    public GameObject popupDialoguePrefab;
    public GameObject craftUIEntryPrefab;

    public Transform UIContainer;

    /* Awake is called when the script instance is being loaded.
    * It initializes the singleton instance and ensures that only one instance of UIManager exists.
    * It also marks the instance to not be destroyed on load, allowing it to persist across scenes.
    */
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject); // breaks respawning
    }


    /* ShowPopup is a public method that creates a popup dialogue in the UI.
    * It takes a message string and a destroy delay float as parameters.
    * It instantiates the popup dialogue prefab, sets the message text, and assigns the destroy delay.
    * If the prefab is not assigned, it logs a warning message.
    * It returns the instantiated popup GameObject.
    * Param 1: message - The message to be displayed in the popup.
    * Param 2: destroyDelay - The time in seconds before the popup is destroyed.
    */
    public GameObject ShowPopup(string message, float destroyDelay)
    {
        if (popupDialoguePrefab == null)
        {
            Debug.LogWarning("Popup dialogue prefab is not assigned in UIManager!");
            return null;
        }

        if (!gameObject.scene.isLoaded) return null;
        GameObject popupInstance = Instantiate(popupDialoguePrefab, UIContainer);
        PopupDialogue popupScript = popupInstance.GetComponent<PopupDialogue>();
        if (popupScript != null)
        {
            popupScript.SetText(message);
            popupScript.destroyDelay = destroyDelay;
        }
        else
        {
            Debug.LogWarning("The instantiated popup does not have a PopupDialogue component!");
        }
        return popupInstance;
    }
}