using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;

namespace SystemHeat
{
  /// <summary>
  /// The connection between a stock ModuleResourceHarvester and the SystemHeat system
  /// </summary>
  public class ModuleSystemHeatReactor: PartModule
  {
    // This should be unique on the part
    [KSPField(isPersistant = false)]
    public string moduleID;

    // This should correspond to the related ModuleSystemHeat
    [KSPField(isPersistant = false)]
    public string systemHeatModuleID;



    /// KSPACTIONS
    /// ----------------------
    
    [KSPAction("Enable Reactor")]
    public void EnableAction(KSPActionParam param) { EnableReactor(); }

    [KSPAction("Disable Reactor")]
    public void DisableAction(KSPActionParam param) { DisableReactor(); }

    [KSPAction("Toggle Reactor")]
    public void ToggleAction(KSPActionParam param)
    {
      if (!Enabled) EnableReactor();
      else DisableReactor();
    }

    public override string GetModuleDisplayName()
    {
      return Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_ModuleName");
    }


    public void Update()
    {

    }

    public void FixedUpdate()
    {
      if (HighLogic.LoadedScene == GameScenes.FLIGHT)
      {
        if (UIName == "")
          UIName = part.partInfo.title;
        ActualPowerPercent = CurrentPowerPercent;

        // Update reactor core integrity readout
        if (CoreIntegrity > 0)
          CoreStatus = String.Format("{0:F2} %", CoreIntegrity);
        else
          CoreStatus = Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Field_CoreStatus_Meltdown");


        // Handle core damage tracking and effects
        HandleCoreDamage();
        // Heat consumption occurs if reactor is on or off
        DoHeatConsumption();

        // IF REACTOR ON
        // =============
        if (Enabled)
        {
          if (TimewarpShutdown && TimeWarp.fetch.current_rate_index >= TimewarpShutdownFactor)
            ToggleResourceConverterAction(new KSPActionParam(0, KSPActionType.Activate));
          if (base.ModuleIsActive() != activeFlag)
          {
            base.lastUpdateTime = Planetarium.GetUniversalTime();
            heatTicker = 60;  
            activeFlag = true;
            // Debug.Log("Turned On");
          }

          DoFuelConsumption();
          DoHeatGeneration();

        }
        // IF REACTOR OFF
        // =============
        else
        {

          // Update UI
          if (CoreIntegrity <= 0f)
          {
            FuelStatus = Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Field_FuelStatus_Meltdown");
            ReactorOutput = Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Field_ReactorOutput_Meltdown");
          }
          else
          {
            FuelStatus = Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Field_FuelStatus_Offline");
            ReactorOutput = Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Field_ReactorOutput_Offline");

          }
        }
        
      }

    }
    private void DoHeatConsumption()
    {
      // save some divisions later
      float coreIntegrity = CoreIntegrity / 100f;
      float reactorThrottle = ActualPowerPercent / 100f;
      if (!Enabled)
        reactorThrottle = 0f;
      float maxHeatGenerationKW = HeatGeneration / 50f;

      // The core temperature where no generation is possible
      float zeroPoint = (float)part.temperature;

      // The core temperature where maximum generation is possible
      float maxPoint = NominalTemperature;

      float temperatureDiff = Mathf.Clamp((float)core.CoreTemperature - zeroPoint, 0f, NominalTemperature);

      // The fraction of generation that is possible.
      float curTempScale = Mathf.Clamp(temperatureDiff / (maxPoint - zeroPoint), 0f, 1f);

      // Fraction showing amount of power available to
      float powerScale = Mathf.Min(reactorThrottle, curTempScale) * coreIntegrity;

      AvailablePower = powerScale * maxHeatGenerationKW;

      // Allocate power to generators/engines
      if (float.IsNaN(AvailablePower))
        AvailablePower = 0f;

      AllocateThermalPower();

      // GUI
      ThermalTransfer = String.Format("{0:F1} {1}", AvailablePower, Localizer.Format("#LOC_NFElectrical_Units_kW"));
      CoreTemp = String.Format("{0:F1}/{1:F1} {2}", (float)core.CoreTemperature, NominalTemperature, Localizer.Format("#LOC_NFElectrical_Units_K"));

      D_TempScale = String.Format("{0:F4}", curTempScale);
      D_PowerScale = String.Format("{0:F4}", powerScale);
    }

