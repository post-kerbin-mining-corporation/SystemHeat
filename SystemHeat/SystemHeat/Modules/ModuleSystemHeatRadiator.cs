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

    protected ModuleSystemHeat heatModule;

    public override void Start()
    {
      heatModule = Utils.FindNamedComponent<ModuleSystemHeat>(this.part, systemHeatModuleID);
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
