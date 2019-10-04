using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;
using KSP.Localization;

namespace SystemHeat.UI
{

  public class UIBaseWindow : MonoBehaviour
  {
    // Control Vars
    protected static bool showWindow = false;
    protected int windowID = new System.Random(2123).Next();
    public Rect windowPos = new Rect(200f, 200f, 700f, 400f);
    private Vector2 scrollPosition = Vector2.zero;
    private float scrollHeight = 0f;
    protected bool initUI = false;

    // Assets
    protected string toolbarUIIconURLOff = "SystemHeat/UI/toolbar_off";
    protected string toolbarUIIconURLOn = "SystemHeat/UI/toolbar_on";
    protected UIResources resources;

    // Stock toolbar button
    protected static ApplicationLauncherButton stockToolbarButton = null;

    public Rect WindowPosition { get {return windowPos; } set { windowPos = value; } }
    public UIResources GUIResources { get { return resources; } }

    /// <summary>
    /// Turn the window on or off
    /// </summary>
    public static void ToggleWindow()
    {
      if (SystemHeatSettings.DebugUI)
        Utils.Log("[UI]: Toggle Window");
      showWindow = !showWindow;
      SystemHeatOverlay.SetVisible(showWindow);
    }

    /// <summary>
    /// Initialize the UI comoponents, do localization, set up styles
    /// </summary>
    protected virtual void InitUI()
    {
      if (SystemHeatSettings.DebugUI)
        Utils.Log("[UI]: Initializing");

      resources = new UIResources();
      initUI = true;
    }

    protected virtual void Awake()
    {
      if (SystemHeatSettings.DebugUI)
        Utils.Log("[UI]: Awake fired");
      GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
      GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);
    }

    protected virtual void Start()
    {
      if (SystemHeatSettings.DebugUI)
        Utils.Log("[UI]: Start fired");

      if (ApplicationLauncher.Ready)
        OnGUIAppLauncherReady();
    }

    protected virtual void OnGUI()
    {
      if (Event.current.type == EventType.Repaint || Event.current.isMouse) {}
        Draw();
    }

    /// <summary>
    /// Draw the UI
    /// </summary>
    protected virtual void Draw()
    {
      if (!initUI)
        InitUI();

      if (showWindow)
      {
        GUI.skin = HighLogic.Skin;
        //windowPos.height = Mathf.Min(scrollHeight + 50f, 96f * 3f + 50f);
        windowPos = GUI.Window(windowID, windowPos, DrawWindow, new GUIContent(), GUIResources.GetStyle("window_main"));
      }
    }

    /// <summary>
    /// Draw the window
    /// </summary>
    /// <param name="windowId">window ID</param>
    protected virtual void DrawWindow(int windowId)
    {}

    // Stock toolbar handling methods
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
      ToggleWindow();
      stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture(showWindow ? toolbarUIIconURLOn : toolbarUIIconURLOff, false));
    }


    protected void OnGUIAppLauncherReady()
    {
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
    }

    protected void onAppLaunchToggleOff()
    {
      if (SystemHeatSettings.DebugUI)
        Utils.Log("[UI]: App Launcher Toggle Off");
      stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture(toolbarUIIconURLOff, false));
    }

    protected void DummyVoid() { }
  }


}
