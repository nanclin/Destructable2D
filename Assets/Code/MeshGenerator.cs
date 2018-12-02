using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {

    [SerializeField] [Range(1, 50)] int MapWidth = 30;
    [SerializeField] [Range(1, 50)] int MapHeight = 30;

    [SerializeField] [Range(0, 7)] private int TreeHeight = 3;
    [SerializeField] bool ShowGrid = true;
    [SerializeField] float Radius = 1;

    [SerializeField] GameObject BoxColliderPrefab;
    [SerializeField] GameObject BombPrefab;

    public MeshFilter MeshFilter;
    //    public Texture2D TileAtlas;

    private Vector3[] Vertices;
    private int[] Triangles;
    private Vector2[] UVs;

    private Sprite[] TileSprites;

    private QuadTree Tree;

    private float[,] Map = new float[,] {
        { 0, 0, 0, 0, },
        { 0, 1, 1, 0, },
        { 0, 0, 1, 0, },
        { 0, 0, 0, 0, },
    };

    private BoxColliderController[,] BoxColliders;

    void Start() {
        InitializeMap(MapWidth, MapHeight);
        GenerateGridColliders(Map);

//        // generate from quadtree
//        Tree = new QuadTree(Vector2.zero, 1, 1, TreeHeight);
////        Tree.SetValueCircle(0, new Vector2(1, 0.5f), 0.3f, Tree);
//        GenerateGrid(Tree);
    }

    void Update() {

        if (Input.GetKeyDown(KeyCode.Return)) {
//            Tree = new QuadTree(Vector2.zero, 1, 1, TreeHeight);

            InitializeMap(MapWidth, MapHeight);
        }

        if (Input.GetMouseButtonDown(0)) {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
////            Tree.SetValueCircle(0, mousePos, Radius, Tree);
////            GenerateGrid(Tree);
//
//            // blast hole through map
//            int rows = Map.GetLength(0);
//            int columns = Map.GetLength(1);
//            float tileSize = 1.0f / rows;
//
//            for (int r = 0; r < Map.GetLength(0); r++) {
//                for (int c = 0; c < Map.GetLength(1); c++) {
//                    Vector2 diff = mousePos - new Vector2(columns - c - 1, rows - r - 1) * tileSize;
//                    if (diff.magnitude < Radius)
//                        Map[c, r] = 0;
//                }
//            }
//
//            // regenerate mesh from map
//            GenerateGridMesh(Map);

            Instantiate(BombPrefab, mousePos, Quaternion.identity);
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
    }

    public void GenerateGridMesh(QuadTree tree) {
        int rows = (int) Mathf.Pow(2, tree.TreeHeight);
        int columns = rows;

        int vertexCount = columns * rows * 4;
        int trianglesCount = columns * rows * 2 * 3;
        Vertices = new Vector3[vertexCount];
        Triangles = new int[trianglesCount];
        UVs = new Vector2[vertexCount];

        AddQTTile(tree);

        MeshFilter.mesh.vertices = Vertices;
        MeshFilter.mesh.triangles = Triangles;
        MeshFilter.mesh.uv = UVs;
        MeshFilter.mesh.RecalculateBounds();
    }

    int VertexIndex = 0;
    int TriangleIndex = 0;

    private void AddQTTile(QuadTree tree) {
        if (tree.SubTrees == null) {
            if (Mathf.Approximately(tree.Value, 1))
                AddTile(tree.Position, tree.Size, Vector2.one, ref VertexIndex, ref TriangleIndex);
            return;
        }

        AddQTTile(tree.SubTrees[0]);
        AddQTTile(tree.SubTrees[1]);
        AddQTTile(tree.SubTrees[2]);
        AddQTTile(tree.SubTrees[3]);
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
                Vector3 pos = new Vector3(c, r) * tileSize;// + (Vector3) Vector2.one * tileSize * 0.5f;

//                // calculate id from neighbours
//                int c0 = (map[r + 0, c + 0] >= treshold) ? 1 : 0;
//                int c1 = (map[r + 0, c + 1] >= treshold) ? 2 : 0;
//                int c2 = (map[r + 1, c + 0] >= treshold) ? 8 : 0;
//                int c3 = (map[r + 1, c + 1] >= treshold) ? 4 : 0;
//                int id = c0 + c1 + c2 + c3;

//                AddMSTile(pos, tileSize, id, ref vertexIndex, ref triangleIndex);

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
        BoxColliders = new BoxColliderController[rows, columns];

        for (int r = 0; r < rows; r++) {
            for (int c = 0; c < columns; c++) {
                Vector3 pos = new Vector3(c, r) * tileSize;// + (Vector3) Vector2.one * tileSize * 0.5f;

                float value = map[columns - c - 1, rows - r - 1];
                if (value > 0.5f) {
                    BoxColliderController box = Instantiate(BoxColliderPrefab, pos, Quaternion.identity).GetComponent<BoxColliderController>();
                    box.Coord = new Vector2(columns - c - 1, rows - r - 1);
                    box.transform.localScale = Vector3.one * tileSize;
                    box.MyOnCollision = OnTileCollision;
                    box.transform.parent = transform;
                    BoxColliders[columns - c - 1, rows - r - 1] = box;
                }
            }
        }

    }

    private void OnTileCollision(Vector2 coord) {
//        int cr = (int) coord.y;
//        int cc = (int) coord.x;
        int rows = Map.GetLength(0);
        int columns = Map.GetLength(1);
        float tileSize = 1.0f / rows;

        bool mapChanged = false;
        for (int r = 0; r < rows; r++) {
            for (int c = 0; c < columns; c++) {
                Vector2 diff = (new Vector2(c, r) - coord) * tileSize;
                if (diff.magnitude < Radius) {

                    // empty cell
                    if (Map[c, r] <= 0)
                        continue;

                    // damage
                    Map[c, r] -= 0.1f;

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
//        Instantiate(BombPrefab, new Vector3(Random.value, 2, 0), Quaternion.identity);

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
//        List<Vector2> uvs = GetMSTileUVs(id);
//        Vector2 gridSize = new Vector2(Map.GetLength(1), Map.GetLength(0));
//        UVs[vertexIndex + 0] = ((Vector2) pos) / gridSize;
//        UVs[vertexIndex + 1] = ((Vector2) pos + Vector2.right) / gridSize;
//        UVs[vertexIndex + 2] = ((Vector2) pos + Vector2.up) / gridSize;
//        UVs[vertexIndex + 3] = ((Vector2) pos + Vector2.one) / gridSize;

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

    //    private void AddMSTile(Vector3 pos, float tileSize, int id, ref int vertexIndex, ref int triangleIndex) {
    //
    //        // vertices
    //        Vertices[vertexIndex + 0] = pos;
    //        Vertices[vertexIndex + 1] = pos + (Vector3) Vector2.right * tileSize;
    //        Vertices[vertexIndex + 2] = pos + (Vector3) Vector2.up * tileSize;
    //        Vertices[vertexIndex + 3] = pos + (Vector3) Vector2.one * tileSize;
    //
    //        // uvs
    //        List<Vector2> uvs = GetMSTileUVs(id);
    //        UVs[vertexIndex + 0] = uvs[0];
    //        UVs[vertexIndex + 1] = uvs[1];
    //        UVs[vertexIndex + 2] = uvs[2];
    //        UVs[vertexIndex + 3] = uvs[3];
    //
    //        // triangles
    //        Triangles[triangleIndex] = vertexIndex;
    //        Triangles[triangleIndex + 1] = vertexIndex + 2;
    //        Triangles[triangleIndex + 2] = vertexIndex + 1;
    //        triangleIndex += 3;
    //
    //        Triangles[triangleIndex] = vertexIndex + 2;
    //        Triangles[triangleIndex + 1] = vertexIndex + 3;
    //        Triangles[triangleIndex + 2] = vertexIndex + 1;
    //        triangleIndex += 3;
    //
    //        vertexIndex += 4;
    //    }

    //    private List<Vector2> GetMSTileUVs(int index) {
    //        Rect rect = TileSprites[index].rect;
    //
    //        float x0 = rect.x;
    //        float x1 = rect.x + rect.width;
    //        float y0 = rect.y;
    //        float y1 = rect.y + rect.height;
    //
    //        // convert to 0-1 range
    //        float u0 = x0 / TileAtlas.width;
    //        float u1 = x1 / TileAtlas.width;
    //        float v0 = y0 / TileAtlas.height;
    //        float v1 = y1 / TileAtlas.height;
    //
    //        return new List<Vector2>() {
    //            new Vector2(u0, v0),
    //            new Vector2(u1, v0),
    //            new Vector2(u0, v1),
    //            new Vector2(u1, v1),
    //        };
    //    }


    void OnDrawGizmos() {
        if (Tree != null)
            DrawQuadTree(Tree);

        // draw blast radius
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(mousePos, Radius);
    }

    private void DrawQuadTree(QuadTree tree) {

        // draw frame
        if (tree == tree.GetRoot) {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(Vector2.one * tree.Size * 0.5f, Vector3.one * tree.Size);
        }

        float cx = tree.Position.x + tree.Size * 0.5f;
        float cy = tree.Position.y + tree.Size * 0.5f;

        if (tree.SubTrees != null) {

            // draw cross
            if (ShowGrid) {
                Gizmos.color = Color.white * 0.5f;
                Gizmos.DrawLine(new Vector2(tree.Position.x, cy), new Vector2(tree.Position.x + tree.Size, cy));
                Gizmos.DrawLine(new Vector2(cx, tree.Position.y), new Vector2(cx, tree.Position.y + tree.Size));
            }

            // go level deeper
            DrawQuadTree(tree.SubTrees[0]);
            DrawQuadTree(tree.SubTrees[1]);
            DrawQuadTree(tree.SubTrees[2]);
            DrawQuadTree(tree.SubTrees[3]);
        }
    }
}
