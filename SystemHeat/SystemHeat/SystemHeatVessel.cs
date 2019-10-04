using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SystemHeat
{

  /// <summary>
  /// The actual heat simulation occurs on a vessel basis.
  /// </summary>
  public class SystemHeatVessel : VesselModule
  {
    #region Accessors
    public SystemHeatSimulator Simulator
    {
      get { return simulator;}
    }
    #endregion

    #region PrivateVariables
    SystemHeatSimulator simulator;
    bool vesselLoaded = false;
    bool dataReady = false;
    #endregion

    protected override void OnStart()
    {
      base.OnStart();
      simulator = new SystemHeatSimulator();

      // These events need to trigger a refresh
      GameEvents.onVesselGoOnRails.Add(new EventData<Vessel>.OnEvent(RefreshVesselData));
      GameEvents.onVesselWasModified.Add(new EventData<Vessel>.OnEvent(RefreshVesselData));
    }

    void OnDestroy()
    {
      // Clean up events when the item is destroyed
      GameEvents.onVesselGoOnRails.Remove(RefreshVesselData);
      GameEvents.onVesselWasModified.Remove(RefreshVesselData);
    }

    void FixedUpdate()
    {
      // Handle collecting data and resetting the vessel
      if (HighLogic.LoadedSceneIsFlight && !dataReady)
      {
        if (!vesselLoaded && FlightGlobals.ActiveVessel == vessel)
        {
          ResetSimulation();
          vesselLoaded = true;
        }
        if (vesselLoaded && FlightGlobals.ActiveVessel != vessel)
        {
          vesselLoaded = false;
        }
      }

      simulator.Simulate();
    }

    /// <summary>
    /// Referesh the data, given a Vessel event
    /// </summary>
    protected void RefreshVesselData(Vessel eventVessel)
    {
      if (SystemHeatSettings.DebugSimulation)
        Utils.Log(String.Format("[SystemHeatVessel]: Refreshing VesselData from Vessel event"));
    }
    /// <summary>
    /// Referesh the data, given a ConfigNode event
    /// </summary>
    protected void RefreshVesselData(ConfigNode node)
    {
      if (SystemHeatSettings.DebugSimulation)
        Utils.Log(String.Format("[SystemHeatVessel]: Refreshing VesselData from save node event", this.GetType().Name));
    }


    /// <summary>
    /// Rebuild all the loops from scratch
    /// </summary>
    protected void ResetSimulation()
    {
      if (vessel == null || vessel.Parts == null)
        return;

      if (SystemHeatSettings.DebugSimulation)
        Utils.Log(String.Format("[SystemHeatVessel]: Resetting Simulation for {0}", vessel.name));

      if (simulator != null)
        simulator.Reset(vessel.Parts);
    }
  }
}
