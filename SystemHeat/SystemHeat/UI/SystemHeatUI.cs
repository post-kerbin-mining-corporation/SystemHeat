using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KSP.UI;
using KSP.UI.Screens;
using KSP.Localization;

namespace SystemHeat.UI
{
  [KSPAddon(KSPAddon.Startup.FlightAndEditor, false)]
  public class SystemHeatUI: MonoBehaviour
  {

    public static SystemHeatUI Instance { get; private set; }
    // Control Vars
    protected static bool showWindow = false;


    // Panel
    protected ToolbarPanel toolbarPanel;

    // Stock toolbar button
    protected string toolbarUIIconURLOff = "SystemHeat/UI/toolbar_off";
    protected string toolbarUIIconURLOn = "SystemHeat/UI/toolbar_on";
    protected static ApplicationLauncherButton stockToolbarButton = null;

    // Data
    protected SystemHeatSimulator simulator;
    protected Vessel thisVessel;

    protected virtual void Awake()
    {
      if (SystemHeatSettings.DebugUI)
        Utils.Log("[SystemHeatUI]: Initializing toolbar");

      GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
      GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);
      
      GameEvents.onGUIApplicationLauncherUnreadifying.Add(new EventData<GameScenes>.OnEvent(OnGUIAppLauncherUnreadifying));
      GameEvents.onVesselChange.Add(new EventData<Vessel>.OnEvent(OnVesselChanged));

      Instance = this;
    }

    public void Start()
    {
      if (ApplicationLauncher.Ready)
        OnGUIAppLauncherReady();
      
      
    }

    protected void CreateToolbarPanel()
    {
      if (SystemHeatSettings.DebugUI)
        Utils.Log("[SystemHeatUI]: Creating toolbar panel");
      GameObject newUIPanel = (GameObject)Instantiate(SystemHeatUILoader.ToolbarPanelPrefab, Vector3.zero, Quaternion.identity);
      newUIPanel.transform.SetParent(UIMasterController.Instance.appCanvas.transform);
      newUIPanel.transform.localPosition = Vector3.zero;
      toolbarPanel = newUIPanel.AddComponent<ToolbarPanel>();
      toolbarPanel.SetVisible(false);
    }
    protected void DestroyToolbarPanel()
    {
      if (SystemHeatSettings.DebugUI)
        Utils.Log("[SystemHeatUI]: Destroying toolbar panel");
      if (toolbarPanel != null)
      {
        Destroy(toolbarPanel.gameObject);
      }
    }

    public void ToggleAppLauncher()
    {
      showWindow = !showWindow;
      toolbarPanel.SetVisible(showWindow);
      if (showWindow)
      {
        SystemHeatOverlay.Instance.SetVisible(toolbarPanel.overlayToggle.isOn);
        SystemHeatDebugUI.SetVisible(toolbarPanel.debugToggle.isOn);
      } else
      {
        SystemHeatOverlay.Instance.SetVisible(false);
        SystemHeatDebugUI.SetVisible(false);
      }

    }

