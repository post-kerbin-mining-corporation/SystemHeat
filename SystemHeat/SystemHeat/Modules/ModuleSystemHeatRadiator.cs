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
    [KSPField(isPersistant = true)]
    public FloatCurve temperatureCurve = new FloatCurve();

    // Current status GUI string
    [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Radiator Status")]
    public string RadiatorStatus = "Offline";

    // Current efficiency GUI string
    [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Radiator Efficiency")]
    public string RadiatorEfficiency = "-1%";


    protected ModuleSystemHeat heatModule;

    public override void Start()
    {
      base.Start();
      heatModule = ModuleUtils.FindNamedComponent<ModuleSystemHeat>(this.part, systemHeatModuleID);
      if (SystemHeatSettings.DebugModules)
      {
        Utils.Log("[ModuleSystemHeatRadiator] Setup completed");
      }
    }
    public override string GetInfo()
    {
      // Need to update this to strip the CoreHeat stuff from it
      string message = base.GetInfo();
      message += String.Format("<b><color=>System Heat Radiation</color></b>\n - {1:F0} kW at {0:F0} K\n - {2:F0} kW at {3:F0} K", 
        temperatureCurve.Curve.keys[0].time, 
        temperatureCurve.Evaluate(temperatureCurve.Curve.keys[0].time),
        temperatureCurve.Evaluate(temperatureCurve.Curve.keys[temperatureCurve.Curve.keys.Length - 1].time),
        temperatureCurve.Curve.keys[temperatureCurve.Curve.keys.Length-1].time
        );
      return message;
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      if (heatModule != null)
      {
        if (base.IsCooling)
        {
          // We only cool if the loop is too hot.
          if (heatModule.currentLoopTemperature > heatModule.nominalLoopTemperature)
          {
            // Note a consumption flux is negative
            float flux = -temperatureCurve.Evaluate(heatModule.currentLoopTemperature);
            heatModule.AddFlux(moduleID, 0f, flux);
          } else
          {
            heatModule.AddFlux(moduleID, 0f, 0f);
          }
          RadiatorEfficiency = $"{(temperatureCurve.Evaluate(heatModule.currentLoopTemperature) / temperatureCurve.Evaluate(temperatureCurve.maxTime)) * 100f}%";
        }
        else
        {
          
          heatModule.AddFlux(moduleID, 0f, 0f);
          RadiatorEfficiency = $"Radiator Offline";
        }

        if (HighLogic.LoadedSceneIsEditor)
        {
          // We only cool if the loop is too hot.
          if (heatModule.currentLoopTemperature > heatModule.nominalLoopTemperature)
          {
            heatModule.AddFlux(moduleID, temperatureCurve.maxTime, -temperatureCurve.Evaluate(temperatureCurve.maxTime));
          }
          else
          {
            heatModule.AddFlux(moduleID, 0f, 0f);
          }
          RadiatorEfficiency = String.Format("{0}%",(temperatureCurve.Evaluate(heatModule.currentLoopTemperature) / temperatureCurve.Evaluate(temperatureCurve.maxTime)) * 100f);
        }
      }
    }
  }
}
