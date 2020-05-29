using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SystemHeat
{
  public static class Utils
  {
    public static string logTag = "SystemHeat";

    public static void Log(string toLog)
    {
      Debug.Log(String.Format("[{0}]: {1}", logTag, toLog));
    }
    public static void LogWarning(string toLog)
    {
      Debug.LogWarning(String.Format("[{0}]: {1}", logTag, toLog));
    }
    public static void LogError(string toLog)
    {
      Debug.LogError(String.Format("[{0}]: {1}", logTag, toLog));
    }
  }

  public static class TransformDeepChildExtension
  {
    //Breadth-first search
    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
      Queue<Transform> queue = new Queue<Transform>();
      queue.Enqueue(aParent);
      while (queue.Count > 0)
      {
        var c = queue.Dequeue();
        if (c.name == aName)
          return c;
        foreach (Transform t in c)
          queue.Enqueue(t);
      }
      return null;
    }

    /*
    //Depth-first search
    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
        foreach(Transform child in aParent)
        {
            if(child.name == aName )
                return child;
            var result = child.FindDeepChild(aName);
            if (result != null)
                return result;
        }
        return null;
    }
    */
  }
}
