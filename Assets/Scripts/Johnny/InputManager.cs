using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    [SerializeField] private InputActionAsset inputActions;

    private InputActionMap buildMap;
    private InputActionMap driveMap;

    private InputAction buildMoveAction;
    private InputAction buildLookAction;
    private InputAction buildBuildAction;
    private InputAction buildRemoveAction;
    private InputAction buildUpAction;
    private InputAction buildDownAction;
    private InputAction buildSwapModeAction;
    private InputAction buildScrollAction;
    private InputAction driveMoveAction;
    private InputAction driveLookAction;
    private InputAction driveShootAction;
    private InputAction driveSwapModeAction;
    private InputAction driveScrollAction;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        buildMap = inputActions.FindActionMap("Build");
        driveMap = inputActions.FindActionMap("Drive");

        if (buildMap != null)
        {
            buildMoveAction = buildMap.FindAction("Move");
            buildLookAction = buildMap.FindAction("Look");
            buildBuildAction = buildMap.FindAction("Build");
            buildRemoveAction = buildMap.FindAction("Remove");
            buildUpAction = buildMap.FindAction("Up");
            buildDownAction = buildMap.FindAction("Down");
            buildSwapModeAction = buildMap.FindAction("Swap Mode");
            buildScrollAction = buildMap.FindAction("Scroll");
        }
        if (driveMap != null)
        {
            driveMoveAction = driveMap.FindAction("Move");
            driveLookAction = driveMap.FindAction("Look");
            driveShootAction = driveMap.FindAction("Fire");
            driveSwapModeAction = driveMap.FindAction("Swap Mode");
            driveScrollAction = driveMap.FindAction("Scroll");
        }

        EnableBuildMap();
    }
    public InputAction GetBuildMoveAction()
    {
        return buildMoveAction;
    }
    public InputAction GetBuildLookAction()
    {
        return buildLookAction;
    }
    public InputAction GetBuildBuildAction()
    {
        return buildBuildAction;
    }
    public InputAction GetBuildRemoveAction()
    {
        return buildRemoveAction;
    }
    public InputAction GetBuildUpAction()
    {
        return buildUpAction;
    }
    public InputAction GetBuildDownAction()
    {
        return buildDownAction;
    }
    public InputAction GetBuildSwapModeAction()
    {
        return buildSwapModeAction;
    }
    public InputAction GetBuildScrollAction()
    {
        return buildScrollAction;
    }
    public InputAction GetDriveMoveAction()
    {
        return driveMoveAction;
    }
    public InputAction GetDriveLookAction()
    {
        return driveLookAction;
    }
    public InputAction GetDriveShootAction()
    {
        return driveShootAction;
    }
    public InputAction GetDriveSwapModeAction()
    {
        return driveSwapModeAction;
    }
    public InputAction GetDriveScrollAction()
    {
        return driveScrollAction;
    }
    public void EnableBuildMap()
    {
        if (buildMap != null) buildMap.Enable();
        if (driveMap != null) driveMap.Disable();
    }

    public void EnableDriveMap()
    {
        if (driveMap != null) driveMap.Enable();
        if (buildMap != null) buildMap.Disable();
    }
}
