//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class TubesScript : MonoBehaviour
//{
//    float panelMinX;
//    float panelMaxX;
//    float panelMinY;
//    float panelMaxY;
//    float panelMinZ;
//    float panelMaxZ;

//    float panelGridStep;

//    int layerCount;

//    int frame = 0;

//    float[,] SensPosA;
//    float[,] SensPosB;
//    float[,] SensPosC;

//    float[,,] interpVals;

//    int[] MCColumnPerLayerPosA;
//    int[] MCColumnPerLayerPosB;
//    int[] MCColumnPerLayerPosC;
//    int[] PanelTempColumn;

//    [SerializeField]
//    private GameObject DataObjectPrefab;

//    private DataScript SensorData;


//    private enum MARCHING_MODE { CUBES, TETRAHEDRON };

//    public Material m_material;

//    private MARCHING_MODE mode = MARCHING_MODE.CUBES;

//    //public int seed = 0;

//    //List<GameObject> meshes = new List<GameObject>();


//    void Start()
//    {
//        Setup();
//    }

//    private void Update()
//    {
//        float timeScale = 5;
//        if (Time.frameCount % timeScale == 0)
//        {
//            if (frame < SensorData.Rows)
//            {
//                Debug.Log(frame + ": " + SensorData.DataTime(frame));
//                InterpolateAllLayers(frame);
//                DrawIsoSurface();
//            }
//            frame++;
//        }
//    }


//    void DrawOneTube(float[,] sensorPos, int[] MCColumnPerLayer)
//    {

//        Mesh mesh;

//        int latCount = 10, lonCount = 20, index;


//        float[,] verts = makeTube(0, 0, -0, 300, latCount, lonCount, t);

//        float layerMC;
//        foreach (Transform child in transform)
//        {
//            Destroy(child.gameObject);
//        }

//        List<Vector3> vertices = new List<Vector3>();
//        List<int> indices = new List<int>();


//        for (int layer = 0; layer < interpVals.GetLength(2); layer++) {


//            layerMC = interpolatePoint2Layer(SensorData, t, sensorPos, MCColumnPerLayer, layer);

//            m_material.color = Color.Lerp(new Color(1F, 0F, 0F, 0.1F), new Color(0F, 0F, 1F, 0.1F), Map(layerMC, 0F, 30F, 0F, 1F));




//            mesh = new Mesh();
//            mesh.SetVertices(TransformVerts(vertices));
//            mesh.SetTriangles(indices, 0);
//            mesh.RecalculateBounds();
//            mesh.RecalculateNormals();

//            GameObject go = new GameObject("Mesh");
//            go.transform.parent = transform;
//            go.AddComponent<MeshFilter>();
//            go.AddComponent<MeshRenderer>();
//            go.GetComponent<Renderer>().material = new Material(m_material);
//            go.GetComponent<MeshFilter>().mesh = mesh;
//            //go.transform.localPosition = new Vector3(-width / 2, -height / 2, -depth / 2);        

//        }
//    }
//    Vector3[] MakeTube(float x, float y, float z, float h, int latCount, int lonCount, int t, float[,] MCPos, int[] MCColumnPerLayer)
//    {

//        float[] radii = new float[latCount];
//        float[] perLayerMC = new float[layerCount];

//        int indx; 
//        Vector3[] p = new Vector3[latCount * lonCount];

//        float[] posALayerRadii = new float[layerCount];
//        float[] posBLayerRadii = new float[layerCount];
//        float[] posCLayerRadii = new float[layerCount];

//        for (int i = 0; i < layerCount; i++)
//            perLayerMC[i] = interpolatePoint2Layer(SensorData, t, MCPos, MCColumnPerLayer, i, layerCount);



//        // Compute the radius for each row 
//        int lay;
//        float latDist, layDist;
//        //Arrays.fill(posARadii, 50);
//        for (int i = 0; i < latCount; i++)
//        {
//            lay = i % (int)Mathf.Floor(latCount / (layerCount - 1));
//            latDist = i - (lay * floor(latCount / (layerCount - 1)));
//            radii[i] = posALayerRadii[lay] * (1 - (latDist / layDist)) + posALayerRadii[lay + 1] * (latDist / layDist);
//        }

