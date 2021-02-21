using System;
using UnityEngine;

public class SensedObjList : ScriptableObject {

    public static SensedObject[] SensedObjs;

    public static void InitList()
    {

        string n = "PEAVY general monitoring plan_with string pot wall 2018.09.22>>";

        string[] sensNames = new string[] { n+"Component#32 5", n+"Component#32 4", n + "Component#32 7", n + "Group 2189>>Component#25", n + "Group 2189>>Component#28", n + "Component#32" };
        string[] sensIDs = new string[] { "225528", "225529", "225530", "225527" , "225531", "225533" }; // TODO double check the ids
        string[] sensUnits = new string[] { "MC", "MC", "MC", "MC", "MC", "MC" };

        int[] layers = new int[] {0, 1, 2, 5, 6, 3 }; //  0-based, 0-> ply 1 ,...
        int numLayers = 7;
        Sensor[] tmpSens = new Sensor[sensNames.Length];
        for (int s = 0; s < tmpSens.Length; s++)
        {
            tmpSens[s] = ScriptableObject.CreateInstance<Sensor>();
            tmpSens[s].Setup(sensNames[s], sensIDs[s], sensUnits[s], layers[s]);
        }

        SensedObjs = new SensedObject[1];
        SensedObjs[0] = ScriptableObject.CreateInstance<SensedObject>();
        SensedObjs[0].Setup(n+"Group#776", "25751", tmpSens, numLayers);
    }
 }
