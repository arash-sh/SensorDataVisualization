using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisScript : MonoBehaviour {
    float panelMinX;
    float panelMaxX;
    float panelMinY;
    float panelMaxY;
    float panelMinZ;
    float panelMaxZ;

    float panelGridStep;

    int layerCount;

    float pointScale;

    int frame = 0;

    Vector3[] SensPosA;
    Vector3[] SensPosB;
    Vector3[] SensPosC;

    float[,,] interpVals;

    int[] MCColumnPerLayerPosA;
    int[] MCColumnPerLayerPosB;
    int[] MCColumnPerLayerPosC;
    int[] PanelTempColumn;

    private GameObject[,,] points;
    private GameObject temperatureGeom;
    private TextMesh temperatureText;
    private enum SPHERE_VIZ_MODE { COLOR, RADIUS };

    private SPHERE_VIZ_MODE VizMode = SPHERE_VIZ_MODE.COLOR;

    [SerializeField]
    private GameObject DataObjectPrefab;
    [SerializeField]
    private GameObject PointGeomPrefab;
    [SerializeField]
    private Material PanelMaterial;

    private DataScript SensorData;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    private bool offlineData = false;


    void Start () {
        Setup();
	}
    private void Update()
    {
        Keyboard();
        Mouse();
        float timeScale = 5;
        if (Time.frameCount % timeScale == 0)
        {
            if (frame < SensorData.Rows)
            {
                //Debug.Log(frame + ": " + SensorData.DataTime(frame));
                InterpolateAllLayers(frame);
                switch (VizMode)
                {
                    case SPHERE_VIZ_MODE.COLOR:
                        DrawTemperature(frame);
                        DrawPoints();
                        break;
                    case SPHERE_VIZ_MODE.RADIUS:
                        DrawSpherePattern(PanleTemperature(frame));
                        break;
                }

                //
            }
            frame++;
        }
    }

    private void Setup()
    {
        Vector3 tmpPos;

        GameObject sketchup = GameObject.Find("PEAVY general monitoring plan_with string pot wall 2018.09.22");
        GameObject rightTLC = sketchup.transform.Find("Group#776").gameObject;


        rightTLC.GetComponent<Renderer>().material = PanelMaterial;
        //leftTLC.GetComponent<Renderer>().material = PanelMaterial;

        layerCount = 7;

        panelMinX = rightTLC.GetComponent<Renderer>().bounds.min.x;
        panelMaxX = rightTLC.GetComponent<Renderer>().bounds.max.x;
        panelMinY = rightTLC.GetComponent<Renderer>().bounds.min.y;
        panelMaxY = rightTLC.GetComponent<Renderer>().bounds.max.y;
        panelMinZ = rightTLC.GetComponent<Renderer>().bounds.min.z;
        panelMaxZ = panelMinZ + 3* (rightTLC.GetComponent<Renderer>().bounds.max.z - panelMinZ);

        panelGridStep = (panelMaxX- panelMinX)/20;
        pointScale = Mathf.Min((panelMaxZ- panelMinZ)/layerCount, panelGridStep/1.5F);

        SensPosA = new Vector3[layerCount];
        SensPosB = new Vector3[layerCount];
        SensPosC = new Vector3[layerCount];

        tmpPos = GameObject.Find("Component#32 5").transform.position;
        for (int i=0;i<layerCount;i++)
            SensPosA[i].Set(tmpPos.x, tmpPos.y, panelMinZ+ i*(panelMaxZ- panelMinZ) / layerCount);
        tmpPos = GameObject.Find("Component#32 4").transform.position;
        SensPosA[1].Set(tmpPos.x, tmpPos.y, SensPosA[1].z);
        tmpPos = GameObject.Find("Component#32 7").transform.position;
        SensPosA[2].Set(tmpPos.x, tmpPos.y, SensPosA[2].z);


        tmpPos = GameObject.Find("Group 2189").transform.Find("Component#25").transform.position;
        for (int i = 0; i < layerCount; i++)
            SensPosB[i].Set(tmpPos.x, tmpPos.y, panelMinZ + i * (panelMaxZ - panelMinZ) / layerCount);
        tmpPos = GameObject.Find("Group 2189").transform.Find("Component#28").transform.position;
        SensPosB[6].Set(tmpPos.x, tmpPos.y, SensPosB[6].z);


        tmpPos = GameObject.Find("Component#32").transform.position;
        for (int i = 0; i < layerCount; i++)
            SensPosC[i].Set(tmpPos.x, tmpPos.y, panelMinZ + i * (panelMaxZ - panelMinZ)/layerCount);


        transform.localScale = new Vector3(panelMaxX - panelMinX, panelMaxY - panelMinY, panelMaxZ - panelMinZ);
        transform.position = new Vector3(0.5F * (panelMaxX + panelMinX), 0.5F * (panelMaxY + panelMinY), 0.5F * (panelMaxZ + panelMinZ));

        if (offlineData)
        {
            SensorData = Instantiate<GameObject>(DataObjectPrefab).GetComponent<DataScriptCSV>();
        }
        else
            SensorData = Instantiate<GameObject>(DataObjectPrefab).GetComponent<DataScriptXML>();

        AssignColumns2Layers();


        int panelW = (int)System.Math.Floor((panelMaxX - panelMinX) / panelGridStep);
        int panelH = (int)System.Math.Floor((panelMaxY - panelMinY) / panelGridStep);

        interpVals = new float[panelH, panelW, layerCount];
        //interpVals = new float[panelH, panelW, layerCount];

        switch (VizMode)
        {
            case SPHERE_VIZ_MODE.COLOR:
                InitTemperature();
                InitPoints();
                break;
            case SPHERE_VIZ_MODE.RADIUS:
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

    Vector3 GetGridPosition(int i, int j, int k)
    {
        float xPos, yPos, zPos;
        xPos = panelGridStep / 2 + panelMinX + j * panelGridStep;//Random.Range(panelMinX, panelMaxX);
        yPos = panelGridStep / 2 + panelMinY + i * panelGridStep;//Random.Range(panelMinX, panelMaxX);
        zPos = SensPosA[k].z;//Random.Range(panelMinX, panelMaxX);
        return new Vector3(xPos, yPos, zPos);
    }

    void InitSpherePattern()
    {
        int w = interpVals.GetLength(1);
        int h = interpVals.GetLength(0);
        int d = interpVals.GetLength(2);
        //float xPos, yPos, zPos;
        points = new GameObject[h, w, layerCount];

        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                for (int k = 0; k < d; k++)
                {
                    points[i, j, k] = Instantiate<GameObject>(PointGeomPrefab);
                    points[i, j, k].transform.position = GetGridPosition(i, j, k);
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
        float r;
        float tempWeight = 60F;
        for (int i = 0; i < panelH; i++)
        {
            for (int j = 0; j < panelW; j++)
            {
                for (int k = 0; k < panelD; k++)
                {
                    r = Map(interpVals[i, j, k], 0F, 50F, 0F, 2F*pointScale);
                    points[i, j, k].transform.parent = null;

                    points[i, j, k].transform.localScale = Vector3.one * r;
                    points[i, j, k].transform.parent = transform;

                    points[i, j, k].GetComponent<Renderer>().material.color = Color.Lerp(new Color(0F, 1F, 0F), new Color(1F, 0F, 0F), temperature / tempWeight);
                }
            }
        }
    }

    float Map(float val, float sMin, float sMax, float dMin, float dMax)
    {
        return Mathf.Lerp(dMin, dMax, (val - sMin) / (sMax - sMin));
    }


    private void InitPoints()
    {
        int w = interpVals.GetLength(1);
        int h = interpVals.GetLength(0);
        int d = interpVals.GetLength(2);

        points = new GameObject[h, w, layerCount];



        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                for (int k = 0; k < d; k++)
                {

                    points[i, j, k] = Instantiate<GameObject>(PointGeomPrefab);

                    points[i, j, k].transform.localScale = Vector3.one * pointScale;
                    points[i, j, k].transform.position = GetGridPosition(i, j, k);
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

        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                for (int k = 0; k < d; k++)
                {
                    //if (!points[i, j, k].activeSelf)
                    //    points[i, j, k].SetActive(true);

                    points[i, j, k].GetComponent<Renderer>().material.color = Color.Lerp(new Color(1F, 0F, 0F), new Color(0F, 0F, 1F), Map(interpVals[i, j, k], 0F, 50F, 0F, 1F));
                    //Debug.Log(interpVals[i, j, k] / 70F);
                    //points[i, j, k].GetComponent<Renderer>().material.color = new Color(Random.Range(0F, 1F), Random.Range(0F, 1F), Random.Range(0F, 1F));
                }
            }
        }
    }

    private void InitTemperature()
    {
        float radius = 0.15F;
        temperatureGeom = Instantiate<GameObject>(PointGeomPrefab);
        temperatureGeom.name = "Temperature Geometry";
        temperatureGeom.transform.localScale = Vector3.one * radius;
        temperatureGeom.transform.position = transform.position + new Vector3(2F*(panelMaxX-panelMinX)/2F, 0, 0);
 

        GameObject GO = new GameObject("TextObj");
        GO.transform.position = temperatureGeom.transform.position + Vector3.forward * radius;

        temperatureText = GO.AddComponent<TextMesh>();
        temperatureText.fontSize = 50;
        temperatureText.transform.localScale = Vector3.one * 0.03F;
        temperatureText.color = new Color(1F, 1F, 1F);

        GO.transform.parent = temperatureGeom.transform;

       temperatureGeom.transform.parent = transform;

 

    }

    private void DrawTemperature(int t)
    {
        float temp = PanleTemperature(t);

        temperatureGeom.GetComponent<Renderer>().material.color = Color.Lerp(new Color(0F, 1F, 0F), new Color(1F, 0F, 0F), Map(temp, 0F, 45F, 0F, 1F));

        temperatureText.text = string.Format("{0:0.00}°C", temp);
        temperatureGeom.transform.GetChild(0).rotation = Quaternion.LookRotation(Camera.main.transform.forward);


    }

    float PanleTemperature(int t)
    {
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
        triA = new Vector3(SensPosA[queryLayer].x, SensPosA[queryLayer].y, SensPosA[queryLayer].z);
        triB = new Vector3(SensPosB[queryLayer].x, SensPosB[queryLayer].y, SensPosB[queryLayer].z);
        triC = new Vector3(SensPosC[queryLayer].x, SensPosC[queryLayer].y, SensPosC[queryLayer].z);

        MCPosA = interpolatePoint2Layer(SensorData, t, SensPosA, MCColumnPerLayerPosA, queryLayer, layerCount);
        MCPosB = interpolatePoint2Layer(SensorData, t, SensPosB, MCColumnPerLayerPosB, queryLayer, layerCount);
        MCPosC = interpolatePoint2Layer(SensorData, t, SensPosC, MCColumnPerLayerPosC, queryLayer, layerCount);

        if (MCPosA == float.NaN || MCPosB == float.NaN || MCPosC == float.NaN)
        {
            Debug.Log("skipped " + t);
            return;
        }
        for (int i = 0; i < interpVals.GetLength(0); i++)
        {
            for (int j = 0; j < interpVals.GetLength(1); j++)
            {
                triP = new Vector3(panelMinX + j * panelGridStep, panelMinY + i * panelGridStep, triA[2]);
                bary = BarycentricCoordinates(triA, triB, triC, triP);
                interpVals[i, j, queryLayer] = bary[0] * MCPosA + bary[1] * MCPosB + bary[2] * MCPosC;
            }
        }

    }

    // interpolate the measurements at a spicific position to queryLayer
    float interpolatePoint2Layer(DataScript measured, int row, Vector3[] pos, int[] measureIndex, int queryLayer, int layerCount)
    {
        //float out;
        Vector3 firstPoint, secondPoint, queryPoint = new Vector3(pos[queryLayer].x, pos[queryLayer].y, pos[queryLayer].z);
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
                    return measured.Value(row, measureIndex[prevLayer]);
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
                    return measured.Value(row, measureIndex[nextLayer]);
            }
        }
        firstPoint = new Vector3(pos[prevLayer].x, pos[prevLayer].y, pos[prevLayer].z);
        secondPoint = new Vector3(pos[nextLayer].x, pos[nextLayer].y, pos[nextLayer].z);

        if (queryLayer > prevLayer && queryLayer < nextLayer)
            return linInterp(measured.Value(row, measureIndex[prevLayer]), measured.Value(row, measureIndex[nextLayer]), firstPoint, secondPoint, queryPoint);
        else
            return linExtrap(measured.Value(row, measureIndex[prevLayer]), measured.Value(row, measureIndex[nextLayer]), firstPoint, secondPoint, queryPoint);
    }

    float linInterp(float valA, float valB, Vector3 posA, Vector3 posB, Vector3 queryPos)
    {
        Vector3 posAposB = posB - posA;
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
        Vector3 AB = B - A;
        Vector3 AC = C - A;

        Vector3 n = Vector3.Cross(AB, AC);
        //println(Math.signum(n[2])==0? 1: Math.signum(n[2]));
        return 0.5F * n.magnitude * (System.Math.Sign(n.z) == 0 ? 1 : System.Math.Sign(n.z)); // TODO: Is there a better way to determine the sign of area?
    }

    void Keyboard()
    {
        Camera cam = Camera.main;
        float speed = 0.8F;
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            VizMode = SPHERE_VIZ_MODE.COLOR;
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            InitTemperature();
            InitPoints();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            VizMode = SPHERE_VIZ_MODE.RADIUS;
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            InitSpherePattern();
        }

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKeyDown("d"))
        {
            cam.transform.Translate(speed * cam.transform.right);
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKeyDown("a"))
        {
            cam.transform.Translate(-speed * cam.transform.right);
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKeyDown("s"))
        {
            cam.transform.Translate(-speed * cam.transform.forward);
        }
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKeyDown("w"))
        {
           cam.transform.Translate(speed * cam.transform.forward);
        }
        if (Input.GetKey(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.Plus) || Input.GetKeyDown("e"))
        {
            cam.transform.Translate(speed * cam.transform.up);
        }
        if (Input.GetKey(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.Minus) || Input.GetKeyDown("q"))
        {
             cam.transform.Translate(-speed * cam.transform.up);
        }
    }


    void Mouse()
    {
        if (Input.GetMouseButton(0)) { 
            float speed = 2.0f;

            Camera cam = Camera.main;

            yaw += speed * Input.GetAxis("Mouse X");
            pitch -= speed * Input.GetAxis("Mouse Y");
            yaw = Mathf.Clamp(yaw, -360, 360);
            pitch = ClampAngle(pitch, -360,360);

            cam.transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        } else {
            float speed = 20f;

            Camera cam = Camera.main;

            float fov = cam.fieldOfView;
            fov -= Input.GetAxis("Mouse ScrollWheel") * speed;
            fov = Mathf.Clamp(fov, 20, 90);
            cam.fieldOfView = fov;
        }

    }

    float ClampAngle(float angle, float min, float max)
    {
        if (angle < min)
            angle += 360F;
        if (angle > max)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }

    void SetChildrenTransparent(GameObject GO, float alpha) {

        Material tmpMat;
        Color tmpCol;
            
        foreach (Transform child in GO.GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.GetComponent<Renderer>() != null)
            {
                tmpMat = child.gameObject.GetComponent<Renderer>().material;
                tmpCol = tmpMat.color;
                tmpCol.a = alpha;
                tmpCol.r = 1F;
                tmpMat.SetColor("_Color", tmpCol);
            }
        }

    }

}
