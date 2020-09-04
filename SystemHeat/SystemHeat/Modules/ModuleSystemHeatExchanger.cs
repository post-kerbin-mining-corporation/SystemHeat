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
  public class ModuleSystemHeatExchanger : PartModule
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

    // This should correspond to the related ModuleSystemHeat
    [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = false, guiName = "#LOC_SystemHeat_ModuleSystemHeatExchanger_Field_Direction", groupName = "heatExchanger", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatExchanger_UIGroup_Title", groupStartCollapsed = false),
      UI_Toggle(enabledText = "", disabledText = "")]
    public bool ToggleSource;

    // Map temperature to heat generated
    [KSPField(isPersistant = false)]
    public FloatCurve temperatureDeltaHeatCurve = new FloatCurve();

    // Map temperature to power consumed
    [KSPField(isPersistant = false)]
    public FloatCurve temperatureDeltaCostCurve = new FloatCurve();

    [KSPField(isPersistant = true, guiActive = false, guiName = "#LOC_SystemHeat_ModuleSystemHeatExchanger_Field_MaxHeatTransfer", groupName = "heatExchanger", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatExchanger_UIGroup_Title"),
      UI_FloatRange(minValue = 0f, maxValue = 2000f,stepIncrement = 25)]
    public float HeatRate = 0f;

    [KSPField(isPersistant = true, guiActive = true, guiName = "#LOC_SystemHeat_ModuleSystemHeatExchanger_Field_TemperatureAdjust", groupName = "heatExchanger", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title"),
      UI_FloatRange(minValue = -1000f, maxValue = 1000f, stepIncrement = 25f)]
    public float OutletAdjustement = 0f;

    [KSPField(isPersistant = true)]
    public bool Enabled = true;

    [KSPField(isPersistant = false)]
    public float CurrentPowerConsumption;


    // UI Fields

    [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "#LOC_SystemHeat_ModuleSystemHeatExchanger_Field_Status", groupName = "heatExchanger", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatExchanger_UIGroup_Title")]
    public string Status = "";
    [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "#LOC_SystemHeat_ModuleSystemHeatExchanger_Field_PowerCost", groupName = "heatExchanger", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatExchanger_UIGroup_Title")]
    public string PowerStatus = "";

    /// KSPEVENTS
    /// ----------------------
    [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "#LOC_SystemHeat_ModuleSystemHeatExchanger_Event_EnableExchanger", active = true, groupName = "heatExchanger", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatExchanger_UIGroup_Title", groupStartCollapsed = false)]
    public void EnableExchanger()
    {
      Enabled = true;
    }
    [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "#LOC_SystemHeat_ModuleSystemHeatExchanger_Event_DisableExchanger", active = false, groupName = "heatExchanger", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatExchanger_UIGroup_Title", groupStartCollapsed = false)]
    public void DisableExchanger()
    {
      Enabled = false;
    }


    protected ModuleSystemHeat heatModule1;
    protected ModuleSystemHeat heatModule2;

    protected ModuleSystemHeat sourceModule;
    protected ModuleSystemHeat destModule;

    public override string GetModuleDisplayName()
    {
      return Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatExchanger_ModuleName") ;
    }

    public override string GetInfo()
    {
      string msg = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatExchanger_UIGroup_Title",
        0f.ToString("F0"), temperatureDeltaCostCurve.Evaluate(0f).ToString("F0"),
        1000f.ToString("F0"), temperatureDeltaCostCurve.Evaluate(1000f).ToString("F0"),
        temperatureDeltaHeatCurve.Evaluate(0).ToString("F0"),
        temperatureDeltaHeatCurve.Evaluate(1000f).ToString("F0")

       );
      return msg;
    }
    public void Start()
    {
      heatModule1 = this.GetComponents<ModuleSystemHeat>().ToList().Find(x => x.moduleID == systemHeatModuleID1);
      heatModule2 = this.GetComponents<ModuleSystemHeat>().ToList().Find(x => x.moduleID == systemHeatModuleID2);

      

      UI_Toggle toggle = (HighLogic.LoadedSceneIsEditor) ? (UI_Toggle)Fields[nameof(ToggleSource)].uiControlEditor: (UI_Toggle)Fields[nameof(ToggleSource)].uiControlFlight;

      if (ToggleSource)
      {
        sourceModule = heatModule1;
        destModule = heatModule2;
      }
      else
      {
        sourceModule = heatModule2;
        destModule = heatModule1;
      }

      toggle.onFieldChanged = ToggleDirection;
      toggle.disabledText = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatExchanger_Field_Direction_String", sourceModule.LoopID, destModule.LoopID);
      toggle.enabledText = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatExchanger_Field_Direction_String", sourceModule.LoopID, destModule.LoopID);


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

    private void ToggleDirection(BaseField field, object oldFieldValueObj)
    {
      Utils.Log($"[ModuleSystemHeatExchanger] Toggled direction of flow");
      ModuleSystemHeat saved = sourceModule;
      sourceModule = destModule;
      destModule = saved;

      UI_Toggle toggle = (HighLogic.LoadedSceneIsEditor) ? (UI_Toggle)Fields[nameof(ToggleSource)].uiControlEditor : (UI_Toggle)Fields[nameof(ToggleSource)].uiControlFlight;
      toggle.disabledText = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatExchanger_Field_Direction_String", sourceModule.LoopID, destModule.LoopID);
      toggle.enabledText = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatExchanger_Field_Direction_String", sourceModule.LoopID, destModule.LoopID);

    }

    /// <summary>
    /// Generates heat in the editor scene
    /// </summary>
    protected void GenerateHeatEditor()
    {
      if (Enabled)
      {
        Fields["PowerStatus"].guiActive = true;
        Fields["PowerStatus"].guiActiveEditor = true;
        float outputHeat = HeatRate;
        float outletTemperature = sourceModule.nominalLoopTemperature;

        float powerCost = temperatureDeltaCostCurve.Evaluate(OutletAdjustement);

        double amt = this.part.RequestResource(PartResourceLibrary.ElectricityHashcode, powerCost * TimeWarp.fixedDeltaTime, ResourceFlowMode.ALL_VESSEL);
        CurrentPowerConsumption = -powerCost;


        outletTemperature = Mathf.Clamp(outletTemperature + OutletAdjustement, 0f, float.MaxValue);

        if (destModule.LoopTemperature <= outletTemperature * 1.5f)
        {
          sourceModule.AddFlux(moduleID, 0f, -HeatRate);


          outputHeat = Mathf.Min(-sourceModule.consumedSystemFlux, HeatRate) + temperatureDeltaHeatCurve.Evaluate(OutletAdjustement);
          
          PowerStatus = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatExchanger_Field_PowerCost_Active", powerCost.ToString("F0"));
          Status = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatExchanger_Field_Status_Active", outletTemperature.ToString("F0"), outputHeat.ToString("F0"), sourceModule.consumedSystemFlux.ToString("F1"));
          destModule.AddFlux(moduleID, outletTemperature, outputHeat);
        }
        else
        {

          PowerStatus = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatExchanger_Field_PowerCost_Active", powerCost.ToString("F0"));
          Status = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatExchanger_Field_Status_TooHot");
          destModule.AddFlux(moduleID, outletTemperature, 0f);
          sourceModule.AddFlux(moduleID, 0f, 0f);

        }

      }
      else
      {
        Fields["PowerStatus"].guiActive = false;
        Fields["PowerStatus"].guiActiveEditor = false;
        Status = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatExchanger_Field_Status_Disabled");
        destModule.AddFlux(moduleID, 0f, 0f);
        sourceModule.AddFlux(moduleID, 0f, 0f);
      }
    }

    /// <summary>
    /// Generates heat in the flight scene
    /// </summary>
    protected void GenerateHeatFlight()
    {

      if (Enabled)
      {
        Fields["PowerStatus"].guiActive = true;
        Fields["PowerStatus"].guiActiveEditor = true;
        float outputHeat = HeatRate;
        float outletTemperature = sourceModule.nominalLoopTemperature;

        float powerCost = temperatureDeltaCostCurve.Evaluate(OutletAdjustement);

        double amt = this.part.RequestResource(PartResourceLibrary.ElectricityHashcode, powerCost * TimeWarp.fixedDeltaTime, ResourceFlowMode.ALL_VESSEL);
        CurrentPowerConsumption = -powerCost;

        if (amt > 0.0000000001)
        {
          outletTemperature = Mathf.Clamp(outletTemperature + OutletAdjustement, 0f, float.MaxValue);

          if (destModule.LoopTemperature <= outletTemperature * 1.5f)
          {
            sourceModule.AddFlux(moduleID, 0f, -HeatRate);


            outputHeat = Mathf.Min(-sourceModule.consumedSystemFlux, HeatRate) + temperatureDeltaHeatCurve.Evaluate(OutletAdjustement);

            PowerStatus = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatExchanger_Field_PowerCost_Active", powerCost.ToString("F1"));
            Status = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatExchanger_Field_Status_Active", outletTemperature.ToString("F0"), outputHeat.ToString("F0"));
            destModule.AddFlux(moduleID, outletTemperature, outputHeat);
          }
          else
          {

            PowerStatus = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatExchanger_Field_PowerCost_Active", powerCost.ToString("F1"));
            Status = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatExchanger_Field_Status_TooHot");
            destModule.AddFlux(moduleID, outletTemperature, 0f);
            sourceModule.AddFlux(moduleID, 0f, 0f);

          }
        }
        else
        {
          Status = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatExchanger_Field_Status_NoPower");
          destModule.AddFlux(moduleID, 0f, 0f);
          sourceModule.AddFlux(moduleID, 0f, 0f);
          Fields["PowerStatus"].guiActive = false;
          Fields["PowerStatus"].guiActiveEditor = false;
        }

      }
      else
      {
        Fields["PowerStatus"].guiActive = false;
        Fields["PowerStatus"].guiActiveEditor = false;
        Status = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatExchanger_Field_Status_Disabled");
        destModule.AddFlux(moduleID, 0f, 0f);
        sourceModule.AddFlux(moduleID, 0f, 0f);
      }
    }

  }
}
