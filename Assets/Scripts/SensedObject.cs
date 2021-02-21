using System;
using UnityEngine.Events;
using UnityEngine;

public class SensedObject: ScriptableObject {

    public int[] TimeIndex{ get; private set; } // for each sensor of the object, the index of the values variabel that corresponds to current time
    public float PanelGridStep{ get { return ObjectGO.GetComponent<Renderer>().bounds.size.x/20; } } // step of the grid on object
    public int LayerCount{ get; private set; }
    public string Name { get; private set; } // name of the object in the 3D model 
    public string ID { get; private set; } // ID of the node (DAQ) on server
    public Sensor[] Sensors { get; private set; } // list of sensors associated with object
    public GameObject ParentGO { get; private set; } // Parent GameObject containing the object, sensors, and possibly other geometry
    public GameObject ObjectGO { get; private set; } // Object geometry in the 3D model
    public GameObject IndicatorGO { get; private set; } // geometry in the 3D model
    public Vector3 SurfaceNormal { get; private set; } // position of geometry in the 3D model
    public bool Selected { get; private set; } // true if geometry selected by clicking on indicator
    public bool Focussed { get; private set; } // true if geometry selected by clicking on object itself
    public bool DataAvailable { get { return TimeIndex != null; } } // true if data availble fir current time

    
    public float[,,] InterpolatedValues { get; private set; } // first dimension is vertical

    public void Setup(string name, string id, Sensor[] sensors, int numLayers)
    {
        ObjectGO = Utilities.GetGameObjectByName(name, new string[] {">>"});
        Name = ObjectGO.name;
        ID = id;
        Selected = Focussed = false;
        ParentGO = new GameObject("SensedObject " + ID);

        ParentGO.transform.position = ObjectGO.GetComponent<Renderer>().bounds.center;
        ObjectGO.transform.parent = ParentGO.transform;
        Sensors = new Sensor[sensors.Length];
        Array.Copy(sensors, Sensors, sensors.Length);
        for (int s = 0; s < Sensors.Length; s++)
            Sensors[s].GO.transform.SetParent(ParentGO.transform);

        LoadIndicator();

        LayerCount = numLayers;
        SurfaceNormal = -Vector3.forward;                                 // TODO how to make this for general objects

        float panelMinX = ObjectGO.GetComponent<Renderer>().bounds.min.x;
        float panelMaxX = ObjectGO.GetComponent<Renderer>().bounds.max.x;
        float panelMinY = ObjectGO.GetComponent<Renderer>().bounds.min.y;
        float panelMaxY = ObjectGO.GetComponent<Renderer>().bounds.max.y;

        //PanelGridStep = (panelMaxX - panelMinX) / 20;  // 20 points along the x axis
        int panelW = (int)System.Math.Floor((panelMaxX - panelMinX) / PanelGridStep);
        int panelH = (int)System.Math.Floor((panelMaxY - panelMinY) / PanelGridStep);

        InterpolatedValues = new float[panelH, panelW, LayerCount];


    }
    void LoadIndicator()
    {
        Bounds bound = ObjectGO.GetComponent<Renderer>().bounds;
        //IndicatorPrefab = Resources.Load<GameObject>(@"Prefabs/SensorIndicatorPrefab");
                                                      //if (IndicatorPrefab == null)
                                                      //    IndicatorGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                                                      //else
        IndicatorGO = GameObject.Instantiate<GameObject>(Specs.SensorIndicatorPrefab);


        IndicatorGO.transform.position = bound.center + (bound.extents.y + Utilities.Scaled(3))* Vector3.up;
        IndicatorGO.transform.SetParent(ParentGO.transform);
        IndicatorGO.AddComponent<MouseHandler>();
        IndicatorGO.GetComponent<MouseHandler>().SetHandler(ObjectSelectedByIndicator, MouseHandler.MOUSE_EVENT.DOWN);
    }


    public float Value(int i, int j, int k)
    {
        return InterpolatedValues[i, j, k];
    }
   
    // interpolate value in the volume
    public void Interpolate(DateTime t)
    {
        //int[] tIndx = FindAllTimeStamps(t);
        TimeIndex = FindAllTimeStamps(t);
        if (TimeIndex != null)
        {
            InterpolateLayer(TimeIndex, 0);
        }
    }

