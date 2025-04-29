using UnityEngine;

public class CursorLocker : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.Escape;

    [SerializeField] private bool lockOnStart = true;

    void Start()
    {
        if (lockOnStart) LockCursor();
        else UnlockCursor();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
                UnlockCursor();
            else
                LockCursor();
        }
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }
}
