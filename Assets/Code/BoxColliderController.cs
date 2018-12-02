using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxColliderController : MonoBehaviour {

    public Vector2 Coord;

    public delegate void OnCollisionDelegate(Vector2 coord);

    public OnCollisionDelegate MyOnCollision;

    void OnCollisionEnter2D(Collision2D collision) {
        MyOnCollision(Coord);
        Destroy(collision.gameObject);
    }
}