    private void SetHeatGeneration(float heat)
    {
      if (Time.timeSinceLevelLoad > 1f)
        GeneratesHeat = true;
      else
        GeneratesHeat = false;

      if (heatTicker <= 0)
      {
        TemperatureModifier = new FloatCurve();
        TemperatureModifier.Add(0f, heat);
      }
      else
      {
        ZeroThermal();

        heatTicker = heatTicker - 1;
      }
      
    }

    // track and set core damage
    private void HandleCoreDamage()
    {
      // Update reactor damage
      float critExceedance = (float)core.CoreTemperature - CriticalTemperature;

      // If overheated too much, damage the core
      if (critExceedance > 0f && TimeWarp.CurrentRate < 100f)
      {
        // core is damaged by Rate * temp exceedance * time
        CoreIntegrity = Mathf.MoveTowards(CoreIntegrity, 0f, CoreDamageRate * critExceedance * TimeWarp.fixedDeltaTime);
      }

      // Calculate percent exceedance of nominal temp
      float tempNetScale = 1f - Mathf.Clamp01((float)((core.CoreTemperature - NominalTemperature) / (MaximumTemperature - NominalTemperature)));


      if (OverheatAnimation != "")
      {
        for (int i = 0; i < overheatStates.Length; i++)

        {
          overheatStates[i].normalizedTime = 1f - tempNetScale;
        }
      }
    }


    #region Repair functions

    public bool TryRepairReactor()
    {
      if (CoreIntegrity <= MinRepairPercent)
      {
        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Message_Repair_CoreTooDamaged"), 5.0f, ScreenMessageStyle.UPPER_CENTER));
        return false;
      }
      if (!ModuleUtils.CheckEVAEngineerLevel(EngineerLevelForRepair))
      {
        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Message_Repair_CoreTooDamaged", EngineerLevelForRepair.ToString("F0")), 5.0f, ScreenMessageStyle.UPPER_CENTER));
        return false;
      }
      if (base.ModuleIsActive())
      {
        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Message_Repair_NotWhileRunning"),
            5.0f, ScreenMessageStyle.UPPER_CENTER));
        return false;
      }
      if (core.CoreTemperature > MaxTempForRepair)
      {
        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Message_Repair_CoreTooHot", MaxTempForRepair.ToString("F0")), 5.0f, ScreenMessageStyle.UPPER_CENTER));
        return false;
      }
      if (CoreIntegrity >= MaxRepairPercent)
      {
        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Message_Repair_CoreAlreadyRepaired", MaxRepairPercent.ToString("F0")),
            5.0f, ScreenMessageStyle.UPPER_CENTER));
        return false;
      }
      return true;
    }

    // Repair the reactor to max Repair percent
    public void DoReactorRepair()
    {
      this.CoreIntegrity = MaxRepairPercent;
      ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Message_Repair_RepairSuccess", 
        MaxRepairPercent.ToString("F0")), 5.0f, ScreenMessageStyle.UPPER_CENTER));
    }

    #endregion

    #region Refuelling
    // Finds time remaining at specified fuel burn rates
    public string FindTimeRemaining(double amount, double rate)
    {
      if (rate < 0.0000001)
      {
        return Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Field_FuelStatus_VeryLong");
      }
      double remaining = amount / rate;
      if (remaining >= 0)
      {
        return Utils.FormatTimeString(remaining);
      }
      {
        return Localizer.Format("#LOC_NFElectrical_ModuleFissionReactor_Field_FuelStatus_Exhausted");
      }
    }
    #endregion
  }


}
}
