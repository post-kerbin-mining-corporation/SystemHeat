using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SystemHeat
{
  public class SystemHeatSimulator
  {
    public Dictionary<int,HeatLoop> HeatLoops { get; private set;}

    public SystemHeatSimulator()
    {
    }
    /// TODO: This should update the simulator in place instead of reset completely
    public void Refresh(List<Part> parts)
    {
      Reset(parts);
    }
    public void Reset(List<Part> parts)
    {
      HeatLoops = new Dictionary<int, HeatLoop>();
      List<ModuleSystemHeat> heatModules = new List<ModuleSystemHeat>();

      for (int i = parts.Count - 1; i >= 0; --i)
      {
        Part part = parts[i];
        heatModules.AddRange(part.GetComponents<ModuleSystemHeat>().ToList());
      }

      if (SystemHeatSettings.DebugSimulation)
        Utils.Log(String.Format("[SystemHeatSimulator]: Building heat loops from {0} ModuleSystemHeat modules", heatModules.Count.ToString()));


      foreach (ModuleSystemHeat heatModule in heatModules)
      {
        AddHeatModule(heatModule);
      }
    }

    /// <summary>
    /// Do simulation in flight
    /// </summary>
    public virtual void Simulate()
    {

      if ( HeatLoops != null)
      {
        foreach (KeyValuePair<int, HeatLoop> kvp in HeatLoops)
        {
          kvp.Value.Simulate(TimeWarp.fixedDeltaTime);
        }
      }
    }

    /// <summary>
    /// Do simulation in the editor
    /// </summary>
    public virtual void SimulateEditor()
    {

      if (HeatLoops != null)
      {
        foreach (KeyValuePair<int, HeatLoop> kvp in HeatLoops)
        {
          kvp.Value.Simulate(SystemHeatSettings.SimulationRateEditor);
        }
      }
    }

    /// <summary>
    /// Add a heat module to the simulation
    /// </summary>
    /// <param name="module">the module to add</param>
    public void AddHeatModule(ModuleSystemHeat module)
    {
      AddHeatModuleToLoop(module.currentLoopID, module);
    }

    /// <summary>
    /// Add a heat module to a specific HeatLoop
    /// </summary>
    /// <param name="loopID">the loop to add to</param>
    /// <param name="module">the module to add</param>
    public void AddHeatModuleToLoop(int loopID, ModuleSystemHeat module)
    {
      // Build a new heat loop as needed
      if (!HeatLoops.ContainsKey(loopID))
      {
        HeatLoops.Add(loopID, new HeatLoop(loopID));
        if (SystemHeatSettings.DebugSimulation)
          Utils.Log(String.Format("[SystemHeatSimulator]: Created new Heat Loop {0}", loopID));
      }
      HeatLoops[loopID].AddHeatModule(module);

      if (SystemHeatSettings.DebugSimulation)
        Utils.Log(String.Format("[SystemHeatSimulator]: Added module {0} to Heat Loop {1}", module.moduleID, loopID));
    }

    /// <summary>
    /// Remove a part with all its heat modules from the system
    /// </summary>
    /// <param name="part">the part to remove</param>
    public void RemovePart(Part part)
    {
      ModuleSystemHeat[] heatModules = part.GetComponents<ModuleSystemHeat>();
      foreach (ModuleSystemHeat module in heatModules)
      {
        RemoveHeatModule(module);
      }
    }

    /// <summary>
    /// Remove a heat module from the system
    /// </summary>
    /// <param name="module">the module to remove</param>
    public void RemoveHeatModule(ModuleSystemHeat module)
    {
      RemoveHeatModuleFromLoop(module.currentLoopID, module);
    }

    /// <summary>
    /// Remove a heat module from a specific HeatLoop
    /// </summary>
    /// <param name="loopID">the loop to remove from</param>
    /// <param name="module">the module to remove</param>
    public void RemoveHeatModuleFromLoop(int loopID, ModuleSystemHeat module)
    {
      HeatLoops[loopID].RemoveHeatModule(module);

      if (SystemHeatSettings.DebugSimulation)
        Utils.Log(String.Format("[SystemHeatSimulator]: Removed module {0} from Heat Loop {1}", module.moduleID, loopID));

      if (HeatLoops[loopID].LoopModules.Count == 0)
      {
        HeatLoops.Remove(loopID);
        if (SystemHeatSettings.DebugSimulation)
          Utils.Log(String.Format("[SystemHeatSimulator]: Heat Loop {0} has no more members, removing", loopID));
      }
    }
  }
}
