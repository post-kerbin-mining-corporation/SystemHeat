using UnityEngine;
using UnityEngine.UI;
using KSP.Localization;

namespace SystemHeat.UI
{
  public class ToolbarSettings
  {
    protected GameObject settingsHeaderObj;
    protected GameObject settingsDataObj;
    protected Text settingsTitle;
    protected GameObject simRateHeader;
    protected GameObject simRateSliderObject;

    protected Text simRateTitle;

    protected Slider simRateSlider;
    protected Text simRateLabel;


    protected int[] rates = new int[] { 1, 5, 10, 40, 100, 1000, 10000 };

    public void Initialize(Transform root)
    {
      settingsHeaderObj = root.FindDeepChild("SettingsHeader").gameObject;
      settingsDataObj = root.FindDeepChild("SettingsData").gameObject;

      settingsTitle = root.FindDeepChild("SettingsHeaderText").GetComponent<Text>();
      simRateTitle = root.FindDeepChild("SimRateTitle").GetChild(0).GetComponent<Text>();

      simRateHeader = root.FindDeepChild("SimRateTitle").gameObject;
      simRateSlider = root.FindDeepChild("Slider").GetComponent<Slider>();
      simRateSliderObject = root.FindDeepChild("SimRateSlider").gameObject;
      simRateLabel = root.FindDeepChild("SimRateSlider").GetChild(1).GetComponent<Text>();

      Localize();

      if (HighLogic.LoadedSceneIsEditor)
      {
        SetVisible(true);
        simRateLabel.text = "100x";
        simRateSlider.maxValue = 6;
        simRateSlider.value = 4;

        simRateSlider.onValueChanged.AddListener(delegate { OnSliderChange(); });
      }
      else
      {
        SetVisible(false);
      }
    }
    protected void Localize()
    {

      settingsTitle.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_SettingsTitle");
      simRateTitle.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_SimulationRateTitle");
    }

    public void SetVisible(bool visibility)
    {
      settingsHeaderObj.SetActive(visibility);
      settingsDataObj.SetActive(visibility);
    }


    protected void OnSliderChange()
    {
      SystemHeatSettings.SimulationRateEditor = TimeWarp.fixedDeltaTime * rates[(int)simRateSlider.value];
      simRateLabel.text = Localizer.Format("#LOC_SystemHeat_ToolbarPanel_SimulationRateValue", rates[(int)simRateSlider.value]);
    }

  }
}
