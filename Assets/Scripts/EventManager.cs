//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Events;

///// <summary>
///// Manages connections between event listeners and event invokers
///// </summary>
//public static class EventManager
//{

//    // save lists of invokers and listeners
//    static List<SensedObject> invokers = new List<SensedObject>();
//    static List<UnityAction<int>> listeners = new List<UnityAction<int>>();


//    public static void AddInvoker(SensedObject invoker)
//    {
//        // add invoker to list and add all listeners to invoker
//        invokers.Add(invoker);
//        foreach (UnityAction<int> listener in listeners)
//        {
//            invoker.AddListener(listener);
//        }
//    }

//    public static void AddListener(UnityAction<int> handler)
//    {
//        // add listener to list and to all invokers
//        listeners.Add(handler);
//        foreach (SensedObject obj in invokers)
//        {
//            obj.AddListener(handler);
//        }
//    }

//}
