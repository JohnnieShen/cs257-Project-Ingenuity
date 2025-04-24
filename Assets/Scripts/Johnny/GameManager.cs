using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject initialCore;
    public GameObject player;

    public GameObject corePrefab;
    public Transform coreParent;
    public GameObject camerHolder;

    private GameObject _currentCore;
    public GameObject visualWidget;
    public Vector3Int blockParentPos;
    [Header("Win-screen")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private bool pauseOnWin = true;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // If you started the scene with a core already in place, register it now:
        if (initialCore != null)
        {
            _currentCore = initialCore;
        }
        else
        {
            Debug.LogWarning("GameManager: no initialCore assigned; will only spawn on first death.");
        }
    }

    void Update()
    {
        if (_currentCore == null){
            SpawnCore();
            coreParent.position = blockParentPos;
            if (ModeSwitcher.instance != null)
            {
                ModeSwitcher.instance.SetMode(ModeSwitcher.Mode.Build);
            }
        }
    }

    private void SpawnCore()
    {
        if (corePrefab == null || coreParent == null)
        {
            Debug.LogError("GameManager: Core prefab or parent is not assigned!");
            return;
        }

        _currentCore = Instantiate(corePrefab, coreParent);
        _currentCore.transform.localPosition = Vector3.zero;
        _currentCore.transform.localRotation = Quaternion.identity;
        _currentCore.transform.localScale = Vector3.one;

        _currentCore.name = corePrefab.name;

        RegisterCore(_currentCore);
        
    }

    private void RegisterCore(GameObject core) {
        if (BlockManager.instance != null)
        {
            BlockManager.instance.gridOrigin = core.transform;
        }
        PlayerMove playerMove = player.GetComponent<PlayerMove>();
        if (playerMove != null)
        {
            playerMove.moveCenter = core.transform;
        }
        BuildSystem buildSystem = player.GetComponent<BuildSystem>();
        if (buildSystem != null)
        {
            buildSystem.commandModule = core.transform;
        }
        Minimap minimap = camerHolder.GetComponentsInChildren<Minimap>(true)[0];
        if (minimap != null)
        {
            minimap.player = core.transform;
        }
        FreeCameraLook cameraLook = camerHolder.GetComponentInChildren<FreeCameraLook>(true);
        if (cameraLook != null)
        {
            cameraLook.SetTarget(core.transform);
            cameraLook.ResetView();
        }
        visualWidget = null;
        foreach (var t in core.GetComponentsInChildren<Transform>(true))
        {
            if (t.CompareTag("BuildVisualWidget"))
            {
                visualWidget = t.gameObject;
                break;
            }
        }
        ModeSwitcher.instance.buildArrow = visualWidget;
        RebuildPlayerBlockManager(core);
        UpdateAllEnemyAITargets(core.transform);
    }
    private void RebuildPlayerBlockManager(GameObject core)
    {
        var bm = BlockManager.instance;
        bm.blocks.Clear();
        bm.blockConnections.Clear();

        var hulls = core.GetComponentsInChildren<Hull>();
        foreach (var h in hulls)
        {
            var rb = h.GetComponent<Rigidbody>();
            var localPos = Vector3Int.RoundToInt(
                core.transform.InverseTransformPoint(h.transform.position)
            );
            bm.AddBlock(localPos, rb);
        }

        StartCoroutine(bm.DelayedRecalculateConnections());
    }
    private void UpdateAllEnemyAITargets(Transform coreTransform)
    {
        if (EnemyBlockManager.instance != null)
        {
            foreach (var enemy in EnemyBlockManager.instance.GetEnemyVehicles())
            {
                if (enemy != null)
                    enemy.playerTarget = coreTransform;
            }
        }
        else
        {
            foreach (var enemy in FindObjectsOfType<EnemyAI>())
            {
                enemy.playerTarget = coreTransform;
            }
        }
    }
    public void WinGame()
    {
        if (winPanel != null)
            winPanel.SetActive(true);

        if (pauseOnWin)
            Time.timeScale = 0f;
        
    }
    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }
}
