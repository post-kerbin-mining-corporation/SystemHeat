using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SystemHeat
{
  /// <summary>
  /// The simulation interface to the heat system. All heat producing or consuming modules
  /// on a vessel interact with an instance of this module to add and remove heat.
  /// </summary>
  public class ModuleSystemHeat: PartModule
  {
    // Unique name of the module on a part
    [KSPField(isPersistant = false)]
    public string moduleID = "heatModule";

    // Name of the icon to use
    [KSPField(isPersistant = false)]
    public string iconName = "Icon_Gears";

    // Volume of coolant provided by this system in L
    [KSPField(isPersistant = false)]
    public float volume = 10f;

    //  -- System level data storage --
    // Current total system temperature of all associated modules
    [KSPField(isPersistant = true, guiActive = false, guiName = "Sys Temp")]
    public float totalSystemTemperature = 0f;

    // Current total system flux of all associated modules
    [KSPField(isPersistant = true, guiActive = false, guiName = "Sys Flux")]
    public float totalSystemFlux = 0f;

    public float systemNominalTemperature = 0f;

    // -- Loop level data storage --
    // Loop that this system is part of
    [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Loop ID")]
    [UI_ChooseOption(affectSymCounterparts = UI_Scene.Editor, options = new[] { "None" }, scene = UI_Scene.All, suppressEditorShipModified = false)]
    public int currentLoopID = 0;

    // Current temperature of the loop
    [KSPField(isPersistant = true, guiActive = false, guiName = "Loop Temp")]
    public float currentLoopTemperature = 0f;

    // Current nominal temperature of the loop
    [KSPField(isPersistant = true, guiActive = false, guiName = "Loop Nom. Temp")]
    public float nominalLoopTemperature = 0f;

    // Current net flux of the loop
    [KSPField(isPersistant = true, guiActive = false, guiName = "Loop Flux")]
    public float currentLoopFlux = 0f;

    // Coolant being used (maps to a COOLANTTYPE)
    [KSPField(isPersistant = false)]
    public string coolantName = "default";

    public int LoopID {
      get {return currentLoopID; }
      set {currentLoopID = value; }
    }

    public float LoopTemperature {
      get {return currentLoopTemperature; }
      set {currentLoopTemperature = value; }
    }

    public float LoopFlux {
      get {return currentLoopFlux; }
      set {currentLoopFlux = value; }
    }

    public string CurrentStatusString
    {
      get
      {
        if (totalSystemFlux >= 0f)
        {
          return $"Temperature Output: {systemNominalTemperature} K \nHeat Output: {totalSystemFlux} kW";
        }
        else
        {
          return $"Maximum Temperature {systemNominalTemperature} K \nHeat Consumed {totalSystemFlux} kW";
        }
      }
    }

    protected Dictionary<string, float> fluxes;
    protected Dictionary<string, float> temperatures;
    protected List<int> loopIDs;

    public void Start()
    {
      loopIDs = new List<int>();
      for (int i = 0; i < SystemHeatSettings.maxLoopCount; i++)
        loopIDs.Add(i);

      fluxes = new Dictionary<string, float>();
      temperatures = new Dictionary<string, float>();
      SetupUI();

      if (SystemHeatSettings.DebugModules)
        Utils.Log("[ModuleSystemHeat]: Setup complete");



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

    void SetupUI()
    {
      BaseField chooseField = Fields["currentLoopID"];
      UI_ChooseOption chooseOption = HighLogic.LoadedSceneIsFlight ? chooseField.uiControlFlight as UI_ChooseOption : chooseField.uiControlEditor as UI_ChooseOption;
      chooseOption.options = new string[] {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9"};
      chooseOption.onFieldChanged = ChangeLoop;
     
    }

    private void ChangeLoop(BaseField field, object oldFieldValueObj)
    {
      //LoopID = currentLoopID;
    }

    /// <summary>
    /// Add heat flux at a given temperature to system
    /// </summary>
    /// <param name="id">the string ID of the source (should be unique)</param>
    /// <param name="sourceTemperature">the temperature of the source</param>
    /// <param name="flux">the flux of the source</param>
    public void AddFlux(string id, float sourceTemperature, float flux)
    {
      fluxes[id] = flux;

      int count = 0;
      if (flux > 0f)
      {
        count++;
        temperatures[id] = sourceTemperature;
      }
      else
      {
        temperatures[id] = 0f;
      }

      totalSystemFlux = fluxes.Sum(x => x.Value);
      totalSystemTemperature = temperatures.Sum(x => x.Value);

      systemNominalTemperature = totalSystemTemperature / count;
    }

    public void UpdateSimulationValues(float nominalTemp, float currentTemp, float currentNetFlux)
    {
      nominalLoopTemperature = nominalTemp;
      currentLoopTemperature = currentTemp;
      currentLoopFlux = currentNetFlux;
    }

    protected void FixedUpdate()
    {
    }
  }
}
