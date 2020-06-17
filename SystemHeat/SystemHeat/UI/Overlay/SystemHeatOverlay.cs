using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SystemHeat;
using KSP.Localization;
using KSP.UI;

namespace SystemHeat.UI
{
  /// <summary>
  /// The master controller for the system heat overlay.
  /// </summary>
  [KSPAddon(KSPAddon.Startup.FlightAndEditor, false)]
  public class SystemHeatOverlay: MonoBehaviour
  {

    public static SystemHeatOverlay Instance { get; private set; }
    public static GameObject UICanvas = null;
    public static bool Drawn { get; private set; }

    protected Transform overlayRoot;
    protected SystemHeatSimulator simulator;
    protected Dictionary<int, OverlayLoop> overlayLoops;
    protected List<OverlayPanel> overlayPanels;

    protected void Awake()
    {
      Drawn = false;
      Instance = this;
      overlayLoops = new Dictionary<int, OverlayLoop>();
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
      }
    }
    protected void Start()
    {
      
    }
    protected void onEnterMapView()
    {
      if (SystemHeatSettings.DebugOverlay)
        Utils.Log("[SystemHeatOverlay]: Entered map view, clearing panels");

      ClearPanels();
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
    protected void LateUpdate()
    {
      
      if (simulator != null && !(HighLogic.LoadedSceneIsFlight && MapView.MapIsEnabled))
      {
        if (simulator.HeatLoops == null || simulator.HeatLoops.Count == 0)
        {
          foreach (KeyValuePair<int,OverlayLoop> kvp in overlayLoops)
          {
            kvp.Value.Destroy();
          }
          overlayLoops.Clear();
        }
        // Update each loop, build a new loop when needed
        foreach (KeyValuePair<int, HeatLoop> kvp in simulator.HeatLoops)
        {
          if (kvp.Value.LoopModules.Count > 1)
          {
            // Check to see if the loops's panels are all built
            foreach(ModuleSystemHeat system in kvp.Value.LoopModules)
            {
              int index = overlayPanels.FindIndex(f => f.heatModule == system);
              if (index == -1)
              {
                Utils.Log($"[SystemHeatOverlay]: Building new OverlayPanel for system {system.moduleID}");
                // new panel instance
                GameObject newUIPanel = (GameObject)Instantiate(SystemHeatUILoader.OverlayPanelPrefab, Vector3.zero, Quaternion.identity);
                newUIPanel.transform.SetParent(UIMasterController.Instance.appCanvas.transform);
                newUIPanel.transform.localPosition = Vector3.zero;
                OverlayPanel panel = newUIPanel.AddComponent<OverlayPanel>();
                panel.parentCanvas = UIMasterController.Instance.appCanvas;
                panel.SetupLoop(kvp.Value, system, Drawn);
                overlayPanels.Add(panel);                  
              } else
              {
                // Update the panel
                overlayPanels[index].UpdateLoop(kvp.Value, system, Drawn);
              }
            }
              
            // Update the overlay
            if (overlayLoops.ContainsKey(kvp.Key))
            {
              overlayLoops[kvp.Key].Update(kvp.Value);
              if (Drawn && !overlayLoops[kvp.Key].Drawn)
                overlayLoops[kvp.Key].SetVisible(Drawn);
            }
            else
            {
              if (SystemHeatSettings.DebugOverlay)
                Utils.Log(String.Format("[SystemHeatOverlay]: Building a new overlay for loop {0}", kvp.Key));
              overlayLoops[kvp.Key] = new OverlayLoop(kvp.Value, overlayRoot, Drawn);
            }
          }
          else
          {
            if (overlayLoops.ContainsKey(kvp.Key) && overlayLoops[kvp.Key].Drawn)
            {
              Utils.Log(String.Format("[SystemHeatOverlay]: Loop has < 2 members, hiding"));
              overlayLoops[kvp.Key].SetVisible(false);

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
      }
      else
      {
        if (simulator != null)
        {
          foreach (KeyValuePair<int, OverlayLoop> kvp in overlayLoops)
          {
            kvp.Value.SetVisible(false);
          }
          foreach (OverlayPanel panel in overlayPanels)
          {
            panel.SetVisibility(false);
          }
        }
        else
        {
          foreach (KeyValuePair<int, OverlayLoop> kvp in overlayLoops)
          {
            kvp.Value.SetVisible(false);
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
      Utils.Log(String.Format("[SystemHeatOverlay]: Visibility set to {0}", visible));

      SetLoopVisiblity(visible);
      SetPanelVisiblity(visible);
      Drawn = visible;
    }

    private void SetPanelVisiblity(bool visible)
    {
      
      for (int i = 0; i< overlayPanels.Count;i++)
      {
        if (overlayPanels[i] != null)
        {
          
          overlayPanels[i].SetVisibility(visible);
        }
      }

    }

    private void SetLoopVisiblity(bool visible)
    {
      if (simulator != null)
      {
        foreach (KeyValuePair<int, OverlayLoop> kvp in overlayLoops)
        {
          kvp.Value.SetVisible(visible);
        }
      } 
      else
      {
        foreach (KeyValuePair<int, OverlayLoop> kvp in overlayLoops)
        {
          kvp.Value.SetVisible(false);
        }
      }
    }
  }
}
