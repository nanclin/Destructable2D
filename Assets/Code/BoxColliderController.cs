using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxColliderController : MonoBehaviour {

    public Vector2 Coord;

    public delegate void OnCollisionDelegate(Vector2 coord, Vector2 blastDirection);

    public OnCollisionDelegate MyOnCollision;

    void OnCollisionEnter2D(Collision2D collision) {
        MyOnCollision(Coord, collision.gameObject.GetComponent<ProjectileController>().Velocity.normalized);
        Destroy(collision.gameObject);
    }
}
