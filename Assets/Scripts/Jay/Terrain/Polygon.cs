using System.Collections.Generic;

public class Polygon
{
  /*
    Author: Jay
    Summary: (outdated) base class for a polygon, takes in three points to create
    a polygon, has scripting to check for neighboring tiles, as well as replacing
    neighbors during procedural generation.
    */

    public List<int> m_Vertices;
    public List<Polygon> m_Neighbors;
    
    public Polygon(int a, int b, int c)
    {
        m_Vertices = new List<int> { a, b, c };
        m_Neighbors = new List<Polygon>();
    }

    public bool IsNeighborOf(Polygon other_poly) {
        int shared_vertices = 0;
        foreach (int vertex in m_Vertices) {
            if (other_poly.m_Vertices.Contains(vertex)) {
                shared_vertices++;
            }
        }
        return shared_vertices == 2;
    }

    public void ReplaceNeighbor(Polygon oldNeighbor, 
                              Polygon newNeighbor)
  {
    for(int i = 0; i < m_Neighbors.Count; i++)
    {
      if(oldNeighbor == m_Neighbors[i])
      {
        m_Neighbors[i] = newNeighbor;
        return;
      }
    }
  }
}