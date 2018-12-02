using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {

    [SerializeField] [Range(1, 50)] int MapWidth = 30;
    [SerializeField] [Range(1, 50)] int MapHeight = 30;

    [SerializeField] float Radius = 1;
    [SerializeField] float ShootForce = 1000;
    [SerializeField] float BlastPower = 0.5f;

    [SerializeField] GameObject BoxColliderPrefab;
    [SerializeField] GameObject BombPrefab;

    public MeshFilter MeshFilter;

    private Vector3[] Vertices;
    private int[] Triangles;
    private Vector2[] UVs;

    private float[,] Map = new float[,] {
        { 0, 0, 0, 0, },
        { 0, 1, 1, 0, },
        { 0, 0, 1, 0, },
        { 0, 0, 0, 0, },
    };

    private BoxColliderController[,] BoxColliders;

    void Start() {
        InitializeMap(MapWidth, MapHeight);
    }

    void Update() {

        if (Input.GetKeyDown(KeyCode.Return)) {
            InitializeMap(MapWidth, MapHeight);
        }

        // shoot
        if (Input.GetMouseButtonDown(0)) {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            GameObject bomb = Instantiate(BombPrefab, mousePos, Quaternion.identity);
            Vector2 dir = (Vector2.one * 0.5f - (Vector2) bomb.transform.position).normalized;
            bomb.GetComponent<Rigidbody2D>().AddForce(dir * ShootForce);
        }
    }

    private void InitializeMap(int width, int height) {
        // generate from 2Dmap
        Map = new float[height, width];
        for (int r = 0; r < Map.GetLength(0); r++) {
            for (int c = 0; c < Map.GetLength(1); c++) {
                Map[r, c] = 1;//Random.value;
            }
        }
        GenerateGridMesh(Map);
        GenerateGridColliders(Map);
    }

    public void GenerateGridMesh(float[,] map) {
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
                if (value > 0) {
                    AddTile(pos, tileSize, new Vector2(1, 1), ref vertexIndex, ref triangleIndex);
                }
            }
        }

        MeshFilter.mesh.vertices = Vertices;
        MeshFilter.mesh.triangles = Triangles;
        MeshFilter.mesh.uv = UVs;
    }

    public void GenerateGridColliders(float[,] map) {
        int rows = map.GetLength(0);
        int columns = map.GetLength(1);
        float tileSize = 1.0f / rows;

        // remove old colliders
        for (int r = 0; r < rows; r++) {
            if (BoxColliders == null) break;
            for (int c = 0; c < columns; c++) {
                BoxColliderController box = BoxColliders[c, r];
                if (box != null)
                    Destroy(box.gameObject);
            }
        }

        // add new box colliders
        BoxColliders = new BoxColliderController[rows, columns];
        for (int r = 0; r < rows; r++) {
            for (int c = 0; c < columns; c++) {
                Vector3 pos = new Vector3(c, r) * tileSize;

                float value = map[columns - c - 1, rows - r - 1];
                if (value > 0.5f) {
                    BoxColliderController box = Instantiate(BoxColliderPrefab).GetComponent<BoxColliderController>();
                    box.transform.parent = transform;
                    box.transform.localPosition = pos;
                    box.transform.localScale = Vector3.one * tileSize;
                    box.Coord = new Vector2(columns - c - 1, rows - r - 1);
                    box.MyOnCollision = OnTileCollision;
                    BoxColliders[columns - c - 1, rows - r - 1] = box;
                }
            }
        }
    }

    private void OnTileCollision(Vector2 coord) {
        int rows = Map.GetLength(0);
        int columns = Map.GetLength(1);
        float tileSize = 1.0f / rows;

        bool mapChanged = false;
        for (int r = 0; r < rows; r++) {
            for (int c = 0; c < columns; c++) {
                Vector2 diff = (coord - new Vector2(c, r)) * tileSize;
                if (diff.magnitude < Radius) {

                    // empty cell
                    if (Map[c, r] <= 0)
                        continue;

                    // damage
                    float falloff = 1 - diff.magnitude / Radius;
                    falloff *= falloff;
                    Map[c, r] -= falloff * BlastPower;

                    // not yet dead
                    if (Map[c, r] > 0)
                        continue;

                    // kill cell
                    mapChanged = true;
                    BoxColliderController box = BoxColliders[c, r];
                    if (box == null)
                        continue;
                    Destroy(box.gameObject);
                    BoxColliders[c, r] = null;
                }
            }
        }

        if (mapChanged)
            GenerateGridMesh(Map);
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

    void OnDrawGizmos() {
        // draw blast radius
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(mousePos, Radius);
    }
}
