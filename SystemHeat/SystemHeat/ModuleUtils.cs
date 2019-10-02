
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SystemHeat
{
  public static class Utils
  {

    public static T FindNamedComponent<T>(Part part, string moduleID)
    {
      T module;
      T[] candidates = part.GetComponents<T>();
      if (candidates == null || candidates.Length == 0)
      {
        Utils.LogError(String.Format("No module of type {0} could be found on {1}, everything will now explode", T, part.partInfo.name));
        return;
      }
      if (string.IsNullOrEmpty(moduleID))
      {
        Utils.LogWarning(String.Format("No moduleID was specified on {0}, using first instance of ModuleSystemHeat", part.partInfo.name));
        module = candidates[0].
      }
      else
      {
        foreach (var sys in candidates)
        {
          if (sys.moduleID == moduleID)
            module = sys;
        }
      }
      if (module == null)
        Utils.LogError(String.Format("No valid module of type {0} could be found on {1}, everything will now explode", T, part.partInfo.name));

      return module
    }
  }
}
