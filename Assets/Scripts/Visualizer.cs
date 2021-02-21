using System;
using System.Collections.Generic;
using UnityEngine;

public class Visualizer : ScriptableObject
{
    private GameObject PointGeomPrefab;      // TODO srialized fields not working for scriptable obj, find another way for setting geometry in editor?
    private GameObject[,,] points;

    private GameObject texGO; //= GameObject.CreatePrimitive(PrimitiveType.Plane);
    private GameObject[] tubes; //= GameObject.CreatePrimitive(PrimitiveType.Plane);
    //private int texSize = 256;
    //Texture3D Tex3D = new Texture3D(256, 256, 256, TextureFormat.RGBA32, true);
    //float[,] Tex ;

    private static Color panelColor = new Color(1, 0.9F, 0.65F, 0.1F);

    private Specs.VIZ_MODE LastVizMode = Specs.ThisVizMode;

    public void Visualize(SensedObject obj, DateTime t)
    {
        obj.Interpolate(t);                                             // TODO interpolates only if needed (not in the )

        //obj.GO.GetComponent<Renderer>().enabled = false;

        if (LastVizMode != Specs.ThisVizMode) // change the visualization mode
            DestroyOtherViz(Specs.ThisVizMode);

        switch (Specs.ThisVizMode)
        {
            case Specs.VIZ_MODE.TEXTURE:
            case Specs.VIZ_MODE.TEXTURE_PAINT:
                SetTexture2D(obj, 0);
                obj.ObjectGO.GetComponent<Renderer>().material = Instantiate<Material>(Specs.TransparenMat);
                obj.ObjectGO.GetComponent<Renderer>().material.color = panelColor;
                //obj.GO.GetComponent<Renderer>().enabled = false;
                break;
            case Specs.VIZ_MODE.PARTICLE_COLOR:
                DrawColoredParticles(obj);
                obj.ObjectGO.GetComponent<Renderer>().material = Instantiate<Material>(Specs.TransparenMat);
                obj.ObjectGO.GetComponent<Renderer>().material.color = panelColor;
                break;
            case Specs.VIZ_MODE.PARTICLE_RADIUS:
                DrawSizedParticles(obj);
                obj.ObjectGO.GetComponent<Renderer>().material = Instantiate<Material>(Specs.TransparenMat);
                obj.ObjectGO.GetComponent<Renderer>().material.color = panelColor;
                //CreatParticles(obj.GO);
                break;
            case Specs.VIZ_MODE.TUBES:
                DrawTubes(obj);
                break;
                //case Specs.VIZ_MODE.PARTICLE_LOOSE:
                //    Destroy(texGO);
                //    DestroyPoints();
                //    CreatParticles(obj.GO);
                //    obj.GO.GetComponent<Renderer>().material = Instantiate<Material>(Specs.TransparenMat);
                //    obj.GO.GetComponent<Renderer>().material.color = panelColor;
                //     break;
        }

        //DrawTubes(obj);

        //// write interpolated values to file (for debugging) 
        //Utilities.WriteMatrix2File(Utilities.Slicer(obj.InterpolatedValues, 0), "interp/" + Time.frameCount + ".txt"); 

        //// Setting the texture of the same geometry (wall) doesn't work due to irregular messhing
        //if (texGO.GetComponent<Renderer>().material.mainTexture == null)
        //    texGO.GetComponent<Renderer>().material.SetTexture("_MainTex", Tex);
        //else
        //    texGO.GetComponent<Renderer>().material.mainTexture = tex;
        //SetTexture2D((Texture2D)obj.GO.GetComponent<Renderer>().material.mainTexture, obj.InterpolatedValues, 0);        

    }
    void DrawTubes(SensedObject obj)
    {
        //int[] tIndx = obj.FindAllTimeStamps(t);
        //if (obj.TimeIndex != null)
        if (obj.DataAvailable)
        {
            if (tubes == null)
            {
                tubes = new GameObject[obj.Sensors.Length];
                //for (int s = 0; s < obj.Sensors.Length; s++)
                //{
                //    //    tubes[s] = new GameObject("TubeMesh");
                //    //tubes[s].AddComponent<MeshFilter>();
                //    //tubes[s].AddComponent<MeshRenderer>();
                //}
            }

            //List<float> MCPerLayer = new List<float>();
            float[] MCPerLayer = new float[obj.LayerCount];
            for (int s = 0; s < obj.Sensors.Length; s++)
            {
                for (int i=0; i<obj.LayerCount; i++)
                    MCPerLayer[i] = UnityEngine.Random.Range(0.4F* Specs.MoistureUpperBound, 0.8F*Specs.MoistureUpperBound);

                //DrawOneTube(MCPerLayer.ToArray());
                if (tubes[s] == null)
                {
                    tubes[s] = new GameObject("TubeMesh");
                    tubes[s].AddComponent<MeshFilter>();
                    tubes[s].AddComponent<MeshRenderer>();
                }

                DrawOneTube(tubes[s], MCPerLayer);
                tubes[s].transform.localRotation = Quaternion.Euler(-90, 0, 0);
                tubes[s].transform.position = new Vector3(obj.Sensors[s].Pos.x, obj.Sensors[s].Pos.y, obj.ParentGO.transform.position.z);

                //tubes[s].transform.localScale = Vector3.one * obj.ObjectGO.GetComponent<Renderer>().bounds.size.z;
                tubes[s].transform.localScale = Vector3.one * 0.003F;
                tubes[s].transform.SetParent(obj.Sensors[s].GO.transform);
                obj.Sensors[s].GO.GetComponent<Renderer>().enabled = false;

                //if (Vector3.Distance(obj.Sensors[0].Pos, obj.Sensors[s].Pos) < 0.1)
                //{
                //    MCPerLayer.Add(obj.Sensors[s].Values[obj.TimeIndex[s]]);
                //}
            }
            //Camera cam = Camera.main;
            //tubeGO.transform.rotation = Quaternion.LookRotation(cam.transform.right) * Quaternion.Euler(-90, 0, 0);
            //tubeGO.transform.position = cam.transform.position + cam.transform.forward * 1.1F + cam.transform.right * 0.75F;

        }
    }
    void DrawOneTube(GameObject tubeGO, float[] vals)
    {
        int latCount = vals.Length;
        int lonCount = 20;
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();

        Vector3[] verts = MakeTube(vals, 0, 0, 0, latCount, lonCount);
        for (int i = 0; i < verts.Length; i++)
            vertices.Add(verts[i]);

        Material tmpMat = tubeGO.GetComponent<Renderer>().material;

        Destroy(tubeGO.GetComponent<Mesh>());

        Mesh mesh = new Mesh{ subMeshCount = latCount - 1};
        mesh.SetVertices(vertices);
        Material[] mats = new Material[mesh.subMeshCount];
        // Create the triangles by adding the indices of the vertices to the list (3 at a time)
        for (int i = 0; i < vals.Length - 1; i++)
        {
            indices.Clear();
            for (int j = 0; j < verts.Length / vals.Length; j++)
            {   // each loop is a quad -> 2 triabgles
                indices.Add(PointIndex(i, j, latCount, lonCount));
                indices.Add(PointIndex(i, j + 1, latCount, lonCount));
                indices.Add(PointIndex(i + 1, j + 1, latCount, lonCount));

                indices.Add(PointIndex(i, j, latCount, lonCount));
                indices.Add(PointIndex(i + 1, j + 1, latCount, lonCount));
                indices.Add(PointIndex(i + 1, j, latCount, lonCount));

            }
            mesh.SetTriangles(indices, i);
            //mats[i].color = Utilities.ThreeColorLerp(vals[i], Specs.MoistureLowerBound, Specs.MoistureUpperBound, new Color(0F, 0F, 1F), new Color(1F, 1F, 0F), new Color(1F, 0F, 0F));
            mats[i] = new Material(tmpMat);
            mats[i].color = Color.Lerp( new Color(1F, 1F, 0F),new Color(0F, 0F, 1F),vals[i+1]/ 15 );
            //Debug.Log(vals[i]);
            //Debug.Log(mats[i].color);
        }
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        tubeGO.GetComponent<Renderer>().materials = mats;
        tubeGO.GetComponent<MeshFilter>().mesh = mesh;
    }
    // create the mesh for a tube
    Vector3[] MakeTube(float[] vals,  float x, float y, float z, int latCount, int lonCount)
    {
        float[] radii = new float[latCount];
        //float[] perLayerMC = new float[latCount];
        int indx;
        Vector3[] p = new Vector3[latCount * lonCount];

        for (int i = 0; i < latCount; i++)
        {
            radii[i] = Utilities.Map(vals[i], Specs.MoistureLowerBound, Specs.MoistureUpperBound, 0,1);
            //Debug.Log(radii[i]);
        }
        // compute the coordinate of the mesh
        for (int i = 0; i < latCount; i++)
        {
            for (int j = 0; j <= lonCount; j++)
            {
                indx = PointIndex(i, j, latCount, lonCount);
                float azi = (float)j / lonCount * 2 * Mathf.PI;
                p[indx].x = radii[i] * Mathf.Cos(azi) + x;
                p[indx].y = Utilities.Map((float)i, 0, latCount,-0.5F,0.5F);
                p[indx].z = radii[i] * Mathf.Sin(azi) + z;
            }
        }
        return p;
    }

