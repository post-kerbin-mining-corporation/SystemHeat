using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace SystemHeat
{

  public enum LogType
  {
    UI,
    Settings,
    Modules,
    Overlay,
    Simulator,
    Any
  }
  public static class Utils
  {
    public static string logTag = "SystemHeat";


    /// <summary>
    /// Log a message with the mod name tag prefixed
    /// </summary>
    /// <param name="str">message string </param>
    public static void Log(string str, LogType logType)
    {
      bool doLog = false;
      if (logType == LogType.Settings && SystemHeatSettings.DebugSettings) doLog = true;
      if (logType == LogType.UI && SystemHeatSettings.DebugUI) doLog = true;
      if (logType == LogType.Modules && SystemHeatSettings.DebugModules) doLog = true;
      if (logType == LogType.Overlay && SystemHeatSettings.DebugOverlay) doLog = true;
      if (logType == LogType.Simulator && SystemHeatSettings.DebugSimulation) doLog = true;
      if (logType == LogType.Any) doLog = true;

      if (doLog)
        Debug.Log(String.Format("[{0}]{1}", logTag, str));
    }

    public static void Log(string str)
    {
      Debug.Log(String.Format("[{0}]{1}", logTag, str));
    }

      public static void LogWarning(string toLog)
    {
      Debug.LogWarning(String.Format("[{0}]{1}", logTag, toLog));
    }
    public static void LogError(string toLog)
    {
      Debug.LogError(String.Format("[{0}]{1}", logTag, toLog));
    }


    public static string ToSI(float d, string format = null)
    {
      if (d == 0.0)
        return d.ToString(format);

      char[] incPrefixes = new[] { 'k', 'M', 'G', 'T', 'P', 'E', 'Z', 'Y' };
      char[] decPrefixes = new[] { 'm', '\u03bc', 'n', 'p', 'f', 'a', 'z', 'y' };

      int degree = Mathf.Clamp((int)Math.Floor(Math.Log10(Math.Abs(d)) / 3), -8, 8);
      if (degree == 0)
        return d.ToString(format);

      double scaled = d * Math.Pow(1000, -degree);

      char? prefix = null;

      switch (Math.Sign(degree))
      {
        case 1: prefix = incPrefixes[degree - 1]; break;
        case -1: prefix = decPrefixes[-degree - 1]; break;
      }

      return scaled.ToString(format) + " " + prefix;
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
