using UnityEngine;
using TMPro;

public class PopupDialogue : MonoBehaviour
{
    public TextMeshProUGUI displayText;

    public float destroyDelay = 2f;

    private void Start()
    {
        if (!displayText) displayText = GetComponentInChildren<TextMeshProUGUI>();

        Destroy(gameObject, destroyDelay);
    }

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
