using System;
using UnityEngine;
using UnityEngine.UI;

public class DataOnClick : MonoBehaviour {
    public Sensor sensor;
    //private GameObject panelGO;
    //private GameObject[] points;
    private GameObject backDrop;

    void OnMouseEnter()
    {
        //DrawPlot();
        //if (backDrop == null)
        LinearPlot();
    }
    private void OnMouseExit()
    {
        Destroy(backDrop);
    }
    private void LinearPlot()                   // TODO disable reflection, shadows, ... for all plot objects 
    {
        float[] vals = sensor.Values;
        if (vals == null)
            return;

        int count = vals.Length;
        string[] times = sensor.TimeStamps;
        string dateTimeFormat = Specs.DateFormat + " " + Specs.TimeFormat;

        Vector3[] verts = new Vector3[count];
        Vector3 minCorner, maxCorner;
        Camera cam = Camera.main;

        DateTime minTime = Utilities.ParseTime(times[0], dateTimeFormat);
        DateTime maxTime = Utilities.ParseTime(times[count - 1], dateTimeFormat);
        DateTime curTime;
        
        float x, y, z, mean=0, minY = Specs.MoistureLowerBound, maxY = Specs.MoistureUpperBound, marginFactor = 0.1F, lineWidth;

        backDrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
        backDrop.transform.localScale = new Vector3(0.8F, 0.8F, 1);
        backDrop.name = "Plot";


        var rndr = backDrop.GetComponent<Renderer>();
        rndr.material = Specs.TransparenMat;//(Material)Resources.Load(@"Materials\TransparentMat", typeof(Material));
        rndr.material.color = new Color(0.6F, 0.6F, 0.6F, 0.8F); ;
        rndr.GetComponent<Renderer>().sortingOrder = -1;
        //rndr.allowOcclusionWhenDynamic = false;

        minCorner = rndr.bounds.min + marginFactor * rndr.bounds.size;
        maxCorner = rndr.bounds.max - marginFactor * rndr.bounds.size;

        LineRenderer ln = backDrop.AddComponent<LineRenderer>();
        ln.positionCount = count;
        lineWidth = 0.015F * rndr.bounds.size.y;
        ln.startWidth = lineWidth;
        ln.endWidth = lineWidth;
        ln.material.color = new Color(1, 0, 0.2F);


        //GameObject text = new GameObject();
        //TextMesh t = text.AddComponent<TextMesh>();
        //t.text = "new text set";
        //t.fontSize = 30;
        //text.transform.SetParent(background.transform);

        for (int i = 0; i < count; i++)
        {
            mean += vals[i];
            // find position of vertices with respect to the backdrop at the center 
            curTime = Utilities.ParseTime(times[i], dateTimeFormat);
            x = Utilities.Map((float)(curTime - minTime).TotalMinutes, 0, (float)(maxTime - minTime).TotalMinutes, minCorner.x, maxCorner.x);
            y = Utilities.Map(vals[i], minY, maxY, minCorner.y, maxCorner.y);
            z = backDrop.transform.position.z;// - 0.008F; // line in front of backdrop
            verts[i] = new Vector3(x, y, z);
        }
        mean /= count;
        backDrop.transform.position += 0.08F*backDrop.transform.forward; // move the backdrop behind the line

        // move the backdrop with respect with respect to camera position and orientation
        backDrop.transform.rotation = Quaternion.LookRotation(cam.transform.right) * Quaternion.Euler(0, -90, 0);
        backDrop.transform.position = cam.transform.position + cam.transform.forward * 1.01F - cam.transform.right * 0.5F * rndr.bounds.size.x;

        // transfer the vertices with repect t 
        for (int i = 0; i < count; i++)
            ln.SetPosition(i, backDrop.transform.TransformPoint(verts[i]));

        DrawAxis(backDrop, minCorner, maxCorner, lineWidth, "Time", "Moisture Content [%]" ,minY.ToString(), maxY.ToString(), minTime.ToString("HH:mm"), maxTime.ToString("HH:mm"));

        AddText(backDrop, "Sensor ID= " + sensor.ID + "\n" + minTime.ToShortDateString() , 0.025F, TextAnchor.LowerCenter, TextAlignment.Center, new Vector3(0, maxCorner.y, 0));
    }

