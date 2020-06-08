using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;
using KSP.Localization;

namespace SystemHeat.UI
{


  [KSPAddon(KSPAddon.Startup.FlightAndEditor, false)]
  public class SystemHeatDebugUI : UIBaseWindow
  {

    #region GUI Variables
    private string windowTitle = "";
    #endregion

    #region GUI Widgets
    UIDebugLoopsView loopView;
    #endregion

    #region Vessel Data
    private Vessel activeVessel;
    private SystemHeatSimulator simulator;
    private int partCount = 0;
    #endregion

    public SystemHeatSimulator Simulator
    {
      get
      {
        return simulator;
      }
    }


    /// <summary>
    /// Find the simulator in flight or editor
    /// </summary>
    public void FindData()
    {
      if (HighLogic.LoadedSceneIsFlight)
      {
        activeVessel = FlightGlobals.ActiveVessel;
        SystemHeatVessel heatVessel = activeVessel.GetComponent<SystemHeatVessel>();
        if (heatVessel != null)
          simulator = heatVessel.Simulator;
        partCount = activeVessel.Parts.Count;
        if (SystemHeatSettings.DebugUI)
          Utils.Log("[Debug UI]: Located Flight data");
      }
      if (HighLogic.LoadedSceneIsEditor)
      {
        if (simulator == null)
        {
          simulator = SystemHeatEditor.Instance.Simulator;
          if (simulator != null && SystemHeatSettings.DebugUI)
            Utils.Log("[Debug UI]: Located Editor data");
        }
      }
    }

    public static void SetVisible(bool state)
    {
      showWindow = state;
    }

    /// <summary>
    /// Initialize the UI widgets, do localization, set up styles
    /// </summary>
    protected override void InitUI()
    {
      windowTitle = "SystemHeat Debug";
      loopView = new UIDebugLoopsView(this);

      base.InitUI();
    }


    protected override void Start()
    {
      base.Start();

      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
        FindData();
    }

    /// <summary>
    /// Draw the UI
    /// </summary>
    protected override void Draw()
    {
      // Fallback to try to get data if we don't have any
      if (HighLogic.LoadedSceneIsFlight)
      {
        if (FlightGlobals.ActiveVessel != null)
        {
          if (simulator == null)
            FindData();
        }
      }
      if (HighLogic.LoadedSceneIsEditor)
      {
        if (simulator == null)
          FindData();
      }
      base.Draw();
    }


    /// <summary>
    /// Draw the window
    /// </summary>
    /// <param name="windowId">window ID</param>
    protected override void DrawWindow(int windowId)
    {
      // Draw the header controls
      DrawHeaderArea();

      if (simulator != null)
      {
        GUILayout.Space(3f);
        loopView.Draw();
      }
      else
      {
        GUILayout.Label("No Vessel Located");
      }
      GUI.DragWindow();
    }


    /// <summary>
    /// Draw the header area
    /// </summary>
    private void DrawHeaderArea()
    {
      GUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
      GUILayout.Label(windowTitle, GUIResources.GetStyle("window_header"), GUILayout.MaxHeight(26f), GUILayout.MinHeight(26f), GUILayout.MinWidth(350f));

      GUILayout.FlexibleSpace();
      Rect buttonRect = GUILayoutUtility.GetRect(22f, 22f);
      GUI.color = resources.GetColor("cancel_color");
      if (GUI.Button(buttonRect, "", GUIResources.GetStyle("button_cancel")))
      {
        ToggleWindow();
      }

      //GUI.DrawTextureWithTexCoords(buttonRect, GUIResources.GetIcon("cancel").iconAtlas, GUIResources.GetIcon("cancel").iconRect);
      GUI.color = Color.white;
      GUILayout.EndHorizontal();
    }

    int ticker = 0;
    void Update()
    {
      // Perform updates of vars
      if (showWindow)
      {
        if (ticker >= SystemHeatSettings.UIUpdateInterval)
        {
          ticker = 0;
          loopView.Update();
        }
        ticker += 1;


        if (HighLogic.LoadedSceneIsFlight)
        {
          // Handle refresh when switching ships
          if (FlightGlobals.ActiveVessel != null)
          {
            if (activeVessel != null)
            {
              if (partCount != activeVessel.parts.Count || activeVessel != FlightGlobals.ActiveVessel)
              {
                FindData();
              }
            }
            else
            {
              FindData();
            }
          }
          if (activeVessel != null)
          {
            if (partCount != activeVessel.parts.Count || activeVessel != FlightGlobals.ActiveVessel)
            {
              FindData();
            }
          }
        }
      }
    }


   

  }


}
