using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SystemHeat;
using KSP.Localization;
using KSP.UI;
using Vectrosity;


namespace SystemHeat.UI
{
  /// <summary>
  /// The master controller for the system heat overlay.
  /// </summary>
  [KSPAddon(KSPAddon.Startup.FlightAndEditor, false)]
  public class SystemHeatOverlay : MonoBehaviour
  {

    public static SystemHeatOverlay Instance { get; private set; }
    public static GameObject UICanvas = null;
    public static bool Drawn { get; private set; }

    protected Transform overlayRoot;
    protected SystemHeatSimulator simulator;
    protected List<OverlayLoop> overlayLoops;

   // protected Dictionary<int, bool> overlayLoopVisibility;
    protected List<OverlayPanel> overlayPanels;

    protected void Awake()
    {
      Drawn = false;
      Instance = this;
      overlayLoops = new List<OverlayLoop>();
      //overlayLoopVisibility = new Dictionary<int, bool>();
      overlayPanels = new List<OverlayPanel>();
      overlayRoot = (new GameObject("SHOverlayRoot")).GetComponent<Transform>();

      if (SystemHeatSettings.DebugOverlay)
        Utils.Log("[SystemHeatOverlay]: Subscribing to events");
      GameEvents.onGameSceneLoadRequested.Add(new EventData<GameScenes>.OnEvent(onSceneChange));

      if (HighLogic.LoadedSceneIsEditor)
      {
        GameEvents.onEditorScreenChange.Add(new EventData<EditorScreen>.OnEvent(onEditorScreenChange));
        GameEvents.onEditorPartDeleted.Add(new EventData<Part>.OnEvent(onEditorPartDeleted));
        GameEvents.onEditorPartPicked.Add(new EventData<Part>.OnEvent(onEditorPartPicked));
        GameEvents.onEditorRestart.Add(new EventVoid.OnEvent(onEditorReset));
        GameEvents.onEditorLoad.Add(new EventData<ShipConstruct, KSP.UI.Screens.CraftBrowserDialog.LoadType>.OnEvent(onEditorLoad));
        GameEvents.onEditorStarted.Add(new EventVoid.OnEvent(onEditorStart));
      }
      else
      {

        GameEvents.OnMapEntered.Add(new EventVoid.OnEvent(onEnterMapView));
        GameEvents.OnMapExited.Add(new EventVoid.OnEvent(onExitMapView));
      }
    }
    protected void OnDestroy()
    {
      if (SystemHeatSettings.DebugOverlay)
        Utils.Log("[SystemHeatOverlay]: Unsubscribing to events");
      GameEvents.onGameSceneLoadRequested.Remove(onSceneChange);

      if (HighLogic.LoadedSceneIsEditor)
      {
        GameEvents.onEditorScreenChange.Remove(onEditorScreenChange);
        GameEvents.onEditorPartDeleted.Remove(onEditorPartDeleted);
        GameEvents.onEditorPartPicked.Remove(onEditorPartPicked);
      }
      else
      {
        GameEvents.OnMapEntered.Remove(onEnterMapView);
        GameEvents.OnMapExited.Remove(onExitMapView);
      }
    }
    protected void Start()
    {

    }
    protected void onEnterMapView()
    {
      if (SystemHeatSettings.DebugOverlay)
        Utils.Log("[SystemHeatOverlay]: Entered map view, clearing panels");

      VectorLine.SetCamera3D(PlanetariumCamera.Camera);
      ClearPanels();
    }
    protected void onExitMapView()
    {
      if (SystemHeatSettings.DebugOverlay)
        Utils.Log("[SystemHeatOverlay]: Entered map view, clearing panels");

      VectorLine.SetCamera3D(FlightCamera.fetch.mainCamera);

    }

    protected void onEditorScreenChange(EditorScreen screen)
    {

      if (SystemHeatSettings.DebugOverlay)
        Utils.Log("[SystemHeatOverlay]: Editor Screen Changed, clearing panels");
      ClearPanels();
    }
    protected void onEditorLoad(ShipConstruct ship, KSP.UI.Screens.CraftBrowserDialog.LoadType loadType)
    {

      if (SystemHeatSettings.DebugOverlay)
        Utils.Log("[SystemHeatOverlay]: Editor Load");
      ClearPanels();
    }
    protected void onEditorReset()
    {

      if (SystemHeatSettings.DebugOverlay)
        Utils.Log("[SystemHeatOverlay]: Editor Reset");
      ClearPanels();
    }