//        // compute the coordinate of the mesh
//        for (int i = 0; i < latCount; i++)
//        {
//            for (int j = 0; j <= lonCount; j++)
//            {
//                indx = pointIndex(i, j, latCount, lonCount);
//                float azi = (float)j / lonCount * 2. * PI;
//                p[indx].x = radii[i] * cos(azi) + x;
//                p[indx].y = -(float)i / latCount * h + y;
//                p[indx].z = radii[i] * sin(azi) + z;

//                //pat->p[indx].nx = cos(azi) / sqrt(2.);
//                //pat->p[indx].ny = -1 / sqrt(2);
//                //pat->p[indx].nz = sin(azi) / sqrt(2.);
//                //normalize(&pat->p[indx].nx, &pat->p[indx].ny, &pat->p[indx].nz);

//                //if (i <= pat->glyphT4Row) {
//                //  pat->p[indx].setRGBr(0., 1., 0.);
//                //  pat->p->setMaterial(BaseDiffuse);
//                //}
//                //else if (i <= pat->glyphT3Row) {
//                //  pat->p[indx].setRGBr(0., 0., 1.);
//                //  pat->p->setMaterial(T3T4Diffuse);
//                //}
//                //else if (i <= pat->glyphT2Row) {
//                //  pat->p[indx].setRGBr(1., 1., 0.);
//                //  pat->p->setMaterial(T2T3Diffuse);
//                //}
//                //else {
//                //  pat->p[indx].setRGBr(1., 0., 0.);
//                //  pat->p->setMaterial(T1T2Diffuse);
//                //}
//            }
//        }
//        return p;
//    }

//    int pointIndex(int lat, int lon, int latCount, int lonCount)
//    {
//        if (lat < 0) lat += (latCount - 1);
//        if (lon < 0) lon += (lonCount - 1);
//        if (lat > latCount - 1) lat -= (lat - 1);
//        if (lon > lonCount - 1) lon -= (lon - 1);
//        return lonCount * lat + lon;
//    }


//    void DrawIsoSurface()
//    {
//        float[] levels = new float[] { 5, 10, 20, 30, 40, 50 };

//        foreach (Transform child in transform)
//        {
//            Destroy(child.gameObject);
//        }
//        //Set the mode used to create the mesh.
//        //Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface.
//        Marching marching = null;
//        if (mode == MARCHING_MODE.TETRAHEDRON)
//            marching = new MarchingTertrahedron();
//        else
//            marching = new MarchingCubes();

//        //Surface is the value that represents the surface of mesh
//        //For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
//        //The target value does not have to be the mid point it can be any value with in the range.
//        for (int l = 0; l < levels.Length; l++)
//        {
//            marching.Surface = levels[l];
//            m_material.color = Color.Lerp(new Color(1F, 0F, 0F, 0.1F), new Color(0F, 0F, 1F, 0.1F), Map(levels[l], 0F, 30F, 0F, 1F));

//            //The size of voxel array.
//            int width = interpVals.GetLength(1);
//            int height = interpVals.GetLength(0);
//            int depth = interpVals.GetLength(2);

//            float[] voxels = new float[width * height * depth];
//            InterpolateAllLayers(frame);

//            //Fill voxels with values. Im using perlin noise but any method to create voxels will work.
//            for (int x = 0; x < height; x++)
//                for (int y = 0; y < width; y++)
//                    for (int z = 0; z < depth; z++)
//                        voxels[x + y * height + z * width * height] = interpVals[x, y, z]; //Random.Range(-1F, 1F);


//            List<Vector3> verts = new List<Vector3>();
//            List<int> indices = new List<int>();

//            //The mesh produced is not optimal. There is one vert for each index.
//            //Would need to weld vertices for better quality mesh.
//            marching.Generate(voxels, width, height, depth, verts, indices);

//            //A mesh in unity can only be made up of 65000 verts.
//            //Need to split the verts between multiple meshes.

//            int maxVertsPerMesh = 30000; //must be divisible by 3, ie 3 verts == 1 triangle
//            int numMeshes = verts.Count / maxVertsPerMesh + 1;

//            for (int i = 0; i < numMeshes; i++)
//            {
//                List<Vector3> splitVerts = new List<Vector3>();
//                List<int> splitIndices = new List<int>();

//                for (int j = 0; j < maxVertsPerMesh; j++)
//                {
//                    int idx = i * maxVertsPerMesh + j;

