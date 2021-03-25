using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using KSP.UI;
using KSP.Localization;

namespace SystemHeat.UI
{
  public class ToolbarPanel : MonoBehaviour
  {
    public bool OverlayMasterState
    {
      get { return overlayToggle.isOn; }
    }
    public bool OverlayLoopState(int id)
    {
      foreach (ToolbarPanelLoopWidget toolbar in loopPanelWidgets)
      {
        if (toolbar.trackedLoopID == id)
          return toolbar.overlayToggle.isOn;
      }
      return false;
    }

    public float SimSituationAltitude
    {
      get { return altitudeSlider.value * 1000f; }
    }
    public float SimSituationVelocity
    {
      get { return velocitySlider.value; }
    }

    public CelestialBody SimSituationBody
    {
      get { return currentBody; }
    }

    public bool active = true;
    public bool panelOpen = false;
    public RectTransform rect;
    public Toggle overlayToggle;
    public Toggle debugToggle;
    public Toggle loopToggle;
    public Text loopToggleTitle;
    public Slider simRateSlider;
    public Text simRateLabel;

    public GameObject situationPanel;
    public RectTransform situationData;

    public GameObject loopPanel;
    public GameObject loopPanelScrollRoot;
    public RectTransform loopPanelScrollRootRect;
    public RectTransform loopPanelScrollViewportRect;

    public GameObject simRateHeader;
    public GameObject simRateSliderObject;

    protected Text simRateTitle;
    protected Text overlayToggleTitle;
    protected Text craftStatsTitle;
    protected Text panelTitle;
    protected Text loopsTitle;
    protected Text settingsTitle;

    protected Text totalIncomingFluxTitle;
    protected Text totalOutgoingFluxTitle;
    protected Text totalLoopsTitle;

    protected Text totalIncomingFluxValue;
    protected Text totalOutgoingFluxValue;
    protected Text totalLoopsValue;

    protected Text noLoopsText;


    protected Text situationTitle;
    protected Text bodyTitle;
    protected Dropdown bodyDopdown;

    protected Button sitationButtonSeaLevel;
    protected Button sitationButtonAtmo;
    protected Button sitationButtonVac;
    protected Text altitudeTitle;
    protected Slider altitudeSlider;
    protected Text altitudeLabel;
    protected InputField altitudeTextArea;

    protected Text velocityTitle;
    protected Slider velocitySlider;
    protected Text velocityLabel;
    protected InputField velocityTextArea;
    protected GameObject situationDataObj;
    protected GameObject situationHeaderObj;

    protected int[] rates = new int[] { 1, 5, 10, 40, 100, 1000, 10000 };

    protected List<int> renderedLoops;
    protected List<ToolbarPanelLoopWidget> loopPanelWidgets;

    protected SystemHeatSimulator simulator;
    protected CelestialBody currentBody;

