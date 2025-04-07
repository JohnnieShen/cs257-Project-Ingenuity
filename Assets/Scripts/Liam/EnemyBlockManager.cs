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
    public static EnemyBlockManager instance;
    public Dictionary<EnemyAI, EnemyVehicleStructure> vehicles = new Dictionary<EnemyAI, EnemyVehicleStructure>();
    private bool isValidating = false;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Debug.LogError("Duplicated AIBlockManager", gameObject);
        InitializeAllAIVehicles();
    }

    public void RegisterBlock(EnemyAI vehicle, Vector3Int localPos, Rigidbody blockRb) {
        if (!vehicles.ContainsKey(vehicle))
            vehicles[vehicle] = new EnemyVehicleStructure();
        
        var structure = vehicles[vehicle];
        if (!structure.blocks.ContainsKey(localPos))
            structure.blocks.Add(localPos, blockRb);
    }
    private void InitializeAllAIVehicles()
    {
        EnemyAI[] allEnemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemy in allEnemies)
        {
            if (!vehicles.ContainsKey(enemy))
            {
                // Debug.Log("Initializing vehicle structure for: " + enemy.name);
                vehicles.Add(enemy, new EnemyVehicleStructure());
                enemy.InitializeVehicleStructure();
            }
        }
    }
    public bool TryGetBlock(EnemyAI vehicle, Vector3Int localPos, out Rigidbody blockRb) {
        blockRb = null;
        if (!vehicles.ContainsKey(vehicle))
            return false;
        var structure = vehicles[vehicle];
        return structure.blocks.TryGetValue(localPos, out blockRb);
    }

    public void RemoveBlock(EnemyAI vehicle, Vector3Int localPos) {
        if (!vehicles.ContainsKey(vehicle)) return;
        // Debug.Log("if reached vehicle contains key, Removing block at " + localPos + " from vehicle " + vehicle.name);
        var structure = vehicles[vehicle];
        if (structure.blocks.ContainsKey(localPos)) {
            structure.blocks.Remove(localPos);
            RemoveConnections(vehicle, localPos);
        }
    }

    public void AddConnection(EnemyAI vehicle, Vector3Int pos1, Vector3Int pos2) {
        AddConnectionDirection(vehicle, pos1, pos2);
        AddConnectionDirection(vehicle, pos2, pos1);
    }

    private void AddConnectionDirection(EnemyAI vehicle, Vector3Int from, Vector3Int to) {
        if (!vehicles.ContainsKey(vehicle)) return;
        var structure = vehicles[vehicle];
        if (!structure.blockConnections.ContainsKey(from))
            structure.blockConnections[from] = new List<Vector3Int>();
        
        if (!structure.blockConnections[from].Contains(to))
            structure.blockConnections[from].Add(to);
    }
    
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
        isValidating = false;
    }

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

    public IEnumerable<KeyValuePair<Vector3Int, Rigidbody>> GetBlocksForVehicle(EnemyAI vehicle)
    {
        if (vehicles.TryGetValue(vehicle, out EnemyVehicleStructure structure))
        {
            return structure.blocks;
        }
        return Enumerable.Empty<KeyValuePair<Vector3Int, Rigidbody>>();
    }
    public IEnumerable<EnemyAI> GetEnemyVehicles() {
        return vehicles.Keys;
    }
}
