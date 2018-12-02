using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour {

    [SerializeField] [Range(1, 50)] int MapWidth = 30;
    [SerializeField] [Range(1, 50)] int MapHeight = 30;

    [SerializeField] [Range(0, 1)] float Treshold1;
    [SerializeField] [Range(0, 1)] float Treshold2;
    [SerializeField] AnimationCurve AnimationCurve;

    [SerializeField] float Radius = 1;
    [SerializeField] float ShootForce = 1000;
    [SerializeField] float BlastPower = 0.5f;

    [SerializeField] GameObject BoxColliderPrefab;
    [SerializeField] GameObject BombPrefab;
    [SerializeField] GameObject TileBreakParticles;

    [SerializeField] MeshGenerator ProceduralMesh;
    [SerializeField] MeshGenerator ProceduralMesh2;

    private BoxColliderController[,] BoxColliders;
    private float[,] Map;
    private float TileSize;

    void Start() {
        TileSize = 1.0f / Mathf.Min(MapWidth, MapHeight);
        InitializeMap(MapWidth, MapHeight);
        ProceduralMesh.GenerateGridMesh(Map, TileSize, Treshold1);
        ProceduralMesh2.GenerateGridMesh(Map, TileSize, Treshold2);
        GenerateGridColliders(Map);
    }

    void Update() {

        if (Input.GetKeyDown(KeyCode.Return)) {
            InitializeMap(MapWidth, MapHeight);
            ProceduralMesh.GenerateGridMesh(Map, TileSize, Treshold1);
            ProceduralMesh2.GenerateGridMesh(Map, TileSize, Treshold2);
            GenerateGridColliders(Map);
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
        for (int r = 0; r < height; r++) {
            for (int c = 0; c < width; c++) {
                Map[r, c] = 1;//Random.value;
            }
        }
    }

    public void GenerateGridColliders(float[,] map) {
        int rows = map.GetLength(0);
        int columns = map.GetLength(1);

        // remove old colliders
        for (int r = 0; r < rows; r++) {
            if (BoxColliders == null) break;
            for (int c = 0; c < columns; c++) {
                BoxColliderController box = BoxColliders[r, c];
                if (box != null)
                    Destroy(box.gameObject);
            }
        }

        // add new box colliders
        BoxColliders = new BoxColliderController[rows, columns];
        for (int r = 0; r < rows; r++) {
            for (int c = 0; c < columns; c++) {
                Vector3 pos = new Vector3(c, r) * TileSize;

                float value = map[rows - r - 1, columns - c - 1];
                if (value > 0.5f) {
                    BoxColliderController box = Instantiate(BoxColliderPrefab).GetComponent<BoxColliderController>();
                    box.transform.parent = transform;
                    box.transform.localPosition = pos;
                    box.transform.localScale = Vector3.one * TileSize;
                    box.Coord = new Vector2(columns - c - 1, rows - r - 1);
                    box.Map = Map;
                    box.MyOnCollision = OnTileCollision;
                    BoxColliders[rows - r - 1, columns - c - 1] = box;
                }
            }
        }
    }

    private void OnTileCollision(Vector2 coord, Vector2 blastDirection) {
        int rows = Map.GetLength(0);
        int columns = Map.GetLength(1);

        bool mapChanged = false;
        for (int r = 0; r < rows; r++) {
            for (int c = 0; c < columns; c++) {
                Vector2 diff = (coord - new Vector2(c, r)) * TileSize;
                if (diff.magnitude < Radius) {

                    // empty cell
                    if (Map[r, c] <= 0)
                        continue;

                    // damage
                    float falloff = 1 - diff.magnitude / Radius;
                    falloff = AnimationCurve.Evaluate(falloff);
                    Map[r, c] -= falloff * BlastPower;
                    mapChanged = true;

                    // not yet dead
                    if (Map[r, c] > 0)
                        continue;

                    // kill cell
                    BoxColliderController box = BoxColliders[r, c];
                    if (box == null)
                        continue;

                    // explosion
                    Quaternion rot = Quaternion.LookRotation(-blastDirection);
                    Instantiate(TileBreakParticles, box.transform.position - Vector3.forward, rot);

                    // destroy
                    Destroy(box.gameObject);
                    BoxColliders[r, c] = null;
                }
            }
        }

        if (mapChanged) {
            ProceduralMesh.GenerateGridMesh(Map, TileSize, Treshold1);
            ProceduralMesh2.GenerateGridMesh(Map, TileSize, Treshold2);
        }
    }
}