//                    if (idx < verts.Count)
//                    {
//                        splitVerts.Add(verts[idx]);
//                        splitIndices.Add(j);
//                    }
//                }

//                if (splitVerts.Count == 0) continue;

//                Mesh mesh = new Mesh();
//                mesh.SetVertices(TransformVerts(splitVerts));
//                mesh.SetTriangles(splitIndices, 0);
//                mesh.RecalculateBounds();
//                mesh.RecalculateNormals();

//                GameObject go = new GameObject("Mesh");
//                go.transform.parent = transform;
//                go.AddComponent<MeshFilter>();
//                go.AddComponent<MeshRenderer>();
//                go.GetComponent<Renderer>().material = new Material(m_material);
//                go.GetComponent<MeshFilter>().mesh = mesh;
//                //go.transform.localPosition = new Vector3(-width / 2, -height / 2, -depth / 2);
//            }
//        }
//    }

//    List<Vector3> TransformVerts(List<Vector3> verts)
//    {
//        //Vector3[] verts = m.vertices;

//        for (int i = 0; i < verts.Count; i++)
//        {
//            verts[i] = GetGridPosition((int)verts[i].x, (int)verts[i].y, (int)verts[i].z);
//        }

//        return verts;
//        //m.vertices = verts;
//        //m.RecalculateBounds();
//        //m.RecalculateNormals();
//    }
//    Vector3 GetGridPosition(int i, int j, int k)
//    {
//        float xPos, yPos, zPos;
//        xPos = panelGridStep / 2 + panelMinX + j * panelGridStep;//Random.Range(panelMinX, panelMaxX);
//        yPos = panelGridStep / 2 + panelMinY + i * panelGridStep;//Random.Range(panelMinX, panelMaxX);
//        zPos = SensPosC[2, k];//Random.Range(panelMinX, panelMaxX);
//        return new Vector3(xPos, yPos, zPos);
//    }

//    float Map(float val, float sMin, float sMax, float dMin, float dMax)
//    {
//        return Mathf.Lerp(dMin, dMax, (val - sMin) / (sMax - sMin));
//    }

//    private void Setup()
//    {

//        //float[] layerZs = new float[] { 0, 5, 10, 15, 20, 25, 30 };

//        panelMinX = -20;
//        panelMaxX = 80;
//        panelMinY = -20;
//        panelMaxY = 120;
//        panelGridStep = 4f;
//        panelMinZ = 0;
//        panelMaxZ = 30;
//        layerCount = 7;

//        SensPosA = new float[,] { { 0, 0, 0, 0, 0, 0, 0},
//                                  { 50, 50, 50, 50, 50, 50, 50},
//                                  { panelMinZ, panelMinZ+(panelMaxZ-panelMinZ)/(layerCount-1), panelMinZ+2*(panelMaxZ-panelMinZ)/(layerCount-1), panelMinZ+3*(panelMaxZ-panelMinZ)/(layerCount-1), panelMinZ+4*(panelMaxZ-panelMinZ)/(layerCount-1), panelMinZ+5*(panelMaxZ-panelMinZ)/(layerCount-1), panelMaxZ} };

//        SensPosB = new float[,] { { 0, 0, 0, 0, 0, 0, 0},
//                                  { 0, 0, 0, 0, 0, 0, 0},
//                                   { panelMinZ, panelMinZ+(panelMaxZ-panelMinZ)/(layerCount-1), panelMinZ+2*(panelMaxZ-panelMinZ)/(layerCount-1), panelMinZ+3*(panelMaxZ-panelMinZ)/(layerCount-1), panelMinZ+4*(panelMaxZ-panelMinZ)/(layerCount-1), panelMinZ+5*(panelMaxZ-panelMinZ)/(layerCount-1), panelMaxZ}};

//        SensPosC = new float[,] { { 20, 20, 20, 20, 20, 20, 20},
//                                  { 50, 50, 50, 50, 50, 50, 50},
//                                  { panelMinZ, panelMinZ+(panelMaxZ-panelMinZ)/(layerCount-1), panelMinZ+2*(panelMaxZ-panelMinZ)/(layerCount-1), panelMinZ+3*(panelMaxZ-panelMinZ)/(layerCount-1), panelMinZ+4*(panelMaxZ-panelMinZ)/(layerCount-1), panelMinZ+5*(panelMaxZ-panelMinZ)/(layerCount-1), panelMaxZ} };


