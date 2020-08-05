using System;
using System.Linq;
using UnityEngine;
using KSP.Localization;

namespace SystemHeat
{
  /// <summary>
  /// A module to provide a heat sink
  /// </summary>
  public class ModuleSystemHeatSink : PartModule
  {
    // This should be unique on the part
    [KSPField(isPersistant = false)]
    public string moduleID;

    // This should correspond to the related ModuleSystemHeat
    [KSPField(isPersistant = false)]
    public string systemHeatModuleID;

    // The max kW the heat sink can consume at once
    [KSPField(isPersistant = false)]
    public float maxHeatRate = 500f;
    
    /// Is the storage enabled?
    [KSPField(isPersistant = true)]
    public bool storageEnabled = true;

    /// // The current  heat stored by the part, in kJ
    [KSPField(isPersistant = true)]
    public float heatStored = 0f;

    /// // The temperature of the storage in the part, in K
    [KSPField(isPersistant = true)]
    public float storageTemperature = 0f;

    // The max heat stored by the part, in kJ
    [KSPField(isPersistant = false)]
    public float heatStorageMaximum = 5000f;

    // The mass of the heat storage (internal only)
    [KSPField(isPersistant = false)]
    public float heatStorageMass = 5f;

    // The Specific heat capacity of the part, in kJ/kg units (internal only)
    [KSPField(isPersistant = false)]
    public float heatStorageSpecificHeat = 1.26f;


    // Safety override 
    [KSPField(isPersistant = true, guiActive = false, guiName = "Dump rate", groupName = "heatsink", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatSink_UIGroup_Title"), UI_FloatRange(minValue = 0f, maxValue = 1500f, stepIncrement = 10f)]
    public float heatStorageDumpRate = 500f;

    // UI Fields
    // UI field for showing heat
    [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "Storage Accepted", groupName = "heatsink", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatSink_UIGroup_Title")]
    public string systemHeatGeneration = "";

    // UI field for showing heat
    [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = false, guiName = "Storage Temperature", groupName = "heatsink", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatSink_UIGroup_Title")]
    public string systemTemperature = "";

    // UI field for showing heat
    [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = false, guiName = "Storage Capacity Used", groupName = "heatsink", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatSink_UIGroup_Title")]
    public string systemHeatStored = "";


    /// KSPEVENTS
    /// ----------------------
    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Store Heat", active = true, groupName = "heatsink", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatSink_UIGroup_Title", groupStartCollapsed = false)]
    public void EnableStorage()
    {
      storageEnabled = true;
    }
    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Dispense Heat", active = false, groupName = "heatsink", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatSink_UIGroup_Title", groupStartCollapsed = false)]
    public void DisableStorage()
    {
      storageEnabled = false;
    }

   
    protected ModuleSystemHeat heatModule;

    public override string GetModuleDisplayName()
    {
      return Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatSink_ModuleName");
    }

    public override string GetInfo()
    {
      // Need to update this to strip the CoreHeat stuff from it
      string message = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatSink_PartInfo",
        (heatStorageMaximum/1000f).ToString("F0"),
        maxHeatRate.ToString("F0")
        );
      return message;
    }

    public void Start()
    {
      heatModule = this.GetComponents<ModuleSystemHeat>().ToList().Find(x => x.moduleID == systemHeatModuleID);
      if (heatModule == null)
        heatModule.GetComponent<ModuleSystemHeat>();

      if (SystemHeatSettings.DebugModules)
      {
        Utils.Log("[ModuleSystemHeatSink] Setup completed");
      }

      SetupUI();
    }
    void SetupUI()
    {
   
      var range = (UI_FloatRange)this.Fields["heatStorageDumpRate"].uiControlFlight;
      range.minValue = 0f;
      range.maxValue = maxHeatRate;

    }

    public void ChangeResource(BaseField field, object oldFieldValueObj)
    {

    }
    public void Update()
    {
      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {
        if (Events["EnableStorage"].active == storageEnabled || Events["DisableStorage"].active != storageEnabled)
        {
          Fields["heatStorageDumpRate"].guiActive = !storageEnabled;
          Events["DisableStorage"].active = storageEnabled;
          Events["EnableStorage"].active = !storageEnabled;
        }
        
       
      }
    }


    public void FixedUpdate()
    {
      if (!heatModule)
        return;

      if (HighLogic.LoadedSceneIsFlight)
      {
        GenerateHeatFlight();
        systemTemperature = String.Format("{0} K", storageTemperature.ToString("F0"));
        systemHeatStored = String.Format("{0}%", (heatStored/heatStorageMaximum *100f).ToString("F0"));
        systemHeatGeneration = String.Format("{0}/{1} kW", (-heatModule.consumedSystemFlux).ToString("F0"), maxHeatRate.ToString("F0"));

      }
      if (HighLogic.LoadedSceneIsEditor)
      {
        GenerateHeatEditor();
        systemTemperature = String.Format("{0} K", storageTemperature.ToString("F0"));
        systemHeatStored = String.Format("{0}/{1} kJ", heatStored.ToString("F0"), heatStorageMaximum.ToString("F0"));
        systemHeatGeneration = String.Format("{0}/{1} kW", (-heatModule.consumedSystemFlux).ToString("F0"), maxHeatRate.ToString("F0"));
      }
    }


    /// <summary>
    /// Generates heat in the editor scene
    /// </summary>
    protected void GenerateHeatFlight()
    {

      if (storageEnabled && heatStored < heatStorageMaximum)
      {
        heatModule.AddFlux(moduleID, 0f, -maxHeatRate);
        Fields["systemHeatGeneration"].guiName = "Storage Accepted";
        if (heatModule.currentLoopTemperature >= heatModule.nominalLoopTemperature)
        {
          storageTemperature = Mathf.Clamp(storageTemperature + (-heatModule.consumedSystemFlux) / (heatStorageSpecificHeat * heatStorageMass), 0f, 5000f); // Q = mcT
          heatStored = Mathf.Clamp(heatStored + (-heatModule.consumedSystemFlux) * TimeWarp.fixedDeltaTime, 0f, heatStorageMaximum);
        }
      }
      else if (!storageEnabled && heatStored > 0f)
      {
        Fields["systemHeatGeneration"].guiName = "Storage Dumping";
        heatModule.AddFlux(moduleID, storageTemperature, heatStorageDumpRate);
        heatStored = Mathf.Clamp(heatStored - heatStorageDumpRate * TimeWarp.fixedDeltaTime, 0f, heatStorageMaximum);
        storageTemperature = storageTemperature - heatStorageDumpRate * TimeWarp.fixedDeltaTime / (heatStorageSpecificHeat * heatStorageMass); // Q = mcT
      }

      else
      {
        heatModule.AddFlux(moduleID, 0f, 0f);
      }
    }

    /// <summary>
    /// Generates heat in the flight scene
    /// </summary>
    protected void GenerateHeatEditor()
    {
      if (storageEnabled && heatStored < heatStorageMaximum)
      {
        heatModule.AddFlux(moduleID, 0f, -maxHeatRate);
      }
      else
      {
        heatModule.AddFlux(moduleID, 0f, 0f);
      }
    }

  }
}
