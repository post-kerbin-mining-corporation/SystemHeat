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
    public string moduleID = heater;

    // Volume of coolant provided by this system in L
    [KSPField(isPersistant = false)]
    public float volume = 10f;

    //  -- System level data storage --
    // Current total system temperature of all associated modules
    [KSPField(isPersistant = true)]
    public float totalSystemTemperature = 0f;

    // Current total system flux of all associated modules
    [KSPField(isPersistant = true)]
    public float totalSystemFlux = 0f;

    // -- Loop level data storage --
    // Loop that this system is part of
    [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Fuel Mode")]
    [UI_ChooseOption(affectSymCounterparts = UI_Scene.None, scene = UI_Scene.All, suppressEditorShipModified = true)]
    public int currentLoopID = 0;

    // Current temperature of the loop
    [KSPField(isPersistant = true)]
    public float currentLoopTemperature = 0f;

    // Current nominal temperature of the loop
    [KSPField(isPersistant = true, guiActive=false, guiName = "Loop Flux")]
    public float nominalLoopTemperature = 0f;

    // Current net flux of the loop
    [KSPField(isPersistant = true, guiActive=false, guiName = "Loop Flux")]
    public float currentLoopFlux = 0f;

    // Coolant being used (maps to a COOLANTTYPE)
    [KSPField(isPersistant = false)]
    public string coolantName = "";

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
    protected Dictionary<string, float> fluxes;

    public override void OnStart(PartModule.StartState state)
    {
      fluxes = new Dictionary<string, float>();
      if (Settings.DebugMode)
        Utils.Log("");
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
      totalSystemFlux = fluxes.Sum(x => x.Value);

      if (sourceTemperature > 0f)
      {
        totalSystemTemperature = totalSystemTemperature + (sourceTemperature - totalSystemTemperature);
      }
    }
    public void UpdateSimulationValues(float nominalTemp, float currentTemp, float currentNetFlux)
    {
      nominalLoopTemperature = nominalTemp;
      currentLoopTemperature = currentTemp;
      currentLoopFlux = currentNetFlux;
    }
    protected void FixedUpdate()
    {
      // Collect the total system flux at the end of an update

    }
  }
}
