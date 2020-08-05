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
  public class ModuleSystemHeatTemperatureAdjuster: PartModule
  {
    // This should be unique on the part
    [KSPField(isPersistant = false)]
    public string moduleID;

    // This should correspond to the related ModuleSystemHeat
    [KSPField(isPersistant = false)]
    public string systemHeatModuleID;

    [KSPField(isPersistant = true)]
    public bool Enabled;

    [KSPField(isPersistant = true, guiActive = true, guiName = "Output Temperature", groupName = "heatadjuster", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title"), UI_FloatRange(minValue = 10f, maxValue = 1500f, stepIncrement = 10f)]
    public float LoopGoalTemperature = 450f;
    
    // Map temperature to heat generated
    [KSPField(isPersistant = false)]
    public FloatCurve temperatureDeltaHeatCurve = new FloatCurve();

    // Map temperature to power consumed
    [KSPField(isPersistant = false)]
    public FloatCurve temperatureDeltaCostCurve = new FloatCurve();

    // UI Fields
    [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "Status", groupName = "heatadjuster", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatSink_UIGroup_Title")]
    public string Status = "";
    

    /// KSPEVENTS
    /// ----------------------
    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Enable Compressor", active = true, groupName = "heatadjuster", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatSink_UIGroup_Title", groupStartCollapsed = false)]
    public void EnableAdjuster()
    {
      Enabled = true;
    }
    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Disable Compressor", active = false, groupName = "heatadjuster", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatSink_UIGroup_Title", groupStartCollapsed = false)]
    public void DisableAdjuster()
    {
      Enabled = false;
    }

    protected ModuleSystemHeat heatModule;

    public override string GetModuleDisplayName()
    {
        return Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatTemperatureAdjuster_ModuleName") ;
    }
    
    public override string GetInfo()
    {
        string msg = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatTemperatureAdjuster_PartInfo",
          0f.ToString("F0"), temperatureDeltaCostCurve.Evaluate(0f).ToString("F0"),
          1000f.ToString("F0"), temperatureDeltaCostCurve.Evaluate(1000f).ToString("F0"),
          temperatureDeltaHeatCurve.Evaluate(0).ToString("F0"),
          temperatureDeltaHeatCurve.Evaluate(1000f).ToString("F0")

         );
      return msg;
    }

    public void Start()
    {
      heatModule = this.GetComponents<ModuleSystemHeat>().ToList().Find(x => x.moduleID == systemHeatModuleID);
      if (heatModule == null)
        heatModule.GetComponent<ModuleSystemHeat>();
      
      if (SystemHeatSettings.DebugModules)
      {
        Utils.Log("[ModuleSystemHeatTemperatureAdjuster] Setup completed");
      }
    }
    public void Update()
    {
      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {
        if (Events["EnableAdjuster"].active == Enabled || Events["DisableAdjuster"].active != Enabled)
        {
          Events["DisableAdjuster"].active = Enabled;
          Events["EnableAdjuster"].active = !Enabled;
        }


      }
    }
    public void FixedUpdate()
    {
      if (HighLogic.LoadedSceneIsFlight)
      {
        GenerateHeatFlight();
      }
      if (HighLogic.LoadedSceneIsEditor)
      {
        GenerateHeatEditor();
      }
    }


    /// <summary>
    /// Generates heat in the editor scene
    /// </summary>
    protected void GenerateHeatEditor()
    {

      if (Enabled)
      {
        float tDelta = LoopGoalTemperature - CalculateNominalTemperature();
        float heatToAdd = temperatureDeltaHeatCurve.Evaluate(tDelta);
        heatModule.AddFlux(moduleID, tDelta, heatToAdd);
      }
      else
      {
        heatModule.AddFlux(moduleID, 0f, 0f);
      }
    }
    protected float CalculateNominalTemperature()
    {
      float temp = 0f;
      float totalPower = 0.001f;

      // Refactor this garbage later
      List<ModuleSystemHeat> modules = new List<ModuleSystemHeat>();
      
      if (HighLogic.LoadedSceneIsEditor)
      {
        modules = SystemHeatEditor.Instance.Simulator.HeatLoops[heatModule.LoopID].LoopModules;
      }
      if (HighLogic.LoadedSceneIsFlight)
      {
        modules = this.vessel.GetComponent<SystemHeatVessel>().Simulator.HeatLoops[heatModule.LoopID].LoopModules;
      }
       
        for (int i = 0; i < modules.Count; i++)
        {
          if (modules[i].moduleID != this.systemHeatModuleID && modules[i].totalSystemFlux > 0f)
          {
            temp += modules[i].systemNominalTemperature * modules[i].totalSystemFlux;
            totalPower += modules[i].totalSystemFlux;
          }
        }
      
      return Mathf.Clamp(GetEnvironmentTemperature(), temp / totalPower, float.MaxValue);
    }
    protected float GetEnvironmentTemperature()
    {
      if (HighLogic.LoadedSceneIsEditor)
        return SystemHeatSettings.SpaceTemperature;
    
      return part.vessel.externalTemperature > 50000d ? SystemHeatSettings.SpaceTemperature : (float)part.vessel.externalTemperature;
      
    }

    /// <summary>
    /// Generates heat in the flight scene
    /// </summary>
    protected void GenerateHeatFlight()
    {
      if (Enabled)
      {
        float tDelta = LoopGoalTemperature - CalculateNominalTemperature();
        float heatToAdd = temperatureDeltaHeatCurve.Evaluate(tDelta);
        float powerCost = temperatureDeltaCostCurve.Evaluate(tDelta);

        double amt = this.part.RequestResource(PartResourceLibrary.ElectricityHashcode, -powerCost * TimeWarp.fixedDeltaTime, ResourceFlowMode.ALL_VESSEL);

        if (amt > 0.0000000001)
        {
          heatModule.AddFlux(moduleID, tDelta, heatToAdd);
          if (tDelta > 0)
          {
            Status = String.Format("Heating Loop at {0} kW", heatToAdd.ToString("F0"));
          }
          else
          {
            Status = String.Format("Cooling Loop at {0} kW", (-heatToAdd).ToString("F0"));
          }
        } 
        else
        {
          heatModule.AddFlux(moduleID, 0f, 0f);
          Status = String.Format("Not enough Electric Charge!");
        }
      }
      else
      {
        heatModule.AddFlux(moduleID, 0f, 0f);
        Status = String.Format("Disabled");
      }
    }

  }
}
