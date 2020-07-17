using System;
using System.Linq;
using UnityEngine;


namespace SystemHeat
{
  /// <summary>
  /// A module to generate SystemHeat from an engine module
  /// </summary>
  public class ModuleSystemHeatSink : PartModule
  {
    // This should be unique on the part
    [KSPField(isPersistant = false)]
    public string moduleID;

    // This should correspond to the related ModuleSystemHeat
    [KSPField(isPersistant = false)]
    public string systemHeatModuleID;

    [KSPField(isPersistant = true)]
    public bool storageEnabled = true;

    // The max kW to consume
    [KSPField(isPersistant = false)]
    public float maxHeatRate = 500f;

    /// <summary>
    ///  Heat storage params
    /// </summary>
    /// // The max heat stored by the part, in kJ
    [KSPField(isPersistant = true)]
    public float heatStored = 0f;

    /// // The max heat stored by the part, in kJ
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


    /// <summary>
    /// Evaporator stuff
    /// </summary>
    /// 

    [KSPField(isPersistant = false)]
    public bool allowOpenCycle = false;

    [KSPField(isPersistant = true)]
    public bool OpenCycleEnabled = false;

    [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Resource")]
    [UI_ChooseOption(affectSymCounterparts = UI_Scene.Editor, options = new[] { "None" }, scene = UI_Scene.All, suppressEditorShipModified = false)]
    public int resourceID = 0;


    // UI Fields
    // UI field for showing heat
    [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "Storage Accepted")]
    public string systemHeatGeneration = "";

    // UI field for showing heat
    [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = false, guiName = "Storage Temperature")]
    public string systemTemperature = "";

    // UI field for showing heat
    [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = false, guiName = "Heat Stored")]
    public string systemHeatStored = "";

    protected ModuleSystemHeat heatModule;

    public override string GetModuleDisplayName()
    {
      return "Heat Sink";
    }

    public override string GetInfo()
    {
      string msg = "";
      return msg;
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
    }
    void SetupUI()
    {
      BaseField chooseField = Fields["currentLoopID"];
      UI_ChooseOption chooseOption = HighLogic.LoadedSceneIsFlight ? chooseField.uiControlFlight as UI_ChooseOption : chooseField.uiControlEditor as UI_ChooseOption;
      chooseOption.options = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
      chooseOption.onFieldChanged = ChangeResource;
    }

    public void ChangeResource(BaseField field, object oldFieldValueObj)
    {

    }

    public void FixedUpdate()
    {
      if (!heatModule)
        return;

      if (HighLogic.LoadedSceneIsFlight)
      {
        GenerateHeatFlight();
        systemTemperature = String.Format("{0} K", storageTemperature.ToString("F0"));
        systemHeatStored = String.Format("{0}/{1} kJ", heatStored.ToString("F0"), heatStorageMaximum.ToString("F0"));
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
        if (heatModule.currentLoopTemperature >= heatModule.nominalLoopTemperature)
        {
          storageTemperature = storageTemperature + (-heatModule.consumedSystemFlux) / (heatStorageSpecificHeat * heatStorageMass); // Q = mcT
          heatStored = Mathf.Clamp(heatStored + (-heatModule.consumedSystemFlux) * TimeWarp.fixedDeltaTime, 0f, heatStorageMaximum);
        }
      }
      else if (!storageEnabled && heatStored > 0f)
      {
        heatModule.AddFlux(moduleID, storageTemperature, maxHeatRate);
        heatStored = Mathf.Clamp(heatStored - maxHeatRate * TimeWarp.fixedDeltaTime, 0f, heatStorageMaximum);
        storageTemperature = storageTemperature - maxHeatRate * TimeWarp.fixedDeltaTime / (heatStorageSpecificHeat * heatStorageMass); // Q = mcT
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