    public void ObjectSelectedByIndicator()
    {
        Selected = true;                                                                 
        Camera cam = Camera.main;
        Bounds bound = ObjectGO.GetComponent<Renderer>().bounds;
        Vector3 desiredPos = bound.center + SurfaceNormal * Utilities.Scaled(30);
        cam.GetComponent<AvatarInteraction>().MoveCam(desiredPos, Quaternion.LookRotation(-SurfaceNormal, Vector3.up));

        BoxCollider col = ObjectGO.GetComponent<BoxCollider>();
        if (col == null)
            col = ObjectGO.AddComponent<BoxCollider>();
        col.bounds.SetMinMax(bound.min, bound.max);

        if (!ObjectGO.GetComponent<MouseHandler>())
        {
            MouseHandler mh = ObjectGO.AddComponent<MouseHandler>();
            mh.SetHandler(ObjectSelectedByObject, MouseHandler.MOUSE_EVENT.BUTTON);
        }
    }


    public void ObjectSelectedByObject()
    {
        Vector3 offset = SurfaceNormal * Utilities.Scaled(15);
        Focussed = !Focussed;
        Specs.MouseControlsCamera = !Focussed;
        if (Focussed)
        {
            ParentGO.AddComponent<ObjectViewer>();
            IndicatorGO.SetActive(false);
            ParentGO.transform.position = ParentGO.transform.position + offset;
            ParentGO.transform.localScale = ParentGO.transform.localScale * 2F;
        }
        else
        {
            IndicatorGO.SetActive(true);
            ObjectViewer viewer = ParentGO.GetComponent<ObjectViewer>();
            ParentGO.transform.position = viewer.InitialPos;
            ParentGO.transform.rotation = viewer.InitialRot;
            ParentGO.transform.localScale = viewer.InitialScl;
            //Specs.MouseControlsCamera = true;
            Destroy(viewer);
        }

    }
    // get the position of the grid points in the world
    public Vector3 GetGridPosition(int i, int j, int k)
    {                                                                  
        float xPos, yPos, zPos;
        Bounds b = ObjectGO.GetComponent<Renderer>().bounds;
        xPos = PanelGridStep / 2 + b.min.x + j * PanelGridStep;
        yPos = PanelGridStep / 2 + b.min.y + i * PanelGridStep;
        zPos = b.center.z + SurfaceNormal.z * b.extents.z;   
        return new Vector3(xPos, yPos, zPos);
    }

    void InterpolateAllLayers(int[] dataIndices)
    {
        for (int i = 0; i < LayerCount; i++)
            InterpolateLayer(dataIndices, i);
    }

