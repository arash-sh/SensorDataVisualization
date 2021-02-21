using System;
using UnityEngine;

public class Sensor : ScriptableObject{
    public string[] TimeStamps= {"2018-05-13 10:00:00"}; // dummy default value
    public float[] Values;
    public bool AwaitingData;

    public GameObject GO { get; private set; }
    public string Name { get; private set; }
    public string ID{ get; private set; }
    public string Unit{ get; private set; }
    public Vector3 Pos { get { return GO.GetComponent<Renderer>().bounds.center; } private set { } }
    public int Layer { get; private set; }

    

    public void Setup(string name, string id, string unit, int layer)
    {
        Name = name;
        ID = id;
        Unit = unit;
        GO = Utilities.GetGameObjectByName(Name, new string[] { ">>" });
        Layer = layer;
        GO.transform.localScale = new Vector3(  20F, 20F, 20F);

        GO.GetComponent<Renderer>().material.color = Color.cyan;
        GO.AddComponent<DataOnClick>();                       
        GO.GetComponent<DataOnClick>().sensor = this;

        GO.AddComponent<BoxCollider>();
        GO.GetComponent<BoxCollider>().bounds.SetMinMax(GO.GetComponent<Renderer>().bounds.min, GO.GetComponent<Renderer>().bounds.max);
        

    }

}
