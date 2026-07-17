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
    public List<OverlayLoop> overlayLoops;

    protected List<OverlayPanel> overlayPanels;

    protected void Awake()
    {
      Drawn = false;
      Instance = this;
      overlayLoops = new List<OverlayLoop>();
      //overlayLoopVisibility = new Dictionary<int, bool>();
      overlayPanels = new List<OverlayPanel>();
      overlayRoot = (new GameObject("SHOverlayRoot")).GetComponent<Transform>();


      Utils.Log("[SystemHeatOverlay]: Subscribing to events", LogType.Overlay);
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

      Utils.Log("[SystemHeatOverlay]: Unsubscribing to events", LogType.Overlay);
      GameEvents.onGameSceneLoadRequested.Remove(onSceneChange);
      GameEvents.onEditorScreenChange.Remove(onEditorScreenChange);
      GameEvents.onEditorPartDeleted.Remove(onEditorPartDeleted);
      GameEvents.onEditorPartPicked.Remove(onEditorPartPicked);
      GameEvents.onEditorRestart.Remove(onEditorReset);
      GameEvents.onEditorLoad.Remove(onEditorLoad);
      GameEvents.onEditorStarted.Remove(onEditorStart);
      GameEvents.OnMapEntered.Remove(onEnterMapView);
      GameEvents.OnMapExited.Remove(onExitMapView);
      ClearPanels();
      DestroyOverlay();
      Instance = null;

    }
    protected void Start()
    {

    }
    protected void onEnterMapView()
    {

      Utils.Log("[SystemHeatOverlay]: Entered map view, clearing panels", LogType.Overlay);

      VectorLine.SetCamera3D(PlanetariumCamera.Camera);
      ClearPanels();
    }
    protected void onExitMapView()
    {

      Utils.Log("[SystemHeatOverlay]: Entered map view, clearing panels", LogType.Overlay);

      VectorLine.SetCamera3D(FlightCamera.fetch.mainCamera);

    }

    protected void onEditorScreenChange(EditorScreen screen)
    {


      Utils.Log("[SystemHeatOverlay]: Editor Screen Changed, clearing panels", LogType.Overlay);
      ClearPanels();
    }
    protected void onEditorLoad(ShipConstruct ship, KSP.UI.Screens.CraftBrowserDialog.LoadType loadType)
    {


      Utils.Log("[SystemHeatOverlay]: Editor Load", LogType.Overlay);
      ClearPanels();
    }
    protected void onEditorReset()
    {


      Utils.Log("[SystemHeatOverlay]: Editor Reset", LogType.Overlay);
      ClearPanels();
    }

    protected void onEditorStart()
    {


      Utils.Log("[SystemHeatOverlay]: Editor Start", LogType.Overlay);
      ClearPanels();
    }

    protected void onSceneChange(GameScenes scene)
    {

      Utils.Log("[SystemHeatOverlay]: Changing Scenes, clearing panels", LogType.Overlay);
      SetVisible(false);
      ClearPanels();
    }

    protected void onEditorPartDeleted(Part part)
    {
      RemoveOverlayObjectsForPart(part);
    }
    protected void onEditorPartPicked(Part part)
    {
      RemoveOverlayObjectsForPart(part);
    }

    private void RemoveOverlayObjectsForPart(Part part)
    {
      if (overlayPanels != null)
      {
        for (int i = overlayPanels.Count - 1; i >= 0; i--)
        {
          OverlayPanel panel = overlayPanels[i];
          ModuleSystemHeat heatModule = panel != null ? panel.heatModule : null;
          if (panel == null || heatModule == null || heatModule.part == null || heatModule.part == part)
          {
            Utils.Log("[SystemHeatOverlay]: Destroying unused overlay panel", LogType.Overlay);
            if (panel != null)
              Destroy(panel.gameObject);
            overlayPanels.RemoveAt(i);
          }
        }
      }

      // Rebuild affected loop renderers on the next update. A loop can retain a
      // destroyed module for the remainder of the editor deletion event.
      if (overlayLoops != null)
      {
        for (int i = overlayLoops.Count - 1; i >= 0; i--)
        {
          OverlayLoop overlayLoop = overlayLoops[i];
          HeatLoop heatLoop = overlayLoop != null ? overlayLoop.heatLoop : null;
          bool remove = overlayLoop == null || heatLoop == null || heatLoop.LoopModules == null;

          if (!remove)
          {
            foreach (ModuleSystemHeat heatModule in heatLoop.LoopModules)
            {
              if (heatModule == null || heatModule.part == null || heatModule.part == part)
              {
                remove = true;
                break;
              }
            }
          }

          if (remove)
          {
            if (overlayLoop != null)
              overlayLoop.Destroy();
            overlayLoops.RemoveAt(i);
          }
        }
      }
    }
    public void ClearPanels()
    {

      Utils.Log("[SystemHeatOverlay]: Cleared all panels", LogType.Overlay);

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
      if (overlayLoops != null)
      {
        foreach (OverlayLoop overLoop in overlayLoops)
        {
          if (overLoop != null)
            overLoop.Destroy();
        }
        overlayLoops.Clear();
      }
    }
    protected void LateUpdate()
    {
      RemoveInvalidOverlayObjects();

      if (simulator != null && !(HighLogic.LoadedSceneIsFlight && MapView.MapIsEnabled))
      {
        if (simulator.HeatLoops == null || simulator.HeatLoops.Count == 0 && overlayLoops.Count > 0)
        {

          Utils.Log(String.Format("[SystemHeatOverlay]: No loops, destroying overlay"), LogType.Overlay);
          DestroyOverlay();
        }
        if (simulator.HeatLoops != null && Drawn)
        {
          // Update each loop, build a new loop when needed
          foreach (HeatLoop loop in simulator.HeatLoops)
          {
            if (!IsValidHeatLoop(loop))
              continue;

            // if we have an overlay for this loop, update it
            OverlayLoop curOverlay = overlayLoops.FirstOrDefault(x => x != null && x.heatLoop != null && x.heatLoop.ID == loop.ID);
            if (curOverlay != null)
            {
              // if no modules, hide the loop
              if (loop.LoopModules.Count <= 1 && curOverlay.Drawn)
              {
                Utils.Log(String.Format("[SystemHeatOverlay]: Loop has < 2 members, hiding"), LogType.Overlay);
                curOverlay.SetVisible(false);
              }
              else if (loop.LoopModules.Count > 1)
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


              Utils.Log(String.Format("[SystemHeatOverlay]: Building a new overlay for loop {0}", loop.ID), LogType.Overlay);
              overlayLoops.Add(new OverlayLoop(loop, overlayRoot, (SystemHeatUI.Instance.OverlayMasterState && SystemHeatUI.Instance.OverlayLoopState(loop.ID))));
            }

            foreach (ModuleSystemHeat system in loop.LoopModules)
            {
              if (system == null)
                continue;

              int index = overlayPanels.FindIndex(f => f != null && f.heatModule == system);

              if (index == -1)
              {
                Utils.Log($"[SystemHeatOverlay]: Building new OverlayPanel for system {system.moduleID}", LogType.Overlay);
                // new panel instance
                GameObject newUIPanel = (GameObject)Instantiate(SystemHeatAssets.OverlayPanelPrefab, Vector3.zero, Quaternion.identity);
                newUIPanel.transform.SetParent(UIMasterController.Instance.actionCanvas.transform);
                newUIPanel.transform.localPosition = Vector3.zero;
                OverlayPanel panel = newUIPanel.AddComponent<OverlayPanel>();
                panel.parentCanvas = UIMasterController.Instance.appCanvas;
                if (system.moduleUsed)
                {
                  panel.SetupLoop(loop, system, (SystemHeatUI.Instance.OverlayMasterState && SystemHeatUI.Instance.OverlayLoopState(loop.ID)));
                }
                else
                {
                  panel.SetupLoop(loop, system, (false && SystemHeatUI.Instance.OverlayLoopState(loop.ID)));
                }
                overlayPanels.Add(panel);

              }
              else
              {

                // Update the panel
                if (system.moduleUsed)
                {
                  overlayPanels[index].UpdateLoop(loop, system, (SystemHeatUI.Instance.OverlayMasterState && SystemHeatUI.Instance.OverlayLoopState(loop.ID)));
                }
                else
                {
                  overlayPanels[index].UpdateLoop(loop, system, (false && SystemHeatUI.Instance.OverlayLoopState(loop.ID)));
                }

              }
            }
          }


          for (int i = overlayLoops.Count - 1; i >= 0; i--)
          {
            OverlayLoop overlayLoop = overlayLoops[i];
            if (overlayLoop == null || overlayLoop.heatLoop == null || !simulator.HasLoop(overlayLoop.heatLoop.ID))
            {
              if (overlayLoop != null)
                overlayLoop.Destroy();
              overlayLoops.RemoveAt(i);
            }
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

    private bool IsValidHeatLoop(HeatLoop heatLoop)
    {
      if (heatLoop == null || heatLoop.LoopModules == null)
        return false;

      foreach (ModuleSystemHeat heatModule in heatLoop.LoopModules)
      {
        if (heatModule == null || heatModule.part == null)
          return false;
      }

      return true;
    }

    private void RemoveInvalidOverlayObjects()
    {
      if (overlayPanels != null)
      {
        for (int i = overlayPanels.Count - 1; i >= 0; i--)
        {
          OverlayPanel panel = overlayPanels[i];
          if (panel == null || panel.heatModule == null || panel.heatModule.part == null)
          {
            Utils.Log("[SystemHeatOverlay]: Destroying unused overlay panel", LogType.Overlay);
            if (panel != null)
              Destroy(panel.gameObject);
            overlayPanels.RemoveAt(i);
          }
        }
      }

      if (overlayLoops != null)
      {
        for (int i = overlayLoops.Count - 1; i >= 0; i--)
        {
          OverlayLoop overlayLoop = overlayLoops[i];
          if (overlayLoop == null || !IsValidHeatLoop(overlayLoop.heatLoop))
          {
            if (overlayLoop != null)
              overlayLoop.Destroy();
            overlayLoops.RemoveAt(i);
          }
        }
      }
    }
    public void AssignSimulator(SystemHeatSimulator sim)
    {
      simulator = sim;
    }
    public void SetVisible(bool visible)
    {
      Utils.Log(String.Format("[SystemHeatOverlay]: Visibility set to {0}", visible), LogType.Overlay);

      RemoveInvalidOverlayObjects();
      Drawn = visible;

      SetLoopVisiblity(visible);
      SetPanelVisiblity(visible);
    }
    public void SetVisible(bool visible, int loopID)
    {
      Utils.Log(String.Format("[SystemHeatOverlay]: Visibility of loop {0} set to {1}", loopID, visible), LogType.Overlay);

      RemoveInvalidOverlayObjects();
      Drawn = visible;

      SetLoopVisiblity(visible, loopID);
      SetPanelVisiblity(visible, loopID);

    }
    private void SetPanelVisiblity(bool visible)
    {

      for (int i = 0; i < overlayPanels.Count; i++)
      {
        OverlayPanel panel = overlayPanels[i];
        if (panel != null && panel.heatModule != null)
        {
          if (panel.heatModule.moduleUsed)
            panel.SetVisibility(visible);
          else
            panel.SetVisibility(false);
        }
      }
    }
    private void SetPanelVisiblity(bool visible, int loopID)
    {

      for (int i = 0; i < overlayPanels.Count; i++)
      {
        OverlayPanel panel = overlayPanels[i];
        if (panel == null || panel.loop == null || panel.heatModule == null) continue;
        if (panel.loop.ID == loopID)
        {
          if (panel.heatModule.moduleUsed)
            panel.SetVisibility(visible);
          else
            panel.SetVisibility(false);
        }
        else
        {
          Drawn = Drawn || panel.active;
        }
      }
    }

    private void SetLoopVisiblity(bool visible)
    {
      if (simulator != null)
      {
        foreach (OverlayLoop loop in overlayLoops)
        {
          if (loop != null && loop.heatLoop != null)
            loop.SetVisible(visible);
        }
      }
      else
      {
        foreach (OverlayLoop loop in overlayLoops)
        {
          if (loop != null && loop.heatLoop != null)
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
          if (loop == null || loop.heatLoop == null) continue;
          if (loop.heatLoop.ID == loopID)
            loop.SetVisible(visible);
          else
            Drawn = Drawn || loop.Drawn;
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
        if (loop == null || loop.heatLoop == null) continue;
        if (loop.heatLoop.ID == loopID)
          return loop.Drawn;
      }
      return true;
    }
  }
}
