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
    // This should be unique on the part
    [KSPField(isPersistant = false)]
    public string moduleID;

    // This should correspond to the related ModuleSystemHeat
    [KSPField(isPersistant = false)]
    public string systemHeatModuleID;

    // This should map system temperature to heat radiated
    [KSPField(isPersistant = false)]
    public FloatCurve temperatureCurve;

    [KSPField(isPersistant = true, guiActive = true, guiName = "Radiator Status")]
    public string RadiatorStatus = "Sleepy";

    [KSPField(isPersistant = true, guiActive = true, guiName = "Radiator Efficiency")]
    public string RadiatorEfficiency = "-1%";

    protected ModuleSystemHeat heatModule;

    public override void Start()
    {
      heatModule = Utils.FindNamedComponent<ModuleSystemHeat>(this.part, systemHeatModuleID);
      if (SystemHeatSettings.DebugPartUI)
      {
        Fields["totalSystemTemperature"].guiActive = true;
        Fields["totalSystemTemperature"].guiActiveEditor = true;
        Fields["totalSystemFlux"].guiActive = true;
        Fields["totalSystemFlux"].guiActiveEditor = true;
        Fields["nominalLoopTemperature"].guiActive = true;
        Fields["nominalLoopTemperature"].guiActiveEditor = true;
        Fields["currentLoopTemperature"].guiActive = true;
        Fields["currentLoopTemperature"].guiActiveEditor = true;
        Fields["currentLoopFlux"].guiActive = true;
        Fields["currentLoopFlux"].guiActiveEditor = true;
      }
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      if (heatModule != null)
      {
        if (base.IsCooling())
        {
          // We only cool if the loop is too hot.
          if (module.currentLoopTemperature > module.nominalLoopTemperature)
          {
            // Note a consumption flux is negative
            float flux = -temperatureCurve.Evaluate(module.currentLoopTemperature);
            heatModule.AddFlux(moduleID, 0f, flux);
          } else
          {
            heatModule.AddFlux(moduleID, 0f, 0f);
          }
        }
      }
    }
  }
}
