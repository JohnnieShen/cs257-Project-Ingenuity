using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class EnemyVehicleStructure {
    public Dictionary<Vector3Int, Rigidbody> blocks = new Dictionary<Vector3Int, Rigidbody>();
    public Dictionary<Vector3Int, List<Vector3Int>> blockConnections = new Dictionary<Vector3Int, List<Vector3Int>>();
}

public class EnemyBlockManager : MonoBehaviour
{
    /*
    * Author: Johnny
    * Summary: This script is responsible for managing the blocks of enemy vehicles in the game. It keeps track of the blocks and their connections, allowing for validation and manipulation of the vehicle structures.
    * The script uses a singleton pattern to ensure that only one instance of the EnemyBlockManager exists in the scene.
    * It provides methods to register blocks, remove blocks, add connections, and validate the structure of enemy vehicles.
    * The script also includes methods to get blocks for a specific vehicle and to get all enemy vehicles in the scene.
    */
    public static EnemyBlockManager instance;
    public Dictionary<EnemyAI, EnemyVehicleStructure> vehicles = new Dictionary<EnemyAI, EnemyVehicleStructure>();
    private bool isValidating = false;

    /* * Awake is called when the script instance is being loaded.
    * It initializes the singleton instance and calls the method to initialize all AI vehicles in the scene.
    */
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Debug.LogError("Duplicated AIBlockManager", gameObject);
        InitializeAllAIVehicles();
    }

    private void OnEnable()
    {
        AreaSpawner.OnSpawnCompleted += InitializeAllAIVehicles;
    }

    private void OnDisable()
    {
        AreaSpawner.OnSpawnCompleted -= InitializeAllAIVehicles;
    }

    /* RegisterBlock is called to register a block for a specific vehicle at a given local position.
    * It checks if the vehicle already has a structure and adds the block to the structure if it doesn't exist.
    * The block is stored in a dictionary with the local position as the key and the Rigidbody as the value.
    * Param 1: vehicle - The enemy vehicle to which the block belongs.
    * Param 2: localPos - The local position of the block in the vehicle's coordinate system.
    * Param 3: blockRb - The Rigidbody component of the block to be registered.
    */
    public void RegisterBlock(EnemyAI vehicle, Vector3Int localPos, Rigidbody blockRb) {
        if (!vehicles.ContainsKey(vehicle))
            vehicles[vehicle] = new EnemyVehicleStructure();
        
        var structure = vehicles[vehicle];
        if (!structure.blocks.ContainsKey(localPos))
            structure.blocks.Add(localPos, blockRb);
    }

    /* InitializeAllAIVehicles is called to initialize the vehicle structures for all enemy AI vehicles in the scene.
    * It finds all EnemyAI components in the scene and creates a new EnemyVehicleStructure for each vehicle if it doesn't already exist.
    * The method also calls the InitializeVehicleStructure method on each enemy to set up the vehicle structure.
    */
    private void InitializeAllAIVehicles()
    {
        EnemyAI[] allEnemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemy in allEnemies)
        {
            if (!vehicles.ContainsKey(enemy))
            {
                //Debug.Log("Initializing vehicle structure for: " + enemy.transform.parent.gameObject.name);
                vehicles.Add(enemy, new EnemyVehicleStructure());
                enemy.InitializeVehicleStructure();
            }
        }
    }

    /* TryGetBlock is called to retrieve a block for a specific vehicle at a given local position.
    * It checks if the vehicle exists in the dictionary and attempts to get the block from the structure.
    * If the block is found, it returns true and sets the blockRb parameter to the retrieved Rigidbody.
    * Param 1: vehicle - The enemy vehicle to which the block belongs.
    * Param 2: localPos - The local position of the block in the vehicle's coordinate system.
    * Param 3: blockRb - The Rigidbody component of the block to be retrieved.
    * Returns: true if the block is found, false otherwise.
    */
    public bool TryGetBlock(EnemyAI vehicle, Vector3Int localPos, out Rigidbody blockRb) {
        blockRb = null;
        if (!vehicles.ContainsKey(vehicle))
            return false;
        var structure = vehicles[vehicle];
        return structure.blocks.TryGetValue(localPos, out blockRb);
    }

    /* RemoveBlock is called to remove a block for a specific vehicle at a given local position.
    * It checks if the vehicle exists in the dictionary and attempts to remove the block from the structure.
    * If the block is found, it removes the block and its connections from the structure.
    * Param 1: vehicle - The enemy vehicle to which the block belongs.
    * Param 2: localPos - The local position of the block in the vehicle's coordinate system.
    */
    public void RemoveBlock(EnemyAI vehicle, Vector3Int localPos) {
        if (!vehicles.ContainsKey(vehicle)) return;
        // Debug.Log("if reached vehicle contains key, Removing block at " + localPos + " from vehicle " + vehicle.name);
        var structure = vehicles[vehicle];
        if (structure.blocks.ContainsKey(localPos)) {
            structure.blocks.Remove(localPos);
            RemoveConnections(vehicle, localPos);
        }
    }

    /* AddConnection is called to add a connection between two blocks for a specific vehicle.
    * It adds the connection in both directions (from pos1 to pos2 and from pos2 to pos1).
    * Param 1: vehicle - The enemy vehicle to which the blocks belong.
    * Param 2: pos1 - The local position of the first block in the vehicle's coordinate system.
    * Param 3: pos2 - The local position of the second block in the vehicle's coordinate system.
    */
    public void AddConnection(EnemyAI vehicle, Vector3Int pos1, Vector3Int pos2) {
        AddConnectionDirection(vehicle, pos1, pos2);
        AddConnectionDirection(vehicle, pos2, pos1);
    }

    /* AddConnectionDirection is a helper method to add a connection in one direction between two blocks for a specific vehicle.
    * It checks if the vehicle exists in the dictionary and adds the connection if it doesn't already exist.
    * Param 1: vehicle - The enemy vehicle to which the blocks belong.
    * Param 2: from - The local position of the first block in the vehicle's coordinate system.
    * Param 3: to - The local position of the second block in the vehicle's coordinate system.
    */
    private void AddConnectionDirection(EnemyAI vehicle, Vector3Int from, Vector3Int to) {
        if (!vehicles.ContainsKey(vehicle)) return;
        var structure = vehicles[vehicle];
        if (!structure.blockConnections.ContainsKey(from))
            structure.blockConnections[from] = new List<Vector3Int>();
        
        if (!structure.blockConnections[from].Contains(to))
            structure.blockConnections[from].Add(to);
    }
    
    /* RemoveConnections is called to remove all connections for a specific vehicle at a given local position.
    * It checks if the vehicle exists in the dictionary and attempts to remove the connections from the structure.
    * If the connections are found, it removes them from the structure.
    * Param 1: vehicle - The enemy vehicle to which the blocks belong.
    * Param 2: pos - The local position of the block in the vehicle's coordinate system.
    */
    public void RemoveConnections(EnemyAI vehicle, Vector3Int pos) {
        if (!vehicles.ContainsKey(vehicle)) return;
        // Debug.Log("if reached vehicle contains key, Removing connections at " + pos + " from vehicle " + vehicle.name);
        var structure = vehicles[vehicle];
        if (structure.blockConnections.ContainsKey(pos)) {
            foreach (var connection in structure.blockConnections[pos]) {
                if (structure.blockConnections.ContainsKey(connection))
                    structure.blockConnections[connection].Remove(pos);
            }
            structure.blockConnections.Remove(pos);
        }
    }

    /* CoreConnectionCheck is called to check the connections of the core block for a specific vehicle.
    * It uses a breadth-first search algorithm to traverse the connections and find all connected blocks.
    * Param 1: structure - The enemy vehicle structure to check the connections for.
    * Returns: A HashSet of Vector3Int representing the positions of all connected blocks.
    */
    public HashSet<Vector3Int> CoreConnectionCheck(EnemyVehicleStructure structure) {
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        Vector3Int corePosition = Vector3Int.zero;
        
        queue.Enqueue(corePosition);
        visited.Add(corePosition);
        
        while (queue.Count > 0) {
            Vector3Int current = queue.Dequeue();
            if (structure.blockConnections.ContainsKey(current)) {
                foreach (Vector3Int neighbor in structure.blockConnections[current]) {
                    if (!visited.Contains(neighbor)) {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }
        return visited;
    }

    /* ValidateStructure is called to validate the structure of a specific vehicle.
    * It checks if the vehicle exists in the dictionary and calls the CoreConnectionCheck method to find all connected blocks.
    * It then iterates through all blocks in the structure and detaches any blocks that are not connected to the core block.
    * Param 1: vehicle - The enemy vehicle to validate the structure for.
    */
    public void ValidateStructure(EnemyAI vehicle) {
        if (isValidating) return;
        if (!vehicles.ContainsKey(vehicle)) return;
        // Debug.Log("Validating structure for vehicle: " + vehicle.name);
        isValidating = true;
        EnemyVehicleStructure structure = vehicles[vehicle];
        HashSet<Vector3Int> connected = CoreConnectionCheck(structure);
        List<Vector3Int> allPositions = new List<Vector3Int>(structure.blocks.Keys);
        
        foreach (Vector3Int pos in allPositions) {
            if (!connected.Contains(pos)) {
                DetachBlock(vehicle, pos);
            }
        }
        var hs = vehicle.GetComponent<HealthSystem>();
        if (hs != null) {
            float newTotal = hs.CalculateTotalHealth();
            hs.OnHealthChanged?.Invoke(newTotal);
        }
        isValidating = false;
    }

    /* DetachBlock is called to detach a block for a specific vehicle at a given local position.
    * It checks if the vehicle exists in the dictionary and attempts to detach the block from the structure.
    * If the block is found, it sets the Rigidbody to be non-kinematic and removes any FixedJoint components.
    * It also sets the canPickup property of the Hull component to true and disables the Wheel and Turret components if they exist.
    * Param 1: vehicle - The enemy vehicle to which the block belongs.
    * Param 2: pos - The local position of the block in the vehicle's coordinate system.
    */
    private void DetachBlock(EnemyAI vehicle, Vector3Int pos) {
        if (!vehicles.ContainsKey(vehicle)) return;
        var structure = vehicles[vehicle];
        if (structure.blocks.TryGetValue(pos, out Rigidbody rb)) {
            rb.isKinematic = false;
            FixedJoint[] joints = rb.GetComponents<FixedJoint>();
            foreach (FixedJoint joint in joints) {
                Destroy(joint);
            }
            Hull hull = rb.GetComponent<Hull>();
            if (hull != null) {
                hull.canPickup = true;
                Wheel wheel = rb.GetComponent<Wheel>();
                if (wheel != null)
                {
                    wheel.enabled = false;
                    wheel.driveInput = 0f;
                    wheel.currentSteerAngle = 0f;
                }

                Turret turret = rb.GetComponent<Turret>();
                if (turret != null)
                {
                    turret.enabled = false;
                    if (turret.blockedLine != null)
                        turret.blockedLine.enabled = false;
                }
            }
        }
        structure.blocks.Remove(pos);
        RemoveConnections(vehicle, pos);
    }

    /* GetBlocksForVehicle is called to retrieve all blocks for a specific vehicle.
    * It checks if the vehicle exists in the dictionary and returns the blocks as an enumerable collection of KeyValuePair.
    * Param 1: vehicle - The enemy vehicle to retrieve the blocks for.
    * Returns: An enumerable collection of KeyValuePair representing the local position and Rigidbody of each block.
    */
    public IEnumerable<KeyValuePair<Vector3Int, Rigidbody>> GetBlocksForVehicle(EnemyAI vehicle)
    {
        if (vehicles.TryGetValue(vehicle, out EnemyVehicleStructure structure))
        {
            return structure.blocks;
        }
        return Enumerable.Empty<KeyValuePair<Vector3Int, Rigidbody>>();
    }

    /* GetAllEnemyVehicles is called to retrieve all enemy vehicles in the scene.
    * It returns an enumerable collection of EnemyAI representing all vehicles managed by the EnemyBlockManager.
    * Returns: An enumerable collection of EnemyAI representing all enemy vehicles.
    */
    public IEnumerable<EnemyAI> GetEnemyVehicles() {
        return vehicles.Keys;
    }
}
