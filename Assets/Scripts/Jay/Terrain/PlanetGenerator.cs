using UnityEngine;
using System.Collections.Generic;

public class PlanetGenerator : MonoBehaviour
{
    /*
    Author: Jay
    Summary: (outdated), based on the tile structure of the Planet script, this
    generator allowed for procedural generation of different permutations of the
    planet by raising and sinking certain tile groups.
    */

    public Material m_Material;
    private GameObject m_PlanetMesh;
    private Planet planet;

    void Start()
    {
        GeneratePlanet();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) {
            GeneratePlanet();
        }
    }

    public void GeneratePlanet()
    {
        if (m_PlanetMesh)
            Destroy(m_PlanetMesh);

        planet = new Planet();
        planet.InitAsIcosohedron();
        planet.Subdivide(4);
        planet.CalculateNeighbors();

        //AddTerrainFeatures();

        m_PlanetMesh = new GameObject("Planet Mesh");
        MeshRenderer surfaceRenderer = m_PlanetMesh.AddComponent<MeshRenderer>();
        surfaceRenderer.material = m_Material;

        Mesh terrainMesh = new Mesh();
        int vertexCount = planet.m_Polygons.Count * 3;
        int[] indices = new int[vertexCount];
        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        Color32[] colors = new Color32[vertexCount];

        Color32 sandLight = new Color32(237, 201, 175, 255); 
        Color32 sandDark  = new Color32(194, 178, 128, 255); 

        for (int i = 0; i < planet.m_Polygons.Count; i++)
        {
            var poly = planet.m_Polygons[i];
            indices[i * 3 + 0] = i * 3 + 0;
            indices[i * 3 + 1] = i * 3 + 1;
            indices[i * 3 + 2] = i * 3 + 2;
            vertices[i * 3 + 0] = planet.m_Vertices[poly.m_Vertices[0]] * 20f;
            vertices[i * 3 + 1] = planet.m_Vertices[poly.m_Vertices[1]] * 20f;
            vertices[i * 3 + 2] = planet.m_Vertices[poly.m_Vertices[2]] * 20f;

            Color32 polyColor = Color32.Lerp(sandLight, sandDark, Random.Range(0.0f, 1.0f));
            colors[i * 3 + 0] = polyColor;
            colors[i * 3 + 1] = polyColor;
            colors[i * 3 + 2] = polyColor;

            normals[i * 3 + 0] = vertices[i * 3 + 0].normalized;
            normals[i * 3 + 1] = vertices[i * 3 + 1].normalized;
            normals[i * 3 + 2] = vertices[i * 3 + 2].normalized;
        }

        terrainMesh.vertices = vertices;
        terrainMesh.normals = normals;
        terrainMesh.colors32 = colors;
        terrainMesh.SetTriangles(indices, 0);

        MeshFilter terrainFilter = m_PlanetMesh.AddComponent<MeshFilter>();
        terrainFilter.mesh = terrainMesh;

        MeshCollider terrainCollider = m_PlanetMesh.AddComponent<MeshCollider>();
        terrainCollider.sharedMesh = terrainMesh;
    }
    private void AddTerrainFeatures() {
        int numFeatures = 10; // # of terrain variations
        for (int i = 0; i < numFeatures; i++)
        {
            // random location on the planet
            Vector3 randomCenter = Random.onUnitSphere;
            float randomRadius = Random.Range(0.1f, 0.3f); // random region size
            PolySet selectedPolys = planet.GetPolysInSphere(randomCenter, randomRadius, planet.m_Polygons);

            if (selectedPolys.Count > 0)
            {
                float extrudeAmount = Random.Range(-0.2f, 0.5f); // - for oceans, + for hills
                planet.Extrude(selectedPolys, extrudeAmount);
            }
        }
    }
}