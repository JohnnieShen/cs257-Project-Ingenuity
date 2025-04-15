using UnityEngine;
using TMPro;

public class PopupDialogue : MonoBehaviour
{
    /*
    * Author: Johnny
    * Summary: This script is responsible for displaying a popup dialogue in the game. It uses TextMeshProUGUI to show the message and destroys itself after a specified delay.
    * The script can be attached to a GameObject in the scene, and it will automatically find the TextMeshProUGUI component in its children.
    * The message can be set using the SetText method, and the popup will be destroyed after the specified delay.
    */

    public TextMeshProUGUI displayText;

    public float destroyDelay = 2f;

    /* Start is called before the first frame update.
    * It checks if the displayText is assigned; if not, it tries to find it in the children of the GameObject.
    * It then destroys the GameObject after the specified delay.
    */
    private void Start()
    {
        if (!displayText) displayText = GetComponentInChildren<TextMeshProUGUI>();

        Destroy(gameObject, destroyDelay);
    }

    /* SetText is a public method that allows setting the text of the popup dialogue.
    * It takes a string message as a parameter and assigns it to the displayText component.
    * If the displayText is not assigned, it logs a warning message.
    */
    public void SetText(string message)
    {
        if (displayText != null)
        {
            displayText.text = message;
        }
        else
        {
            Debug.LogWarning("TextMeshProUGUI reference not assigned on " + gameObject.name);
        }
    }
}
