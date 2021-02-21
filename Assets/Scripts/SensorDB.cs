using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Xml;

public class SensorDB : ScriptableObject {

    //public string DateFormat = "YYYY-MM-DD";
    private string SessID;
    public string Day { get; private set; }
    public bool Connected { get; private set; }
    //public bool Busy { get; private set; }
    public string NodeID { get; private set; }

    //private float[] Values;

    static private string user = "*****";//server credentials removed on GitHub
    static private string pass = "*****"; 
    //private string jobid = "*****";
    private string server = "https://analytics.*****.ca/api/"; 
    private string loginAction = "?action=login&user_username=" + user + "&user_password=" + pass;
    private string logoutAction = "?action=logout";
    //private string nodelistAction = "?action=listNode&jobID=";
    private string sensordataAction = "?action=listSensorData&sensorID=";


    public void Init()
    {
        Connected = false;
        
        //Busy = false;

        //Rows = 0;
        //Columns = 1;
        //NodeID = "25751";
        //StartCoroutine(LogIn());
    }

    public IEnumerator ReadSensorData(Sensor sensor, string startDate, string endDate)
    {
        //Busy = true;
        sensor.AwaitingData = true;
        UnityWebRequest www = UnityWebRequest.Get(server + sensordataAction + sensor.ID + "&startDate=" + startDate + "&endDate=" + endDate);
        www.SetRequestHeader("Cookie", string.Format("PHPSESSID={0}", SessID));
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(www.downloadHandler.text);
            XmlNodeList nodesList = xmlDoc.GetElementsByTagName("engUnit");
            XmlNodeList dateList = xmlDoc.GetElementsByTagName("timestamp");

            //Rows = nodesList.Count;
            float[] values = new float[nodesList.Count];
            string[] timeStamps =  new string[nodesList.Count];
            float tmpVal;
            for (int i = 0; i < values.Length; i++)
            {
                //Debug.Log(nozesList[i].InnerText);

                timeStamps[i] = dateList[i].InnerText;
                if (float.TryParse(nodesList[i].InnerText, out tmpVal))
                    values[i] = float.Parse(nodesList[i].InnerText);
                else
                    values[i] = float.NaN;

                

            }
            sensor.Values = new float[values.Length];
            Array.Copy(values,sensor.Values, values.Length);
            sensor.TimeStamps = new String[timeStamps.Length];
            Array.Copy(timeStamps, sensor.TimeStamps, timeStamps.Length);
            //Debug.Log("End Reading " + sensor.ID + " #" + values.Length);
            // Or retrieve results as binary data
            //byte[] results = www.downloadHandler.data;
        }

        sensor.AwaitingData = false;
        //Busy = false;
    }

    public IEnumerator LogIn() // made public to 
    {
        //Busy = true;
        UnityWebRequest www = UnityWebRequest.Get(server + loginAction);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            XmlDocument xmlDoc = new XmlDocument();
            //Dictionary<string, string> dict = www.GetResponseHeaders();
            //foreach (var kvp in dict)
            //{

            //    Debug.Log( kvp.Key  +":   " + kvp);  
            //}

            xmlDoc.LoadXml(www.downloadHandler.text);
            XmlNodeList nodesList = xmlDoc.GetElementsByTagName("PHPSESSID");
            if (nodesList.Count == 1)
            {
                SessID = nodesList[0].InnerText;
                Connected = true;
            }

        }
        //Busy = false;
    }

    //IEnumerator GetNodeList()
    //{
    //    UnityWebRequest www = UnityWebRequest.Get(server + nodelistAction + NodeID);
    //    www.SetRequestHeader("Cookie", string.Format("PHPSESSID={0}", sessID));
    //    yield return www.SendWebRequest();

    //    if (www.isNetworkError || www.isHttpError)
    //    {
    //        Debug.Log(www.error);
    //    }
    //    else
    //    {
    //        XmlDocument xmlDoc = new XmlDocument();
    //        xmlDoc.LoadXml(www.downloadHandler.text);
    //        XmlNodeList nodesList = xmlDoc.GetElementsByTagName("name");
    //        foreach (XmlNode node in nodesList)
    //        {
    //            Debug.Log(node.InnerText);

    //        }
    //        // Or retrieve results as binary data
    //        //byte[] results = www.downloadHandler.data;
    //    }

    //}

    public IEnumerator LogOut()
    {
        //Busy = true;
        UnityWebRequest www = UnityWebRequest.Get(server + logoutAction);
        www.SetRequestHeader("Cookie", string.Format("PHPSESSID={0}", SessID));
        yield return www.SendWebRequest();
 
        if (www.isNetworkError || www.isHttpError)

            Debug.Log(www.error);
        else
        {
            Debug.Log(www.downloadHandler.text);
            Connected = false;
        }
        //Busy = false;
    }


}
