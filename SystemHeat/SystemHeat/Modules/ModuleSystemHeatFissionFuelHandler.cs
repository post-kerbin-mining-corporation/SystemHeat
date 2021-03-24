using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;

namespace SystemHeat.Modules
{
  public class ModuleSystemHeatFissionFuelContainer: PartModule
  {

    // Fuel that is dangerous to transfer
    [KSPField(isPersistant = false)]
    public string WasteResourceName = "DepletedFuel";

    // Fuel that is safe to transfer
    [KSPField(isPersistant = false)]
    public string FuelResourceName = "EnrichedUranium";

    [KSPField(isPersistant = false)]
    public int EngineerLevelForRefuel = 3;

    // Color Changer for waste
    [KSPField(isPersistant = false)]
    public string wasteModuleID;

    // Color Changer for waste
    [KSPField(isPersistant = false)]
    public string fuelModuleID;

    private ModuleColorChanger wasteColorChanger;
    private ModuleColorChanger fuelColorChanger;

    public override string GetInfo()
    {
      return Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionFuelContainer_PartInfo", EngineerLevelForRefuel.ToString());
    }
    public override string GetModuleDisplayName()
    {
      return Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionFuelContainer_ModuleName");
    }


    public override void OnAwake()
    {
      base.OnAwake();
      if (HighLogic.LoadedSceneIsFlight)
      {
        GameEvents.onCrewBoardVessel.Add(new EventData<GameEvents.FromToAction<Part,Part>>.OnEvent(onCrewBoardVessel));
        GameEvents.onVesselCrewWasModified.Add(new EventData<Vessel>.OnEvent(onVesselCrewWasModified));
      }
    }

    void OnDestroy()
    {
      // Clean up events when the item is destroyed
      GameEvents.onCrewBoardVessel.Remove(onCrewBoardVessel);

      GameEvents.onVesselCrewWasModified.Remove(onVesselCrewWasModified);
    }
    public void onCrewBoardVessel(GameEvents.FromToAction<Part, Part> action)
    {
      if (HighLogic.LoadedSceneIsFlight)
      {
        Utils.Log("[ModuleSystemHeatFissionFuelContainer]: New crew boarded");
        HandleTransferModes();
      }
    }

    public void onVesselCrewWasModified(Vessel ves)
    {
      if (HighLogic.LoadedSceneIsFlight)
      {
        Utils.Log("[ModuleSystemHeatFissionFuelContainer]: Vessel crew was modified");
        HandleTransferModes();
      }
    }

    public void Start()
    {
     
      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {
        if (fuelModuleID != "")
        {
          fuelColorChanger = this.GetComponents<ModuleColorChanger>().ToList().Find(i => i.moduleID == fuelModuleID);
        }
        if (wasteModuleID != "")
        {
          wasteColorChanger = this.GetComponents<ModuleColorChanger>().ToList().Find(i => i.moduleID == wasteModuleID);
        }
      }

      if (HighLogic.LoadedSceneIsFlight)
        HandleTransferModes();
    }

    private void FixedUpdate()
    {
      
      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {
        if (fuelColorChanger)
        {
          fuelColorChanger.SetScalar((float)(GetResourceAmount(FuelResourceName, false) / GetResourceAmount(FuelResourceName, true)));
        }
        if (wasteColorChanger)
        {
          wasteColorChanger.SetScalar((float)(GetResourceAmount(WasteResourceName, false) / GetResourceAmount(WasteResourceName, true)));
        }
      }

    }

    /// <summary>
    /// Change the transfer mode of a specified resource
    /// </summary>
    /// <param name="resourceName"></param>
    /// <param name="mode"></param>
    public void ChangeTransferMode(string resourceName, ResourceTransferMode mode)
    {
      var obj = PartResourceLibrary.Instance.GetDefinition(resourceName).GetType().GetField("_resourceTransferMode",
         System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      obj.SetValue(PartResourceLibrary.Instance.GetDefinition(resourceName), mode);
    }

    /// <summary>
    /// Get the engineer level of the crew
    /// </summary>
    /// <returns></returns>
    public int GetCrewLevel()
    {
      int maxLvl = 0;
      foreach (var crew in part.vessel.GetVesselCrew())
      {
        if (crew.experienceTrait.TypeName == "Engineer" && crew.experienceLevel > maxLvl)
          maxLvl = crew.experienceLevel;

      }
      return maxLvl;
    }

    /// <summary>
    /// Edit transfer modes in response to an event
    /// </summary>
    public void HandleTransferModes()
    {
      if (GetCrewLevel() >= EngineerLevelForRefuel)
      {
        Utils.Log($"[ModuleSystemHeatFissionFuelContainer]: Crew level is  {GetCrewLevel()}, setting transfer for fuel to PUMP");
        ChangeTransferMode(FuelResourceName, ResourceTransferMode.PUMP);
        ChangeTransferMode(WasteResourceName, ResourceTransferMode.PUMP);
      }
      else
      {
        Utils.Log($"[ModuleSystemHeatFissionFuelContainer]: Crew level is  {GetCrewLevel()}, setting transfer for fuel to NONE");
        ChangeTransferMode(FuelResourceName, ResourceTransferMode.NONE);
        ChangeTransferMode(WasteResourceName, ResourceTransferMode.NONE);
      }
    }
    /// <summary>
    /// Get the amount of a resource in a part
    /// </summary>
    /// <param name="nm"></param>
    /// <returns></returns>
    public double GetResourceAmount(string nm)
    {
      if (this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id) != null)
        return this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id).amount;
      else
        return 0.0;
    }
    /// <summary>
    /// Get the amount of a resource in a part
    /// </summary>
    /// <param name="nm"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public double GetResourceAmount(string nm, bool max)
    {
      if (max)
        if (this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id) != null)
          return this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id).maxAmount;
        else
          return 0.0;

      else
        return GetResourceAmount(nm);
    }
  }
}
