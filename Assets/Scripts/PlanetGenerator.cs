using UnityEngine;
using System.Collections.Generic;

public class PlanetGenerator : MonoBehaviour
{
    public Material m_Material;
    private GameObject m_PlanetMesh;
    private Planet planet;

    void Start()
    {
        GeneratePlanet();
    }

    public void GeneratePlanet()
    {
        if (m_PlanetMesh)
            Destroy(m_PlanetMesh);

        planet = new Planet();
        planet.InitAsIcosohedron();
        planet.Subdivide(2);

        m_PlanetMesh = new GameObject("Planet Mesh");
        MeshRenderer surfaceRenderer = m_PlanetMesh.AddComponent<MeshRenderer>();
        surfaceRenderer.material = m_Material;

        Mesh terrainMesh = new Mesh();
        int vertexCount = planet.m_Polygons.Count * 3;
        int[] indices = new int[vertexCount];
        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        Color32[] colors = new Color32[vertexCount];

        Color32 sandLight = new Color32(237, 201, 175, 255); // Light sand
        Color32 sandDark  = new Color32(194, 178, 128, 255); // Darker sand

        for (int i = 0; i < planet.m_Polygons.Count; i++)
        {
            var poly = planet.m_Polygons[i];
            indices[i * 3 + 0] = i * 3 + 0;
            indices[i * 3 + 1] = i * 3 + 1;
            indices[i * 3 + 2] = i * 3 + 2;
            vertices[i * 3 + 0] = planet.m_Vertices[poly.m_Vertices[0]] * 10f;
            vertices[i * 3 + 1] = planet.m_Vertices[poly.m_Vertices[1]] * 10f;
            vertices[i * 3 + 2] = planet.m_Vertices[poly.m_Vertices[2]] * 10f;

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
}