    protected void onEditorStart()
    {

      if (SystemHeatSettings.DebugOverlay)
        Utils.Log("[SystemHeatOverlay]: Editor Start");
      ClearPanels();
    }

    protected void onSceneChange(GameScenes scene)
    {
      if (SystemHeatSettings.DebugOverlay)
        Utils.Log("[SystemHeatOverlay]: Changing Scenes, clearing panels");
      SetVisible(false);
      ClearPanels();
    }

    protected void onEditorPartDeleted(Part part)
    {
      if (overlayPanels != null)
        for (int i = overlayPanels.Count - 1; i >= 0; i--)
        {
          if (overlayPanels[i].heatModule.part == part)
          {
            if (SystemHeatSettings.DebugOverlay)
              Utils.Log(String.Format("[SystemHeatOverlay]: Destroying unusued overlay panel because it was deleted"));

            Destroy(overlayPanels[i].gameObject);
            overlayPanels.RemoveAt(i);
          }
        }
    }
    protected void onEditorPartPicked(Part part)
    {
      if (overlayPanels != null)
        for (int i = overlayPanels.Count - 1; i >= 0; i--)
        {
          if (overlayPanels[i].heatModule.part == part)
          {
            if (SystemHeatSettings.DebugOverlay)
              Utils.Log(String.Format("[SystemHeatOverlay]: Destroying unusued overlay panel because it was deleted"));

            Destroy(overlayPanels[i].gameObject);
            overlayPanels.RemoveAt(i);
          }
        }
    }
    public void ClearPanels()
    {
      if (SystemHeatSettings.DebugOverlay)
        Utils.Log("[SystemHeatOverlay]: Cleared all panels");

      for (int i = 0; i < overlayPanels.Count; i++)
      {
        if (overlayPanels[i] != null)
          Destroy(overlayPanels[i].gameObject);


      }
      overlayPanels.Clear();
    }

