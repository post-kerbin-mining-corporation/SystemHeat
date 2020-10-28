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
    public Slider simRateSlider;
    public Text simRateLabel;
    public GameObject loopPanel;
    public GameObject loopPanelScrollRoot;

    public GameObject simRateHeader;
    public GameObject simRateSliderObject;

    protected Text totalIncomingFluxTitle;
    protected Text totalOutgoingFluxTitle;
    protected Text totalLoopsTitle;

    protected Text totalIncomingFluxValue;
    protected Text totalOutgoingFluxValue;
    protected Text totalLoopsValue;

    protected Text noLoopsText;

    protected int[] rates = new int[] { 1, 5, 10, 40, 100, 1000, 10000 };

    protected List<int> renderedLoops;
    protected List<ToolbarPanelLoopWidget> loopPanelWidgets;

    protected SystemHeatSimulator simulator;

    public void Awake()
    {
      renderedLoops = new List<int>();
      loopPanelWidgets = new List<ToolbarPanelLoopWidget>();
      // Find all the components
      rect = this.GetComponent<RectTransform>();
      
      totalIncomingFluxTitle = transform.FindDeepChild("HeatGenerationTitle").GetComponent<Text>();
      totalOutgoingFluxTitle = transform.FindDeepChild("HeatRejectionTitle").GetComponent<Text>();
      totalLoopsTitle = transform.FindDeepChild("LoopCountTitle").GetComponent<Text>();

      noLoopsText = transform.FindDeepChild("NoLoopText").GetComponent<Text>();
      loopPanel = transform.FindDeepChild("PanelColumn2").gameObject;
      loopPanelScrollRoot = transform.FindDeepChild("Scrolly").gameObject;

      totalIncomingFluxValue = transform.FindDeepChild("HeatGenerationValue").GetComponent<Text>();
      totalOutgoingFluxValue = transform.FindDeepChild("HeatRejectionValue").GetComponent<Text>();
      totalLoopsValue = transform.FindDeepChild("LoopCountValue").GetComponent<Text>();

      debugToggle = transform.FindDeepChild("DebugToggle").GetComponent<Toggle>();
      overlayToggle = transform.FindDeepChild("OverlayToggle").GetComponent<Toggle>();

      simRateHeader = transform.FindDeepChild("SimRateTitle").gameObject;
      simRateSlider = transform.FindDeepChild("Slider").GetComponent<Slider>();
      simRateSliderObject = transform.FindDeepChild("SimRateSlider").gameObject;
      simRateLabel = transform.FindDeepChild("SimRateLabel").GetComponent<Text>();

      debugToggle.onValueChanged.AddListener(delegate { ToggleDebug(); });
      overlayToggle.onValueChanged.AddListener(delegate { ToggleOverlay(); });

      debugToggle.gameObject.SetActive(false);
      if (HighLogic.LoadedSceneIsEditor)
      {
        simRateHeader.SetActive(true);
        simRateSliderObject.gameObject.SetActive(true);
        simRateLabel.text = "100x";
        simRateSlider.maxValue = 6;
        simRateSlider.value = 4;

        simRateSlider.onValueChanged.AddListener(delegate { OnSliderChange(); });
      }
      if (HighLogic.LoadedSceneIsFlight)
      {
        simRateHeader.SetActive(false);
     
        simRateSliderObject.gameObject.SetActive(false);
        
      }
    }

    protected void Update()
    {
      if (simulator != null)
      {
        // Turn on the no loops text if there are no loops
        if (simulator.HeatLoops.Count == 0)
        {
          if (loopPanelWidgets.Count > 0)
          {
            DestroyLoopWidgets();
          }

          if (!noLoopsText.gameObject.activeSelf)
            noLoopsText.gameObject.SetActive(true);

          if (loopPanel.activeSelf)
          {
            loopPanel.SetActive(false);
          }
        }
        else
        {
          if (!loopPanel.activeSelf)
          {
            loopPanel.SetActive(true);
          }

          PollLoopWidgets();
          if (noLoopsText.gameObject.activeSelf)
            noLoopsText.gameObject.SetActive(false);

          
        }
        totalLoopsValue.text = simulator.HeatLoops.Count.ToString();
        totalOutgoingFluxValue.text = String.Format("{0:F0} kW", simulator.TotalHeatRejection);
        totalIncomingFluxValue.text = String.Format("{0:F0} kW",simulator.TotalHeatGeneration);
      }
    }
    void DestroyLoopWidgets()
    {
      for (int i = loopPanelWidgets.Count-1; i >= 0; i--)
      {
        Destroy(loopPanelWidgets[i].gameObject);
      }
      loopPanelWidgets.Clear();
    }

    void PollLoopWidgets()
    {
      for (int i = loopPanelWidgets.Count-1; i >= 0; i--)
      {
        if (!simulator.HeatLoops.ContainsKey(loopPanelWidgets[i].trackedLoopID))
        {
          Destroy(loopPanelWidgets[i].gameObject);
          loopPanelWidgets.RemoveAt(i);
        }
      }
      foreach (KeyValuePair<int, HeatLoop> keyValuePair in simulator.HeatLoops)
      {
        bool generateWidget = true;
        for (int i = loopPanelWidgets.Count-1; i >= 0; i--)
        {
          if (loopPanelWidgets[i].trackedLoopID == keyValuePair.Key)
            generateWidget = false;
        }

        if (generateWidget)
        {
          GameObject newObj = (GameObject)Instantiate(SystemHeatUILoader.ToolbarPanelLoopPrefab, Vector3.zero, Quaternion.identity);
          newObj.transform.SetParent(loopPanelScrollRoot.transform);
          //newWidget.transform.localPosition = Vector3.zero;
          ToolbarPanelLoopWidget newWidget = newObj.AddComponent<ToolbarPanelLoopWidget>();
          newWidget.AssignSimulator(simulator);
          newWidget.SetLoop(keyValuePair.Value.ID);
          newWidget.SetVisible(true);
          loopPanelWidgets.Add(newWidget);
        }
      }
    }

    public void OnSliderChange()
    {
      SystemHeatSettings.SimulationRateEditor = TimeWarp.fixedDeltaTime * rates[(int)simRateSlider.value];
      simRateLabel.text = String.Format("{0}x", rates[(int)simRateSlider.value]);
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
      if (!overlayToggle.isOn)
        SystemHeatOverlay.Instance.SetVisible(false);
      else
      {
        foreach (ToolbarPanelLoopWidget widget in loopPanelWidgets)
        {
          SystemHeatOverlay.Instance.SetVisible(widget.overlayToggle.isOn, widget.trackedLoopID);
        }
      }
    }
  }
}
