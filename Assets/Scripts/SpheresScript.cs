using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SpheresScript : MonoBehaviour {

    float panelMinX;
    float panelMaxX;
    float panelMinY;
    float panelMaxY;
    float panelMinZ;
    float panelMaxZ;

    float panelGridStep;

    int layerCount;

    int frame = 0;

    float[,] SensPosA;
    float[,] SensPosB;
    float[,] SensPosC;

    float[,,] interpVals;

    int[] MCColumnPerLayerPosA;
    int[] MCColumnPerLayerPosB;
    int[] MCColumnPerLayerPosC;
    int[] PanelTempColumn;

    private GameObject[,,] points;
    private GameObject temperatureGeom;
    private TextMesh temperatureText;


    [SerializeField]
    private GameObject DataObjectPrefab;
    [SerializeField]
    private GameObject PointGeomPrefab;

    private DataScript SensorData;

    //private enum SPHERE_VIZ_MODE { COLOR, RADIUS };

    private Specs.VIZ_MODE Mode = Specs.ThisVizMode;

    void Start () {
        Setup();
    }

    //void Update () {

    //}
    private void Update()
    {
        Keyboard();
        float timeScale = 5;
        if (Time.frameCount % timeScale == 0)
        {
            if (frame < SensorData.Rows)
            {
                //Debug.Log(frame + ": " + SensorData.DataTime(frame));
                InterpolateAllLayers(frame);
                switch (Mode)
                {
                    case Specs.VIZ_MODE.PARTICLE_COLOR:
                        DrawTemperature(frame);
                        DrawPoints();
                        break;
                    case Specs.VIZ_MODE.PARTICLE_RADIUS:
                        DrawSpherePattern(PanleTemperature(frame));
                        break;
                }

                //
            }
            frame++;
        }
    }
    private void Setup() {

        //float[] layerZs = new float[] { 0, 5, 10, 15, 20, 25, 30 };

        panelMinX = -20;
        panelMaxX = 80;
        panelMinY = -20;
        panelMaxY = 120;
        panelGridStep = 4f;
        panelMinZ = 0;
        panelMaxZ = 30;
        layerCount = 7;

        SensPosA = new float[,] { { 0, 0, 0, 0, 0, 0, 0},
                                  { 50, 50, 50, 50, 50, 50, 50},
                                  { panelMinZ, panelMinZ+(panelMaxZ-panelMinZ)/(layerCount-1), panelMinZ+2*(panelMaxZ-panelMinZ)/(layerCount-1), panelMinZ+3*(panelMaxZ-panelMinZ)/(layerCount-1), panelMinZ+4*(panelMaxZ-panelMinZ)/(layerCount-1), panelMinZ+5*(panelMaxZ-panelMinZ)/(layerCount-1), panelMaxZ} };

        SensPosB = new float[,] { { 0, 0, 0, 0, 0, 0, 0},
                                  { 0, 0, 0, 0, 0, 0, 0},
                                   { panelMinZ, panelMinZ+(panelMaxZ-panelMinZ)/(layerCount-1), panelMinZ+2*(panelMaxZ-panelMinZ)/(layerCount-1), panelMinZ+3*(panelMaxZ-panelMinZ)/(layerCount-1), panelMinZ+4*(panelMaxZ-panelMinZ)/(layerCount-1), panelMinZ+5*(panelMaxZ-panelMinZ)/(layerCount-1), panelMaxZ}};

        SensPosC = new float[,] { { 20, 20, 20, 20, 20, 20, 20},
                                  { 50, 50, 50, 50, 50, 50, 50},
                                  { panelMinZ, panelMinZ+(panelMaxZ-panelMinZ)/(layerCount-1), panelMinZ+2*(panelMaxZ-panelMinZ)/(layerCount-1), panelMinZ+3*(panelMaxZ-panelMinZ)/(layerCount-1), panelMinZ+4*(panelMaxZ-panelMinZ)/(layerCount-1), panelMinZ+5*(panelMaxZ-panelMinZ)/(layerCount-1), panelMaxZ} };


        transform.localScale = new Vector3(panelMaxX - panelMinX, panelMaxY - panelMinY, panelMaxZ - panelMinZ);
        //transform.position = new Vector3(-panelMinX, -panelMinY, -panelMinZ);
        transform.position = new Vector3(0.5F * (panelMaxX + panelMinX), 0.5F * (panelMaxY + panelMinY), 0.5F * (panelMaxZ + panelMinZ));

        SensorData = Instantiate<GameObject>(DataObjectPrefab).GetComponent <DataScriptCSV> ();

        AssignColumns2Layers();

        int panelW = (int)System.Math.Floor((panelMaxX - panelMinX) / panelGridStep);
        int panelH = (int)System.Math.Floor((panelMaxY - panelMinY) / panelGridStep);

        interpVals = new float[panelH, panelW, layerCount];

        switch(Mode){
            case Specs.VIZ_MODE.PARTICLE_COLOR:
                InitTemperature();
                InitPoints();
                break;
            case Specs.VIZ_MODE.PARTICLE_RADIUS:
                InitSpherePattern();
                break;
        }
    }

    private void AssignColumns2Layers()
    {
        if (SensorData.NodeID == 0)
        {
            Debug.Log("Cannot assign measurements to layers due unspecified node");
            Application.Quit(); // doesn't quit in editor mode
        }
        else if (SensorData.NodeID == 25751)
        {
            MCColumnPerLayerPosA = new int[] { 6, 7, 8, -1, -1, -1, -1 }; // Upper Corner Ply 1 2 3
            MCColumnPerLayerPosB = new int[] { -1, -1, -1, -1, -1, 5, 9 }; //By Floor Connection Ply 6 7 
            MCColumnPerLayerPosC = new int[] { -1, -1, -1, 11, -1, -1, -1 }; // Upper By Tendons
            PanelTempColumn = new int[] { 4, 10 };

        }
        else if (SensorData.NodeID == 25752)
        {
            MCColumnPerLayerPosA = new int[] { 9, 7, -1, -1, -1, 8, 10 }; // Bottom Corner Ply 2 6 1 7
            MCColumnPerLayerPosB = new int[] { -1, 13, -1, 12, -1, -1, -1 }; // Edge by Hardware Ply 2 4   
            MCColumnPerLayerPosC = new int[] { -1, -1, -1, -1, 11, -1, -1 }; // Middle Panel Ply 5
            PanelTempColumn = new int[] { 6 };
        }
        else
        {
            Debug.Log("Cannot assign measurements to layers due to unknown node");
            Application.Quit(); // doesn't quit in editor mode
        }
    }

    Vector3 GetGridPosition(int i, int j, int k) {
        float xPos, yPos, zPos;
        xPos = panelGridStep / 2 + panelMinX + j * panelGridStep;//Random.Range(panelMinX, panelMaxX);
        yPos = panelGridStep / 2 + panelMinY + i * panelGridStep;//Random.Range(panelMinX, panelMaxX);
        zPos = SensPosC[2, k];//Random.Range(panelMinX, panelMaxX);
        return new Vector3(xPos, yPos, zPos);
    }

    void InitSpherePattern()
    {
        int w = interpVals.GetLength(1);
        int h = interpVals.GetLength(0);
        int d = interpVals.GetLength(2);
        //float xPos, yPos, zPos;
        points = new GameObject[h, w, layerCount];

        for (int i = 0; i < h; i++) {
            for (int j = 0; j < w; j++){
                for (int k = 0; k < d; k++){
                     points[i, j, k] = Instantiate<GameObject>(PointGeomPrefab);
                    //points[i, j, k].SetActive(false);
                    //xPos = panelGridStep / 2 + panelMinX + j * panelGridStep;//Random.Range(panelMinX, panelMaxX);
                    //yPos = panelGridStep / 2 + panelMinY + i * panelGridStep;//Random.Range(panelMinX, panelMaxX);
                    //zPos = SensPosC[2, k];//Random.Range(panelMinX, panelMaxX);
                    //points[i, j, k].transform.position = new Vector3(xPos, yPos, zPos);
                    points[i, j, k].transform.position = GetGridPosition(i,j,k);
                    points[i, j, k].transform.parent = transform;
                }
            }
        } 
    }
    void DrawSpherePattern(float temperature)
    {
        int panelW = interpVals.GetLength(1);
        int panelH = interpVals.GetLength(0);
        int panelD = interpVals.GetLength(2);

        float pointScale;
        float tempWeight = 60F;
        for (int i = 0; i < panelH; i++) {
            for (int j = 0; j < panelW; j++) {
                for (int k = 0; k < panelD; k++) {
                    pointScale = Map(interpVals[i,j,k], 0F, 50F, 0F, panelGridStep*1.5F);
                    //pointScale = Mathf.Lerp(interpVals[i,j,k]/50, 0F, panelGridStep);
                    //Debug.Log(pointScale);
                    points[i, j, k].transform.parent = null;

                    points[i, j, k].transform.localScale = Vector3.one * pointScale;
                    points[i, j, k].transform.parent = transform;

                    points[i, j, k].GetComponent<Renderer>().material.color = Color.Lerp(new Color(0F, 1F, 0F), new Color(1F, 0F, 0F), temperature / tempWeight);
                }
            }
        } 
    }

    float Map(float val, float sMin, float sMax, float dMin, float dMax) {
        return Mathf.Lerp(dMin, dMax, (val - sMin) / (sMax - sMin));
    }


    private void InitPoints()
    {
        int w = interpVals.GetLength(1);
        int h = interpVals.GetLength(0);
        int d = interpVals.GetLength(2);
        //float xPos, yPos, zPos;
        points = new GameObject[h,w,layerCount];
        float pointScale = panelGridStep/1.5F;

        for (int i = 0; i < h; i++){
            for (int j = 0; j < w; j++){
                for (int k = 0; k < d; k++){

                    points[i,j,k] = Instantiate<GameObject>(PointGeomPrefab);
                    //points[i, j, k].SetActive(false);
                    //xPos = panelGridStep/2 + panelMinX + j * panelGridStep;//Random.Range(panelMinX, panelMaxX);
                    //yPos = panelGridStep / 2 + panelMinY + i * panelGridStep;//Random.Range(panelMinX, panelMaxX);
                    //zPos = SensPosC[2,k];//Random.Range(panelMinX, panelMaxX);

                    points[i, j, k].transform.localScale = Vector3.one * pointScale;
                    //points[i,j,k].transform.position = new Vector3(xPos, yPos, zPos);
                    points[i,j,k].transform.position = GetGridPosition(i,j,k);
                    points[i, j, k].transform.parent = transform;
                }
            }
        }
    }

    private void DrawPoints()
    {
        int w = interpVals.GetLength(1);
        int h = interpVals.GetLength(0);
        int d = interpVals.GetLength(2);

        //Vector3[] pntPos = new Vector3[count];
 
        //GameObject cubePrefab = (GameObject)Resources.Load(@"Prefabs\CubePrefab");
        //BoxCollider collider;
        for (int i = 0; i < h; i++){
            for (int j = 0; j < w; j++){
                for (int k = 0; k < d; k++){
                    //if (!points[i, j, k].activeSelf)
                    //    points[i, j, k].SetActive(true);

                    points[i, j, k].GetComponent<Renderer>().material.color = Color.Lerp(new Color (1F,0F,0F), new Color(0F,0F,1F), Map(interpVals[i, j, k],0F,50F,0F,1F));
                    //Debug.Log(interpVals[i, j, k] / 70F);
                    //points[i, j, k].GetComponent<Renderer>().material.color = new Color(Random.Range(0F, 1F), Random.Range(0F, 1F), Random.Range(0F, 1F));
                }
            }
        }
    }

    private void InitTemperature()
    {
        float radius =30;
        temperatureGeom = Instantiate<GameObject>(PointGeomPrefab);
        temperatureGeom.transform.localScale = Vector3.one * radius;
        temperatureGeom.transform.position = new Vector3(-80, 50, 0);
        temperatureGeom.transform.parent = transform;

  
        GameObject GO = new GameObject("TextObj");
        GO.transform.position = temperatureGeom.transform.position+Vector3.forward* radius;
        //temperatureText.transform.Rotate(Vector3.up, 90);
        //textGo.transform.LookAt(Camera.main.transform.position);

        temperatureText = GO.AddComponent<TextMesh>();
        temperatureText.fontSize = 150;
        temperatureText.color = new Color(1F, 1F, 1F);
        GO.transform.parent = transform;


        //print(data.DataName(index));


    }

    private void DrawTemperature(int t)
    {
        float temp = PanleTemperature(t);
        //float tempWeight = 60F;

        //if (PanelTempColumn.Length == 1)
        //    temp = SensorData.Value(t, PanelTempColumn[0]);
        //else
        //{
        //    for (int i = 0; i < PanelTempColumn.Length; i++)
        //        temp += SensorData.Value(t, PanelTempColumn[i])/ PanelTempColumn.Length;

        //}

        temperatureGeom.GetComponent<Renderer>().material.color = Color.Lerp(new Color(0F, 1F, 0F), new Color(1F, 0F, 0F), Map(temp,0F,45F,0F,1F));

        temperatureText.text =string.Format("{0:0.00}°C", temp);


    }

    float PanleTemperature(int t) {
        float temp = 0;
 
        if (PanelTempColumn.Length == 1)
            temp = SensorData.Value(t, PanelTempColumn[0]);
        else
        {
            for (int i = 0; i < PanelTempColumn.Length; i++)
                temp += SensorData.Value(t, PanelTempColumn[i]) / PanelTempColumn.Length;

        }
        return temp;
    }

    void InterpolateAllLayers(int t)
    {
        for (int i = 0; i < layerCount; i++)
            InterpolateLayer(t, i);
    }

    // interpolate the measurements to layer queryLayer then interpolate over the layer
    void InterpolateLayer(int t, int queryLayer)
    {
        float MCPosA, MCPosB, MCPosC;
        Vector3 bary, triA, triB, triC, triP;

        // assign the mesurements to the right locations
        triA = new Vector3(SensPosA[0,queryLayer], SensPosA[1,queryLayer], SensPosA[2,queryLayer]);
        triB = new Vector3(SensPosB[0,queryLayer], SensPosB[1,queryLayer], SensPosB[2,queryLayer]);
        triC = new Vector3(SensPosC[0,queryLayer], SensPosC[1,queryLayer], SensPosC[2,queryLayer]);

        MCPosA = interpolatePoint2Layer(SensorData, t, SensPosA, MCColumnPerLayerPosA, queryLayer, layerCount);
        MCPosB = interpolatePoint2Layer(SensorData, t, SensPosB, MCColumnPerLayerPosB, queryLayer, layerCount);
        MCPosC = interpolatePoint2Layer(SensorData, t, SensPosC, MCColumnPerLayerPosC, queryLayer, layerCount);

        if (MCPosA == float.NaN || MCPosB == float.NaN || MCPosC == float.NaN)
        {
            Debug.Log("skipped " + t);
            return ;
        }
        for (int i = 0; i < interpVals.GetLength(0); i++)
        {
            for (int j = 0; j < interpVals.GetLength(1); j++)
            {
                triP = new Vector3(panelMinX + j * panelGridStep, panelMinY + i * panelGridStep, triA[2] );
                bary = BarycentricCoordinates(triA, triB, triC, triP);
                interpVals[i,j,queryLayer] = bary[0] * MCPosA + bary[1] * MCPosB + bary[2] * MCPosC;
            }
        }

    }

    // interpolate the measurements at a spicific position to queryLayer
    float interpolatePoint2Layer(DataScript measured, int row, float[,] pos, int[] measureIndex, int queryLayer, int layerCount)
    {
        //float out;
        Vector3 firstPoint, secondPoint, queryPoint = new Vector3(pos[0,queryLayer], pos[1,queryLayer], pos[2,queryLayer]);
        int i, prevLayer = -1, nextLayer = -1;
        if (measureIndex[queryLayer] != -1)
        {
            return measured.Value(row, measureIndex[queryLayer]);
        }
        else
        {
            // find first valid measurement in previous layers
            for (i = queryLayer - 1; i > 0; i--)
                if (measureIndex[i] != -1)
                {
                    prevLayer = i;
                    break;
                }
            // find first valid measurement in next layers
            for (i = queryLayer + 1; i < layerCount; i++)
                if (measureIndex[i] != -1)
                {
                    nextLayer = i;
                    break;
                }

            if (prevLayer == -1)
            {       // No measurement bofore query layer
                prevLayer = nextLayer;
                for (i = prevLayer + 1; i < layerCount; i++)
                    if (measureIndex[i] != -1)
                    {
                        nextLayer = i;
                        break;
                    }
                if (nextLayer == prevLayer)
                    return measured.Value(row,measureIndex[prevLayer]);
            }
            else if (nextLayer == -1)
            {        // No measurement after query layer
                nextLayer = prevLayer;
                for (i = nextLayer - 1; i > 0; i--)
                    if (measureIndex[i] != -1)
                    {
                        nextLayer = i;
                        break;
                    }
                if (nextLayer == prevLayer)
                    return measured.Value(row,measureIndex[nextLayer]);
            }
        }
        firstPoint = new Vector3(pos[0,prevLayer], pos[1,prevLayer], pos[2,prevLayer]);
        secondPoint = new Vector3(pos[0,nextLayer], pos[1,nextLayer], pos[2,nextLayer]);
       
        if (queryLayer > prevLayer && queryLayer < nextLayer)
            return linInterp(measured.Value(row,measureIndex[prevLayer]), measured.Value(row, measureIndex[nextLayer]), firstPoint, secondPoint, queryPoint);
        else
            return linExtrap(measured.Value(row, measureIndex[prevLayer]), measured.Value(row, measureIndex[nextLayer]), firstPoint, secondPoint, queryPoint);
    }

    float linInterp(float valA, float valB, Vector3 posA, Vector3 posB, Vector3 queryPos)
    {
        Vector3 posAposB = posB- posA;
        Vector3 posAqueryPos = queryPos - posA;

        float weight = posAqueryPos.magnitude / posAposB.magnitude;
        return valA * (1 - weight) + valB * weight;
    }
    float linExtrap(float valA, float valB, Vector3 posA, Vector3 posB, Vector3 queryPos)
    {
        Vector3 posAposB = posB - posA;
        Vector3 posAqueryPos = queryPos - posA;

        float slope = (valA - valB) / posAposB.magnitude;

        return valA + slope * posAqueryPos.magnitude;
    }

    Vector3 BarycentricCoordinates(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
    {
        Vector3 coordinates = new Vector3();
        coordinates[0] = triArea(P, B, C) / triArea(A, B, C); // alpha
        coordinates[1] = triArea(P, C, A) / triArea(A, B, C); //beta
        coordinates[2] = triArea(P, A, B) / triArea(A, B, C); // gamma
                                                              //float gamma = 1 - alpha - beta;
        return coordinates;
    }

    float triArea(Vector3 A, Vector3 B, Vector3 C)
    {
        Vector3 AB = B-A;
        Vector3 AC = C-A;

        Vector3 n = Vector3.Cross(AB, AC);
        //println(Math.signum(n[2])==0? 1: Math.signum(n[2]));
        return 0.5F * n.magnitude * (System.Math.Sign(n.z) == 0 ? 1 : System.Math.Sign(n.z)); // TODO: Is there a better way to determine the sign of area?
    }

    void Keyboard() {
        if (Input.GetKeyDown("q"))
        {
            Mode = Specs.VIZ_MODE.PARTICLE_COLOR;
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            InitTemperature();
            InitPoints();
        }
        else if (Input.GetKeyDown("w"))
        {
            Mode = Specs.VIZ_MODE.PARTICLE_RADIUS;
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            InitSpherePattern();
        }

    }

    //Color heatMapColor(float value, float minValue, float maxValue, float minColor, float maxColor, int lowColorIndex, int highColorIndex)
    //{
    //    float[] c = new float[3];

    //    c[lowColorIndex] = map(-value, -maxValue, -minValue, minColor, maxColor);
    //    c[highColorIndex] = map(value, minValue, maxValue, minColor, maxColor);

    //    return color(c[0], c[1], c[2]);
    //}


    //    float[] cross(float[] v1, float[] v2)
    //    {
    //        float[] out = new float[3];      

    //  out[0] = v1[1] * v2[2] - v1[2] * v2[1];
    //  out[1] = v1[2] * v2[0] - v1[0] * v2[2];
    //  out[0] = v1[0] * v2[1] - v1[1] * v2[0];
    //  return out;
    //}

}
