using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class DataScript : MonoBehaviour {

    //public int Count { get; protected set; }

    protected string[,] Values;
    public int Columns { get; protected set; }
    public int Rows { get; protected set; }
    public int NodeID { get; protected set; }
    public int SensorID { get; protected set; }

    //protected float[] Vals;
    void Start () {
		
	}
    public float Value(int i, int j)
    {
        float val;
        if (float.TryParse(Values[i, j], out val))
            return val;
        else
            return float.NaN;
    }

}
