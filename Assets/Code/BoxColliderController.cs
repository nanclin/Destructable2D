using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxColliderController : MonoBehaviour {

    public Vector2 Coord;
    public float[,] Map;

    public delegate void OnCollisionDelegate(Vector2 coord, Vector2 blastDirection);

    public OnCollisionDelegate MyOnCollision;

    void OnCollisionEnter2D(Collision2D collision) {
        MyOnCollision(Coord, collision.gameObject.GetComponent<ProjectileController>().Velocity.normalized);
        Destroy(collision.gameObject);
    }

    void OnDrawGizmos() {
        float tileSize = 1.0f / Mathf.Min(Map.GetLength(1), Map.GetLength(0));
        Vector3 pos = transform.position + (Vector3) Vector2.one * 0.5f * tileSize;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(pos, Vector3.one * Map[(int) Coord.y, (int) Coord.x] * tileSize);
    }
}
