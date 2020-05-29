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
  [KSPAddon(KSPAddon.Startup.EditorAny, false)]
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
    }
    protected void Start()
    {
      overlayRoot = (new GameObject("SHOverlayRoot")).GetComponent<Transform>();
      if (SystemHeatSettings.DebugOverlay)
        Utils.Log("[Overlay]: Start fired");
    }

    protected void FixedUpdate()
    {
      if (SystemHeatOverlay.Drawn)
      {
        if (simulator == null)
        {
          FindSimulator();
        }
        if (simulator != null)
        {
          // Update each loop, build a new loop when needed
          foreach (KeyValuePair<int, HeatLoop> kvp in simulator.HeatLoops)
          {
            if (kvp.Value.LoopModules.Count > 1)
            {
              foreach(ModuleSystemHeat system in kvp.Value.LoopModules)
              {
                int index = overlayPanels.FindIndex(f => f.heatModule == system);
                if (index == -1)
                {
                  Utils.Log($"[Overlay]: Building new OverlayPanel for system {system.moduleID}");
                  // new panel instance
                  GameObject newUIPanel = (GameObject)Instantiate(SystemHeatUILoader.OverlayPanelPrefab, Vector3.zero, Quaternion.identity);
                  newUIPanel.transform.SetParent(UIMasterController.Instance.appCanvas.transform);
                  newUIPanel.transform.localPosition = Vector3.zero;
                  OverlayPanel panel = newUIPanel.AddComponent<OverlayPanel>();
                  panel.parentCanvas = UIMasterController.Instance.appCanvas;
                  panel.SetupLoop(kvp.Value, system);
                  overlayPanels.Add(panel);                  
                } else
                {
                  // Update the panel
                  overlayPanels[index].SetupLoop(kvp.Value, system);
                }
              }
              

              if (overlayLoops.ContainsKey(kvp.Key))
              {
                overlayLoops[kvp.Key].Update(kvp.Value);
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
              if (overlayLoops.ContainsKey(kvp.Key))
              {
                overlayLoops[kvp.Key].SetVisible(false);
              }
            }
           
          }
        }
        for (int i = overlayPanels.Count - 1; i >= 0; i--)
        {
          if (overlayPanels[i].heatModule == null)
          {
            if (SystemHeatSettings.DebugOverlay)
              Utils.Log(String.Format("[SystemHeatOverlay]: Destroying unusued overlay panel"));

            Destroy(overlayPanels[i]);
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
      }
    }
    protected void FindSimulator()
    {
      if (SystemHeatSettings.DebugOverlay)
        Utils.Log(String.Format("[SystemHeatOverlay]: Finding Simulator"));
      if (HighLogic.LoadedSceneIsEditor)
      {
        simulator = SystemHeatEditor.Instance.Simulator;
      }
    }

    public static void SetVisible(bool visible)
    {
      Utils.Log(String.Format("[SystemHeatOverlay]: Visibility set to {0}", visible));
      Drawn = visible;
    }

  }
}
