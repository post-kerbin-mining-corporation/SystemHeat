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
  public class ModuleSystemHeatExchanger: PartModule
  {
    // This should be unique on the part
    [KSPField(isPersistant = false)]
    public string moduleID;

    // This should correspond to the related ModuleSystemHeat
    [KSPField(isPersistant = false)]
    public string systemHeatModuleID1;

    // This should correspond to the related ModuleSystemHeat
    [KSPField(isPersistant = false)]
    public string systemHeatModuleID2;


    [KSPField(isPersistant = true, guiActive = false, guiName = "Transfer rate", groupName = "heatExchanger", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatSink_UIGroup_Title"), UI_FloatRange(minValue = 0f, maxValue = 1500f, stepIncrement = 50f)]
    public float transferRate = 500f;

    // This should correspond to the related ModuleSystemHeat
    [KSPField(isPersistant = true)]
    public bool Enabled = true;

    /// KSPEVENTS
    /// ----------------------
    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Enable Exchanger", active = true, groupName = "heatExchanger", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatSink_UIGroup_Title", groupStartCollapsed = false)]
    public void EnableExchanger()
    {
      Enabled = true;
    }
    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Disable Exchanger", active = false, groupName = "heatExchanger", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatSink_UIGroup_Title", groupStartCollapsed = false)]
    public void DisableExchanger()
    {
      Enabled = false;
    }

    protected ModuleSystemHeat heatModule1;
    protected ModuleSystemHeat heatModule2;

    public override string GetModuleDisplayName()
    {
        return "Convector" ;
    }

    public override string GetInfo()
    {
        string msg = "";
        return msg;
    }

    public void Start()
    {
      heatModule1 = this.GetComponents<ModuleSystemHeat>().ToList().Find(x => x.moduleID == systemHeatModuleID1);
      heatModule2 = this.GetComponents<ModuleSystemHeat>().ToList().Find(x => x.moduleID == systemHeatModuleID2);

      if (SystemHeatSettings.DebugModules)
      {
        Utils.Log("[ModuleSystemHeatSink] Setup completed");
      }
    }
    public void Update()
    {
      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {
        if (Events["EnableExchanger"].active == Enabled || Events["DisableExchanger"].active != Enabled)
        {
          Events["DisableExchanger"].active = Enabled;
          Events["EnableExchanger"].active = !Enabled;
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
        float calculatedTransferRate = transferRate;
        if (heatModule1.LoopTemperature > heatModule2.LoopTemperature)
        {
          heatModule2.AddFlux(moduleID, heatModule1.LoopTemperature, calculatedTransferRate);
          heatModule1.AddFlux(moduleID, 0f, -calculatedTransferRate);
        }
        if (heatModule1.LoopTemperature <= heatModule2.LoopTemperature)
        {
          heatModule1.AddFlux(moduleID, heatModule2.LoopTemperature, calculatedTransferRate);
          heatModule2.AddFlux(moduleID, 0f, -calculatedTransferRate);
        }
      } 
      else
      {
        heatModule1.AddFlux(moduleID, 0f, 0f);
        heatModule2.AddFlux(moduleID, 0f, 0f);
      }
    }

    /// <summary>
    /// Generates heat in the flight scene
    /// </summary>
    protected void GenerateHeatFlight()
    {
    }

  }
}
