using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour {

    public Vector2 Velocity;
    public Rigidbody2D Rigibody;
	
    // Update is called once per frame
    void FixedUpdate() {
        Velocity = Rigibody.velocity;
        if (transform.position.y < -10)
            Destroy(gameObject);
    }
}
