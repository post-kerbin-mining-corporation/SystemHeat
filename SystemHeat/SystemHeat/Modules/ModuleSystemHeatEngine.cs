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
  public class ModuleSystemHeatEngine : PartModule
  {
    // This should be unique on the part
    [KSPField(isPersistant = false)]
    public string moduleID;

    // This should correspond to the related ModuleSystemHeat
    [KSPField(isPersistant = false)]
    public string systemHeatModuleID;

    // The engine module to generate heat off of
    [KSPField(isPersistant = false)]
    public string engineModuleID = "";

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
    [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "#LOC_SystemHeat_ModuleSystemHeatEngine_Field_HeatGeneration")]
    public string systemHeatGeneration = "";

    // UI field for showing heat
    [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "#LOC_SystemHeat_ModuleSystemHeatEngine_Field_Temperature")]
    public string systemTemperature = "";


    protected ModuleSystemHeat heatModule;
    protected ModuleEngines engineModule;
    protected MultiModeEngine multiModule;

    public override string GetModuleDisplayName()
    {
      return Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatEngine_DisplayName");
    }

    public override string GetInfo()
    {
      string msg = "";
      ModuleEnginesFX[] engines = part.GetComponents<ModuleEnginesFX>();
      msg += Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatEngine_PartInfo",
        systemPower.ToString("F0"),
        systemOutletTemperature.ToString("F0"),
        temperatureCurve.Curve.keys[temperatureCurve.Curve.keys.Length - 1].time.ToString("F0")
        );

      return msg;
    }

    public void Start()
    {
      heatModule = ModuleUtils.FindHeatModule(this.part, systemHeatModuleID);

      if (engineModuleID != "")
        engineModule = this.GetComponents<ModuleEngines>().ToList().Find(x => x.engineID == engineModuleID);

      if (engineModule == null)
        engineModule = this.GetComponent<ModuleEngines>();

      multiModule = this.GetComponent<MultiModeEngine>();

      Utils.Log("[ModuleSystemHeatEngine] Setup completed", LogType.Modules);

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
      if (multiModule != null)
      {
        if ((multiModule.runningPrimary && engineModuleID == multiModule.primaryEngineID) || (!multiModule.runningPrimary && engineModuleID == multiModule.secondaryEngineID))
        {
          engineFraction = engineModule.thrustPercentage / 100f;
          Fields["systemHeatGeneration"].guiActiveEditor = true;
          Fields["systemTemperature"].guiActiveEditor = true;
          heatModule.AddFlux(moduleID, systemOutletTemperature, engineFraction * systemPower, true);
          systemHeatGeneration = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatEngine_Field_HeatGeneration_Running", (engineFraction * systemPower).ToString("F0"));
        }
        else
        {

          systemHeatGeneration = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatEngine_Field_HeatGeneration_Running", (engineFraction * systemPower).ToString("F0"));
          heatModule.AddFlux(moduleID, 0f, engineFraction * systemPower, false);
          Fields["systemHeatGeneration"].guiActiveEditor = false;
          Fields["systemTemperature"].guiActiveEditor = false;

        }
      }
      else
      {

        engineFraction = engineModule.thrustPercentage / 100f;
        Fields["systemHeatGeneration"].guiActiveEditor = true;
        Fields["systemTemperature"].guiActiveEditor = true;
        heatModule.AddFlux(moduleID, systemOutletTemperature, engineFraction * systemPower, true);
        systemHeatGeneration = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatEngine_Field_HeatGeneration_Running", (engineFraction * systemPower).ToString("F0"));
      }

    }

    /// <summary>
    /// Generates heat in the flight scene
    /// </summary>
    protected void GenerateHeatFlight()
    {
      float engineFraction = 0f;

      if (engineModule.EngineIgnited || engineModule.requestedThrottle > 0f)
      {
        engineFraction = engineModule.requestedThrottle;
        heatModule.AddFlux(moduleID, systemOutletTemperature, engineFraction * systemPower, true);
        Fields["systemHeatGeneration"].guiActive = true;
        Fields["systemTemperature"].guiActive = true;
        systemHeatGeneration = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatEngine_Field_HeatGeneration_Running", (engineFraction * systemPower).ToString("F0"));
        systemTemperature = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatEngine_Field_Temperature_Running", heatModule.currentLoopTemperature.ToString("F0"), systemOutletTemperature.ToString("F0"));
      }
      else
      {
        heatModule.AddFlux(moduleID, 0f, 0f, false);
        Fields["systemHeatGeneration"].guiActive = false;
        Fields["systemTemperature"].guiActive = false;
        systemHeatGeneration = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatEngine_Field_HeatGeneration_Running", (engineFraction * systemPower).ToString("F0"));
        systemTemperature = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatEngine_Field_Temperature_Running", heatModule.currentLoopTemperature.ToString("F0"), systemOutletTemperature.ToString("F0"));
      }


    }
    protected void UpdateSystemHeatFlight()
    {
      if (engineModule.EngineIgnited)
      {
        if (heatModule.currentLoopTemperature > shutdownTemperature)
        {
          ScreenMessages.PostScreenMessage(
            new ScreenMessage(
              Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatEngine_Message_Overheat",
                                                             shutdownTemperature.ToString("F0"),
                                                             part.partInfo.title),
                                                             3.0f,
                                                             ScreenMessageStyle.UPPER_CENTER));
          engineModule.Events["Shutdown"].Invoke();

          Utils.Log("[ModuleSystemHeatEngine] Engine overheated: fired shutdown", LogType.Modules);

        }
      }
    }

  }
}
