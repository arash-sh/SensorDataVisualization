using System;
using System.IO;
using UnityEngine;
using System.Collections;
public static class Utilities
{
    // scale the value with respect to worldscale
    public static float Scaled(float x) { return x * Specs.WorldScale; }
    
    // Extrapolation along a line given by two points A and B, with query point between them 
   public static float LinInterp(float valA, float valB, Vector3 posA, Vector3 posB, Vector3 queryPos)
    {
        Vector3 posAposB = posB - posA;
        Vector3 posAqueryPos = queryPos - posA;

        float weight = posAqueryPos.magnitude / posAposB.magnitude;
        return valA * (1 - weight) + valB * weight;
    }

    // Extrapolation along a line given by two points A and B, with query point not between them 
    public static float LinExtrap(float valA, float valB, Vector3 posA, Vector3 posB, Vector3 queryPos)
    {
        Vector3 posAposB = posB - posA;
        Vector3 posAqueryPos = queryPos - posA;

        float slope = (valA - valB) / posAposB.magnitude;

        return valA + slope * posAqueryPos.magnitude;
    }

    // Barycentric coordinates of a point p with respect to triangle ABC
    public static Vector3 BarycentricCoordinates(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
    {
        Vector3 coordinates = new Vector3();
        coordinates.x = TriArea(P, B, C) / TriArea(A, B, C); // alpha
        coordinates.y = TriArea(P, C, A) / TriArea(A, B, C); //beta
        coordinates.z = TriArea(P, A, B) / TriArea(A, B, C); // gamma
                                                              //float gamma = 1 - alpha - beta;
        return coordinates;
    }

    public static Color ThreeColorLerp(float val, float sMin, float sMax, Color c1, Color c2, Color c3)
    {
        Color c;
        val = Map(val, sMin, sMax, 0, 1); // normalize the value to range
        if (val < 0.5F)
        {
            c = Color.Lerp(c1, c2, val*2F);
        }
        else
        {
            c = Color.Lerp(c2, c3, (val - 0.5F) * 2F);
        }

        return c;
    }


    // Signed area of a triangle ABC
    public static float TriArea(Vector3 A, Vector3 B, Vector3 C)
    {
        Vector3 AB = B - A;
        Vector3 AC = C - A;

        Vector3 n = Vector3.Cross(AB, AC);
        //println(Math.signum(n[2])==0? 1: Math.signum(n[2]));
        return 0.5F * n.magnitude * (System.Math.Sign(n.z) == 0 ? 1 : System.Math.Sign(n.z)); // TODO: Is there a better way to determine the sign of area?
    }

    // linear interpolate a value with respect to source and destination range
    public static float Map(float val, float sMin, float sMax, float dMin, float dMax)
    {
        return Mathf.Lerp(dMin, dMax, (val - sMin) / (sMax - sMin));
    }

    // writes a 2D float matrix into a file, columns separated by tabs
    public static void WriteMatrix2File(float[,] vals, string fileName)
    {
        TextWriter writer = new StreamWriter(fileName);
        string line;
        for (int i = 0; i < vals.GetLength(0); i++)
        {
            line = "";
            for (int j = 0; j < vals.GetLength(1); j++)
            {
                line = line + vals[i, j].ToString("#.00") + "\t";
            }
            line = line.Remove(line.Length - 1, 1); // remove the last tab

            writer.WriteLine(line);
        }
        writer.Flush();
        writer.Close();
    }
    
    // Slice a 3D matrix with respecto to 3rd dimension 
    public static float[,] Slicer(float[,,] vals, int sliceIndex)
    {
        float[,] slice = new float[vals.GetLength(0), vals.GetLength(1)];

        for (int i = 0; i < vals.GetLength(0); i++)
            for (int j = 0; j < vals.GetLength(1); j++)
                slice[i,j] = vals[i,j,sliceIndex];

        return slice;
    }
    // Finds the gameobject in the hierarchy, eg. name = "parentName>>childName", delim = ">>"
    public static GameObject GetGameObjectByName(string name, string[] delims)
    {
        GameObject go;

        string[] tokens = name.Split(delims, StringSplitOptions.RemoveEmptyEntries); ;
        if (tokens.Length < 1)
            return null;
        go = GameObject.Find(tokens[0]);
        for (int i = 1; i < tokens.Length; i++)
        {
            go = go.transform.Find(tokens[i]).gameObject;
        }
        return go;
    }

    // parse a string with given format into DateTime
    public static DateTime ParseTime(string s, String format)
    {
        DateTime output = new DateTime();

        if (DateTime.TryParseExact(s, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out output))
            return output;
        else
            return DateTime.MinValue;
        //string format = "YYYY-MM-DD";
        //Char delim = '-';
        //string[] tokens = s.Split(delim);
        //string[] formatTokens = format.Split(delim);

        //if (tokens.Length != formatTokens.Length)
        //    return "ERROR: unknown format";
        //for (int i =0; i< formatTokens.Length;i++)
        //{
        //    if (formatTokens[i].Contains(returnType))
        //        return tokens[i];
        //}

        //return "ERROR: unknown format";

    }

    public static void LoadRessources()
    {

        Specs.TransparenMat = (Material)Resources.Load(@"Materials\TransparentMat");
        //public static Material HazardMat = (Material)Resources.Load(@"Materials\HazardColorMat");

        Specs.SensorIndicatorPrefab = (GameObject)Resources.Load<GameObject>(@"Prefabs/SensorIndicatorPrefab");
        Specs.WeatherPrefab = (GameObject)Resources.Load<GameObject>(@"Prefabs/WeatherPrefab");
        Specs.LowPolySpherePrefab = (GameObject)Resources.Load<GameObject>(@"Prefabs/LowPolySpherePrefab");

    }

    public static GameObject CreatePlane(float width = 1f, float height=1, int resX = 2,int resY=2, string name="Plane")
    {
        //// Based on: http://wiki.unity3d.com/index.php/ProceduralPrimitives

        //Debug.Log("reX : " + resX + "    resY = " + resY);

        GameObject GO = new GameObject(name);
 
        MeshFilter filter = GO.AddComponent<MeshFilter>();
        Mesh mesh = filter.mesh;
        mesh.Clear();

        //// Vertices		
        Vector3[] vertices = new Vector3[resX * resY];

        for (int y = 0; y < resY; y++)
        {
            float yPos = ((float)y / (resY - 1) - .5f) * height;
            for (int x = 0; x < resX; x++)
            {
                float xPos = ((float)x / (resX - 1) - .5f) * width;
                vertices[x + y * resX] = new Vector3(xPos, yPos, 0);
            }
        }

        //// Normales
        Vector3[] normales = new Vector3[vertices.Length];
        for (int n = 0; n < normales.Length; n++)
            normales[n] = Vector3.back;

        //// UVs		
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int v = 0; v < resY; v++)
        {
            for (int u = 0; u < resX; u++)
            {
                uvs[u + v * resX] = new Vector2((float)u / (resX - 1), (float)v / (resY - 1));
            }
        }

        //// Triangles
        int[] triangles = new int[(resX - 1) * (resY - 1) * 6]; // clockwise order
        int t = 0;
        for (int face = 0; face < triangles.Length/6; face++)
        {
            // Retrieve lower left corner from face ind
            int i = face % (resX - 1) + (face / (resX - 1) * resX);

            // lower tirangle
            triangles[t++] = i + resX;
            triangles[t++] = i + 1;
            triangles[t++] = i;
            // upper tirangle
            triangles[t++] = i + resX;
            triangles[t++] = i + resX + 1;
            triangles[t++] = i + 1;
        }

        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();

        Renderer rndr = GO.AddComponent<MeshRenderer>();
        //rndr.bounds.SetMinMax(new Vector3(-width / 2, -height / 2, 0), new Vector3(width / 2, height / 2, 0));
        //BoxCollider col = GO.AddComponent<BoxCollider>();
        //col.bounds.SetMinMax(rndr.bounds.min, rndr.bounds.max);
        return GO;
    }
}