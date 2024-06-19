using KSP.Localization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SystemHeat
{

  public class ModuleSystemHeatFissionReactor : PartModule, IContractObjectiveModule
  {
    // This should be unique on the part
    [KSPField(isPersistant = false)]
    public string moduleID;

    /// <summary>
    /// ModuleSystemHeat module to use
    /// </summary>
    [KSPField(isPersistant = false)]
    public string systemHeatModuleID;

    // --- General -----
    /// <summary>
    /// Is the reactor on?
    /// </summary>
    [KSPField(isPersistant = true)]
    public bool Enabled = false;

    /// <summary>
    /// Is hibernation enabled?
    /// </summary>
    [KSPField(isPersistant = true)]
    public bool HibernateOnWarp = false;

    /// <summary>
    /// Are we currently hibernating?
    /// </summary>
    [KSPField(isPersistant = true)]
    public bool Hibernating = false;

    /// <summary>
    /// Manual or automated throttle control
    /// </summary>
    [KSPField(isPersistant = true)]
    public bool ManualControl = false;


    /// <summary>
    /// Current reactor power setting (min-100, tweakable) 
    /// </summary>
    [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_CurrentPowerPercent", groupName = "fissionreactor", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title"), UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 1f)]
    public float CurrentReactorThrottle = 100f;


    /// <summary>
    /// Current reactor power level
    /// </summary>
    [KSPField(isPersistant = true)]
    public float CurrentThrottle = 0f;

    /// <summary>
    /// Minimum reactor power level
    /// </summary>
    [KSPField(isPersistant = false)]
    public float MinimumThrottle = 25f;

    /// <summary>
    /// Rate of reactor throttle increase
    /// </summary>
    [KSPField(isPersistant = false)]
    public float ThrottleIncreaseRate = 1f;

    /// <summary>
    /// Rate of reactor throttle decrease
    /// </summary>
    [KSPField(isPersistant = false)]
    public float ThrottleDecreaseRate = 1f;

    // -- POWER

    /// <summary>
    /// Does the reactor generate power?
    /// </summary>
    [KSPField(isPersistant = false)]
    public bool GeneratesElectricity = true;

    /// <summary>
    /// Amount of power generated by reactor at various throttles
    /// </summary>
    [KSPField(isPersistant = false)]
    public FloatCurve ElectricalGeneration = new FloatCurve();

    /// <summary>
    /// Amount of power currently generated by reactor
    /// </summary>
    [KSPField(isPersistant = true)]
    public float CurrentElectricalGeneration = 0f;

    // Reactor Status string
    [KSPField(isPersistant = false, guiActive = true, guiName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_GeneratorStatus", groupName = "fissionreactor", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title")]
    public string GeneratorStatus;

    /// <summary>
    /// Fuel name
    /// </summary>
    [KSPField(isPersistant = false)]
    public string FuelName = "EnrichedUranium";

    // --- Thermals -----
    /// <summary>
    /// Waste heat generation curve
    /// </summary>
    [KSPField(isPersistant = false)]
    public FloatCurve HeatGeneration = new FloatCurve();

    /// <summary>
    /// Efficiency - can be specified.
    /// </summary>
    [KSPField(isPersistant = false)]
    public float Efficiency = -1f;

    /// <summary>
    /// Current heat generation
    /// </summary>
    [KSPField(isPersistant = false)]
    public float CurrentHeatGeneration;

    /// <summary>
    /// Real internal core temperature
    /// </summary>
    [KSPField(isPersistant = true)]
    public float InternalCoreTemperature = 0f;

    /// <summary>
    /// Rate at which core temp responds to loop temperature adjustements
    /// </summary>
    [KSPField(isPersistant = true)]
    public float InternalCoreTemperatureResponseScale = 0.25f;

    /// <summary>
    /// Nominal reactor temperature (where the reactor should live)
    /// </summary>
    [KSPField(isPersistant = false)]
    public float NominalTemperature = 900f;

    /// <summary>
    /// Critical reactor temperature (reactor function reduced)
    /// </summary>
    [KSPField(isPersistant = false)]
    public float CriticalTemperature = 1400f;

    /// <summary>
    /// Maximum reactor temperature (core damage after this)
    /// </summary>
    [KSPField(isPersistant = false)]
    public float MaximumTemperature = 2000f;

    /// <summary>
    /// Temperature value for auto-shutdown
    /// </summary>
    [KSPField(isPersistant = true, guiActive = true, guiName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_CurrentSafetyOverride", groupName = "fissionreactor", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title"), UI_FloatRange(minValue = 700f, maxValue = 6000f, stepIncrement = 100f)]
    public float CurrentSafetyOverride = 1000f;

    [KSPField(isPersistant = true)]
    public bool FirstLoad = true;

    [KSPField(isPersistant = true)]
    public double LastUpdateTime = -1d;

    [KSPField(isPersistant = false)]
    public bool allowManualShutdownTemperatureControl = true;

    [KSPField(isPersistant = false)]
    public bool allowManualControl = true;

    [KSPField(isPersistant = false)]
    public bool allowHibernate = true;

    // REPAIR VARIABLES
    // integrity of the core

    /// <summary>
    /// If this is toggled, we'll override the the settings
    /// </summary>
    [KSPField(isPersistant = false)]
    public bool OverrideRepairSettings = false;

    [KSPField(isPersistant = true)]
    public float CoreIntegrity = 100f;

    // Rate the core is damaged, in % per S per K
    [KSPField(isPersistant = false)]
    public float CoreDamageRate = 0.005f;

    // Engineer level to repair the core
    [KSPField(isPersistant = false)]
    public int EngineerLevelForRepair = 5;

    [KSPField(isPersistant = false)]
    public float RepairAmountPerKit = 25;

    [KSPField(isPersistant = false)]
    public float MaxRepairPercent = 75;

    [KSPField(isPersistant = false)]
    public float MinRepairPercent = 10;

    [KSPField(isPersistant = false)]
    public float MaxTempForRepair = 325;


    [KSPField(isPersistant = false)]
    public string uiGroupName = "fissionreactor";

    [KSPField(isPersistant = false)]
    public string uiGroupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title";

    /// UI FIELDS
    /// --------------------
    // Reactor Status string
    [KSPField(isPersistant = false, guiActive = true, guiName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_ReactorOutput", groupName = "fissionreactor", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title")]
    public string ReactorOutput;

    // integrity of the core
    [KSPField(isPersistant = false, guiActive = true, guiName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_CoreTemp", groupName = "fissionreactor", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title")]
    public string CoreTemp;

    // integrity of the core
    [KSPField(isPersistant = false, guiActive = true, guiName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_CoreStatus", groupName = "fissionreactor", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title")]
    public string CoreStatus;

    // Fuel Status string
    [KSPField(isPersistant = false, guiActive = true, guiName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_FuelStatus", groupName = "fissionreactor", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title")]
    public string FuelStatus;

    /// KSPEVENTS
    /// ----------------------
    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Event_Enable_Title", active = true, groupName = "fissionreactor", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title", groupStartCollapsed = false)]
    public void EnableReactor()
    {
      ReactorActivated();
    }
    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Event_Disable_Title", active = false, groupName = "fissionreactor", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title", groupStartCollapsed = false)]
    public void DisableReactor()
    {
      ReactorDeactivated();
    }
    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Event_EnableManual_Title", active = true, groupName = "fissionreactor", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title", groupStartCollapsed = false)]
    public void EnableManual()
    {
      SetManualControl(true);
    }
    [KSPEvent(guiActive = false, guiActiveEditor = false, guiName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Event_DisableManual_Title", active = true, groupName = "fissionreactor", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title", groupStartCollapsed = false)]
    public void DisableManual()
    {
      SetManualControl(false);
    }
    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Event_EnableHibernate_Title", active = true, groupName = "fissionreactor", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title", groupStartCollapsed = false)]
    public void EnableHibernate()
    {
      HibernateOnWarp = true;
      GameEvents.onPartPack.Add(new EventData<Part>.OnEvent(GoOnRails));
      GameEvents.onPartUnpack.Add(new EventData<Part>.OnEvent(GoOffRails));
    }
    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Event_DisableHibernate_Title", active = true, groupName = "fissionreactor", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title", groupStartCollapsed = false)]
    public void DisableHibernate()
    {
      HibernateOnWarp = false;
      GameEvents.onPartPack.Remove(GoOnRails);
      GameEvents.onPartUnpack.Remove(GoOffRails);
    }

    // Try to fix the reactor
    [KSPEvent(externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 3.5f, guiName = "Repair Reactor")]
    public void RepairReactor()
    {
      if (TryRepairReactor())
      {
        DoReactorRepair();
      }
    }
    /// KSPACTIONS
    /// ----------------------

    [KSPAction("Toggle Hibernate")]
    public void ToggleHibernateAction(KSPActionParam param)
    {
    }

    [KSPAction("Enable Reactor")]
    public void EnableAction(KSPActionParam param)
    {
      EnableReactor();
    }

    [KSPAction("Disable Reactor")]
    public void DisableAction(KSPActionParam param)
    {
      DisableReactor();
    }

    [KSPAction("Toggle Reactor")]
    public void ToggleAction(KSPActionParam param)
    {
      if (!Enabled) EnableReactor();
      else DisableReactor();
    }


    protected ModuleSystemHeat heatModule;
    protected List<ResourceRatio> inputs;
    protected List<ResourceRatio> outputs;

    public override string GetModuleDisplayName()
    {
      return Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_ModuleName");
    }

    public override string GetInfo()
    {
      double baseRate = 0d;
      for (int i = 0; i < inputs.Count; i++)
      {
        if (inputs[i].ResourceName == FuelName)
          baseRate = inputs[i].Ratio;
      }
      return
          Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_PartInfo",
          ElectricalGeneration.Evaluate(100f).ToString("F0"),
          FindTimeRemaining(this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(FuelName).id).amount, baseRate),
          Utils.ToSI((HeatGeneration.Evaluate(100f) - ElectricalGeneration.Evaluate(100f)), "F0"),
          NominalTemperature.ToString("F0"),
          NominalTemperature.ToString("F0"),
          CriticalTemperature.ToString("F0"),
          MaximumTemperature.ToString("F0"),
          ThrottleIncreaseRate.ToString("F0"),
          MinimumThrottle.ToString("F0"));

    }

    public virtual void GoOnRails(Part p)
    {

      if (HibernateOnWarp && Enabled)
      {
        Hibernating = true;
        ReactorDeactivated();
        Utils.Log($"[ModuleSystemHeatFissionReactor] Reactor was on: Going on rails and hibernating reactor", LogType.Modules);
      }
    }
    public virtual void GoOffRails(Part p)
    {

      if (HibernateOnWarp)
      {
        if (Hibernating)
        {
          ReactorActivated();
          Hibernating = false;
          Utils.Log($"[ModuleSystemHeatFissionReactor] Going off rails and resuming reactor", LogType.Modules);
        }
      }
    }
    public virtual void SetManualControl(bool state)
    {

      Fields["CurrentSafetyOverride"].guiActive = allowManualShutdownTemperatureControl;

      if (allowManualControl)
      {
        ManualControl = state;
        Events["EnableManual"].guiActive = !ManualControl;
        Events["DisableManual"].guiActive = ManualControl;
        Fields["CurrentReactorThrottle"].guiActive = ManualControl;
      }
      else
      {
        Events["EnableManual"].guiActive = false;
        Events["DisableManual"].guiActive = false;
        Fields["CurrentReactorThrottle"].guiActive = false;
      }
    }
    public virtual void ReactorActivated()
    {
      Enabled = true;
    }

    public virtual void ReactorDeactivated()
    {
      Enabled = false;
    }

    public virtual void Start()
    {
      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {
        if (inputs == null || inputs.Count == 0)
        {
          ConfigNode node = ModuleUtils.GetModuleConfigNode(part, moduleName);
          if (node != null)
            OnLoad(node);
        }

        heatModule = ModuleUtils.FindHeatModule(this.part, systemHeatModuleID);

        var range = (UI_FloatRange)this.Fields["CurrentSafetyOverride"].uiControlEditor;
        range.minValue = 0f;
        range.maxValue = MaximumTemperature;

        range = (UI_FloatRange)this.Fields["CurrentSafetyOverride"].uiControlFlight;
        range.minValue = 0f;
        range.maxValue = MaximumTemperature;

        range = (UI_FloatRange)this.Fields["CurrentReactorThrottle"].uiControlEditor;
        range.minValue = MinimumThrottle;

        range = (UI_FloatRange)this.Fields["CurrentReactorThrottle"].uiControlFlight;
        range.minValue = MinimumThrottle;


        foreach (BaseField field in this.Fields)
        {
          if (!string.IsNullOrEmpty(field.group.name)) continue;

          if (field.group.name == uiGroupName)
            field.group.displayName = Localizer.Format(uiGroupDisplayName);
        }

        foreach (BaseEvent baseEvent in this.Events)
        {
          if (!string.IsNullOrEmpty(baseEvent.group.name)) continue;

          if (baseEvent.group.name == uiGroupName)
            baseEvent.group.displayName = Localizer.Format(uiGroupDisplayName);
        }

        if (!GeneratesElectricity)
        {
          if (Efficiency == -1f)
          {
            Efficiency = 0f;
          }
          Fields["GeneratorStatus"].guiActive = false;
        }
        else
        {
          if (Efficiency == -1f)
          {
            Efficiency = ElectricalGeneration.Evaluate(100f) / HeatGeneration.Evaluate(100f);
          }
        }

        if (FirstLoad)
        {
          if (HighLogic.LoadedSceneIsFlight)
          {
            CurrentThrottle = 0f;
            CurrentReactorThrottle = 0f;
            InternalCoreTemperature = 0f;
            this.CurrentSafetyOverride = this.CriticalTemperature;
            FirstLoad = false;
          }
        }
      }
      if (HighLogic.LoadedSceneIsEditor)
      {
        Fields["CurrentSafetyOverride"].guiActiveEditor = allowManualShutdownTemperatureControl;
        Fields["CurrentReactorThrottle"].guiActive = true;
        CurrentReactorThrottle = 100f;
      }

      if (HighLogic.LoadedSceneIsFlight)
      {
        DoCatchup();
        SetManualControl(ManualControl);
      }

    }
    public override void OnAwake()
    {
      base.OnAwake();
      if (HighLogic.LoadedSceneIsFlight)
      {
        GameEvents.OnVesselRollout.Add(new EventData<ShipConstruct>.OnEvent(OnVesselRollout));
        if (HibernateOnWarp)
        {
          GameEvents.onPartPack.Add(new EventData<Part>.OnEvent(GoOnRails));
          GameEvents.onPartUnpack.Add(new EventData<Part>.OnEvent(GoOffRails));
        }
      }
    }
    void OnDestroy()
    {
      // Clean up events when the item is destroyed
      GameEvents.OnVesselRollout.Remove(OnVesselRollout);

      GameEvents.onPartPack.Remove(GoOnRails);
      GameEvents.onPartUnpack.Remove(GoOffRails);
    }
    /// <summary>
    /// 
    /// </summary>
    protected void OnVesselRollout(ShipConstruct node)
    {
      CoreIntegrity = 100f;
      CurrentHeatGeneration = 0f;
      InternalCoreTemperature = 0f;
    }

    public void DoCatchup()
    {
      if (part.vessel.missionTime > 0.0)
      {

        if (Enabled)
        {
          double elapsedTime = Planetarium.GetUniversalTime() - LastUpdateTime;
          if (elapsedTime > 0d)
          {
            Utils.Log($"[SystemHeatFissionReactor] Catching up {elapsedTime} s of time on load", LogType.Modules);
            float fuelThrottle = CurrentReactorThrottle / 100f;

            foreach (ResourceRatio ratio in inputs)
            {
              Utils.Log($"[SystemHeatFissionReactor] Consuming {fuelThrottle * ratio.Ratio * elapsedTime} u of {ratio.ResourceName} on load", LogType.Modules);
              double amt = this.part.RequestResource(ratio.ResourceName, fuelThrottle * ratio.Ratio * elapsedTime, ratio.FlowMode);

            }
            foreach (ResourceRatio ratio in outputs)
            {
              double amt = this.part.RequestResource(ratio.ResourceName, -fuelThrottle * ratio.Ratio * elapsedTime, ratio.FlowMode);
            }

          }
        }
      }
    }

    public override void OnLoad(ConfigNode node)
    {
      base.OnLoad(node);


      /// Load resource nodes
      ConfigNode[] inNodes = node.GetNodes("INPUT_RESOURCE");

      inputs = new List<ResourceRatio>();
      for (int i = 0; i < inNodes.Length; i++)
      {
        ResourceRatio p = new ResourceRatio();
        p.Load(inNodes[i]);
        inputs.Add(p);
      }
      ConfigNode[] outNodes = node.GetNodes("OUTPUT_RESOURCE");

      outputs = new List<ResourceRatio>();
      for (int i = 0; i < outNodes.Length; i++)
      {
        ResourceRatio p = new ResourceRatio();
        p.Load(outNodes[i]);
        outputs.Add(p);
      }

    }

    public void Update()
    {
      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {
        if (Events["EnableReactor"].active == Enabled || Events["DisableReactor"].active != Enabled)
        {
          Events["DisableReactor"].active = Enabled;
          Events["EnableReactor"].active = !Enabled;
        }
        if (allowHibernate)
        {
          if (Events["EnableHibernate"].active == HibernateOnWarp || Events["DisableHibernate"].active != HibernateOnWarp)
          {
            Events["DisableHibernate"].active = HibernateOnWarp;
            Events["EnableHibernate"].active = !HibernateOnWarp;
          }
        }
        else
        {
          Events["DisableHibernate"].active = false;
          Events["EnableHibernate"].active = false;
        }
        if (part.IsPAWVisible())
        {
          UpdatePAW();
        }
      }
    }

    protected void UpdatePAW()
    {

      // Update reactor core integrity readout
      if (CoreIntegrity > 0)
      {
        CoreStatus = String.Format("{0:F2} %", CoreIntegrity);
      }
      else
      {
        CoreStatus = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_CoreStatus_Meltdown");
      }

      if (HighLogic.LoadedSceneIsFlight)
      {
        CoreTemp = String.Format("{0:F0}/{1:F0} {2}", InternalCoreTemperature, NominalTemperature, Localizer.Format("#LOC_SystemHeat_Units_K"));

        if (CoreIntegrity <= 0f)
        {
          FuelStatus = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_FuelStatus_Meltdown");
          ReactorOutput = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_ReactorOutput_Meltdown");
        }
        else
        {
          ReactorOutput = String.Format("{0:F1} {1}", Utils.ToSI(CurrentHeatGeneration, "F0"), "W");
        }

        if (Enabled)
        {
          if (GeneratesElectricity)
          {
            if (fuelCheckPassed)
            {
              GeneratorStatus = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_GeneratorStatus_Normal", CurrentElectricalGeneration.ToString("F1"));
            }
            else
            {
              GeneratorStatus = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_GeneratorStatus_Offline");
            }
          }

          // Find the time remaining at current rate
          FuelStatus = FindTimeRemaining(
            this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(FuelName).id).amount,
            burnRate);
        }
        else
        {
          GeneratorStatus = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_GeneratorStatus_Offline");
          // Update UI
          if (CoreIntegrity <= 0f)
          {
            FuelStatus = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_FuelStatus_Meltdown");
            ReactorOutput = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_ReactorOutput_Meltdown");
          }
          else
          {
            FuelStatus = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_FuelStatus_Offline");
          }
        }
      }
    }

    public virtual void FixedUpdate()
    {
      if (HighLogic.LoadedSceneIsEditor)
      {
        CurrentThrottle = CurrentReactorThrottle;
        HandleHeatGenerationEditor();
        if (GeneratesElectricity)
        {
          CurrentElectricalGeneration = ElectricalGeneration.Evaluate(CurrentThrottle);
        }

      }
      if (HighLogic.LoadedSceneIsFlight)
      {
        if (part.vessel.missionTime > 0.0)
        {
          LastUpdateTime = Planetarium.GetUniversalTime();
        }

        HandleCore();
        HandleThrottle();
        HandleHeatGeneration();

        // IF REACTOR ON
        // =============
        if (Enabled)
        {
          HandleResourceActivities(TimeWarp.fixedDeltaTime);
          if (InternalCoreTemperature > CurrentSafetyOverride)
          {
            ScreenMessages.PostScreenMessage(new ScreenMessage(
              Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Message_EmergencyShutdown", CurrentSafetyOverride.ToString("F0"), part.partInfo.title
              ), 5.0f, ScreenMessageStyle.UPPER_CENTER));
            ReactorDeactivated();
          }
        }
        // IF REACTOR OFF
        // =============
        else
        {
          CurrentElectricalGeneration = 0f;
        }
      }
    }

    protected virtual void HandleThrottle()
    {
      if (!Enabled)
      {
        CurrentThrottle = Mathf.MoveTowards(CurrentThrottle, 0f, TimeWarp.fixedDeltaTime * ThrottleDecreaseRate);
      }
      else
      {
        CurrentThrottle = Mathf.MoveTowards(CurrentThrottle, CurrentReactorThrottle, TimeWarp.fixedDeltaTime * ThrottleIncreaseRate);
      }
    }
    protected virtual float CalculateWasteHeatGeneration()
    {
      return (HeatGeneration.Evaluate(CurrentThrottle) * (1f - Efficiency)) * CoreIntegrity / 100f;
    }

    protected virtual float CalculateHeatGeneration()
    {
      return (HeatGeneration.Evaluate(CurrentThrottle)) * CoreIntegrity / 100f;
    }
    protected virtual float CalculateHeatGenerationEditor()
    {
      return (HeatGeneration.Evaluate(CurrentReactorThrottle)) * (1f - Efficiency);
    }
    protected virtual void HandleHeatGeneration()
    {

      // Determine heat to be generated
      CurrentHeatGeneration = CalculateWasteHeatGeneration();
      if (heatModule)
      {
        if (Enabled)

          heatModule.AddFlux(moduleID, NominalTemperature, CurrentHeatGeneration, true);
        else
        {
          heatModule.AddFlux(moduleID, 0f, CurrentHeatGeneration, false);
        }
      }
    }
    protected virtual void HandleHeatGenerationEditor()
    {
      CurrentHeatGeneration = CalculateHeatGenerationEditor();
      if (heatModule)
      {
        heatModule.AddFlux(moduleID, NominalTemperature, CurrentHeatGeneration, true);
      }
    }


    // handle core activities
    private void HandleCore()
    {
      if (heatModule)
      {
        InternalCoreTemperature = Mathf.Lerp(InternalCoreTemperature, heatModule.LoopTemperature,
          InternalCoreTemperatureResponseScale * TimeWarp.fixedDeltaTime);
      }
      else
      {
        if (Enabled)
        {
          InternalCoreTemperature = Mathf.Lerp(InternalCoreTemperature, NominalTemperature,
          InternalCoreTemperatureResponseScale * CurrentThrottle / 100f * TimeWarp.fixedDeltaTime);
        }
        else
        {
          InternalCoreTemperature = Mathf.Lerp(InternalCoreTemperature, GetEnvironmentTemperature(),
          InternalCoreTemperatureResponseScale * TimeWarp.fixedDeltaTime);
        }
      }

      // Update reactor damage
      float critExceedance = InternalCoreTemperature - CriticalTemperature;

      // If overheated too much, damage the core
      if (critExceedance > 0f && TimeWarp.CurrentRate < 100f)
      {
        if (SystemHeatGameSettings_ReactorDamage.ReactorDamage)
          // core is damaged by Rate * temp exceedance * time
          CoreIntegrity = Mathf.MoveTowards(CoreIntegrity, 0f, CoreDamageRate * critExceedance * TimeWarp.fixedDeltaTime);
      }
    }

    protected virtual float CalculateGoalThrottle(float timeStep)
    {
      double shipEC = 0d;
      double shipMaxEC = 0d;
      // Determine need for power
      part.GetConnectedResourceTotals(PartResourceLibrary.ElectricityHashcode, out shipEC, out shipMaxEC, true);

      float maxGeneration = ElectricalGeneration.Evaluate(100f) * CoreIntegrity / 100f;
      float minGeneration = ElectricalGeneration.Evaluate(MinimumThrottle) * timeStep;
      float idealGeneration = Mathf.Min(maxGeneration * timeStep, (float)(shipMaxEC - shipEC));
      float powerToGenerate = Mathf.Max(minGeneration, idealGeneration);

      return (powerToGenerate / timeStep) / maxGeneration * 100f;
    }

    public virtual string GetContractObjectiveType()
    {
      return "Generator";
    }
    public virtual bool CheckContractObjectiveValidity()
    {
      return true;
    }

    protected double burnRate = 0d;
    protected bool fuelCheckPassed = false;

    private void HandleResourceActivities(float timeStep)
    {

      if (!ManualControl)
        CurrentReactorThrottle = CalculateGoalThrottle(timeStep);

      fuelCheckPassed = true;
      burnRate = 0d;
      float fuelThrottle = CurrentReactorThrottle / 100f;

      // Check for full-ness
      foreach (ResourceRatio ratio in outputs)
      {
        if (CheckFull(ratio.ResourceName, fuelThrottle * ratio.Ratio * timeStep))
        {
          Utils.Log($"[ModuleSystemHeatFissionReactor]: Reactor waste storage full");
          ReactorDeactivated();
          return;
        }

      }
      // Check for fuel and consume
      foreach (ResourceRatio ratio in inputs)
      {
        double amt = this.part.RequestResource(ratio.ResourceName, fuelThrottle * ratio.Ratio * timeStep, ratio.FlowMode);

        if (MinimumThrottle > 0)
        {
          if (amt < 0.0000000000001)
          {
            Utils.Log($"[ModuleSystemHeatFissionReactor]: Reactor has no fuel!");
            ReactorDeactivated();
            fuelCheckPassed = false;
          }
        }
        else
        {
          if (fuelThrottle > 0.001f)
          {
            if (amt < 0.0000000000001)
            {
              Utils.Log($"[ModuleSystemHeatFissionReactor]: Reactor has no fuel!");
              ReactorDeactivated();
              fuelCheckPassed = false;
            }
          }
        }
        if (ratio.ResourceName == FuelName)
          burnRate = fuelThrottle * ratio.Ratio;
      }
      // If fuel consumed, add waste
      if (fuelCheckPassed)
      {
        foreach (ResourceRatio ratio in outputs)
        {
          double amt = this.part.RequestResource(ratio.ResourceName, -fuelThrottle * ratio.Ratio * timeStep, ratio.FlowMode);
        }
      }

      if (GeneratesElectricity)
      {
        if (HighLogic.LoadedSceneIsEditor)
        {
          CurrentElectricalGeneration = ElectricalGeneration.Evaluate(CurrentReactorThrottle);
        }
        if (fuelCheckPassed)
        {
          CurrentElectricalGeneration = ElectricalGeneration.Evaluate(CurrentThrottle);
          this.part.RequestResource(PartResourceLibrary.ElectricityHashcode, -CurrentElectricalGeneration * timeStep, ResourceFlowMode.ALL_VESSEL);
        }
      }

    }
    public bool CheckFull(string nm, double eps)
    {

      if (this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id) != null)
        if (this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id).amount + eps >=
          this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id).maxAmount)
          return true;
        else
          return false;


      return false;
    }
    protected float GetEnvironmentTemperature()
    {
      if (HighLogic.LoadedSceneIsEditor)
        return SystemHeatSettings.SpaceTemperature;

      if (part.vessel.mainBody.GetTemperature(part.vessel.altitude) > 50000d)
      {
        return SystemHeatSettings.SpaceTemperature;
      }
      return Mathf.Clamp((float)part.vessel.mainBody.GetTemperature(part.vessel.altitude), SystemHeatSettings.SpaceTemperature, 50000f);


    }

    #region Repair
    public bool TryRepairReactor()
    {
      float repairThreshold = SystemHeatGameSettings_ReactorDamage.RepairThreshold * 100f;
      int engineerLevel = SystemHeatGameSettings_ReactorDamage.EngineerLevel;
      float maxRepair = SystemHeatGameSettings_ReactorDamage.RepairMax * 100f;
      if (OverrideRepairSettings)
      {
        repairThreshold = MinRepairPercent;
        engineerLevel = EngineerLevelForRepair;
        maxRepair = MaxRepairPercent;
      }
      if (CoreIntegrity <= repairThreshold)
      {
        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Message_Repair_CoreTooDamaged"), 5.0f, ScreenMessageStyle.UPPER_CENTER));
        return false;
      }
      if (FlightGlobals.ActiveVessel.VesselValues.RepairSkill.value < engineerLevel)
      {
        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Message_Repair_EngineerLevelTooLow", engineerLevel.ToString("F0")), 5.0f, ScreenMessageStyle.UPPER_CENTER));
        return false;
      }
      if (FlightGlobals.ActiveVessel.evaController.ModuleInventoryPartReference.TotalAmountOfPartStored("evaRepairKit") < 1)
      {
        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Message_Repair_NoKits"), 5.0f, ScreenMessageStyle.UPPER_CENTER));
        return false;
      }
      if (Enabled)
      {
        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Message_Repair_NotWhileRunning"),
            5.0f, ScreenMessageStyle.UPPER_CENTER));
        return false;
      }
      if (heatModule.LoopTemperature > MaxTempForRepair)
      {
        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Message_Repair_CoreTooHot", MaxTempForRepair.ToString("F0")), 5.0f, ScreenMessageStyle.UPPER_CENTER));
        return false;
      }

      if (CoreIntegrity >= maxRepair)
      {
        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Message_Repair_CoreAlreadyRepaired", maxRepair.ToString("F0")),
            5.0f, ScreenMessageStyle.UPPER_CENTER));
        return false;
      }
      return true;
    }

    // Repair the reactor to max Repair percent
    public void DoReactorRepair()
    {
      float repairPercent = SystemHeatGameSettings_ReactorDamage.RepairPerKit * 100f;
      float maxRepair = SystemHeatGameSettings_ReactorDamage.RepairMax * 100f;
      if (OverrideRepairSettings)
      {
        maxRepair = MaxRepairPercent;
        repairPercent = RepairAmountPerKit;
      }
      FlightGlobals.ActiveVessel.evaController.ModuleInventoryPartReference.RemoveNPartsFromInventory("evaRepairKit", 1, playSound: true);

      this.CoreIntegrity = Mathf.Clamp(this.CoreIntegrity + repairPercent, 0f, maxRepair);
      ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Message_Repair_RepairSuccess",
        this.CoreIntegrity.ToString("F0")), 5.0f, ScreenMessageStyle.UPPER_CENTER));
    }

    #endregion

    #region Refuelling
    // Finds time remaining at specified fuel burn rates
    public string FindTimeRemaining(double amount, double rate)
    {
      if (rate < 0.0000001)
      {
        return Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_FuelStatus_VeryLong");
      }
      double remaining = amount / rate;
      if (remaining >= 0)
      {
        return ModuleUtils.FormatTimeString(remaining);
      }
      {
        return Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_FuelStatus_Exhausted");
      }
    }
    #endregion
  }


}

