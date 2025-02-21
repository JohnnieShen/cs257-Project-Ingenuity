using System.Collections.Generic;

public class Polygon
{
    public List<int> m_Vertices;

    public Polygon(int a, int b, int c)
    {
        m_Vertices = new List<int> { a, b, c };
    }
}