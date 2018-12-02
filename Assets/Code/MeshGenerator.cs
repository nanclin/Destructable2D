using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {

    [SerializeField] private MeshFilter MeshFilter;

    private Vector3[] Vertices;
    private int[] Triangles;
    private Vector2[] UVs;

    private BoxColliderController[,] BoxColliders;

    public void GenerateGridMesh(float[,] map, float treshold = 0) {
        int rows = map.GetLength(0);
        int columns = map.GetLength(1);
        float tileSize = 1.0f / rows;

        int vertexCount = columns * rows * 4;
        int trianglesCount = columns * rows * 2 * 3;
        Vertices = new Vector3[vertexCount];
        Triangles = new int[trianglesCount];
        UVs = new Vector2[vertexCount];

        int vertexIndex = 0;
        int triangleIndex = 0;
        for (int r = 0; r < rows; r++) {
            for (int c = 0; c < columns; c++) {
                Vector3 pos = new Vector3(c, r) * tileSize;
                float value = map[columns - c - 1, rows - r - 1];
                if (value > treshold) {
                    AddTile(pos, tileSize, new Vector2(1, 1), ref vertexIndex, ref triangleIndex);
                }
            }
        }

        MeshFilter.mesh.vertices = Vertices;
        MeshFilter.mesh.triangles = Triangles;
        MeshFilter.mesh.uv = UVs;
    }

    private void AddTile(Vector3 pos, float tileSize, Vector2 gridSize, ref int vertexIndex, ref int triangleIndex) {
        // vertices
        Vertices[vertexIndex + 0] = pos;
        Vertices[vertexIndex + 1] = pos + (Vector3) Vector2.right * tileSize;
        Vertices[vertexIndex + 2] = pos + (Vector3) Vector2.up * tileSize;
        Vertices[vertexIndex + 3] = pos + (Vector3) Vector2.one * tileSize;

        // uvs
        UVs[vertexIndex + 0] = ((Vector2) pos) / gridSize;
        UVs[vertexIndex + 1] = ((Vector2) pos + Vector2.right * tileSize) / gridSize;
        UVs[vertexIndex + 2] = ((Vector2) pos + Vector2.up * tileSize) / gridSize;
        UVs[vertexIndex + 3] = ((Vector2) pos + Vector2.one * tileSize) / gridSize;

        // triangles
        Triangles[triangleIndex] = vertexIndex;
        Triangles[triangleIndex + 1] = vertexIndex + 2;
        Triangles[triangleIndex + 2] = vertexIndex + 1;
        triangleIndex += 3;

        Triangles[triangleIndex] = vertexIndex + 2;
        Triangles[triangleIndex + 1] = vertexIndex + 3;
        Triangles[triangleIndex + 2] = vertexIndex + 1;
        triangleIndex += 3;

        vertexIndex += 4;
    }
}