    public void ResetOverlay()
    {

      ClearPanels();
    }
    protected void DestroyOverlay()
    {
      foreach (OverlayLoop overLoop in overlayLoops)
      {
        overLoop.Destroy();
      }
      overlayLoops.Clear();
      //overlayLoopVisibility.Clear();
    }
    protected void LateUpdate()
    {

      if (simulator != null && !(HighLogic.LoadedSceneIsFlight && MapView.MapIsEnabled))
      {

        if (simulator.HeatLoops == null || simulator.HeatLoops.Count == 0)
        {
          Utils.Log(String.Format("[SystemHeatOverlay]: No loops, destroying overlay"));
          DestroyOverlay();
        }
        // Update each loop, build a new loop when needed
        foreach (HeatLoop loop in simulator.HeatLoops)
        {
          // if we have an overlay for this loop, update it
          OverlayLoop curOverlay = overlayLoops.FirstOrDefault(x => x.heatLoop.ID == loop.ID);
          if (curOverlay != null)
          {
            // if no modules, hide the loop
            if (loop.LoopModules.Count <= 1)
            {
              Utils.Log(String.Format("[SystemHeatOverlay]: Loop has < 2 members, hiding"));
              curOverlay.SetVisible(false);
            }
            else
            {
              
              curOverlay.SetVisible((SystemHeatUI.Instance.OverlayMasterState && SystemHeatUI.Instance.OverlayLoopState(loop.ID)));
              if (curOverlay.Drawn)
              {
                curOverlay.Update(loop);
              }
            }
          }
          // else build a new loop
          else
          {

            if (SystemHeatSettings.DebugOverlay)
              Utils.Log(String.Format("[SystemHeatOverlay]: Building a new overlay for loop {0}", loop.ID));
            overlayLoops.Add(new OverlayLoop(loop, overlayRoot, (SystemHeatUI.Instance.OverlayMasterState && SystemHeatUI.Instance.OverlayLoopState(loop.ID))));
          }

          foreach (ModuleSystemHeat system in loop.LoopModules)
          {
            int index = overlayPanels.FindIndex(f => f.heatModule == system);
            if (index == -1)
            {
              Utils.Log($"[SystemHeatOverlay]: Building new OverlayPanel for system {system.moduleID}");
              // new panel instance
              GameObject newUIPanel = (GameObject)Instantiate(SystemHeatUILoader.OverlayPanelPrefab, Vector3.zero, Quaternion.identity);
              newUIPanel.transform.SetParent(UIMasterController.Instance.actionCanvas.transform);
              newUIPanel.transform.localPosition = Vector3.zero;
              OverlayPanel panel = newUIPanel.AddComponent<OverlayPanel>();
              panel.parentCanvas = UIMasterController.Instance.appCanvas;
              panel.SetupLoop(loop, system, (SystemHeatUI.Instance.OverlayMasterState && SystemHeatUI.Instance.OverlayLoopState(loop.ID)));
              overlayPanels.Add(panel);
            }
            else
            {

              // Update the panel
              overlayPanels[index].UpdateLoop(loop, system, (SystemHeatUI.Instance.OverlayMasterState && SystemHeatUI.Instance.OverlayLoopState(loop.ID)));
            }
          }
        }


        for (int i = overlayPanels.Count - 1; i >= 0; i--)
        {
          if (overlayPanels[i].heatModule == null)
          {
            if (SystemHeatSettings.DebugOverlay)
              Utils.Log(String.Format("[SystemHeatOverlay]: Destroying unusued overlay panel"));

            Destroy(overlayPanels[i].gameObject);
            overlayPanels.RemoveAt(i);
          }
        }
        foreach (OverlayLoop l in overlayLoops)
        {
          if (!simulator.HasLoop(l.heatLoop.ID))
          {
            l.SetVisible(false);
          }
          
        }
      }
      else
      {
        DestroyOverlay();
        foreach (OverlayPanel panel in overlayPanels)
        {
          panel.SetVisibility(false);
        }

      }
    }
    public void AssignSimulator(SystemHeatSimulator sim)
    {
      simulator = sim;
    }
    public void SetVisible(bool visible)
    {
      Utils.Log(String.Format("[SystemHeatOverlay]: Visibility set to {0}", visible));

      SetLoopVisiblity(visible);
      SetPanelVisiblity(visible);

      //foreach (int id in overlayLoopVisibility.Keys.ToList())
      //{
      //  overlayLoopVisibility[id] = visible;
      //}
    }
    public void SetVisible(bool visible, int loopID)
    {
      Utils.Log(String.Format("[SystemHeatOverlay]: Visibility of loop {0} set to {1}", loopID, visible));

      SetLoopVisiblity(visible, loopID);
      SetPanelVisiblity(visible, loopID);


     // overlayLoopVisibility[loopID] = visible;
    }
    private void SetPanelVisiblity(bool visible)
    {

      for (int i = 0; i < overlayPanels.Count; i++)
      {
        if (overlayPanels[i] != null)
        {

          overlayPanels[i].SetVisibility(visible);
        }
      }
    }
    private void SetPanelVisiblity(bool visible, int loopID)
    {

      for (int i = 0; i < overlayPanels.Count; i++)
      {
        if (overlayPanels[i] != null && overlayPanels[i].loop.ID == loopID)
        {
          overlayPanels[i].SetVisibility(visible);
        }
      }
    }

    private void SetLoopVisiblity(bool visible)
    {
      if (simulator != null)
      {
        foreach (OverlayLoop loop in overlayLoops)
        {
          loop.SetVisible(visible);
        }
      }
      else
      {
        foreach (OverlayLoop loop in overlayLoops)
        {
          loop.SetVisible(false);
        }
      }
    }
    private void SetLoopVisiblity(bool visible, int loopID)
    {
      if (simulator != null)
      {
        foreach (OverlayLoop loop in overlayLoops)
        {
          if (loop.heatLoop.ID == loopID)
            loop.SetVisible(visible);
        }
      }
      else
      {

      }
    }
    public bool CheckLoopVisibility(int loopID)
    {
      foreach (OverlayLoop loop in overlayLoops)
      {
        if (loop.heatLoop.ID == loopID)
          return loop.Drawn;
      }
      return true;
    }
  }
}
