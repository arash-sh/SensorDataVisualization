using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexPaint : MonoBehaviour {

    //private Mesh Msh;
    private Collider Col;
    private float[] AlphaMask;
    private Texture2D Tex;
    private int[] Triangles;
    private List<Vector2> UVs = new List<Vector2>();
    //LineRenderer Ln;

    GameObject BrushGO; // gameobject indicating the brush

    void Awake() {
        
        if ((Col = GetComponent<MeshCollider>())== null) // triangle index only works with mesh colliders?
            Col = gameObject.AddComponent<MeshCollider>();

        // Draw the ray
        //if ((Ln = gameObject.GetComponent<LineRenderer>())== null)
        //    Ln = gameObject.AddComponent<LineRenderer>();
        //Ln.positionCount = 2;
        //Vector3[] points = new Vector3[2] {Vector3.zero,Vector3.zero };
        //Ln.SetPositions(points);
        //Ln.material.color = Color.red;

        BrushGO = CreatBrush();

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Triangles = mesh.triangles;
        mesh.GetUVs(0, UVs);


        if ((Tex = (Texture2D)GetComponent<Renderer>().material.mainTexture) == null)
            return;        

        AlphaMask = new float[Tex.width* Tex.height];

    }

    //public void Paint(Color[] pixels) {  // not calling it from SetTexture2D, cause it is not executed at each frame 
    void Update()
    {
        int texW = Tex.width, texH= Tex.height; // pixel dimensions of the texture
        Color[] pixels = Tex.GetPixels();
        float rayLen = Utilities.Scaled(1000); // the maximum length of the ray for painting
        RaycastHit hitInfo;
        Ray CamRay = Camera.main.ScreenPointToRay(Input.mousePosition); ;

        if (Col.Raycast(CamRay, out hitInfo, rayLen) && Input.GetMouseButton(1)) // hit the object
        {

            BrushGO.transform.position = hitInfo.point;
            BrushGO.SetActive(true);

            Vector3 bary = hitInfo.barycentricCoordinate;
            Vector2 uvHit =  bary[0] * UVs[Triangles[hitInfo.triangleIndex * 3 + 0]] + bary[1] * UVs[Triangles[hitInfo.triangleIndex * 3 + 1]] + bary[2] * UVs[Triangles[hitInfo.triangleIndex * 3 + 2]]; 

            PaintPoint(pixels, uvHit, 10, texW, texH);//(int)Mathf.Round(Mathf.Min(texW, texH)*0.1F)


            //uv1[triangles[hitInfo.triangleIndex * 3 + 0]] = Vector2.zero;
            //uv1[triangles[hitInfo.triangleIndex * 3 + 1]] = Vector2.zero;
            //uv1[triangles[hitInfo.triangleIndex * 3 + 2]] = Vector2.zero;
            //Msh.SetUVs(0, uv1);



            //// store the texture as image for debugging 
            //System.IO.File.WriteAllBytes("test.png", tex.EncodeToPNG());
            //Debug.Break();
        }
        else // no hit with ray
        {
            BrushGO.SetActive(false);
        }

        for (int i = 0; i < pixels.Length; i++)
            if (AlphaMask[i] > 0)
                pixels[i].a = AlphaMask[i];
        Tex.SetPixels(pixels);
        Tex.Apply();
        //return pixels;

        //Ln.SetPositions(points);
    }

    private void PaintPoint(Color[] pixels, Vector2 uv, int radius , int w, int h)
    {
        //Debug.Log(uv);
        int x = (int)Utilities.Map(uv[0], 0, 1, 0, (float)w);
        int y = (int)Utilities.Map(uv[1], 0, 1, 0, (float)h);
        float dist, strength;
        //int radius = 10; // radius in pixel around the selected pixel
        for (int i = (int) Mathf.Max(x-radius,0); i<(int)Mathf.Min(x+radius,w) ; i++)
        {
            dist = Mathf.Abs(x - i);
            for (int j = (int) Mathf.Max(y-radius+dist,0); j<(int)Mathf.Min(y+radius-dist,h) ; j++)
            {
                dist = Mathf.Sqrt(Mathf.Pow(x-i,2) + Mathf.Pow(y - j, 2));
                strength = Utilities.Map(radius - dist, 0, 0.9F*radius, 0.1F, 1);
                //Debug.Log("i = " + i + "   j = " + j);
                AlphaMask[i + (j * w)] = Mathf.Min(strength + AlphaMask[i + (j * w)], 1);
                //pixels[i + (j * w)].a = AlphaMask[i + (j * w)];
            }

        }


    }

    GameObject CreatBrush() {
        GameObject GO = GameObject.CreatePrimitive(PrimitiveType.Sphere);//Instantiate<GameObject> (Specs.LowPolySpherePrefab);
        GO.name = "Brush";
        GO.GetComponent<Renderer>().material = Instantiate<Material>(Specs.TransparenMat);
        GO.GetComponent<Renderer>().material.SetColor("_Color", new Color(1, 1, 1, 0.5F));
        GO.transform.localScale = Vector3.one * 0.2F;
        GO.transform.SetParent(this.transform);

        Collider coll = GO.GetComponent<Collider>();
        if (coll != null)   // remove collider to keep the object clickable
            Destroy(coll);

        return GO;
    }
    //private int UV2Pixel(Vector2 uv, int w, int h)
    //{
    //    int x, y;
    //    x = (int) Utilities.Map(uv[0], 0, 1, 0, (float)w); 
    //    y = (int) Utilities.Map(uv[1], 0, 1, 0, (float)h);
    //    Debug.Log(uv.ToString() + " -> " + x + " / " + w + "     ,    " + y + " / " + h + "       ->"+ (x + (y * w)));
    //    return x + (y * w);
    //}

}
