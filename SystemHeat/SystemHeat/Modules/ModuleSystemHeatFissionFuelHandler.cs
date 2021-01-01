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
    public string RefuelCargoPartName = "systemheat-nuclear-container-1";

    [KSPField(isPersistant = false)]
    public float MaxTemperatureForRefuel = 330;

    [KSPField(isPersistant = false)]
    public int EngineerLevelForRefuel = 3;

    // Color Changer for waste
    [KSPField(isPersistant = false)]
    public string wasteModuleID;

    // Color Changer for waste
    [KSPField(isPersistant = false)]
    public string fuelModuleID;

    [KSPEvent(externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 3.5f, guiName = "Store Fuel")]
    public void StoreFuel()
    {
      if (CheckEVARequirements() && CheckPartRequirements())
      {
        TransferResourceFromEVA(FuelResourceName);
      }
    }
    [KSPEvent(externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 3.5f, guiName = "Store Waste")]
    public void StoreWaste()
    {
      if (CheckEVARequirements() && CheckPartRequirements())
      {
        TransferResourceFromEVA(WasteResourceName);
      }
    }

    [KSPEvent(externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 3.5f, guiName = "Collect Fuel")]
    public void CollectFuel()
    {
      if (CheckEVARequirements() && CheckPartRequirements())
      {
        TransferResourceToEVA(FuelResourceName);
      }
    }
    [KSPEvent(externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 3.5f, guiName = "Collect Waste")]
    public void CollectWaste()
    {
      if (CheckEVARequirements() && CheckPartRequirements())
      {
        TransferResourceToEVA(WasteResourceName);
      }
    }

    private ModuleColorChanger wasteColorChanger;
    private ModuleColorChanger fuelColorChanger;

    public override string GetInfo()
    {
      return Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionFuelContainer_PartInfo");
    }
    public override string GetModuleDisplayName()
    {
      return Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionFuelContainer_ModuleName");
    }

    public void Start()
    {
      Events["StoreWaste"].guiName = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionFuelContainer_Field_Store_Name", WasteResourceName) ;
      Events["StoreFuel"].guiName = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionFuelContainer_Field_Store_Name", FuelResourceName);
      Events["CollectWaste"].guiName = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionFuelContainer_Field_Collect_Name", WasteResourceName);
      Events["CollectFuel"].guiName = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionFuelContainer_Field_Collect_Name", FuelResourceName);
      

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
    }

    private void FixedUpdate()
    {
      if (HighLogic.LoadedSceneIsFlight)
      {
        
        if (FlightGlobals.ActiveVessel.evaController != null)
        {
          if (FlightGlobals.ActiveVessel.evaController.ModuleInventoryPartReference.TotalAmountOfPartStored(RefuelCargoPartName) >= 1)
          {
            if (GetResourceAmount(WasteResourceName) <= 0d)
            {
              Events["CollectWaste"].active = false;
            }
            else
            {
              Events["CollectWaste"].active = true;
            }
            if (GetResourceAmount(FuelResourceName) <= 0d)
            {
              Events["CollectFuel"].active = false;
            }
            else
            {
              Events["CollectFuel"].active = true;
            }

            if (GetResourceEVAAmount(WasteResourceName) <= 0d)
            {
              Events["StoreWaste"].active = false;
            }
            else
            {
              Events["StoreWaste"].active = true;
            }
            if (GetResourceEVAAmount(FuelResourceName) <= 0d)
            {
              Events["StoreFuel"].active = false;
            }
            else
            {
              Events["StoreFuel"].active = true;
            }
          }
          else
          { 
            Events["CollectWaste"].active = false;
            Events["CollectFuel"].active = false;
            Events["StoreFuel"].active = false;
            Events["StoreWaste"].active = false;
          }
        }
      }
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

    protected double GetResourceEVAAmount(string resourceName)
    {

      double amt = 0d;
      if (FlightGlobals.ActiveVessel.evaController != null)
      {
        for (int i = 0; i < FlightGlobals.ActiveVessel.evaController.ModuleInventoryPartReference.InventorySlots; i++)
        {
          if (!FlightGlobals.ActiveVessel.evaController.ModuleInventoryPartReference.IsSlotEmpty(i))
          {
            StoredPart sPart = FlightGlobals.ActiveVessel.evaController.ModuleInventoryPartReference.storedParts[i];
            if (sPart.partName == RefuelCargoPartName)
            {
              ProtoPartResourceSnapshot res = sPart.snapshot.resources.Find(x => x.resourceName == resourceName);
              amt += res.amount;
            }
          }
        }
      }
      return amt;
    }
    protected void TransferResourceToEVA(string resourceName)
    {
      double availableResource = GetResourceAmount(resourceName, true);
      double toRemove = 0d;
      for (int i = 0; i < FlightGlobals.ActiveVessel.evaController.ModuleInventoryPartReference.InventorySlots; i++)
      {
        if (availableResource > 0d)
        {
          if (!FlightGlobals.ActiveVessel.evaController.ModuleInventoryPartReference.IsSlotEmpty(i))
          {
            StoredPart sPart = FlightGlobals.ActiveVessel.evaController.ModuleInventoryPartReference.storedParts[i];
            if (sPart.partName == RefuelCargoPartName)
            {
              ProtoPartResourceSnapshot res = sPart.snapshot.resources.Find(x => x.resourceName == resourceName);
              double availableSpace = res.maxAmount - res.amount;
              double addable = UtilMath.Min(availableSpace, availableResource);
              toRemove += addable;

              Utils.Log($"Added {addable} {resourceName} to {sPart.partName} ({availableResource} units in source, {availableSpace} space in stored part)");

              availableResource = UtilMath.Clamp(availableResource - addable, 0, availableResource);
              res.amount = UtilMath.Clamp(res.amount + addable, 0d, res.maxAmount);
            }
          }
        }
      }
      ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionFuelContainer_Message_Collected",
        toRemove.ToString("F2"),
        resourceName,
        part.partInfo.title
        ), 5.0f, ScreenMessageStyle.UPPER_CENTER));;
      Utils.Log($"Removed {toRemove} {resourceName} from {part.partInfo.title}");
      part.RequestResource(resourceName, toRemove, ResourceFlowMode.NO_FLOW);
    }

    protected void TransferResourceFromEVA(string resourceName)
    {
      double availableSpace = GetResourceAmount(resourceName, true) - GetResourceAmount(resourceName);
      double toAdd = 0d;
      for (int i=0;i< FlightGlobals.ActiveVessel.evaController.ModuleInventoryPartReference.InventorySlots; i++)
      {
        if (availableSpace > 0d)
        {
          if (!FlightGlobals.ActiveVessel.evaController.ModuleInventoryPartReference.IsSlotEmpty(i))
          {
            StoredPart sPart = FlightGlobals.ActiveVessel.evaController.ModuleInventoryPartReference.storedParts[i];
            if (sPart.partName == RefuelCargoPartName)
            {
              ProtoPartResourceSnapshot res = sPart.snapshot.resources.Find(x => x.resourceName == resourceName);
              double availableResource = res.amount;
              double addable = UtilMath.Min(availableSpace, availableResource);
              toAdd += addable;

              Utils.Log($"Removed {addable} {resourceName} from {sPart.partName} ({availableResource} units in part, {availableSpace} space in target)");

              availableSpace = UtilMath.Clamp(availableSpace - addable, 0, availableSpace);
              res.amount = UtilMath.Clamp(res.amount - addable, 0d, res.maxAmount);
            }
          }
        }
      }
      ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionFuelContainer_Message_Stored",
       toAdd.ToString("F2"),
       resourceName,
       part.partInfo.title
       ), 5.0f, ScreenMessageStyle.UPPER_CENTER)); ;
      Utils.Log($"Added {toAdd} {resourceName} to {part.partInfo.title}");
      part.RequestResource(resourceName, -toAdd, ResourceFlowMode.NO_FLOW);
    }
    /// <summary>
    /// Test to see if the EVA kerbal can transfer resources
    /// </summary>
    /// <returns></returns>
    protected bool CheckEVARequirements()
    {
      if (FlightGlobals.ActiveVessel.VesselValues.RepairSkill.value < EngineerLevelForRefuel)
      {
        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionFuelContainer_Message_AbortEngineerLevel", EngineerLevelForRefuel.ToString("F0")), 5.0f, ScreenMessageStyle.UPPER_CENTER));
        return false;
      }
      if (FlightGlobals.ActiveVessel.evaController.ModuleInventoryPartReference.TotalAmountOfPartStored(RefuelCargoPartName) < 1)
      {
        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionFuelContainer_Message_NoFuelContainer"), 5.0f, ScreenMessageStyle.UPPER_CENTER));
        return false;
      }
      return true;
    }

    /// <summary>
    /// Test to see if this part can transfter resources
    /// </summary>
    /// <param name="nm"></param>
    /// <returns></returns>
    protected bool CheckPartRequirements()
    {
      // Some modules need to be off.
      ModuleSystemHeat heat = GetComponent<ModuleSystemHeat>();
      ModuleSystemHeatConverter[] converters = GetComponents<ModuleSystemHeatConverter>();
      ModuleSystemHeatFissionReactor reactor = GetComponent<ModuleSystemHeatFissionReactor>();
      ModuleSystemHeatFissionEngine engine = GetComponent<ModuleSystemHeatFissionEngine>();

      // Fail if a converter is on
      foreach (ModuleSystemHeatConverter converter in converters)
      { 
        if (converter != null && converter.ModuleIsActive())
          {
          ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionFuelContainer_Message_AbortFromRunningConverter"), 5.0f, ScreenMessageStyle.UPPER_CENTER));
          return false;
        }
      }
      // Fail if a reactor is on
      if (reactor != null && reactor.Enabled)
      {
        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionFuelContainer_Message_AbortFromRunningReactor"), 5.0f, ScreenMessageStyle.UPPER_CENTER));
        return false;
      }
      //Fail if an nuclear engine is on
      if (engine != null && engine.Enabled)
      {
        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionFuelContainer_Message_AbortFromRunningReactor"), 5.0f, ScreenMessageStyle.UPPER_CENTER));
        return false;
      }

      // Fail if the part is too hot
      if (heat != null && heat.LoopTemperature > MaxTemperatureForRefuel)
      {
        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionFuelContainer_Message_AbortTooHot", MaxTemperatureForRefuel.ToString("F0")), 5.0f, ScreenMessageStyle.UPPER_CENTER));
        return false;
      }

      return true;
    }

    // Helpbers for getting a resource amount
    public double GetResourceAmount(string nm)
    {
      if (this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id) != null)
        return this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id).amount;
      else
        return 0.0;
    }
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
