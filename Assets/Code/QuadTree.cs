using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class QuadTree {

    private QuadTree Root;

    public QuadTree GetRoot { get { return Root == null ? this : Root; } }

    public QuadTree[] SubTrees;
    public int Depth;
    public int TreeHeight;
    public Vector2 Position;
    public float Size;
    public float Value;

    private const float DefaultValue = 1;

    public QuadTree(Vector2 pos, float size, float value, int treeHeight, QuadTree root = null, int depth = 0) {
        Position = pos;
        Size = size;
        Root = root;
        TreeHeight = treeHeight;
        Depth = depth;
        Value = value;
    }

    public void SetValue(Vector2 pos, float value, QuadTree root = null, int depth = 0) {
        // reached leaf
        if (depth >= root.TreeHeight) {
            Value = value;
            return;
        }

        float x = pos.x;
        float y = pos.y;

        // split
        float hs = this.Size * 0.5f; // half size
        if (SubTrees == null) {
            SubTrees = new QuadTree[4];
            Vector2 newPos0 = this.Position + new Vector2(hs, hs);
            Vector2 newPos1 = this.Position + new Vector2(0, hs);
            Vector2 newPos2 = this.Position + new Vector2(0, 0);
            Vector2 newPos3 = this.Position + new Vector2(hs, 0);
            SubTrees[0] = new QuadTree(newPos0, hs, DefaultValue, TreeHeight - 1, root, depth + 1);
            SubTrees[1] = new QuadTree(newPos1, hs, DefaultValue, TreeHeight - 1, root, depth + 1);
            SubTrees[2] = new QuadTree(newPos2, hs, DefaultValue, TreeHeight - 1, root, depth + 1);
            SubTrees[3] = new QuadTree(newPos3, hs, DefaultValue, TreeHeight - 1, root, depth + 1);
        }

        QuadTree nextSubTree = null;
        float cx = this.Position.x + hs; // center x
        float cy = this.Position.y + hs; // center y
        if (x > cx && y > cy) {
            // quadrant 1
            nextSubTree = SubTrees[0];
        } else if (x <= cx && y > cy) {
            // quadrant 2
            nextSubTree = SubTrees[1];
        } else if (x <= cx && y <= cy) {
            // quadrant 3
            nextSubTree = SubTrees[2];
        } else if (x > cx && y <= cy) {
            // quadrant 4
            nextSubTree = SubTrees[3];
        }
        nextSubTree.SetValue(pos, value, root, depth + 1);
    }

    public float GetValue(Vector2 pos, QuadTree root = null, int depth = 0) {
        // reached leaf
        if (depth >= root.TreeHeight || this.SubTrees == null) {
            return Value;
        }

        QuadTree nextSubTree = null;
        float x = pos.x;
        float y = pos.y;
        float hs = this.Size * 0.5f; // half size
        float cx = this.Position.x + hs; // center x
        float cy = this.Position.y + hs; // center y
        if (x > cx && y > cy) {
            // quadrant 1
            nextSubTree = SubTrees[0];
        } else if (x <= cx && y > cy) {
            // quadrant 2
            nextSubTree = SubTrees[1];
        } else if (x <= cx && y <= cy) {
            // quadrant 3
            nextSubTree = SubTrees[2];
        } else if (x > cx && y <= cy) {
            // quadrant 4
            nextSubTree = SubTrees[3];
        }
        return nextSubTree.GetValue(pos, root, depth + 1);
    }

    public void SetValueCircle(float value, Vector2 center, float radius, QuadTree root = null, int depth = 0) {

        // check collision
        float deltaX = center.x - Mathf.Max(this.Position.x, Mathf.Min(this.Position.x + this.Size, center.x));
        float deltaY = center.y - Mathf.Max(this.Position.y, Mathf.Min(this.Position.y + this.Size, center.y));
        float dist = (deltaX * deltaX + deltaY * deltaY) - (radius * radius);

        // not colliding
        if (dist > 0) {
            return;
        }

        // leaf reached
        if (depth == root.TreeHeight) {
            this.Value = value;
            return;
        }

        // split
        if (SubTrees == null) {
            float hs = this.Size * 0.5f; // half size
            SubTrees = new QuadTree[4];
            Vector2 newPos0 = this.Position + new Vector2(hs, hs);
            Vector2 newPos1 = this.Position + new Vector2(0, hs);
            Vector2 newPos2 = this.Position + new Vector2(0, 0);
            Vector2 newPos3 = this.Position + new Vector2(hs, 0);
            SubTrees[0] = new QuadTree(newPos0, hs, DefaultValue, this.TreeHeight - 1, root, depth + 1);
            SubTrees[1] = new QuadTree(newPos1, hs, DefaultValue, this.TreeHeight - 1, root, depth + 1);
            SubTrees[2] = new QuadTree(newPos2, hs, DefaultValue, this.TreeHeight - 1, root, depth + 1);
            SubTrees[3] = new QuadTree(newPos3, hs, DefaultValue, this.TreeHeight - 1, root, depth + 1);
        }

        // continue checking in lower dimension
        this.SubTrees[0].SetValueCircle(value, center, radius, root, depth + 1);
        this.SubTrees[1].SetValueCircle(value, center, radius, root, depth + 1);
        this.SubTrees[2].SetValueCircle(value, center, radius, root, depth + 1);
        this.SubTrees[3].SetValueCircle(value, center, radius, root, depth + 1);
    }
}