    public void Awake()
    {
      renderedLoops = new List<int>();
      loopPanelWidgets = new List<ToolbarPanelLoopWidget>();
      // Find all the components
      rect = this.GetComponent<RectTransform>();

      panelTitle = transform.FindDeepChild("PanelTitleText").GetComponent<Text>();

      // Craft Stats
      craftStatsTitle = transform.FindDeepChild("StatsHeaderText").GetComponent<Text>();
      totalIncomingFluxTitle = transform.FindDeepChild("HeatGenerationTitle").GetComponent<Text>();
      totalOutgoingFluxTitle = transform.FindDeepChild("HeatRejectionTitle").GetComponent<Text>();
      totalLoopsTitle = transform.FindDeepChild("LoopCountTitle").GetComponent<Text>();

      totalIncomingFluxValue = transform.FindDeepChild("HeatGenerationValue").GetComponent<Text>();
      totalOutgoingFluxValue = transform.FindDeepChild("HeatRejectionValue").GetComponent<Text>();
      totalLoopsValue = transform.FindDeepChild("LoopCountValue").GetComponent<Text>();

      // Situation
      situationHeaderObj = transform.FindDeepChild("SituationHeader").gameObject;
      situationDataObj = transform.FindDeepChild("SituationData").gameObject;
      situationData = situationDataObj.GetComponent<RectTransform>();
      situationTitle = transform.FindDeepChild("SituationHeaderText").GetComponent<Text>();
      bodyTitle = transform.FindDeepChild("BodyLabel").GetComponent<Text>();
      bodyDopdown = transform.FindDeepChild("BodyDropdown").GetComponent<Dropdown>();

      sitationButtonSeaLevel = transform.FindDeepChild("SeaLevelButton").GetComponent<Button>();
      sitationButtonAtmo = transform.FindDeepChild("AltButton").GetComponent<Button>();
      sitationButtonVac = transform.FindDeepChild("VacButton").GetComponent<Button>();
      altitudeTitle = transform.FindDeepChild("AltLabel").GetComponent<Text>();
      altitudeSlider = transform.FindDeepChild("AltSlider").GetComponent<Slider>();
      altitudeLabel = transform.FindDeepChild("AltUnits").GetComponent<Text>();
      altitudeTextArea = transform.FindDeepChild("AltInput").GetComponent<InputField>();

      velocityTitle = transform.FindDeepChild("VelLabel").GetComponent<Text>();
      velocitySlider = transform.FindDeepChild("VelSlider").GetComponent<Slider>();
      velocityLabel = transform.FindDeepChild("VelUnits").GetComponent<Text>();
      velocityTextArea = transform.FindDeepChild("VelInput").GetComponent<InputField>();


      // Settings

      settingsTitle = transform.FindDeepChild("SettingsHeaderText").GetComponent<Text>();

      loopToggle = transform.FindDeepChild("LoopToggle").GetComponent<Toggle>();
      overlayToggle = transform.FindDeepChild("OverlayToggle").GetComponent<Toggle>();
      simRateTitle = transform.FindDeepChild("SimRateTitle").GetChild(0).GetComponent<Text>();
      overlayToggleTitle = transform.FindDeepChild("OverlayLabel").GetComponent<Text>();
      loopToggleTitle = transform.FindDeepChild("LoopToggleLabel").GetComponent<Text>();

      simRateHeader = transform.FindDeepChild("SimRateTitle").gameObject;
      simRateSlider = transform.FindDeepChild("Slider").GetComponent<Slider>();
      simRateSliderObject = transform.FindDeepChild("SimRateSlider").gameObject;
      simRateLabel = transform.FindDeepChild("SimRateSlider").GetChild(1).GetComponent<Text>();

      // Loop Panel
      loopsTitle = transform.FindDeepChild("LoopsHeaderText").GetComponent<Text>();
      noLoopsText = transform.FindDeepChild("NoLoopText").GetComponent<Text>();
      loopPanel = transform.FindDeepChild("PanelColumn2").gameObject;
      loopPanelScrollRoot = transform.FindDeepChild("Scrolly").gameObject;
      loopPanelScrollRootRect = transform.FindDeepChild("Scrolly").GetComponent<RectTransform>();
      loopPanelScrollViewportRect = transform.FindDeepChild("ScrollViewPort").GetComponent<RectTransform>();



      loopPanel.SetActive(loopToggle.isOn);

      // Setup objects
      /// Situation is only visible in editor
      if (HighLogic.LoadedSceneIsEditor)
      {

        currentBody = FlightGlobals.GetHomeBody();
        bodyDopdown.AddOptions(FlightGlobals.Bodies.Select(x => x.name).ToList());
        for (int i = 0; i < bodyDopdown.options.Count; i++)
        {
          if (bodyDopdown.options[i].text == currentBody.name)
            bodyDopdown.SetValueWithoutNotify(i);
        }

        SetBody(currentBody);

        bodyDopdown.onValueChanged.AddListener(delegate { OnBodyDropdownChange(); });
        sitationButtonVac.onClick.AddListener(delegate { OnVacButtonClicked(); });
        sitationButtonSeaLevel.onClick.AddListener(delegate { OnSeaLevelButtonClicked(); });
        sitationButtonAtmo.onClick.AddListener(delegate { OnAltitudeButtonClicked(); });

        velocitySlider.onValueChanged.AddListener(delegate { OnVelSliderChange(); });
        velocityTextArea.onValueChanged.AddListener(delegate { OnVelInputChange(); });

        altitudeSlider.onValueChanged.AddListener(delegate { OnAltSliderChange(); });
        altitudeTextArea.onValueChanged.AddListener(delegate { OnAltInputChange(); });
      }
      else if (HighLogic.LoadedSceneIsFlight)
      {
        situationHeaderObj.SetActive(false);
        situationDataObj.SetActive(false);

      }



      /// Settings
      loopToggle.onValueChanged.AddListener(delegate { ToggleLoopPanel(); });
      overlayToggle.onValueChanged.AddListener(delegate { ToggleOverlay(); });

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
        float panelSize = 215;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelSize);
        loopPanelScrollViewportRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelSize - 45f);
        loopPanelScrollRootRect.GetComponent<LayoutElement>().minHeight = panelSize - 45f;
      }

      Localize();
    }
    void Localize()
    {

      totalIncomingFluxTitle.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_OutgoingFluxTitle");
      totalOutgoingFluxTitle.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_IncomingFluxTile");
      totalLoopsTitle.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_LoopCountTitle");

      loopsTitle.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_LoopsTitle");
      settingsTitle.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_SettingsTitle");
      craftStatsTitle.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_CraftStatsTitle");
      panelTitle.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_Title");

      simRateTitle.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_SimulationRateTitle");
      overlayToggleTitle.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_OverlayToggle");
    }

    void SetBody(CelestialBody b)
    {

      if (currentBody.atmosphere)
      {
        SetSituationAtmosphereControlState(true);
        altitudeSlider.maxValue = (float)b.atmosphereDepth / 1000f;
        altitudeSlider.minValue = 0;
        altitudeSlider.SetValueWithoutNotify((float)b.atmosphereDepth / 1000f);
        altitudeTextArea.SetTextWithoutNotify(altitudeSlider.value.ToString("F0"));

        velocitySlider.maxValue = b.GetObtVelocity().magnitude;
        velocitySlider.minValue = 0f;
        velocitySlider.SetValueWithoutNotify(0f);
        velocityTextArea.SetTextWithoutNotify("0");
      }
      else
      {
        SetSituationAtmosphereControlState(false);
      }
    }
    protected void SetSituationAtmosphereControlState(bool state)
    {
      float panelSize;

      if (state)
      {
        panelSize = 370f;
        situationData.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 140f);
      }
      else
      {
        panelSize = 260f;
        situationData.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 30f);
      }
      rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelSize);
      loopPanelScrollViewportRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelSize - 45f);
      loopPanelScrollRootRect.GetComponent<LayoutElement>().minHeight = panelSize - 45f;


      velocitySlider.gameObject.SetActive(state);
      velocityLabel.gameObject.SetActive(state);
      velocityTextArea.gameObject.SetActive(state);
      velocityTitle.gameObject.SetActive(state);
      altitudeSlider.gameObject.SetActive(state);
      altitudeLabel.gameObject.SetActive(state);
      altitudeTextArea.gameObject.SetActive(state);
      altitudeTitle.gameObject.SetActive(state);
      sitationButtonSeaLevel.gameObject.SetActive(state);
      sitationButtonVac.gameObject.SetActive(state);
      sitationButtonAtmo.gameObject.SetActive(state);

      sitationButtonSeaLevel.gameObject.SetActive(state);
      sitationButtonVac.gameObject.SetActive(state);
      //sitationButtonAtmo.gameObject.SetActive(state);
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

          //if (loopPanel.activeSelf)
          //{
          //  loopPanel.SetActive(false);
          //}
        }
        else
        {
          //if (!loopPanel.activeSelf)
          //{
          //  loopPanel.SetActive(true);
          //}

          PollLoopWidgets();
          if (noLoopsText.gameObject.activeSelf)
            noLoopsText.gameObject.SetActive(false);


        }
        totalLoopsValue.text = simulator.HeatLoops.Count.ToString();
        totalOutgoingFluxValue.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_OutgoingFluxValue", simulator.TotalHeatRejection.ToString("F0"));
        totalIncomingFluxValue.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_IncomingFluxValue", simulator.TotalHeatGeneration.ToString("F0"));
      }
    }
    void DestroyLoopWidgets()
    {
      for (int i = loopPanelWidgets.Count - 1; i >= 0; i--)
      {
        Destroy(loopPanelWidgets[i].gameObject);
      }
      loopPanelWidgets.Clear();
    }

    void PollLoopWidgets()
    {
      for (int i = loopPanelWidgets.Count - 1; i >= 0; i--)
      {
        if (!simulator.HasLoop(loopPanelWidgets[i].trackedLoopID))
        {
          Destroy(loopPanelWidgets[i].gameObject);
          loopPanelWidgets.RemoveAt(i);
        }
      }
      foreach (HeatLoop loop in simulator.HeatLoops)
      {
        bool generateWidget = true;
        for (int i = loopPanelWidgets.Count - 1; i >= 0; i--)
        {
          if (loopPanelWidgets[i].trackedLoopID == loop.ID)
            generateWidget = false;
        }

        if (generateWidget)
        {
          Utils.Log("[UI]: Generatoing a new loop widget", LogType.UI);
          GameObject newObj = (GameObject)Instantiate(SystemHeatUILoader.ToolbarPanelLoopPrefab, Vector3.zero, Quaternion.identity);
          newObj.transform.SetParent(loopPanelScrollRoot.transform);
          //newWidget.transform.localPosition = Vector3.zero;
          ToolbarPanelLoopWidget newWidget = newObj.AddComponent<ToolbarPanelLoopWidget>();
          newWidget.AssignSimulator(simulator);
          newWidget.SetLoop(loop.ID);
          newWidget.SetVisible(true);
          loopPanelWidgets.Add(newWidget);
        }
      }
    }

    public void OnBodyDropdownChange()
    {
      Utils.Log($"[ToolbarPanel]: Selected body {bodyDopdown.options[bodyDopdown.value].text}", LogType.UI);
      foreach (CelestialBody body in FlightGlobals.Bodies)
      {
        if (body.name == bodyDopdown.options[bodyDopdown.value].text)
        {
          currentBody = body;
          SetBody(currentBody);
        }
      }
    }
    public void OnVelSliderChange()
    {
      velocityTextArea.SetTextWithoutNotify(velocitySlider.value.ToString("F0"));
    }
    public void OnAltSliderChange()
    {
      altitudeTextArea.SetTextWithoutNotify(altitudeSlider.value.ToString("F0"));
    }
    public void OnVelInputChange()
    {
      velocitySlider.SetValueWithoutNotify(float.Parse(velocityTextArea.text));
    }

    public void OnAltInputChange()
    {
      altitudeSlider.SetValueWithoutNotify(float.Parse(altitudeTextArea.text));
    }

    public void OnSeaLevelButtonClicked()
    { altitudeSlider.value = 0f; }
    public void OnVacButtonClicked()
    { altitudeSlider.value = (float)currentBody.atmosphereDepth / 1000f; }
    public void OnAltitudeButtonClicked()
    { }
    public void OnSliderChange()
    {
      SystemHeatSettings.SimulationRateEditor = TimeWarp.fixedDeltaTime * rates[(int)simRateSlider.value];
      simRateLabel.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_SimulationRateValue", rates[(int)simRateSlider.value]);
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
    public void ToggleLoopPanel()
    {

      loopPanel.SetActive(!loopPanel.activeSelf);


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
