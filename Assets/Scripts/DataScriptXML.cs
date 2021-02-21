using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Xml;


public class DataScriptXML : DataScript {
    string sessID;
    string Day;
    public bool Connected { get; set; } 
    //public string NodeID { get; private set; } 

    // The server credentials are removed for the purpose of sharing on Github.
    // Therefore, no data will be visualized in this version
    static private string user = "*****";//server credentials removed on GitHub
    static private string pass = "*****"; 
    static private string jobid = "*****";
    static private string server = "https://analytics.*****.ca/api/";
    static private string loginAction = "?action=login&user_username="+ user + "&user_password="+pass;
    static private string logoutAction = "?action=logout";
    static private string nodelistAction = "?action=listNode&jobID=" + jobid;
    static private string sensordataAction = "?action=listSensorData&sensorID=" ;

    void Start()
    {
        Connected = false;
        Rows = 0;
        Columns = 1;
        NodeID = 25751;
        StartCoroutine(LogIn());
    }

    private void Update()
    {

        if (Connected)
        {
            GetSonsorRedingsOnDay("225527", "2017-12-10");
            Connected = false;
        }
    }
    private void GetSonsorRedingsOnDay(string sensorID, string day )
    {
        Day = day;
        if (Connected)
        {
            StartCoroutine(ReadSensorData(sensorID, Day, Day));
        }
    }
    IEnumerator ReadSensorData(string sensorID, string startDate, string endDate)
    {
            UnityWebRequest www = UnityWebRequest.Get(server + sensordataAction + sensorID + "&startDate=" +startDate + "&endDate="+endDate);
        www.SetRequestHeader("Cookie", string.Format("PHPSESSID={0}", sessID));
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
            XmlNodeList DateList = xmlDoc.GetElementsByTagName("timestamp");

            Rows = nodesList.Count;
            Values = new string[Rows,Columns];
            for(int i = 0; i< Rows;  i++)
            {
                Debug.Log(nodesList[i].InnerText);
                Values[i, Columns - 1] = nodesList[i].InnerText;
            }
            // Or retrieve results as binary data
            //byte[] results = www.downloadHandler.data;
        }

    }
    void OnApplicationQuit()
    {
        StartCoroutine(LogOut());
    }
    IEnumerator LogIn()
    {
        UnityWebRequest www = UnityWebRequest.Get(server+loginAction);
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

            // Show results as text
            xmlDoc.LoadXml(www.downloadHandler.text);
            XmlNodeList nodesList =  xmlDoc.GetElementsByTagName("PHPSESSID");
            if (nodesList.Count == 1)
            {
                sessID = nodesList[0].InnerText;
                Connected = true;
            }

        }
    }

    IEnumerator GetNodeList()
    {
        UnityWebRequest www = UnityWebRequest.Get(server+nodelistAction);
        www.SetRequestHeader("Cookie", string.Format("PHPSESSID={0}", sessID));
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(www.downloadHandler.text);
            XmlNodeList nodesList =  xmlDoc.GetElementsByTagName("name");
            foreach (XmlNode node in nodesList)
            {
                Debug.Log(node.InnerText);   

            }
            // Or retrieve results as binary data
            //byte[] results = www.downloadHandler.data;
        }

    }

    IEnumerator LogOut()
    {
        UnityWebRequest www = UnityWebRequest.Get(server+logoutAction);
        www.SetRequestHeader("Cookie", string.Format("PHPSESSID={0}", sessID));
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
            Debug.Log(www.downloadHandler.text);
    }


}
