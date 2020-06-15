using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SystemHeat
{
  /// <summary>
  /// This is the editor version of the systemHeat simulator interface.
  /// </summary>
  [KSPAddon(KSPAddon.Startup.EditorAny, false)]
  public class SystemHeatEditor: MonoBehaviour
  {
    #region Accessors
    public SystemHeatSimulator Simulator
    {
      get { return simulator;}
    }
    public static SystemHeatEditor Instance { get; private set; }

    public bool Ready { get {return dataReady; }}

    #endregion

    #region PrivateVariables
    SystemHeatSimulator simulator;

    bool dataReady = false;
    #endregion

    protected void Awake()
    {
      Instance = this;
    }
    protected void Start()
    {
      SetupEditorCallbacks();
    }
    protected void FixedUpdate()
    {
      if (simulator != null)
      {
        simulator.SimulateEditor();
      }
    }
    #region Editor
    protected void SetupEditorCallbacks()
    {
      /// Add events for editor modifications
      if (HighLogic.LoadedSceneIsEditor)
      {
        GameEvents.onEditorShipModified.Add(new EventData<ShipConstruct>.OnEvent(onEditorVesselModified));
        GameEvents.onEditorRestart.Add(new EventVoid.OnEvent(onEditorVesselReset));
        GameEvents.onEditorStarted.Add(new EventVoid.OnEvent(onEditorVesselStart));
        GameEvents.onEditorPartDeleted.Add(new EventData<Part>.OnEvent(onEditorPartDeleted));
        GameEvents.onEditorPodDeleted.Add(new EventVoid.OnEvent(onEditorVesselReset));
        GameEvents.onEditorLoad.Add(new EventData<ShipConstruct, KSP.UI.Screens.CraftBrowserDialog.LoadType>.OnEvent(onEditorVesselLoad));
        
        GameEvents.onPartRemove.Add(new EventData<GameEvents.HostTargetAction<Part, Part>>.OnEvent(onEditorVesselPartRemoved));
      }
      else
      {
        GameEvents.onEditorShipModified.Remove(new EventData<ShipConstruct>.OnEvent(onEditorVesselModified));
        GameEvents.onEditorRestart.Remove(new EventVoid.OnEvent(onEditorVesselReset));
        GameEvents.onEditorStarted.Remove(new EventVoid.OnEvent(onEditorVesselStart));
        GameEvents.onEditorPodDeleted.Remove(new EventVoid.OnEvent(onEditorVesselReset));
       GameEvents.onEditorPartDeleted.Remove(new EventData<Part>.OnEvent(onEditorPartDeleted));
        GameEvents.onEditorLoad.Remove(new EventData<ShipConstruct, KSP.UI.Screens.CraftBrowserDialog.LoadType>.OnEvent(onEditorVesselLoad));
        GameEvents.onPartRemove.Remove(new EventData<GameEvents.HostTargetAction<Part, Part>>.OnEvent(onEditorVesselPartRemoved));
      }
    }

    protected void InitializeEditorConstruct(ShipConstruct ship, bool forceReset)
    {
      dataReady = false;
      if (ship != null)
      {
        if (simulator == null)
        {
          simulator = new SystemHeatSimulator();
          simulator.Reset(ship.Parts);
        }
        if (simulator != null && forceReset)
        {
          simulator.Reset(ship.Parts);
        } else
        {
          simulator.Refresh(ship.Parts);
        }
        if (SystemHeatSettings.DebugSimulation)
        {
          Utils.Log(String.Format("[SystemHeatEditor]: Dumping simulator: \n{0}", simulator.ToString()));
        }
        dataReady = true;
      }
      else
      {
        if (SystemHeatSettings.DebugSimulation)
        {
          Utils.Log(String.Format("[SystemHeatEditor]: Ship is null"));
        }
        simulator = new SystemHeatSimulator();
      }
    }

    protected void RemovePart(Part p)
    {
      simulator.RemovePart(p);
    }
    #endregion


    #region Game Events
    public void onEditorPartDeleted(Part part)
    {
      if (SystemHeatSettings.DebugSimulation)
        Utils.Log("[SystemHeatEditor]: Part Delete");
      if (!HighLogic.LoadedSceneIsEditor) { return; }

      InitializeEditorConstruct(EditorLogic.fetch.ship, false);
    }
    public void onEditorVesselReset()
    {
      if (SystemHeatSettings.DebugSimulation)
        Utils.Log("[SystemHeatEditor]: Vessel RESET");
      if (!HighLogic.LoadedSceneIsEditor) { return; }
      InitializeEditorConstruct(EditorLogic.fetch.ship, true);
    }
    public void onEditorVesselStart()
    {
      if (SystemHeatSettings.DebugSimulation)
        Utils.Log("[SystemHeatEditor]: Vessel START");
      if (!HighLogic.LoadedSceneIsEditor) { return; }
      InitializeEditorConstruct(EditorLogic.fetch.ship, true);
    }
    public void onEditorVesselLoad(ShipConstruct ship, KSP.UI.Screens.CraftBrowserDialog.LoadType type)
    {
      if (SystemHeatSettings.DebugSimulation)
        Utils.Log("[SystemHeatEditor]: Vessel LOAD");
      if (!HighLogic.LoadedSceneIsEditor) { return; }
      InitializeEditorConstruct(ship, true);
    }
    public void onEditorVesselPartRemoved(GameEvents.HostTargetAction<Part, Part> p)
    {
      if (SystemHeatSettings.DebugSimulation)
        Utils.Log("[SystemHeatEditor]: Vessel PART REMOVE");
      if (!HighLogic.LoadedSceneIsEditor) { return; }

      if (simulator == null)
        InitializeEditorConstruct(EditorLogic.fetch.ship, false);
      else
        RemovePart(p.target);
    }
    public void onEditorVesselModified(ShipConstruct ship)
    {
      if (SystemHeatSettings.DebugSimulation)
        Utils.Log("[SystemHeatEditor]: Vessel MODIFIED");
      if (!HighLogic.LoadedSceneIsEditor) { return; }
      InitializeEditorConstruct(ship, false);
    }
    #endregion
  }

}
