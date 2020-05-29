using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SystemHeat;
using KSP.Localization;

namespace SystemHeat.UI
{
  public class UIDebugLoopsView: UIWidget
  {
    protected SystemHeatDebugUI dataHost;

    #region GUI Strings
    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="uiHost">The instance of the main UI class to link to </param>
    public UIDebugLoopsView(SystemHeatDebugUI uiHost):base(uiHost)
    {
      dataHost = uiHost;
    }

    public void Draw()
    {
      if (dataHost.Simulator.HeatLoops != null)
      {
        for (int i = 0; i < dataHost.Simulator.HeatLoops.Count; i++)
        {
          DrawLoop(dataHost.Simulator.HeatLoops[i]);
        }
      }
    }


    protected void DrawLoop(HeatLoop loop)
    {
      GUILayout.BeginVertical(UIHost.GUIResources.GetStyle("block_background"));
      GUILayout.BeginHorizontal();
      GUILayout.Label(String.Format("ID {0}",loop.ID));

      GUILayout.BeginVertical();
      GUILayout.Label(String.Format("Loop Temperature {0:F1}/{1:F1} K", loop.Temperature, loop.NominalTemperature), UIHost.GUIResources.GetStyle("negative_category_header"));
      GUILayout.Label(String.Format("Loop Flux {0} kW",loop.NetFlux), UIHost.GUIResources.GetStyle("negative_category_header"));
      GUILayout.EndVertical();
      GUILayout.BeginVertical();
      GUILayout.Label(String.Format("Using coolant {0}",loop.CoolantName), UIHost.GUIResources.GetStyle("negative_category_header"));
      GUILayout.Label(String.Format("Volume {0}",loop.Volume), UIHost.GUIResources.GetStyle("negative_category_header"));
      GUILayout.EndVertical();
      GUILayout.BeginVertical();
      GUILayout.Label(String.Format("Number of timesteps {0}", loop.numSteps), UIHost.GUIResources.GetStyle("negative_category_header"));
      GUILayout.Label(String.Format("Time step {0}", loop.timeStep), UIHost.GUIResources.GetStyle("negative_category_header"));
      GUILayout.EndVertical();

 
      GUILayout.EndHorizontal();
      foreach (ModuleSystemHeat mod in loop.LoopModules)
      {

        GUILayout.Label(String.Format("Part {0}: Total Flux: {1} kW Total Temp: {2}K Volume: {3}", mod.part.partInfo.title, mod.totalSystemFlux, mod.totalSystemTemperature, mod.volume), UIHost.GUIResources.GetStyle("negative_category_header"));
      }
      GUILayout.EndVertical();
    }


    public void Update()
    {

    }
  }
}
