using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SystemHeat;
using KSP.Localization;

namespace SystemHeat.UI
{
  /// <summary>
  /// The master controller for the system heat overlay.
  /// </summary>
  [KSPAddon(KSPAddon.Startup.EditorAny, false)]
  public class SystemHeatOverlay: MonoBehaviour
  {

    public static SystemHeatOverlay Instance { get; private set; }

    public static bool Drawn { get; private set; }

    protected Transform overlayRoot;
    protected SystemHeatSimulator simulator;
    protected Dictionary<int, OverlayLoop> overlayLoops;

    protected void Awake()
    {
      Drawn = false;
      Instance = this;
      overlayLoops = new Dictionary<int, OverlayLoop>();
    }
    protected void Start()
    {
      overlayRoot = (new GameObject("SHOverlayRoot")).GetComponent<Transform>();

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
            if (overlayLoops.ContainsKey(kvp.Key))
            {
              overlayLoops[kvp.Key].Update();
            }
            else
            {
              if (SystemHeatSettings.DebugOverlay)
                Utils.Log(String.Format("[SystemHeatOverlay]: Building a new overlay for loop {0}", kvp.Key));
              overlayLoops[kvp.Key] = new OverlayLoop(kvp.Value, overlayRoot, Drawn);
            }
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
