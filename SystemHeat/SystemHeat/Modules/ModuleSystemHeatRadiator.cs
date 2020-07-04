using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;

namespace SystemHeat
{
  /// <summary>
  /// The connection between a stock ModuleActiveRadiator and the SystemHeat system
  /// </summary>
  public class ModuleSystemHeatRadiator: ModuleActiveRadiator
  {
    // This should be unique on the part and identifies the module
    [KSPField(isPersistant = false)]
    public string moduleID;

    // This should correspond to the related ModuleSystemHeat. If not specified, the first module will be found
    [KSPField(isPersistant = false)]
    public string systemHeatModuleID;

    // This should map system temperature to heat radiated
    [KSPField(isPersistant = false)]
    public FloatCurve temperatureCurve = new FloatCurve();

    // Current status GUI string
    [KSPField(isPersistant = false, guiActive = false, guiActiveEditor = false, guiName = "#LOC_SystemHeat_ModuleSystemHeatRadiator_RadiatorStatus_Title")]
    public string RadiatorStatus = "Offline";

    // Current efficiency GUI string
    [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "#LOC_SystemHeat_ModuleSystemHeatRadiator_RadiatorEfficiency_Title")]
    public string RadiatorEfficiency = "-1%";


    protected ModuleSystemHeat heatModule;

    public override void Start()
    {
      base.Start();
      heatModule = this.GetComponents<ModuleSystemHeat>().ToList().Find(x => x.moduleID == systemHeatModuleID);
      if (heatModule == null)
        heatModule.GetComponent<ModuleSystemHeat>();
      if (SystemHeatSettings.DebugModules)
      {
        Utils.Log("[ModuleSystemHeatRadiator] Setup completed");
      }
    }
    public override string GetInfo()
    {
      // Need to update this to strip the CoreHeat stuff from it
      string message = base.GetInfo();
      message += Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatRadiator_PartInfo", 
        temperatureCurve.Curve.keys[0].time.ToString("F0"), 
        temperatureCurve.Evaluate(temperatureCurve.Curve.keys[0].time).ToString("F0"),
        temperatureCurve.Evaluate(temperatureCurve.Curve.keys[temperatureCurve.Curve.keys.Length - 1].time).ToString("F0"),
        temperatureCurve.Curve.keys[temperatureCurve.Curve.keys.Length-1].time.ToString("F0")
        );
      return message;
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();

      if (heatModule != null)
      {
        //Utils.Log($"{0} {temperatureCurve.Evaluate(0f)}\n{350f} {temperatureCurve.Evaluate(350f)}\n{1000} {temperatureCurve.Evaluate(1000f)}\n");
        if (HighLogic.LoadedSceneIsFlight)
        {
          if (base.IsCooling)
          {
            float flux = -temperatureCurve.Evaluate(heatModule.currentLoopTemperature);
            heatModule.AddFlux(moduleID, 0f, flux);
            RadiatorEfficiency = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatRadiator_RadiatorEfficiency_Running", (flux / temperatureCurve.Evaluate(temperatureCurve.Curve.keys[temperatureCurve.Curve.keys.Length - 1].time) * 100f).ToString("F0"));
          }
          else
          {

            heatModule.AddFlux(moduleID, 0f, 0f);
            RadiatorEfficiency = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatRadiator_RadiatorEfficiency_Offline");
          }
        }

        if (HighLogic.LoadedSceneIsEditor)
        {
          float flux = -1.0f * temperatureCurve.Evaluate(heatModule.currentLoopTemperature);
          heatModule.AddFlux(moduleID, 0f, flux);

          //Utils.Log($"BLAH {heatModule.LoopTemperature} {flux} {temperatureCurve.Evaluate(heatModule.LoopTemperature)}");
          RadiatorEfficiency = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatRadiator_RadiatorEfficiency_Running", ((temperatureCurve.Evaluate(heatModule.currentLoopTemperature) / temperatureCurve.Evaluate(temperatureCurve.Curve.keys[temperatureCurve.Curve.keys.Length - 1].time)) * 100f).ToString("F0"));
        }
      }
    }
  }
}
