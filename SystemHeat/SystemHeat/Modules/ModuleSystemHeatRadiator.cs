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
  public class ModuleSystemHeatRadiator : ModuleActiveRadiator
  {
    /// <summary>
    /// Unique identifier
    /// </summary>
    [KSPField(isPersistant = false)]
    public string moduleID;

    /// <summary>
    /// This should correspond to the related ModuleSystemHeat. If not specified, the first module will be found
    /// </summary>
    [KSPField(isPersistant = false)]
    public string systemHeatModuleID;

    /// <summary>
    /// This should map system temperature to heat radiated
    /// </summary>
    [KSPField(isPersistant = false)]
    public FloatCurve temperatureCurve = new FloatCurve();

    /// <summary>
    /// UI: Current status
    /// </summary>
    [KSPField(isPersistant = false, guiActive = false, guiActiveEditor = false, guiName = "#LOC_SystemHeat_ModuleSystemHeatRadiator_RadiatorStatus_Title")]
    public string RadiatorStatus = "Offline";


    /// <summary>
    /// UI: Current radiator of efficiency
    /// </summary>
    [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "#LOC_SystemHeat_ModuleSystemHeatRadiator_RadiatorEfficiency_Title")]
    public string RadiatorEfficiency = "-1%";

    /// <summary>
    /// UI: Current status
    /// </summary>
    [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = false, guiName = "#LOC_SystemHeat_ModuleSystemHeatRadiator_RadiatorStatus_Title")]
    public string ConvectionStatus = "Offline";

    /// <summary>
    /// ID of the linked scalar module for animating heat
    /// </summary>
    [KSPField(isPersistant = false)]
    public string scalarModuleID;

    /// <summary>
    /// Temperature value at which glow starts to be visible
    /// </summary>
    [KSPField(isPersistant = false)]
    public float draperPoint = 650;

    /// <summary>
    /// Temperature value at which glow is maxed
    /// </summary>
    [KSPField(isPersistant = false)]
    public float maxTempAnimation = -1f;

    /// <summary>
    /// Rate at which heat animates
    /// </summary>
    [KSPField(isPersistant = false)]
    public float heatAnimationRate = 0.1f;


    protected ModuleSystemHeat heatModule;
    protected ModuleSystemHeatColorAnimator scalarModule;

    public override void Start()
    {
      base.Start();
      heatModule = ModuleUtils.FindHeatModule(this.part, systemHeatModuleID);

      if (scalarModuleID != "")
        scalarModule = part.GetComponents<ModuleSystemHeatColorAnimator>().ToList().Find(x => x.moduleID == scalarModuleID);

      if (maxTempAnimation == -1f)
        maxTempAnimation = (float)part.maxTemp;

      maxTempAnimation -= draperPoint;

      Utils.Log("[ModuleSystemHeatRadiator] Setup completed", LogType.Modules);

    }

    public override string GetModuleDisplayName()
    {
      return Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatRadiator_DisplayName");
    }
    public override string GetInfo()
    {

      string message = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatRadiator_PartInfo",
        temperatureCurve.Curve.keys[0].time.ToString("F0"),
        temperatureCurve.Evaluate(temperatureCurve.Curve.keys[0].time).ToString("F0"),
        temperatureCurve.Evaluate(temperatureCurve.Curve.keys[temperatureCurve.Curve.keys.Length - 1].time).ToString("F0"),
        temperatureCurve.Curve.keys[temperatureCurve.Curve.keys.Length - 1].time.ToString("F0")
        );
      message += base.GetInfo();
      return message;
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();

      if (heatModule != null)
      {

        if (HighLogic.LoadedSceneIsFlight)
        {
          if (base.IsCooling)
          {
            float radiativeFlux = -temperatureCurve.Evaluate(heatModule.LoopTemperature);

            //ConvectionStatus = $"c: {convectiveFlux}";

            if (heatModule.LoopTemperature >= heatModule.nominalLoopTemperature)
              heatModule.AddFlux(moduleID, 0f, radiativeFlux);
            else
              heatModule.AddFlux(moduleID, 0f, 0f);


            RadiatorEfficiency = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatRadiator_RadiatorEfficiency_Running",
              (-radiativeFlux / temperatureCurve.Evaluate(temperatureCurve.Curve.keys[temperatureCurve.Curve.keys.Length - 1].time) * 100f).ToString("F0"));

            if (scalarModule != null)
            {
              scalarModule.SetScalar(Mathf.MoveTowards(scalarModule.GetScalar, Mathf.Clamp01((heatModule.currentLoopTemperature - draperPoint) / maxTempAnimation), TimeWarp.fixedDeltaTime * heatAnimationRate));
            }
          }
          else
          {

            heatModule.AddFlux(moduleID, 0f, 0f);
            RadiatorEfficiency = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatRadiator_RadiatorEfficiency_Offline");
            if (scalarModule != null)
              scalarModule.SetScalar(Mathf.MoveTowards(scalarModule.GetScalar, 0f, TimeWarp.fixedDeltaTime * heatAnimationRate));
          }


        }

        if (HighLogic.LoadedSceneIsEditor)
        {
          float flux = -1.0f * temperatureCurve.Evaluate(heatModule.LoopTemperature);
          if (heatModule.LoopTemperature >= heatModule.nominalLoopTemperature)
            heatModule.AddFlux(moduleID, 0f, flux);
          else
            heatModule.AddFlux(moduleID, 0f, 0f);

          RadiatorEfficiency = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatRadiator_RadiatorEfficiency_Running",
            ((-temperatureCurve.Evaluate(heatModule.LoopTemperature) / temperatureCurve.Evaluate(temperatureCurve.Curve.keys[temperatureCurve.Curve.keys.Length - 1].time)) * 100f).ToString("F0"));
        }
      }
    }


  }
}
