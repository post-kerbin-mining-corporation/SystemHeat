using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UnityEngine;
using KSP.Localization;

namespace SystemHeat.UI
{
  public class ToolbarPanelLoopWidget : MonoBehaviour
  {
    bool active = true;

    public RectTransform rect;
    public Toggle overlayToggle;
    public Text overlayToggleText;
    public Text temperatureTextHeader;
    public Text temperatureTextValue;
    public Text fluxTextHeader;
    public Text fluxTextValue;

    public Image swatch;

    public int trackedLoopID = -1;
    SystemHeatSimulator simulator;

    public void Awake()
    {
      FindComponents();
    }

    void FindComponents()
    {
      // Find all the components
      rect = this.GetComponent<RectTransform>();
      overlayToggle = transform.FindDeepChild("LoopToggle").GetComponent<Toggle>();
      swatch = transform.FindDeepChild("Swatch").GetComponent<Image>();
      overlayToggleText = transform.FindDeepChild("LoopToggleName").GetComponent<Text>();
      temperatureTextHeader = transform.FindDeepChild("TempText").GetComponent<Text>();
      temperatureTextValue = transform.FindDeepChild("TempDataText").GetComponent<Text>();
      fluxTextHeader = transform.FindDeepChild("FluxText").GetComponent<Text>();
      fluxTextValue = transform.FindDeepChild("FluxDataText").GetComponent<Text>();

      overlayToggle.onValueChanged.AddListener(delegate { ToggleOverlay(); });

      Localize();
    }
    void Localize()
    {

    }
    public void SetVisible(bool state)
    {
      active = state;
      rect.gameObject.SetActive(state);
    }

    public void SetLoop(int loopID)
    {
      if (rect == null) FindComponents();

      trackedLoopID = loopID;
      if (simulator != null)
      {
        overlayToggleText.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_LoopTitle", trackedLoopID.ToString());
        swatch.color = SystemHeatSettings.GetLoopColor(trackedLoopID);
        fluxTextHeader.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_LoopFluxTitle");
        temperatureTextHeader.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_LoopTemperatureTitle");
      }
    }
    public void AssignSimulator(SystemHeatSimulator sim)
    {
      simulator = sim;
    }

    void Update()
    {
      if (trackedLoopID != -1 && simulator != null && simulator.HeatLoops.ContainsKey(trackedLoopID))
      {

        temperatureTextValue.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_LoopTemperatureValue",
          simulator.HeatLoops[trackedLoopID].Temperature.ToString("F0"),
          simulator.HeatLoops[trackedLoopID].NominalTemperature.ToString("F0"));

        string prefix = "";
        if (simulator.HeatLoops[trackedLoopID].NetFlux == 0f)
          prefix = "";
        if (simulator.HeatLoops[trackedLoopID].NetFlux > 0f)
          prefix = "+";
        if (simulator.HeatLoops[trackedLoopID].NetFlux < 0f)
          prefix = "";

        fluxTextValue.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_LoopFluxValue", prefix,
        simulator.HeatLoops[trackedLoopID].NetFlux.ToString("F1"));

        if (simulator.HeatLoops[trackedLoopID].Temperature > simulator.HeatLoops[trackedLoopID].NominalTemperature)
        {
          Color32 c;
          HexColorField.HexToColor("fe8401", out c);
          temperatureTextValue.color = c;
        }
        else
        {
          Color32 c;
          HexColorField.HexToColor("B4D455", out c);
          temperatureTextValue.color = c;
        }

        if (simulator.HeatLoops[trackedLoopID].NetFlux > 0)
        {
          Color32 c;
          HexColorField.HexToColor("fe8401", out c);
          fluxTextValue.color = c;
        }
        else
        {
          Color32 c;
          HexColorField.HexToColor("B4D455", out c);
          fluxTextValue.color = c;
        }
      }
    }
    public void ToggleOverlay()
    {
      SystemHeatOverlay.Instance.SetVisible(overlayToggle.isOn, trackedLoopID);
    }
  }
}