    protected void FindSimulator()
    {
      if (HighLogic.LoadedSceneIsEditor)
      {
        simulator = SystemHeatEditor.Instance.Simulator;
      }
      if (HighLogic.LoadedSceneIsFlight)
      {
        SystemHeatVessel heatVessel = FlightGlobals.ActiveVessel.GetComponent<SystemHeatVessel>();
        if (heatVessel != null)
          simulator = heatVessel.Simulator;
        if (SystemHeatSettings.DebugUI)
          Utils.Log($"[SystemHeatOverlay]: Located Flight data on vessel {FlightGlobals.ActiveVessel.vesselName}");
      }
    }
    protected void FindSimulator(Vessel v)
    {
      if (HighLogic.LoadedSceneIsEditor)
      {
        simulator = SystemHeatEditor.Instance.Simulator;
      }
      if (HighLogic.LoadedSceneIsFlight)
      {
        SystemHeatVessel heatVessel = v.GetComponent<SystemHeatVessel>();
        if (heatVessel != null)
          simulator = heatVessel.Simulator;
        if (SystemHeatSettings.DebugUI)
          Utils.Log($"[SystemHeatOverlay]: Located Flight data on vessel {v.vesselName}");
      }
    }
    void FixedUpdate()
    {
      if (simulator == null)
      {
        FindSimulator();
       
      }
      if (simulator != null)
      {
        SystemHeatOverlay.Instance.AssignSimulator(simulator);
        toolbarPanel.AssignSimulator(simulator);
      }
    }
    void Update()
    {
      if (showWindow)
      {

        if (HighLogic.LoadedSceneIsFlight)
        {
          /// TODO: Handle refresh of application launcher when switching ships
          if (FlightGlobals.ActiveVessel != null)
          {
            if (thisVessel == null)
            {
              thisVessel = FlightGlobals.ActiveVessel;
            }
            if (thisVessel != FlightGlobals.ActiveVessel)
            {
              thisVessel = FlightGlobals.ActiveVessel; 
            }
          }
          if (toolbarPanel.loopPanel.activeSelf)
            toolbarPanel.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 400f);
          toolbarPanel.rect.position = stockToolbarButton.GetAnchorUL() - new Vector3(toolbarPanel.rect.rect.width, toolbarPanel.rect.rect.height, 0f);
        }
        if (HighLogic.LoadedSceneIsEditor)
        {
          if (stockToolbarButton != null)
            toolbarPanel.rect.position = stockToolbarButton.GetAnchorUL();
          
        }
      }
    }
    public void OnVesselChanged(Vessel v)
    {
      FindSimulator(v);
      if (SystemHeatSettings.DebugUI)
        Utils.Log($"[UI]: OnVesselChanged Fired to {v.vesselName}");

      SystemHeatOverlay.Instance.ClearPanels();
      SystemHeatOverlay.Instance.AssignSimulator(simulator);

      if (toolbarPanel)
        toolbarPanel.AssignSimulator(simulator);
      
    }
    #region Stock Toolbar Methods
    public void OnDestroy()
    {
      if (SystemHeatSettings.DebugUI)
        Utils.Log("[UI]: OnDestroy Fired");
      // Remove the stock toolbar button
      GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
      if (stockToolbarButton != null)
      {
        ApplicationLauncher.Instance.RemoveModApplication(stockToolbarButton);
      }
    }

    protected void OnToolbarButtonToggle()
    {
      if (SystemHeatSettings.DebugUI)
        Utils.Log("[UI]: Toolbar Button Toggled");
      
      stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture(showWindow ? toolbarUIIconURLOn : toolbarUIIconURLOff, false));
      ToggleAppLauncher();
    }


    protected void OnGUIAppLauncherReady()
    {
      showWindow = false;
      if (SystemHeatSettings.DebugUI)
        Utils.Log("[UI]: App Launcher Ready");
      if (ApplicationLauncher.Ready && stockToolbarButton == null)
      {
        stockToolbarButton = ApplicationLauncher.Instance.AddModApplication(
            OnToolbarButtonToggle,
            OnToolbarButtonToggle,
            DummyVoid,
            DummyVoid,
            DummyVoid,
            DummyVoid,
            ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.FLIGHT,
            (Texture)GameDatabase.Instance.GetTexture(toolbarUIIconURLOff, false));
      }
      CreateToolbarPanel();
    }

    protected void OnGUIAppLauncherDestroyed()
    {
      if (SystemHeatSettings.DebugUI)
        Utils.Log("[UI]: App Launcher Destroyed");
      if (stockToolbarButton != null)
      {
        ApplicationLauncher.Instance.RemoveModApplication(stockToolbarButton);
        stockToolbarButton = null;
      }
      DestroyToolbarPanel();
    }


    protected void OnGUIAppLauncherUnreadifying(GameScenes scene)
    {
      
      if (SystemHeatSettings.DebugUI)
        Utils.Log("[UI]: App Launcher Unready");
      
      DestroyToolbarPanel();
    }

    protected void onAppLaunchToggleOff()
    {
      if (SystemHeatSettings.DebugUI)
        Utils.Log("[UI]: App Launcher Toggle Off");
      stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture(toolbarUIIconURLOff, false));
    }

    protected void DummyVoid() { }

    public void ResetAppLauncher()
    {
      if (SystemHeatSettings.DebugUI)
        Utils.Log("[UI]: Reset App Launcher");
      //FindData();
      if (stockToolbarButton == null)
      {
        stockToolbarButton = ApplicationLauncher.Instance.AddModApplication(
            OnToolbarButtonToggle,
            OnToolbarButtonToggle,
            DummyVoid,
            DummyVoid,
            DummyVoid,
            DummyVoid,
            ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.FLIGHT,
            (Texture)GameDatabase.Instance.GetTexture(toolbarUIIconURLOff, false));
      }

    }
    #endregion
  }
}
