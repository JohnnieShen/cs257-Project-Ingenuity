using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject popupDialoguePrefab;
    public GameObject craftUIEntryPrefab;

    public Transform UIContainer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public GameObject ShowPopup(string message, float destroyDelay)
    {
        if (popupDialoguePrefab == null)
        {
            Debug.LogWarning("Popup dialogue prefab is not assigned in UIManager!");
            return null;
        }

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