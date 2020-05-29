
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SystemHeat
{

  public struct ResourceBaseRatio
  {
    public string ResourceName;
    public double ResourceRatio;

    public ResourceBaseRatio(string name, double ratio)
    {
      ResourceName = name;
      ResourceRatio = ratio;
    }
  }
  public static class ModuleUtils
  {
    /// <summary>
    /// Recalculates a ConversionRecipe based on some input scales
    /// </summary>
    /// <param name="fraction"></param>
    /// <param name="inputs"></param>
    /// <param name="outputs"></param>
    /// <param name="inputList"></param>
    /// <param name="outputList"></param>
    /// <param name="_recipe"></param>
    /// <returns></returns>
    public static ConversionRecipe RecalculateRatios(float fraction, List<ResourceBaseRatio> inputs, List<ResourceBaseRatio> outputs, List<ResourceRatio> inputList, List<ResourceRatio> outputList, ConversionRecipe _recipe)
    {

      for (int i = 0; i < _recipe.Inputs.Count; i++)
      {
        for (int j = 0; j < inputs.Count; j++)
        {
          if (inputs[j].ResourceName == inputList[i].ResourceName)
          {
            _recipe.Inputs[i] = new ResourceRatio(inputList[i].ResourceName, inputs[j].ResourceRatio * fraction, inputList[i].DumpExcess);

          }
        }
      }
      for (int i = 0; i < _recipe.Outputs.Count; i++)
      {
        for (int j = 0; j < outputs.Count; j++)
        {
          if (outputs[j].ResourceName == outputList[i].ResourceName)
          {
            _recipe.Outputs[i] = new ResourceRatio(outputList[i].ResourceName, inputs[j].ResourceRatio * fraction, outputList[i].DumpExcess);
          }
        }
      }

      return _recipe;
    }




    public static T FindNamedComponent<T>(Part part, string moduleID)
    {
      T module = default(T);
      T[] candidates = part.GetComponents<T>();
      if (candidates == null || candidates.Length == 0)
      {
        Utils.LogError(String.Format("[]: No module of type {0} could be found on {1}, everything will now explode", typeof(T).ToString(), part.partInfo.name));
        return module;
      }
      if (string.IsNullOrEmpty(moduleID))
      {
        Utils.LogWarning(String.Format("No moduleID was specified on {0}, using first instance of {1}", part.partInfo.name, typeof(T).ToString()));
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