    // returns the index of a vertex depending on latitude and logitude
    int PointIndex(int lat, int lon, int latCount, int lonCount)
    {
        if (lat < 0) lat += (latCount - 1);
        if (lon < 0) lon += (lonCount - 1);
        if (lat > latCount - 1) lat -= (lat - 1);
        if (lon > lonCount - 1) lon -= (lon - 1);
        return lonCount * lat + lon;
    }

    void SetTexture2D(SensedObject obj, int depth)
    {
        float[,,] vals = obj.InterpolatedValues;
        int panelW = vals.GetLength(1);
        int panelH = vals.GetLength(0);
        int i, j;
        if (texGO == null)
            InitTexture(obj);
        Texture2D tex = (Texture2D)texGO.GetComponent<Renderer>().material.mainTexture;
        Color[] colorArray = new Color[tex.width * tex.height];
        float alpha = (Specs.ThisVizMode == Specs.VIZ_MODE.TEXTURE_PAINT ? 0 : 1);
        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                j = (int)Math.Floor(Utilities.Map(x, 0, tex.width, 0, panelW));
                i = (int)Math.Floor(Utilities.Map(y, 0, tex.height, 0, panelH));

                //Color c = Color.Lerp(new Color(1F, 1F, 0F), new Color(0F, 1F, 0F), Utilities.Map(vals[i, j , depth], 0F, 30F, 0F, 1F));
                Color c = Utilities.ThreeColorLerp(vals[i,j,depth], Specs.MoistureLowerBound, Specs.MoistureUpperBound, new Color(0F, 0F, 1F, alpha), new Color(1F, 1F, 0F, alpha), new Color(1F, 0F, 0F, alpha));
                //c.a = (Specs.ThisVizMode == Specs.VIZ_MODE.TEXTURE_PAINT ? colorArray[x + (y * tex.width)].a : 1); 
                colorArray[x + (y * tex.width)] = c;

            }
        }
        tex.SetPixels(colorArray);

        //if (Specs.ThisVizMode == Specs.VIZ_MODE.TEXTURE_PAINT)
        //{
        //    VertexPaint painter = texGO.GetComponent<VertexPaint>();
        //    if (painter == null)
        //        painter = texGO.AddComponent<VertexPaint>();
        //    //painter.Paint();
            
        //}
        //colorArray[0] = Color.black;
        //tex.SetPixels(colorArray);

        tex.Apply();
    }


    //void CreatParticlesInObject(GameObject go)
     //// There is a problem with setting the colors of points in the run time (bug?) when using material all particles have the same color, when using start color, color is not applied  
   //{

    //    ParticleSystem prtclSys;
    //    if (!(prtclSys = go.GetComponent<ParticleSystem>()))
    //    {
    //        prtclSys = go.AddComponent<ParticleSystem>();
    //        //go.GetComponent<ParticleSystemRenderer>().material = new Material(Shader.Find("Diffuse"));
    //    }


    //    var shp = prtclSys.shape;
    //    shp.enabled = true;
    //    shp.shapeType = ParticleSystemShapeType.Box;
    //    shp.position = go.GetComponent<Renderer>().bounds.center - go.transform.position;
    //    shp.scale = go.GetComponent<Renderer>().bounds.size;

    //    var main = prtclSys.main;
    //    main.startSpeed = 0F;
    //    main.startSize = 0.05F;
    //    main.startLifetime = 1F;
    //    main.prewarm = true;
    //    Material m = go.GetComponent<ParticleSystemRenderer>().material;
    //    m.color = Color.red;
        
    //    //main.startColor = Color.red;
    //    //go.GetComponent<ParticleSystemRenderer>().material = null;


    //    var emssn = prtclSys.emission;
    //    emssn.rateOverTime = 200F;

    //    //var rndr = prtclSys.GetComponent<Renderer>();

    //    int count = prtclSys.particleCount;
    //    ParticleSystem.Particle[] prtcls = new ParticleSystem.Particle[count];
    //    count = prtclSys.GetParticles(prtcls);

    //    main.startColor = new Color(0, 0, 1);

    //    for (int i = 0; i < count; i++)
    //    {
    //        prtcls[i].startSize = 5 * prtcls[i].startSize;

    //    }

    //    //Debug.Log(go.transform.position - go.GetComponent<Renderer>().bounds.center);

    //}
    /// 
    //void SetTexture3D(float[,,] vals)
    //{

    //    Color[] colorArray = new Color[Tex.width * Tex.height * Tex.depth];
    //    //Texture3D texture = new Texture3D(size, size, size, TextureFormat.RGBA32, true);
    //    float r = 1.0f / (Tex.width - 1.0f);
    //    for (int x = 0; x < Tex.width; x++)
    //    {
    //        for (int y = 0; y < Tex.height; y++)
    //        {
    //            for (int z = 0; z < Tex.depth; z++)
    //            {
    //                Color c = new Color(x * r, y * r, z * r, 1.0f);
    //                colorArray[x + (y * Tex.width) + (z * Tex.width * Tex.height)] = c;
    //            }
    //        }
    //    }
    //    Tex.SetPixels(colorArray);
    //    Tex.Apply();
    //    //return texture;
    //}
    public void InitTexture(SensedObject obj)
    {
        //texGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
        //texGO = GameObject.CreatePrimitive(PrimitiveType.Plane);

        Bounds b = obj.ObjectGO.GetComponent<Renderer>().bounds;

        texGO = Utilities.CreatePlane(b.size.x, b.size.y, obj.InterpolatedValues.GetLength(2), obj.InterpolatedValues.GetLength(1), "TexturePlane");
        //texGO = Utilities.CreatePlane(1,2 , 2, 6, "TexturePlane");

        //texGO.name = "TexturePlane";

        // Put the plane in front of wall (this only works with this iorientation of wall?)  // TODO find a better way ?
        texGO.transform.position = b.center - obj.SurfaceNormal * b.extents.z;

        //texGO.transform.localScale = b.size;
        //texGO.transform.Rotate(new Vector3(0, 90, -90));
        //texGO.GetComponent<Renderer>().bounds.SetMinMax(b.min, b.max);
        texGO.transform.SetParent(obj.ParentGO.transform);
 
        // make sure texture dimensions are power of 2 and close to dimensions of interpolated values
        int texW = (int)Mathf.Max(Mathf.Pow(2, Mathf.Round(Mathf.Log(obj.InterpolatedValues.GetLength(1), 2))), 4);
        int texH = (int)Mathf.Max(Mathf.Pow(2, Mathf.Round(Mathf.Log(obj.InterpolatedValues.GetLength(0), 2))), 4);
        texGO.GetComponent<Renderer>().material = Instantiate<Material>(Specs.TransparenMat);
        texGO.GetComponent<Renderer>().material.SetColor("_Color",Color.white); // Make sure the alpha is set to 1
        texGO.GetComponent<Renderer>().material.SetTexture("_MainTex", new Texture2D(texW, texH));

        if (Specs.ThisVizMode == Specs.VIZ_MODE.TEXTURE_PAINT)
            texGO.AddComponent<VertexPaint>();

        //obj.GO.GetComponent<Renderer>().material = new Material(texGO.GetComponent<Renderer>().material);
        //obj.GO.GetComponent<Renderer>().material.SetTexture("_MainTex", new Texture2D(256,256));
        // remove collider
    }

    private void InitPoints(SensedObject obj)
    {
        float[,,] interpVals = obj.InterpolatedValues;
        int w = interpVals.GetLength(1);
        int h = interpVals.GetLength(0);
        int d = interpVals.GetLength(2);
                                                             d = 1;  // TODO remove if points have to be scatters in the whole volume
        //float xPos, yPos, zPos;
        points = new GameObject[h, w, d];

        LoadPointGeometry();
        float pointScale = obj.PanelGridStep/PointGeomPrefab.GetComponent<Renderer>().bounds.size.x ;

        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                //for (int k = 0; k < d; k++)
                for (int k = 0; k < d; k++)
                {
                    points[i, j, k] = Instantiate<GameObject>(PointGeomPrefab);
                    //points[i, j, k] = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    points[i, j, k].transform.localScale = Vector3.one * pointScale;
                    points[i, j, k].transform.position = obj.GetGridPosition(i, j, k);
                    points[i, j, k].transform.parent = obj.ParentGO.transform;

                    //points[i, j, k].SetActive(false);
               }
            }
        }
    }

    private void DestroyOtherViz(Specs.VIZ_MODE thisViz)
    {
        if (thisViz != Specs.VIZ_MODE.PARTICLE_COLOR && thisViz != Specs.VIZ_MODE.PARTICLE_RADIUS)
            DestroyPoints();

        if (thisViz != Specs.VIZ_MODE.TEXTURE)
            Destroy(texGO);

        if (thisViz != Specs.VIZ_MODE.TEXTURE_PAINT)
            Destroy(texGO);

        if (thisViz != Specs.VIZ_MODE.TUBES)
            DestroyTubes();

        LastVizMode = thisViz;
    }

    // Destroy all the points
    private void DestroyPoints()
    {
        if (points == null)
            return;
        for (int i = 0; i < points.GetLength(0); i++)
            for (int j = 0; j < points.GetLength(1); j++)
                for (int k = 0; k < points.GetLength(2); k++)
                    Destroy(points[i, j, k]);
        points = null;
    }

    private void DestroyTubes()
    {
        if (tubes == null)
            return;
        for (int i = 0; i < tubes.Length; i++)
        {
            //tubes[i].GetComponentInParent<Renderer>().enabled = true; // make sensor geometry visible again
            tubes[i].transform.parent.gameObject.GetComponentInParent<Renderer>().enabled = true; // make sensor geometry visible again
            Destroy(tubes[i]);
        }
        tubes = null;
    }
    
    void DrawSizedParticles(SensedObject obj)
    {
        if (points == null)
            InitPoints(obj);

        int w = points.GetLength(1);
        int h = points.GetLength(0);
        int d = points.GetLength(2);

        float pointScale;
        //float tempWeight = 60F;
        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                for (int k = 0; k < d; k++)
                {
                    pointScale = Utilities.Map(obj.InterpolatedValues[i, j, k], Specs.MoistureLowerBound, Specs.MoistureUpperBound, obj.PanelGridStep / PointGeomPrefab.GetComponent<Renderer>().bounds.size.x * 2.5F, 0);
                    //pointScale = Mathf.Lerp(interpVals[i,j,k]/50, 0F, panelGridStep);
                    //Debug.Log(pointScale);
                    points[i, j, k].transform.parent = null;

                    points[i, j, k].transform.localScale = Vector3.one * pointScale;
                    points[i, j, k].transform.parent = obj.ParentGO.transform;
                    points[i, j, k].GetComponent<Renderer>().material.color = Color.yellow;
                }
            }
        }
    }

    private void DrawColoredParticles(SensedObject obj)
    {
        float[,,] interpVals = obj.InterpolatedValues;

        if (points == null)
            InitPoints(obj);

        int w = points.GetLength(1);
        int h = points.GetLength(0);
        int d = points.GetLength(2);

        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                for (int k = 0; k < d; k++)
                {
                    //if (!points[i, j, k].activeSelf)
                    //    points[i, j, k].SetActive(true);
                    points[i, j, k].GetComponent<Renderer>().material.color = Utilities.ThreeColorLerp(interpVals[i, j, k], Specs.MoistureLowerBound, Specs.MoistureUpperBound, new Color(0F, 0F, 1F), new Color(1F, 1F, 0F), new Color(1F, 0F, 0F));
                }
            }
        }
    }
    private void LoadPointGeometry()
    {                                         
        if (PointGeomPrefab == null)
            PointGeomPrefab = Specs.LowPolySpherePrefab;
    }
}