//        transform.localScale = new Vector3(panelMaxX - panelMinX, panelMaxY - panelMinY, panelMaxZ - panelMinZ);
//        //transform.position = new Vector3(-panelMinX, -panelMinY, -panelMinZ);
//        transform.position = new Vector3(0.5F * (panelMaxX + panelMinX), 0.5F * (panelMaxY + panelMinY), 0.5F * (panelMaxZ + panelMinZ));

//        SensorData = Instantiate<GameObject>(DataObjectPrefab).GetComponent<DataScript>();

//        AssignColumns2Layers();

//        int panelW = (int)System.Math.Floor((panelMaxX - panelMinX) / panelGridStep);
//        int panelH = (int)System.Math.Floor((panelMaxY - panelMinY) / panelGridStep);

//        interpVals = new float[panelH, panelW, layerCount];


//    }

//    private void AssignColumns2Layers()
//    {
//        if (SensorData.Path == null)
//        {
//            Debug.Log("Cannot assign measurements to layers due to missing data header");
//            Application.Quit(); // doesn't quit in editor mode
//        }
//        else if (SensorData.Path.Contains("7077"))
//        {
//            MCColumnPerLayerPosA = new int[] { 6, 7, 8, -1, -1, -1, -1 }; // Upper Corner Ply 1 2 3
//            MCColumnPerLayerPosB = new int[] { -1, -1, -1, -1, -1, 5, 9 }; //By Floor Connection Ply 6 7 
//            MCColumnPerLayerPosC = new int[] { -1, -1, -1, 11, -1, -1, -1 }; // Upper By Tendons
//            PanelTempColumn = new int[] { 4, 10 };

//        }
//        else if (SensorData.Path.Contains("9136"))
//        {
//            MCColumnPerLayerPosA = new int[] { 9, 7, -1, -1, -1, 8, 10 }; // Bottom Corner Ply 2 6 1 7
//            MCColumnPerLayerPosB = new int[] { -1, 13, -1, 12, -1, -1, -1 }; // Edge by Hardware Ply 2 4   
//            MCColumnPerLayerPosC = new int[] { -1, -1, -1, -1, 11, -1, -1 }; // Middle Panel Ply 5
//            PanelTempColumn = new int[] { 6 };
//        }
//        else
//        {
//            Debug.Log("Cannot assign measurements to layers due to unknown file");
//            Application.Quit(); // doesn't quit in editor mode
//        }
//    }

//    void InterpolateAllLayers(int t)
//    {
//        for (int i = 0; i < layerCount; i++)
//            InterpolateLayer(t, i);
//    }

//    // interpolate the measurements to layer queryLayer then interpolate over the layer
//    void InterpolateLayer(int t, int queryLayer)
//    {
//        float MCPosA, MCPosB, MCPosC;
//        Vector3 bary, triA, triB, triC, triP;

//        // assign the mesurements to the right locations
//        triA = new Vector3(SensPosA[0, queryLayer], SensPosA[1, queryLayer], SensPosA[2, queryLayer]);
//        triB = new Vector3(SensPosB[0, queryLayer], SensPosB[1, queryLayer], SensPosB[2, queryLayer]);
//        triC = new Vector3(SensPosC[0, queryLayer], SensPosC[1, queryLayer], SensPosC[2, queryLayer]);

//        MCPosA = interpolatePoint2Layer(SensorData, t, SensPosA, MCColumnPerLayerPosA, queryLayer, layerCount);
//        MCPosB = interpolatePoint2Layer(SensorData, t, SensPosB, MCColumnPerLayerPosB, queryLayer, layerCount);
//        MCPosC = interpolatePoint2Layer(SensorData, t, SensPosC, MCColumnPerLayerPosC, queryLayer, layerCount);

//        if (MCPosA == float.NaN || MCPosB == float.NaN || MCPosC == float.NaN)
//        {
//            Debug.Log("skipped " + t);
//            return;
//        }
//        for (int i = 0; i < interpVals.GetLength(0); i++)
//        {
//            for (int j = 0; j < interpVals.GetLength(1); j++)
//            {
//                triP = new Vector3(panelMinX + j * panelGridStep, panelMinY + i * panelGridStep, triA[2]);
//                bary = BarycentricCoordinates(triA, triB, triC, triP);
//                interpVals[i, j, queryLayer] = bary[0] * MCPosA + bary[1] * MCPosB + bary[2] * MCPosC;
//            }
//        }

