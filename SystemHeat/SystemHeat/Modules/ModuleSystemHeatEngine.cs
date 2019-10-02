using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;

namespace SystemHeat
{
  /// <summary>
  /// A module to generate SystemHeat from an engine module
  /// </summary>
  public class ModuleSystemHeatEngine: PartModule
  {
    // This should be unique on the part
    [KSPField(isPersistant = false)]
    public string moduleID;

    // This should correspond to the related ModuleSystemHeat
    [KSPField(isPersistant = false)]
    public string systemHeatModuleID;

    // The engine module to generate heat off of
    [KSPField(isPersistant = false)]
    public string engineModuleID;

    // The current system temperature
    [KSPField(isPersistant = false)]
    public float systemTemperature = 0f;

    // The current system power
    [KSPField(isPersistant = false)]
    public float systemPower = 0f;

    [KSPField(isPersistant = false, guiActive = true, guiName = "Heat Generation")]
    public string systemHeatGeneration = "";

    [KSPField(isPersistant = false)]
    public Vector2 systemTemperatureRange = new Vector2(500f, 1000f);

    protected ModuleSystemHeat heatModule;
    protected ModuleEngines engineModule;

    public override string GetModuleDisplayName()
    {
        return "Engine Heat" ;
    }

    public override string GetInfo()
    {
        string msg = "";
        engines = part.GetComponents<ModuleEnginesFX>();

        msg += String.Format("Thermal Output: {0} kW\nThermal Output Temperature {1} K\nNominal Operating Temperature Range {2}-{3} K",
          systemPower.ToString("F1"),
          systemTemperature.ToString("F1"),
          systemTemperatureRange.x.ToString("F1"),
          systemTemperatureRange.y.ToString("F1"),
          );

        return msg;
    }

    public override void Start()
    {
      heatModule = Utils.FindNamedComponent<ModuleSystemHeat>(this.part, systemHeatModuleID);
      engineModule = Utils.FindNamedComponent<ModuleEngines>(this.part, engineModuleID);
    }

    public override void FixedUpdate()
    {
      if (HighLogic.LoadedSceneIsFlight)
      {
        if (heatModule != null && engineModule != null)
        {
          GenerateHeatFlight();
          UpdateSystemHeatFlight();
        }
      }
      if (HighLogic.LoadedSceneIsEditor)
      {
        if (heatModule != null && engineModule != null)
        {
          GenerateHeatEditor();
        }
      }
    }
    protected void GenerateHeatEditor()
    {

      float engineFraction = 0f;
      if (engineModule.isActiveAndEnabled)
          engineFraction = engineModule.thrustPercentage / 100f;

      systemHeatGeneration = String.Format("{0:F1} kW", engineFraction * systemPower);
      heatModule.AddFlux(this, systemTemperature, engineFraction* systemPower);

    }
    protected void GenerateHeatFlight()
    {
      float engineFraction = 0f;
      if (engineModule.isActiveAndEnabled)
          engineFraction = engine.GetCurrentThrust() / engine.GetMaxThrust();

      systemHeatGeneration = String.Format("{0:F1} kW", engineFraction * systemPower);
      heatModule.AddFlux(this, systemTemperature, engineFraction* systemPower);
    }
    protected void UpdateSystemHeatFlight()
    {
      if (engine.EngineIgnited)
      {
        if (heatModule.currentLoopTemperature > systemTemperatureRange.y)
        {
          ScreenMessages.PostScreenMessage(new ScreenMessage(String.Format("Engine system temperature was exceeded on {0}! Emergency shutdown!",
                                                             part.partInfo.title),
                                                             3.0f,
                                                             ScreenMessageStyle.UPPER_CENTER));
          engine.Events["Shutdown"].Invoke();
        }
      }
    }

  }
}
