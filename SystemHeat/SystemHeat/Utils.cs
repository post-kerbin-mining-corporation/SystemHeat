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
}
