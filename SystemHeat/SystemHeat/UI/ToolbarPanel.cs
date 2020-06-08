using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using KSP.UI;

namespace SystemHeat.UI
{
  public class ToolbarPanel: MonoBehaviour
  {

     
    public bool active = true;
    public bool panelOpen = false;
    public RectTransform rect;
    public Toggle overlayToggle;
    public Toggle debugToggle;


    protected Text totalIncomingFluxTitle;
    protected Text totalOutgoingFluxTitle;
    protected Text totalLoopsTitle;

    protected Text totalIncomingFluxValue;
    protected Text totalOutgoingFluxValue;
    protected Text totalLoopsValue;


    protected SystemHeatSimulator simulator;

    public void Awake()
    {
      // Find all the components
      rect = this.GetComponent<RectTransform>();

      totalIncomingFluxTitle = transform.FindDeepChild("HeatGenerationTitle").GetComponent<Text>();
      totalOutgoingFluxTitle = transform.FindDeepChild("HeatRejectionTitle").GetComponent<Text>();
      totalLoopsTitle = transform.FindDeepChild("LoopCountTitle").GetComponent<Text>();

      totalIncomingFluxValue = transform.FindDeepChild("HeatGenerationValue").GetComponent<Text>();
      totalOutgoingFluxValue = transform.FindDeepChild("HeatRejectionValue").GetComponent<Text>();
      totalLoopsValue = transform.FindDeepChild("LoopCountValue").GetComponent<Text>();

      debugToggle = transform.FindDeepChild("DebugToggle").GetComponent<Toggle>();
      overlayToggle = transform.FindDeepChild("OverlayToggle").GetComponent<Toggle>();


      debugToggle.onValueChanged.AddListener(delegate { ToggleDebug(); });
      overlayToggle.onValueChanged.AddListener(delegate { ToggleOverlay(); });

    }

    protected void Update()
    {
      if (simulator != null)
      {
        totalLoopsValue.text = simulator.HeatLoops.Count.ToString();
        totalOutgoingFluxValue.text = String.Format("{0:F0} kW", simulator.TotalHeatRejection);
        totalIncomingFluxValue.text = String.Format("{0:F0} kW",simulator.TotalHeatGeneration);
      }
    }
    public void SetVisible(bool state)
    {
      active = state;
      rect.gameObject.SetActive(state);
    }
    public void AssignSimulator(SystemHeatSimulator sim)
    {
      simulator = sim;
    }
    public void ToggleDebug()
    {
      SystemHeatDebugUI.ToggleWindow();
    }
    public void ToggleOverlay()
    {
      SystemHeatOverlay.Instance.SetVisible(overlayToggle.isOn);
    }
  }
}
