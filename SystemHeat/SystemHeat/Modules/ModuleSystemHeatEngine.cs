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
    public float systemOutletTemperature = 0f;

    // The current system power
    [KSPField(isPersistant = false)]
    public float systemPower = 0f;


    // Temeperature at which we scram
    [KSPField(isPersistant = false)]
    public float shutdownTemperature = 4000f;

    // Map loop temperature to system efficiency (0-1.0)
    [KSPField(isPersistant = false)]
    public FloatCurve temperatureCurve = new FloatCurve();

    // UI Fields
    // UI field for showing heat
    [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "Engine Heat Generation")]
    public string systemHeatGeneration = "";

    // UI field for showing heat
    [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "Engine System Temperature")]
    public string systemTemperature = "";


    protected ModuleSystemHeat heatModule;
    protected ModuleEngines engineModule;

    public override string GetModuleDisplayName()
    {
        return "Engine Heat" ;
    }

    public override string GetInfo()
    {
        string msg = "";
        ModuleEnginesFX[] engines = part.GetComponents<ModuleEnginesFX>();
        msg += String.Format("<b>Thermal Output:</b> {0} kW\n<b>System Temperature:</b> {1} K\n<b>Maximum Temperature</b> {2} K",
          systemPower.ToString("F0"),
          systemOutletTemperature.ToString("F0"),
          temperatureCurve.Curve.keys[temperatureCurve.Curve.keys.Length-1].time.ToString("F0")
          );

        return msg;
    }

    public void Start()
    {
      heatModule = ModuleUtils.FindNamedComponent<ModuleSystemHeat>(this.part, systemHeatModuleID);
      engineModule = ModuleUtils.FindNamedComponent<ModuleEngines>(this.part, engineModuleID);
      if (SystemHeatSettings.DebugModules)
      {
        Utils.Log("[ModuleSystemHeatEngine] Setup completed");
      }
    }

    public void FixedUpdate()
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


    /// <summary>
    /// Generates heat in the editor scene
    /// </summary>
    protected void GenerateHeatEditor()
    {

      float engineFraction = 0f;
      if (engineModule.isActiveAndEnabled)
          engineFraction = engineModule.thrustPercentage / 100f;

      systemHeatGeneration = String.Format("{0:F1} kW", engineFraction * systemPower);
      heatModule.AddFlux(moduleID, systemOutletTemperature, engineFraction* systemPower);

    }

    /// <summary>
    /// Generates heat in the flight scene
    /// </summary>
    protected void GenerateHeatFlight()
    {
      float engineFraction = 0f;
      if (engineModule.isActiveAndEnabled)
      {
        engineFraction = engineModule.requestedThrottle;
        heatModule.AddFlux(moduleID, systemOutletTemperature, engineFraction * systemPower);
      } else
      {
        heatModule.AddFlux(moduleID, 0f, engineFraction * systemPower);
      }
      systemHeatGeneration = String.Format("{0:F0} kW", engineFraction * systemPower);
      systemTemperature = String.Format("{0:F0}/{1:F0} K", heatModule.currentLoopTemperature, systemOutletTemperature);

    }
    protected void UpdateSystemHeatFlight()
    {
      if (engineModule.EngineIgnited)
      {
        if (heatModule.currentLoopTemperature > shutdownTemperature)
        {
          ScreenMessages.PostScreenMessage(
            new ScreenMessage(
              String.Format("Engine system maximum temperature of {0} K was exceeded on {1}! Emergency shutdown!",
                                                             shutdownTemperature.ToString("F0"),
                                                             part.partInfo.title),
                                                             3.0f,
                                                             ScreenMessageStyle.UPPER_CENTER));
          engineModule.Events["Shutdown"].Invoke();
          if (SystemHeatSettings.DebugModules)
          {
            Utils.Log("[ModuleSystemHeatEngine] Engine overheated: fired shutdown");
          }
        }
      }
    }

  }
}