    // interpolate the measurements to layer queryLayer then interpolate over the layer
    void InterpolateLayer(int[] dataIndices, int queryLayer)
    {
        int desiredSensCount = 3;  // number of sensed positions (measured or interpolated) needed on each layer, 3 for tirangle 
        int layerSensCount = 0;    // number of sensed positions (measured or interpolated) on a layer
        int indx = 0;
        //float[] valueAtPos = new float[Sensors.Length];

        int[] sensorLayerDist = new int[Sensors.Length];    // distance (in terms of number of layers) of sensors to query layer
        int[] sensorLayerDistPermut = new int[Sensors.Length]; // permutation of distance array, needed after sorting the array

        Vector3 bary, queryPoint; // placeholder for bary centric coordinates, and the query point
        Vector3[] verts = new Vector3[desiredSensCount];    // points on layer needed for interpolatation (vertices of polygon)
        float[] sensedValues = new float[desiredSensCount]; // values (measured or interpolated) on the layer
        bool tooClose;  // flag to check if two sensed locations are too close (i.e. same sensro location )

        for (int i = 0; i < Sensors.Length; i++)
        {
            sensorLayerDist[i] = Math.Abs(queryLayer - Sensors[i].Layer);
            sensorLayerDistPermut[i] = i;
        }
        //Array.Sort(sensorLayerDist);
        Array.Sort(sensorLayerDist, sensorLayerDistPermut); 


        // while not enough points on the layer, interpolate sensors to layer 
        while (layerSensCount < desiredSensCount && indx < Sensors.Length)
        {
            tooClose = false;
            // if sensor is too close to prev. sensor positions, skip
            for (int j = 0; j < indx; j++)      // TODO finde a beeter threshold for distance , check distance between points projected to same layer
                if (Vector3.Distance(Sensors[sensorLayerDistPermut[j]].Pos, Sensors[sensorLayerDistPermut[indx]].Pos) < Utilities.Scaled(0.1F) * ObjectGO.GetComponent<Renderer>().bounds.size.magnitude)
                {
                    tooClose = true;
                    indx++;
                    break;
                }
            if (tooClose)
                continue;

            if (sensorLayerDist[indx] == 0)
            {
                verts[layerSensCount] = new Vector3(Sensors[sensorLayerDistPermut[indx]].Pos.x, Sensors[sensorLayerDistPermut[indx]].Pos.y, Sensors[sensorLayerDistPermut[indx]].Pos.z);
                sensedValues[layerSensCount] = Sensors[sensorLayerDistPermut[indx]].Values[dataIndices[sensorLayerDistPermut[indx]]];
            }
            else
            {           
                verts[layerSensCount] = new Vector3(Sensors[sensorLayerDistPermut[indx]].Pos.x, Sensors[sensorLayerDistPermut[indx]].Pos.y, Sensors[sensorLayerDistPermut[indx]].Pos.z);
                sensedValues[layerSensCount] = Sensors[sensorLayerDistPermut[indx]].Values[dataIndices[sensorLayerDistPermut[indx]]];

            }
            indx++;
            layerSensCount++;
        }
        //Debug.Log(string.Format("({0},  {1},  {2})", sensedValues[0], sensedValues[1], sensedValues[2]));
        for (int i = 0; i < InterpolatedValues.GetLength(0); i++)
        {
            for (int j = 0; j < InterpolatedValues.GetLength(1); j++)
            {
                queryPoint = GetGridPosition(i, j, queryLayer);
                bary = Utilities.BarycentricCoordinates(verts[0], verts[1], verts[2], queryPoint);
                //interpVals[i, j, queryLayer] = bary[0] * sensedValues[0] + bary[1] * MCPosB + bary[2] * MCPosC;
                InterpolatedValues[i, j, queryLayer] = bary[0] * sensedValues[0] + bary[1] * sensedValues[1] + bary[2] * sensedValues[2];
            }

        }
    }

    // for all sensors, find the indices of the data that correxponds to the current time t 
    public int[] FindAllTimeStamps(DateTime t)                     // TODO is it better to find the time stamp in viz starter?  
    {
        int numSens = Sensors.Length;
        int[] tIndx = new int[numSens];
        bool dataAvailable = true;
        for (int s = 0; s < numSens; s++)
        {
            tIndx[s] = -1;
            if (Sensors[s].TimeStamps == null)
            {
                dataAvailable = false;
                break;
            }
            for (int i = 0; i < Sensors[s].TimeStamps.Length - 1; i++)
            {   
                // found exact timStamps 
                if (t == Utilities.ParseTime(Sensors[s].TimeStamps[i], Specs.DateFormat + " " + Specs.TimeFormat))
                {   
                    tIndx[s] = i;
                    break;
                }
                else if (t > Utilities.ParseTime(Sensors[s].TimeStamps[i], Specs.DateFormat + " " + Specs.TimeFormat))
                {   
                    // t between two timeStamps, take the smaller one                           
                    if (t < Utilities.ParseTime(Sensors[s].TimeStamps[i + 1], Specs.DateFormat + " " + Specs.TimeFormat))
                    {   
                        tIndx[s] = i;
                        break;
                    }
                    // needed to avoid missing the last element
                    else if (t == Utilities.ParseTime(Sensors[s].TimeStamps[i + 1], Specs.DateFormat + " " + Specs.TimeFormat))
                    {
                        tIndx[s] = i + 1;
                        break;
                    }

                }

            }
            if (tIndx[s] < 0)
            {
                dataAvailable = false;
                break;
            }
        }
        if (dataAvailable)
            return tIndx;
        else
            return null;
    }


}   