    void DrawAxis(GameObject parent, Vector3 min, Vector3 max, float lineW, string xTitle, string yTitle ,string minYLabel, string maxYLabel, string minXLabel, string maxXLabel)
    {
        Vector3[] verts = new Vector3[3];
        GameObject axisGO = new GameObject("Axis");
        LineRenderer axis = axisGO.AddComponent<LineRenderer>();
        axis.positionCount = verts.Length;
        axis.startWidth = lineW;
        axis.endWidth = lineW;
        axis.material.color = new Color(0, 0, 0F);

        verts[0] = parent.transform.TransformPoint(new Vector3(min.x, max.y, Math.Min(max.z, min.z)));
        verts[1] = parent.transform.TransformPoint(new Vector3(min.x, min.y, Math.Min(max.z, min.z)));
        verts[2] = parent.transform.TransformPoint(new Vector3(max.x, min.y, Math.Min(max.z, min.z)));

        axis.SetPositions(verts);
 
        axisGO.transform.SetParent(parent.transform);

        // add labels
        AddText(parent, minYLabel, 0.02F, TextAnchor.LowerRight, TextAlignment.Center, new Vector3(min.x, min.y, min.z));
        AddText(parent, maxYLabel, 0.02F, TextAnchor.UpperRight, TextAlignment.Center, new Vector3(min.x, max.y, min.z));
        AddText(parent, minXLabel, 0.02F, TextAnchor.UpperLeft, TextAlignment.Center, new Vector3(min.x, min.y, min.z));
        AddText(parent, maxXLabel, 0.02F, TextAnchor.UpperRight, TextAlignment.Center, new Vector3(max.x, min.y, max.z));

        AddText(parent, "\n"+xTitle, 0.025F, TextAnchor.UpperCenter, TextAlignment.Center, new Vector3(0, -(max.y-min.y)/2, max.z));
        GameObject tmpGO =AddText(parent, yTitle+ "\n\n", 0.025F, TextAnchor.MiddleCenter, TextAlignment.Center, new Vector3(min.x, 0, min.z));
        tmpGO.transform.localRotation = Quaternion.Euler(0, 0, 90);
        //tmpGO.transform.localPosition += Vector3.up * tmpGO.GetComponent<Renderer>().bounds.extents.x;
        //tmpGO.transform.localPosition += Vector3.up * tmpGO.GetComponent<TextM;

    }

    GameObject AddText(GameObject parent, string text, float charSize, TextAnchor anchor, TextAlignment alignment, Vector3 localPos)
    {
        GameObject GO = new GameObject(text);

        TextMesh labels = GO.AddComponent<TextMesh>();
        labels.characterSize = charSize;
        labels.anchor = anchor;
        labels.alignment = alignment;
        labels.text = text;
        labels.offsetZ = -0.03F;
        //labels.GetComponent<Renderer>().allowOcclusionWhenDynamic = false;


        GO.transform.SetParent(parent.transform);
        labels.transform.localPosition = localPos;
        //labels.transform.localPosition = new Vector3(0,0,0);
        labels.transform.localRotation = Quaternion.identity;

        return GO;
    }
    //void somethingWithText(){
    //GameObject canvasGO = GameObject.Find("Canvas").gameObject;
    //panelGO = new GameObject("ValuePanel", typeof(RectTransform));
    //panelGO.GetComponent<RectTransform>().SetParent( canvasGO.transform);


    //GameObject txtGo = ;
    //txtGo.transform.SetParent( panelGO.transform);
    //var txtComp = txtGo.AddComponent<Text>();
    //Text newText = transform.gameObject.AddComponent<Text>(); //This is the old code, it's generates a Null Exception
    //txtComp.text = sensor.Values[0].ToString();
    //txtComp.font = fontMessage;
    //txtComp.color = color;
    //txtComp.fontSize = 16;
    //txtComp.Add(txtGo);