//    }

//    // interpolate the measurements at a spicific position to queryLayer
//    float interpolatePoint2Layer(DataScript measured, int row, float[,] pos, int[] measureIndex, int queryLayer, int layerCount)
//    {
//        //float out;
//        Vector3 firstPoint, secondPoint, queryPoint = new Vector3(pos[0, queryLayer], pos[1, queryLayer], pos[2, queryLayer]);
//        int i, prevLayer = -1, nextLayer = -1;
//        if (measureIndex[queryLayer] != -1)
//        {
//            return measured.Value(row, measureIndex[queryLayer]);
//        }
//        else
//        {
//            // find first valid measurement in previous layers
//            for (i = queryLayer - 1; i > 0; i--)
//                if (measureIndex[i] != -1)
//                {
//                    prevLayer = i;
//                    break;
//                }
//            // find first valid measurement in next layers
//            for (i = queryLayer + 1; i < layerCount; i++)
//                if (measureIndex[i] != -1)
//                {
//                    nextLayer = i;
//                    break;
//                }

//            if (prevLayer == -1)
//            {       // No measurement bofore query layer
//                prevLayer = nextLayer;
//                for (i = prevLayer + 1; i < layerCount; i++)
//                    if (measureIndex[i] != -1)
//                    {
//                        nextLayer = i;
//                        break;
//                    }
//                if (nextLayer == prevLayer)
//                    return measured.Value(row, measureIndex[prevLayer]);
//            }
//            else if (nextLayer == -1)
//            {        // No measurement after query layer
//                nextLayer = prevLayer;
//                for (i = nextLayer - 1; i > 0; i--)
//                    if (measureIndex[i] != -1)
//                    {
//                        nextLayer = i;
//                        break;
//                    }
//                if (nextLayer == prevLayer)
//                    return measured.Value(row, measureIndex[nextLayer]);
//            }
//        }
//        firstPoint = new Vector3(pos[0, prevLayer], pos[1, prevLayer], pos[2, prevLayer]);
//        secondPoint = new Vector3(pos[0, nextLayer], pos[1, nextLayer], pos[2, nextLayer]);

//        if (queryLayer > prevLayer && queryLayer < nextLayer)
//            return linInterp(measured.Value(row, measureIndex[prevLayer]), measured.Value(row, measureIndex[nextLayer]), firstPoint, secondPoint, queryPoint);
//        else
//            return linExtrap(measured.Value(row, measureIndex[prevLayer]), measured.Value(row, measureIndex[nextLayer]), firstPoint, secondPoint, queryPoint);
//    }

//    float linInterp(float valA, float valB, Vector3 posA, Vector3 posB, Vector3 queryPos)
//    {
//        Vector3 posAposB = posB - posA;
//        Vector3 posAqueryPos = queryPos - posA;

//        float weight = posAqueryPos.magnitude / posAposB.magnitude;
//        return valA * (1 - weight) + valB * weight;
//    }
//    float linExtrap(float valA, float valB, Vector3 posA, Vector3 posB, Vector3 queryPos)
//    {
//        Vector3 posAposB = posB - posA;
//        Vector3 posAqueryPos = queryPos - posA;

//        float slope = (valA - valB) / posAposB.magnitude;

//        return valA + slope * posAqueryPos.magnitude;
//    }

//    Vector3 BarycentricCoordinates(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
//    {
//        Vector3 coordinates = new Vector3();
//        coordinates[0] = triArea(P, B, C) / triArea(A, B, C); // alpha
//        coordinates[1] = triArea(P, C, A) / triArea(A, B, C); //beta
//        coordinates[2] = triArea(P, A, B) / triArea(A, B, C); // gamma
//                                                              //float gamma = 1 - alpha - beta;
//        return coordinates;
//    }

//    float triArea(Vector3 A, Vector3 B, Vector3 C)
//    {
//        Vector3 AB = B - A;
//        Vector3 AC = C - A;

//        Vector3 n = Vector3.Cross(AB, AC);
//        //println(Math.signum(n[2])==0? 1: Math.signum(n[2]));
//        return 0.5F * n.magnitude * (System.Math.Sign(n.z) == 0 ? 1 : System.Math.Sign(n.z)); // TODO: Is there a better way to determine the sign of area?
//    }

//}
