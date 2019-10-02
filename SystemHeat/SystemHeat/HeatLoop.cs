
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SystemHeat
{
  /// <summary>
  /// This class holds the configuration and simulation for a single Heat Loop
  /// </summary>
  public class HeatLoop
  {
    public int ID { get; set; }

    public float Temperature { get; set; }
    public float NominalTemperature { get; set; }
    public float NetFlux { get; set; }

    public float Volume { get; set; }

    public string CoolantName { get; set; }
    public CoolantType CoolantType;

    public List<ModuleSystemHeat> LoopModules
    {
      get {return modules}
    }

    protected List<ModuleSystemHeat> modules;

    /// <summary>
    /// Build a new HeatLoop
    /// </summary>
    /// <param name="id">The loop ID number</param>
    public HeatLoop(int id)
    {
      ID = id;
      modules = new List<ModuleSystemHeat>();
    }

    /// <summary>
    /// Build a new HeatLoop from a list of modules
    /// </summary>
    /// <param name="id">the loop ID</param>
    /// <param name="heatModules">the modules to add</param>
    public HeatLoop(int id, List<ModuleSystemHeat> heatModules)
    {
      ID = id;
      modules = new List<ModuleSystemHeat>();
      modules = heatModules;

      // Get loop properties set up
      CoolantName = GetCoolantType();
      CoolantType = SytemHeatSettings.GetCoolantType(CoolantName);
      Volume = CalculateLoopVolume();
      NominalTemperature = CalculateNominalTemperature();
    }

    /// <summary>
    /// Add a ModuleSystemHeat to this loop. Adding means adding Volume and recalculating the nominal temperature
    /// </summary>
    /// <param name="heatModule">the module to add</param>
    public AddHeatModule(ModuleSystemHeat heatModule)
    {
      volume += heatModule.volume;
      modules.Add(heatModule);
      heatModule.coolantName = CoolantName;
      // Recalculate the nominal temperature
      NominalTemperature = CalculateNominalTemperature();
    }

    /// <summary>
    /// Remove a ModuleSystemHeat to this loop. Removing means removing Volume and recalculating the nominal temperature
    /// </summary>
    /// <param name="heatModule">the module to remove</param>
    public RemoveHeatModule(ModuleSystemHeat heatModule)
    {
      volume -= heatModule.volume;
      modules.Remove(heatModule);
      // Recalculate the nominal temperature
      NominalTemperature = CalculateNominalTemperature();
    }

    /// <summary>
    /// Simulate this loop given a time warp level. First breaks the time step down if needed,
    /// then proceeds to iterate over all  time steps
    /// </summary>
    /// <param name="fixedDeltaTime">the current fixed delta time</param>
    public void Simulate(float fixedDeltaTime)
    {
      int numSteps = CalculateStepCount(fixedDeltaTime);
      float timeStep = fixedDeltaTime/(float)numSteps;

      for (int i = 0; i < numSteps; i++)
      {
          SimulateIteration(timeStep);
      }
    }

    /// <summary>
    /// Calculates the number of simulation steps we need to take if the time warp is high.
    /// This tries to be smart by assessing the rate of change in the loop - if it is low, we do not need to
    /// simulate rapidly. High rates need more simulation cycles
    /// </summary>
    /// <param name="fixedDeltaTime">the current fixed delta time</param>
    protected int CalculateStepCount(float fixedDeltaTime)
    {
      // Calculate the approximate predicted change in temp for the time step and the heat parameters
      float predictedDeltaTPerStep = CalculateNetFlux() / (Volume * CoolantType.Density * CoolantType.HeatCapacity) * fixedDeltaTime;

      return Mathf.Clamp((int)(predictedDeltaTPerStep/SystemHeatSettings.MaxDeltaTPerStep), SystemHeatSettings.MinSteps, SystemHeatSettings.MaxSteps);
    }

    /// <summary>
    /// Calculates the next flux of the loop
    /// </summary>
    protected float CalculateNetFlux()
    {
      float currentNetFlux = 0f;
      for (int i=0; i< modules.Count; i++)
      {
        currentNetFlux += modules[i].totalSystemFlux;
      }
      return currentNetFlux;
    }
    /// <summary>
    /// Simulates a single iteration of the heat loop. Broadly:
    /// 1) finds all fluxes in the Loop
    /// 2) calculates the temperature change of the loop
    /// 3) propagates all new values to the simulation members
    /// </summary>
    /// <param name="timeStep">the time step</param>
    void SimulateIteration(float timeStep)
    {
      // Calculate the loop net flux
      float currentNetFlux = CalculateNetFlux();
      float absFlux = Mathf.Abs(currentNetFlux);
      NetFlux = currentNetFlux;

      // Determine the ideal change in temperature
      float deltaTemperatureIdeal = NetFlux / (Volume * CoolantType.Density * CoolantType.HeatCapacity);

      // Flux has be be higher than a tolerance threshold in order to do things
      if (absFlux > SytemHeatSettings.AbsFluxThreshold)
      {
        // If flux is positive
        if (currentNetFlux > 0f)
        {
          Temperature += deltaTemperatureIdeal;
        } else if (currentNetFlux < 0f)
        {
          // If current temp is greater than nominal temp
          if (Temperature > NominalTemperature)
          {
            Temperature += deltaTemperatureIdeal;
          }
          // If current temp is lower than nominal temp
          if (Temperature < NominalTemperature)
          {
            Temperature += deltaTemperatureIdeal;
          }

        }
        else
        {
          // Unlikely case of perfectly stable flux
        }
      }
      // Propagate to all modules
      for (int i=0; i< modules.Count; i++)
      {
        modules[i].UpdateSimulationValues(NominalTemperature, Temperature, NetFlux);
      }
    }

    /// <summary>
    /// Gets the coolant type of the loop
    /// </summary>
    protected string GetCoolantType()
    {
      return modules[0].coolantName;
    }

    /// <summary>
    /// Gets the total volume of the loop based on its members
    /// </summary>
    protected float CalculateLoopVolume()
    {
      float total = 0f;
      for (int i=0; i< modules.Count; i++)
      {
        total += modules[i].volume;
      }
      return total;
    }

    /// <summary>
    /// Calculates the nomical temperature of the loop based on its members
    /// </summary>
    protected float CalculateNominalTemperature()
    {
      float temp = 0f;
      float totalPower = 0.001f;
      for (int i=0; i< modules.Count; i++)
      {
        if (modules[i].totalSystemFlux > 0f)
        {
          temp += modules[i].totalSystemTemperature * modules[i].totalSystemFlux;
          totalPower += modules[i].totalSystemFlux;
        }
      }
      return temp/totalPower;
    }
  }
}
