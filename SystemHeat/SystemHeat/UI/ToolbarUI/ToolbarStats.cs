using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KSP.Localization;

namespace SystemHeat.UI
{
  public class ToolbarStats
  {
    protected Text craftStatsTitle;
    protected Text totalIncomingFluxTitle;
    protected Text totalOutgoingFluxTitle;
    protected Text totalLoopsTitle;

    protected Text totalIncomingFluxValue;
    protected Text totalOutgoingFluxValue;
    protected Text totalLoopsValue;

    protected RectTransform dataPanel;
    protected Button loopButton;
    protected Text loopButtonText;

    public void Initialize(Transform root, ToolbarLoops loopPanel)
    {

      craftStatsTitle = root.FindDeepChild("StatsHeaderText").GetComponent<Text>();

      dataPanel = root.FindDeepChild("StatsDataPanel").GetComponent<RectTransform>();
      
      totalIncomingFluxTitle = root.FindDeepChild("HeatGenerationTitle").GetComponent<Text>();
      totalOutgoingFluxTitle = root.FindDeepChild("HeatRejectionTitle").GetComponent<Text>();
      totalLoopsTitle = root.FindDeepChild("LoopCountTitle").GetComponent<Text>();

      totalIncomingFluxValue = root.FindDeepChild("HeatGenerationValue").GetComponent<Text>();
      totalOutgoingFluxValue = root.FindDeepChild("HeatRejectionValue").GetComponent<Text>();
      totalLoopsValue = root.FindDeepChild("LoopCountValue").GetComponent<Text>();

      loopButton = root.FindDeepChild("MoreButton").GetComponent<Button>();
      loopButtonText = root.FindDeepChild("MoreButtonText").GetComponent<Text>();
      
      loopButton.onClick.AddListener(delegate { loopPanel.ToggleLoopPanel(); });
      if (HighLogic.LoadedSceneIsEditor)
      {
        SetButtonDirection(true);
      }
      if (HighLogic.LoadedSceneIsFlight)
      {
        SetButtonDirection(false);
      }
    }

    protected void Localize()
    {
      craftStatsTitle.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_CraftStatsTitle");
      totalIncomingFluxTitle.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_OutgoingFluxTitle");
      totalOutgoingFluxTitle.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_IncomingFluxTile");
      totalLoopsTitle.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_LoopCountTitle");

    }

    protected void SetButtonDirection(bool direction)
    {
      if (direction)
      {
        loopButtonText.text = "▶";
        //dataPanel.offsetMax
      }
      else
      {
        loopButtonText.text = "◂";
      }
    }
    public void Update(SystemHeatSimulator simulator)
    {
      totalLoopsValue.text = simulator.HeatLoops.Count.ToString();
      totalOutgoingFluxValue.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_OutgoingFluxValue", Utils.ToSI(simulator.TotalHeatRejection, "F0"));
      totalIncomingFluxValue.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_IncomingFluxValue", Utils.ToSI(simulator.TotalHeatGeneration, "F0"));
    }

  }
}
