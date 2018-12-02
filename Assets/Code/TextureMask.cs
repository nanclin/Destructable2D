using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureMask : MonoBehaviour {

    public float Radius = 1;
    public Renderer Renderer;
    [Range(0,5)] public float Power = 2;

    private static int Width = 100;
    private static int Height = 100;
    private float[,] Map = new float[Height, Width];
    private Texture2D Texture;
    private Color[] ColorArray;


	// Use this for initialization
	void Start () {

        Texture = Renderer.material.mainTexture as Texture2D;;
        Renderer.material.mainTexture = Texture;
        Width = Texture.width;
        Height = Texture.height;
        Map = new float[Height, Width];
        ColorArray = new Color[Width * Height];

        // init debug texture
        //DebugTexture = new Texture2D(Width, Height);
        //DebugTexture.filterMode = FilterMode.Point;
        //Renderer.material.mainTexture = DebugTexture;
        //ColorArray = new Color[Width * Height];

        // init map with random noise
        for (int r = 0; r < Height; r++)
        {
            for (int c = 0; c < Width; c++)
            {
                //Map[r, c] = Random.value;
                Map[r, c] = Texture.GetPixel(c, r).a;

                // write map to the texture
                int i = r * Width + c;
                ColorArray[i] = Texture.GetPixel(c, r);//Color.Lerp(new Color(0,0,0,0), Color.white, Map[r, c]);
            }
        }

	}
	
	// Update is called once per frame
	void Update () {


        // apply brush
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            ApplyBrush(mousePos);
        }


        // 
        for (int r = 0; r < Height; r++)
        {
            for (int c = 0; c < Width; c++)
            {
                // write map to the texture
                int i = r * Width + c;
                ColorArray[i].a = Map[r, c];//Color.Lerp(new Color(0, 0, 0, 0), Color.white, Map[r, c]);
            }
        }

        // apply and upload pixels to the texture
        Texture.SetPixels(ColorArray);
        Texture.Apply();
	}


    void ApplyBrush(Vector2 pos)
    {
        for (int r = 0; r < Height; r++)
        {
            for (int c = 0; c < Width; c++)
            {


                Vector2 coord = new Vector2(c / (float)Width, r / (float)Height);
                coord *= (Vector2)transform.localScale;
                coord -= new Vector2(0.5f, 0.5f) * (Vector2)transform.localScale;
                coord += (Vector2)transform.position;
                float dist = (coord - pos).magnitude;


                if (dist > Radius) continue;

                //// fade brush
                //float distNormalized = 1 - dist / Radius;
                //Map[r, c] += Input.GetKey(KeyCode.LeftShift) ? distNormalized : -distNormalized;
                //Map[r, c] = Mathf.Clamp01(Map[r, c]);


                //// solid brush
                //Map[r, c] = Input.GetKey(KeyCode.LeftShift) ? 1 : 0;

                // spray brush
                float distNormalized = Mathf.Pow(1 - dist / Radius, Power);
                float stamp = Random.value * distNormalized;
                Map[r, c] += Input.GetKey(KeyCode.LeftShift) ? 1 : -stamp;
                Map[r, c] = Mathf.Clamp01(Map[r, c]);
            }
        }
    }


    void OnDrawGizmos()
    { // draw brush debug
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(mousePos, Radius);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        ApplyBrush(col.contacts[0].point);
        Destroy(col.gameObject);
    }
}
