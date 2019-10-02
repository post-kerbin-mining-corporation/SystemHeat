using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;

namespace SystemHeat
{
  [KSPAddon(KSPAddon.Startup.EveryScene, false)]
  public class SystemHeat : MonoBehaviour
  {
    public static SystemHeat Instance { get; private set; }

    protected void Awake()
    {
      Instance = this;
    }
    protected void Start()
    {
      SytemHeatSettings.Load();
    }
  }

  /// <summary>
  /// Static class to hold settings and configuration
  /// </summary>
  public static class SystemHeatSettings
  {
    // Emit UI debug messages
    public static bool DebugUI = true;
    // Emit Overlay debug messages
    public static bool DebugOverlay = true;
    // Emit simulation debug messages
    public static bool DebugSimulation = true;
    // Emit module  debug messages
    public static bool DebugModules = true;

    // Show debug info in part UIs
    public static bool DebugPartUI = true;

    public static float TimeWarpLimit = 100f;

    // Simulation settings
    // The maximum allows change in temperature per simulation time step
    public static float MaxDeltaTPerStep = 25f;
    // The minimum number of time steps to take in a simulation frame
    public static int MinSteps = 1;
    // The maximum number of time steps to take in a simulation frame
    public static int MaxSteps = 25;
    // The standard timestep in the editor
    public static float SimulationRateEditor = 1f;

    // Loop flux tolerance
    public static float AbsFluxThreshold = 0.5f;


    public static float OverlayBaseLineWidth;
    public static string OverlayLineShader;

    public static float OverlayBaseGlowWidth;
    public static string OverlayGlowShader;

    public static float OverlayPadding;
    public static float OverlayBoundsPadding;


    public static Dictionary<string, CoolantType> CoolantData;

    /// <summary>
    /// Load data from configuration
    /// </summary>
    public static void Load()
    {
      ConfigNode settingsNode;

      Utils.Log("[Settings]: Started loading");
      if (GameDatabase.Instance.ExistsConfigNode("SystemHeat/SYSTEMHEAT"))
      {
        Utils.Log("[Settings]: Located settings file");

        settingsNode = GameDatabase.Instance.GetConfigNode("SystemHeat/SYSTEMHEAT");

        settingsNode.TryGetValue("MinimumWarpFactor", ref TimeWarpLimit);
      }
      else
      {
        Utils.Log("[Settings]: Couldn't find settings file, using defaults");
      }


      Utils.Log("[Settings]: Loading coolant types ");
      ConfigNode[] coolantNodes = GameDatabase.Instance.GetConfigNodes("COOLANTTYPE");

      CoolantData = new Dictionary<string, CoolantType>();
      foreach (ConfigNode node in coolantNodes)
      {
        CoolantType newCoolant = new CoolantType(node);
        CoolantData.Add(newCoolant.Name, newCoolant);
      }
      Utils.Log("[Settings]: Loaded coolant types");

      Utils.Log("[Settings]: Finished loading");
    }

    public static GetCoolantType(string name)
    {
      CoolantData coolant;
      if (CoolantData.TryGetValue(name, out coolant))
        return coolant;
      else
      {
        Utils.LogWarning(String.Format("[Settings] {0} not found, using defaults", name));
        return new CoolantType();
      }
    }
  }

  /// <summary>
  /// Defines a type of coolant
  /// </summary>
  public class CoolantType
  {
    public string Name { get; set; }
    public string Title { get; set; }
    public float Density { get; set; }
    public float HeatCapacity { get; set; }

    public CoolantType(ConfigNode node)
    {
      Load(node);
    }
    public CoolantType()
    {
      Name = "undefined";
      Title = "undefined"
      Density = 1000f;
      HeatCapacity = 1000f;
    }

    public void Load(ConfigNode node)
    {
      Name = node.GetValue("name");
      Title = Localizer.Format(node.GetValue("title"));

      node.TryGetValue("density", ref Density);
      node.TryGetValue("heatCapacity", ref HeatCapacity);

      Utils.Log(String.Fomat("Loaded coolant {0}", this.ToString()));
    }

    public string ToString()
    {
      return String.Format("{0}: Density {1}, heat Capacity {2}", Name, Density, HeatCapacity);
    }
  }
}
