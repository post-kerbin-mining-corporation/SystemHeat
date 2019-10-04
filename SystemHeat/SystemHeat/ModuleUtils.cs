
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SystemHeat
{
  public static class ModuleUtils
  {

    public static T FindNamedComponent<T>(Part part, string moduleID)
    {
      T module = default(T);
      T[] candidates = part.GetComponents<T>();
      if (candidates == null || candidates.Length == 0)
      {
        Utils.LogError(String.Format("No module of type {0} could be found on {1}, everything will now explode", typeof(T).ToString(), part.partInfo.name));
        return module;
      }
      if (string.IsNullOrEmpty(moduleID))
      {
        Utils.LogWarning(String.Format("No moduleID was specified on {0}, using first instance of ModuleSystemHeat", typeof(T).ToString(), part.partInfo.name));
        module = candidates[0];
      }
      else
      {
        foreach (var sys in candidates)
        {
          if ((string)sys.GetType().GetProperty("moduleID").GetValue(sys, null) == moduleID)
            module = sys;
        }
      }
      if (module == null)
        Utils.LogError(String.Format("No valid module of type {0} could be found on {1}, everything will now explode", typeof(T).ToString(), part.partInfo.name));

      return module;
    }
  }
}
