using System.Collections.Generic;
using UnityEngine;

public enum NormalCalculationMode
{
    /// <summary>
    ///  The normals are not weighted.
    /// </summary>
    Unweighted,
    /// <summary>
    ///  The normals are weighted by the face area.
    /// </summary>
    AreaWeighted,
    /// <summary>
    ///  The normals are weighted by the vertex angle on each face.
    /// </summary>
    AngleWeighted,
    /// <summary>
    ///  The normals are weighted by both the face area and the vertex angle on each face.
    /// </summary>   
    AreaAndAngleWeighted
}

public static class NormalsCalculator
{
    private class PosData
    {
        /// <summary>
        /// All triangles that have a vertex in position
        /// </summary>
        public List<int> Triangles = new();
        
        /// <summary>
        /// All vertices in position
        /// </summary>
        public List<int> Vertices = new();
    }

    public static void RecalculateNormals(this Mesh mesh, NormalCalculationMode mode, float smoothingAngle)
    {
        if (mode == NormalCalculationMode.Unweighted)
        {
            mesh.RecalculateNormals();
            return;
        }

        var data = new Dictionary<Vector3, PosData>();

        var vertices = mesh.vertices;
        var normals = mesh.normals;
        var triangles = mesh.triangles;
        var weightedNormals = new Vector3[triangles.Length / 3];

        for (int i = 0; i < triangles.Length; i += 3)
        {            
            int i1 = triangles[i];
            int i2 = triangles[i + 1];
            int i3 = triangles[i + 2];

            Vector3 v1 = vertices[i2] - vertices[i1];
            Vector3 v2 = vertices[i3] - vertices[i1];

            int triIndex = i / 3;

            weightedNormals[triIndex] = Vector3.Cross(v1, v2);

            for (int j = i; j <= i + 2; j++)
            {
                int vertIndex = triangles[j];
                normals[vertIndex] = weightedNormals[triIndex].normalized;

                var vert = vertices[vertIndex];

                if (!data.ContainsKey(vert))
                {
                    data.Add(vert, new PosData());
                }

                data[vert].Triangles.Add(triIndex);
                data[vert].Vertices.Add(vertIndex);
            }
        }

        foreach (var posData in data.Values)
        {
            foreach (var vertIndex in posData.Vertices)
            {
                var sum = Vector3.zero;

                foreach (var triIndex in posData.Triangles)
                {
                    float angle = Vector3.Angle(normals[vertIndex], weightedNormals[triIndex]);

                    if (angle > smoothingAngle)
                    {
                        continue;
                    }

                    var index = triIndex * 3;

                    int i1 = triangles[index];
                    int i2 = triangles[index + 1];
                    int i3 = triangles[index + 2];

                    var adjacents = GetAdjacentVertices(vertices[vertIndex], vertices[i1], vertices[i2], vertices[i3]);

                    Vector3 v1 = adjacents.Item1 - vertices[vertIndex];
                    Vector3 v2 = adjacents.Item2 - vertices[vertIndex];

                    angle = Vector3.Angle(v1, v2);

                    switch (mode)
                    {
                        case NormalCalculationMode.AreaWeighted:
                            sum += weightedNormals[triIndex];
                            break;
                        case NormalCalculationMode.AngleWeighted:
                            sum += weightedNormals[triIndex].normalized * angle;
                            break;
                        case NormalCalculationMode.AreaAndAngleWeighted:
                            sum += weightedNormals[triIndex] * angle;
                            break;
                    }
                }

                normals[vertIndex] = sum.normalized;
            }
        }

        mesh.normals = normals;
    }

    private static (Vector3, Vector3) GetAdjacentVertices(Vector3 v, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        if (v == v1)
        {
            return (v2, v3);
        }

        if (v == v2)
        {
            return (v1, v3);
        }

        return (v1, v2);
    }
}
