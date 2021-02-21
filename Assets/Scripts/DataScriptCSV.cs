using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
//using Math;

public class DataScriptCSV : DataScript {
    private char delimiter;
    string[] header;
    protected string text;

    //private string[,] values;
    //public int Columns { get; private set; }
    //public int Rows { get; protected set; }
    bool hasHeader;

    private string dataFileName;

    //int dataCount = 0;
    //List<float> dataValues = new List<float>();
    //List<string> dataNames = new List<string>();
    //private List<Dictionary<string, object>> pointList;


    public bool HasHeader
    {
        get { return hasHeader; }
    }

    //public string Path
    //{
    //    get { return System.IO.Path.Combine(Application.streamingAssetsPath, dataFileName); }
    //}

    public string DataTime(int i)
    {
            return Values[i, 0];
    }
    //public string DataName(int indx)
    //{
    //    return dataNames[indx];
    //}
    void Awake()
    { 
        Setup("7077_allSensors_hours_cleaned.csv",250,500,0,true,',');
    }
    IEnumerator loadStreamingAsset(string fileName)
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);

        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            WWW www = new WWW(filePath);
            yield return www;
            Debug.Log(www.text);
            text = www.text;
        }
        else
            text = System.IO.File.ReadAllText(filePath);
    }
    private void Setup(string fileName,int startRow, int rowCount, int clmnCount, bool hasHdr, char delim)
    {
        dataFileName = fileName;
        delimiter = delim;
        hasHeader = hasHdr;
        startRow = Math.Max(0, startRow-1);

        if (fileName.Contains("7077")) 
            NodeID = 25751;
        else if (fileName.Contains("9136"))
            NodeID = 25752;
            

        //StreamReader input = File.OpenText(Path.Combine(Application.streamingAssetsPath, dataFileName));

        StartCoroutine(loadStreamingAsset(dataFileName));

        //string path = System.IO.Path.Combine(Application.streamingAssetsPath, dataFileName);
        //WWW www2 = new WWW(path);
        //yield return www2;
        //path = www2.text;
        //Debug.Log(path);
        //if (path.Contains("://") || path.Contains(":///"))
        //{
        //    WWW www = new WWW(path);
        //    yield return www;
        //    path = www.text;
        //    Debug.Log(path);
        //}

        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string[] tokens = lines[0].Split(delimiter);

        Columns = clmnCount < 1 ? tokens.Length : Math.Min(clmnCount, tokens.Length);

        header = new string[Columns];
        //StreamReader input = null;
        if (hasHeader)
        {
            startRow = Math.Max(1,startRow);
            //header = tokens;
            for (int i = 0; i < Columns; i++)
                header[i] = tokens[i].Trim();
            //Console.WriteLine(i + ":   " + header[i]);
        }
        Rows = rowCount < 1 ? lines.Length - startRow : Math.Min(rowCount, lines.Length - startRow);

        Values = new string[Rows, Columns];

        for (int indx = 0; indx < Rows; indx++)
        {
            tokens = lines[indx + startRow].Split(delimiter);
            for (int i = 0; i < Columns; i++)
                Values[indx, i] = tokens[i].Trim();
        }
    }

    //public void readFile(string[] lines,int startRow) {
    //    //string line;
    //    int indx = startRow;// < 1 ? 0 : startRow - 1;
    //    string[] tokens;
    //    //int startRow = 0; // index for row and columns



        // create stream reader object
        //input = File.OpenText(Path.Combine(Application.streamingAssetsPath, dataFileName));

        // read in names and values
        //string names = input.ReadLine();

        //line = input.ReadLine();



        //// set configuration data fields
        //SetConfigurationDataFields(values);
    //}
    
    //public DataScript(string path)
    //    {
    //        string line;
    //        dataFileName = path;
    //        Console.WriteLine("data script");
    //        // read and save configuration data from file
    //        StreamReader input = null;
    //        try
    //        {
    //            // create stream reader object
    //            input = File.OpenText(Path.Combine(Application.streamingAssetsPath, dataFileName));

    //            // read in names and values
    //            //string names = input.ReadLine();

    //            line = input.ReadLine();

    //            while (line != null)
    //            {
    //                string[] tokens = line.Split(',');
    //                dataNames.Add(tokens[0]);
    //                dataValues.Add(float.Parse(tokens[1]));
    //                line = input.ReadLine();
    //            }

    //            //// set configuration data fields
    //            //SetConfigurationDataFields(values);
    //        }
    //        catch (Exception e)
    //        {
    //            Console.WriteLine(e.Message);
    //        }
    //        finally
    //        {
    //            // always close input file
    //            if (input != null)
    //            {
    //                input.Close();
    //            }
    //        }
    //    }


}