    //Instantiate<GameObject>()
    //Debug.Log(sensor.Values[0]);
    //Destroy(this.gameObject);
    //}


    //private void DrawPlot()
    //{
    //    float[] vals = sensor.Values;
    //    if (vals == null)
    //        return;

    //    int count = vals.Length;
    //    string[] times = sensor.TimeStamps;
    //    string dateTimeFormat = Specs.DateFormat + " " + Specs.TimeFormat;

    //    DateTime minTime = Utilities.ParseTime(times[0], dateTimeFormat);
    //    DateTime maxTime = Utilities.ParseTime(times[count - 1], dateTimeFormat);
    //    DateTime curTime;
    //    TimeSpan timeSpn = maxTime - minTime;
    //    float x, y, z, marginFactor = 0.2F, pointScale;
    //    Color pointColor = new Color(0, 1, 0.2F);
    //    GameObject pointPrefab = (GameObject)Resources.Load(@"Prefabs\SphereLowPoly");
    //    points = new GameObject[count];
    //    background = GameObject.CreatePrimitive(PrimitiveType.Quad);
    //    background.transform.localScale = new Vector3(0.8F, 0.8F, 1);
    //    Camera cam = Camera.main;

    //    var rndr = background.GetComponent<Renderer>();

    //    rndr.material = (Material)Resources.Load(@"Materials\TransparentMat", typeof(Material));
    //    rndr.material.color = new Color(0.8F, 0.8F, 0.8F, 0.8F); ;
    //    Vector3 min = rndr.bounds.min;
    //    Vector3 max = rndr.bounds.max;
    //    pointScale = (max - min).x / (count);
    //    LineRenderer ln = background.AddComponent<LineRenderer>();
    //    ln.positionCount = count;
    //    ln.startWidth = pointScale / 10;
    //    ln.endWidth = pointScale / 10;
    //    //GameObject text = new GameObject();
    //    //TextMesh t = text.AddComponent<TextMesh>();
    //    //t.text = "new text set";
    //    //t.fontSize = 30;
    //    //text.transform.SetParent(background.transform);

    //    for (int i = 0; i < count; i++)
    //    {
    //        curTime = Utilities.ParseTime(times[i], dateTimeFormat);
    //        x = Utilities.Map((float)(curTime - minTime).TotalMinutes, 0, (float)timeSpn.TotalMinutes, min.x * (1 - marginFactor), max.x * (1 - marginFactor));
    //        y = Utilities.Map(vals[i], Specs.MoistureLowerBound, Specs.MoistureUpperBound, min.y * (1 - marginFactor), max.y * (1 - 2 * marginFactor));
    //        z = background.transform.position.z - 0.008F;
    //        //points[i] = GameObject.CreatePrimitive(PrimitiveType.Quad);
    //        points[i] = Instantiate<GameObject>(pointPrefab);

    //        points[i].transform.position = new Vector3(x, y, z);
    //        points[i].transform.localScale = Vector3.one * .0005F;
    //        //points[i].transform.rotation = Quaternion.LookRotation(points[i].transform.position - cam.transform.position);
    //        //points[i].transform.rotation = background.transform.rotation;
    //        points[i].GetComponent<Renderer>().material.color = pointColor;

    //        points[i].transform.SetParent(background.transform);

    //    }

    //     //background.transform.position = cam.transform.position + cam.transform.forward * 1.1F - cam.transform.right * 1.5F;
    //    //background.transform.rotation = Quaternion.LookRotation(background.transform.position - cam.transform.position, background.transform.up) * Quaternion.Euler(0,90,0);
    //    background.transform.rotation = Quaternion.LookRotation(cam.transform.right) * Quaternion.Euler(0, -90, 0);
    //    background.transform.position = cam.transform.position + cam.transform.forward * 1.01F - cam.transform.right * 0.5F * rndr.bounds.size.x;

    //    for (int i = 0; i < count; i++)
    //        ln.SetPosition(i, points[i].transform.position);
    //    ln.material.color = pointColor;


    //}

}
