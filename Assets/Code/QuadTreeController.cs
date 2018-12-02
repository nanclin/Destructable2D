using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeController : MonoBehaviour {

    [Header("Press return key to generate quadtree")]
    [SerializeField] [Range(0, 5)] private int TreeHeight = 3;
    [SerializeField] bool ShowGrid = true;
    [SerializeField] bool ShowOnlyFullValues = false;
    [SerializeField] float Radius = 1;

    private QuadTree Tree;

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            Tree = new QuadTree(Vector2.zero, 1, 0, TreeHeight);
        }

        if (Input.GetMouseButton(0) && Tree != null) {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Input.GetKey(KeyCode.LeftShift))
                Debug.Log(Tree.GetValue(mousePos, Tree));
            else {
//                Tree.SetValue(mousePos, 1, Tree); // point set value
                Tree.SetValueCircle(1, mousePos, Radius, Tree); // circle set value
            }
        }

    }

    void OnDrawGizmos() {
        if (Tree != null)
            DrawQuadTree(Tree);

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

        // draw value
        float t = tree.Depth / (float) (tree.GetRoot.TreeHeight);
        float size = Mathf.Lerp(0.2f, 0.02f, t);
        float transparency = Mathf.Lerp(0.1f, 1, t);
        if (ShowOnlyFullValues) {
            if (tree.Value == 1) {
                Gizmos.color = Color.white * transparency;
                Gizmos.DrawCube(new Vector2(cx, cy), Vector3.one * size);
            }
        } else {
            Gizmos.color = Color.Lerp(Color.black, Color.white, t) * transparency;
            Gizmos.DrawCube(new Vector2(cx, cy), Vector3.one * size);
        }

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
