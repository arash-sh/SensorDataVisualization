using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class VizStarter : MonoBehaviour {

    private SensorDB DBConnection;
    private GameObject TxtGO;
    private GameObject Avatar;
    private DateTime StartTime = Specs.StartTime;//new DateTime(2017, 12, 25, 0, 0, 0);
    private DateTime EndTime = Specs.EndTime;//new DateTime(2017, 12, 25, 5, 0, 0);
    private DateTime CurrentTime;
    private readonly string DateTimeFormat = Specs.DateFormat + " " +Specs.TimeFormat;
    private Visualizer[] vizObjs;
    private float frameTime = 0;

    private GameObject ParticleGO;

    void Start () {
        //Application.targetFrameRate = Specs.FrameRate;     // not working on all platforms?

        CurrentTime = StartTime.Date.AddHours(-Specs.TimeHourStep);
        DBConnection = ScriptableObject.CreateInstance<SensorDB>();
        DBConnection.Init();
        StartCoroutine(DBConnection.LogIn());
        SensedObjList.InitList();

        vizObjs = new Visualizer[SensedObjList.SensedObjs.Length];
        for (int i = 0; i < SensedObjList.SensedObjs.Length; i++)
        {
            vizObjs[i] = ScriptableObject.CreateInstance<Visualizer>();
        }

        TxtGO = GameObject.Find("Canvas").transform.Find("DatePanel").gameObject.transform.Find("Text").gameObject;

   
    }
	
	// Update is called once per frame
	void Update () {
        frameTime += Time.deltaTime;
        if (frameTime >= 1F/Specs.FrameRate && CurrentTime<=EndTime)  
        {
            frameTime = 0;
            CurrentTime =CurrentTime.AddHours(Specs.TimeHourStep);  
            TxtGO.GetComponent<TextMeshProUGUI>().text = CurrentTime.ToString(DateTimeFormat);
            TxtGO.GetComponentInParent<Image>().enabled = true;
            //Debug.Log(CurrentTime.ToString(DateTimeFormat));

            Camera cam = gameObject.GetComponent<Camera>();
            Ray r = new Ray(cam.transform.position, cam.transform.forward); // cast ray in forward direction of camera

            for (int i = 0; i < SensedObjList.SensedObjs.Length; i++)
            {
                //if (SensorObjList.SensedObjs[i].GO.GetComponent<Renderer>().bounds.IntersectRay(r)) // object is in front of cam
                if (SensedObjList.SensedObjs[i].Selected)                   
                {
                    for (int s = 0; s < SensedObjList.SensedObjs[i].Sensors.Length; s++)
                    {
                        if (!DataAvailable(SensedObjList.SensedObjs[i].Sensors[s]))// data from sensor s for the current time is not loaded
                        {
                            //Debug.Log("-------Not loaded  " + CurrentDate.ToString(DateTimeFormat));
                            if (DBConnection.Connected && !SensedObjList.SensedObjs[i].Sensors[s].AwaitingData)
                            {
                                //Debug.Log("Loading "+ SensorObjList.SensedObjs[i].Sensors[s].ID + "  for date " + CurrentTime.ToString(Specs.DateFormat));
                                StartCoroutine(DBConnection.ReadSensorData(SensedObjList.SensedObjs[i].Sensors[s], CurrentTime.ToString(Specs.DateFormat), CurrentTime.ToString(Specs.DateFormat)));
                            }
                        }
                        else
                        {
                            //Debug.Log("-------Available" + s);
                        }

                    }
                    vizObjs[i].Visualize(SensedObjList.SensedObjs[i], CurrentTime);

                }
                else
                {
                    //Debug.Log("No intersection with ray:" + i + "  " + r.ToString());
                    SensedObjList.SensedObjs[i].ObjectGO.GetComponent<Renderer>().enabled = true;
                                                                                    // TODO destroy the quad of visualization
                }
           }

            if (Specs.ThisVizMode == Specs.VIZ_MODE.PARTICLE_LOOSE)
            {
                if (ParticleGO == null)
                {
                    ParticleGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    ParticleGO.transform.position = new Vector3(181, 2, 6);
                    ParticleGO.transform.localScale = new Vector3(6, 2, 6);
                    Destroy(ParticleGO.GetComponent<BoxCollider>());
                    ParticleGO.GetComponent<MeshRenderer>().enabled = false;
                }
                CreateLooseParticles(ParticleGO);
            }
            else if (Specs.ThisVizMode != Specs.VIZ_MODE.PARTICLE_LOOSE)
                Destroy(ParticleGO);
        }
        //ScriptableObject.Destroy(viz);
    }


    void CreateLooseParticles(GameObject go)
    {
        ParticleSystem prtclSys;
        if (!(prtclSys = go.GetComponent<ParticleSystem>()))
        {
            prtclSys = go.AddComponent<ParticleSystem>();
             var shp = prtclSys.shape;
            shp.enabled = true;
            shp.shapeType = ParticleSystemShapeType.Box;
            shp.position = go.GetComponent<Renderer>().bounds.center - go.transform.position;
 
            var main = prtclSys.main;
            main.startSpeed = 0F;
            main.startSize =0.5F;
            main.startLifetime = 3F;
            main.prewarm = true;
            main.startColor = Color.black;


            var emssn = prtclSys.emission;
            emssn.rateOverTime = 60F;

            var rndr = prtclSys.GetComponent<ParticleSystemRenderer>();
            GameObject tmpGO = Instantiate<GameObject>(Specs.LowPolySpherePrefab);
            rndr.renderMode = ParticleSystemRenderMode.Mesh;
            rndr.mesh = tmpGO.GetComponent<MeshFilter>().mesh;
            Destroy(tmpGO);
        }


        int count = prtclSys.particleCount;
        ParticleSystem.Particle[] prtcls = new ParticleSystem.Particle[count];
        count = prtclSys.GetParticles(prtcls);

        

    }

    private bool DataAvailable(Sensor sensor)
    {
        if (sensor.TimeStamps == null)
            return false;

        DateTime time1, time2;

        time1 = Utilities.ParseTime(sensor.TimeStamps[0], DateTimeFormat);
        time2 = Utilities.ParseTime(sensor.TimeStamps[sensor.TimeStamps.Length - 1], DateTimeFormat);

        if (CurrentTime>= time1 && CurrentTime <= time2)
            return true;
        else
            return false;
    }


    void OnApplicationQuit()
    {
        StartCoroutine(DBConnection.LogOut());
    }
}
