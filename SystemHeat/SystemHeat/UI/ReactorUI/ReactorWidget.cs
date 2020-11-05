using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using KSP.UI;
using KSP.Localization;
using System.Reflection;

namespace SystemHeat.UI
{
  public class ReactorWidget: MonoBehaviour
  {
    public RectTransform rect;
    public Text reactorName;
    public Toggle onToggle;
    public Text onToggleLabel;
    public Toggle chargeToggle;

    public GameObject spacer;

    public GameObject chargeElement;
    public Slider chargeSlider;
    public Image sliderFill;

    public Image warningIcon;
    public RectTransform iconRoot;
    public RectTransform dataRoot;
    public RectTransform infoRoot;
    public Button headerButton;
    public Dictionary<string, ReactorDataField> datafields;
    protected PartModule module;
    protected ModuleSystemHeat heatModule;

    public void Awake()
    {
      FindComponents();
    }
    void FindComponents()
    {
      rect = this.transform as RectTransform;

      reactorName = transform.FindDeepChild("ReactorName").GetComponent<Text>();
      onToggle = transform.FindDeepChild("OnToggle").GetComponent<Toggle>();
      headerButton  = transform.FindDeepChild("Header").GetComponent<Button>();
      warningIcon = transform.FindDeepChild("WarningIcon").GetComponent<Image>();
      iconRoot = transform.FindDeepChild("Icons") as RectTransform;
      dataRoot = transform.FindDeepChild("DataPanel") as RectTransform;
      infoRoot = transform.FindDeepChild("InfoPanel") as RectTransform;
      onToggleLabel = transform.FindDeepChild("OnLabel").GetComponent<Text>();

      spacer = transform.FindDeepChild("Space").gameObject;
      chargeElement = transform.FindDeepChild("ChargeArea").gameObject;
      chargeToggle = transform.FindDeepChild("ChargeArea").GetComponent<Toggle>();
      chargeSlider = transform.FindDeepChild("Slider").GetComponent<Slider>();
      sliderFill = transform.FindDeepChild("Fill").GetComponent<Image>();

      headerButton.onClick.RemoveAllListeners();
      onToggle.onValueChanged.RemoveAllListeners();
      chargeToggle.onValueChanged.RemoveAllListeners();

      headerButton.onClick.AddListener(delegate { ToggleData(); });
      onToggle.onValueChanged.AddListener(delegate { ToggleReactor(); });
      chargeToggle.onValueChanged.AddListener(delegate { ToggleCharge(); });

      Localize();
    }

    void Localize()
    {
      onToggleLabel.text = Localizer.Format("#LOC_SystemHeat_ReactorPanel_ReactorOnToggleLabel");
    }
    public void SetReactor(PartModule m)
    {
      if (rect == null)
        FindComponents();

      if (SystemHeatSettings.DebugUI)
        Utils.Log($"[ReactorWidget]: Setting up widget for PM {m}");
      module = m;
      reactorName.text = m.part.partInfo.title;
      datafields = new Dictionary<string, ReactorDataField>();
      // Set the data depending on the reactor type
      iconRoot.gameObject.SetActive(false);
      heatModule = module.GetComponent<ModuleSystemHeat>();
      if (SystemHeatSettings.DebugUI)
        Utils.Log($"[ReactorWidget]: Setting up specifc properties for for PM {m}");
      if (m.moduleName == "ModuleSystemHeatFissionReactor")
      {
        iconRoot.gameObject.SetActive(true);
        iconRoot.FindDeepChild("FissionReactorIcon").gameObject.SetActive(true);
        AddDataWidget("heatGenerated", Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_HeatGeneratedTitle"));
        AddDataWidget("powerGenerated", Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_PowerGeneratedTitle"));
        AddDataWidget("lifetime", Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_CoreLifeTitle"));
        AddDataWidget("temperature", Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_CoreTemperatureTitle"));
        bool isOn = false;
        bool.TryParse(module.Fields.GetValue("Enabled").ToString(), out isOn);
        onToggle.isOn = isOn;


        spacer.SetActive(false);
        chargeElement.SetActive(false);

      }
      if (m.moduleName == "ModuleSystemHeatFissionEngine")
      {
        iconRoot.gameObject.SetActive(true);
        iconRoot.FindDeepChild("FissionEngineIcon").gameObject.SetActive(true);
        AddDataWidget("heatGenerated", Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_HeatGeneratedTitle"));
        AddDataWidget("powerGenerated", Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_PowerGeneratedTitle"));
        AddDataWidget("lifetime", Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_CoreLifeTitle"));
        AddDataWidget("temperature", Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_CoreTemperatureTitle"));
        bool isOn = false;
        bool.TryParse(module.Fields.GetValue("Enabled").ToString(), out isOn);
        onToggle.isOn = isOn;

        spacer.SetActive(false);
        chargeElement.SetActive(false);

      }
      if (m.moduleName == "ModuleFusionEngine")
      {
        iconRoot.gameObject.SetActive(true);
        iconRoot.FindDeepChild("FusionEngineIcon").gameObject.SetActive(true);
        AddDataWidget("heatGenerated", Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_HeatGeneratedTitle"));
        AddDataWidget("powerGenerated", Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_PowerGeneratedTitle"));
        AddDataWidget("lifetime", Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_CoreLifeTitle"));
        AddDataWidget("temperature", Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_CoreTemperatureTitle"));
        bool isOn = false;
        
        bool.TryParse(module.Fields.GetValue("Enabled").ToString(), out isOn);
        onToggle.isOn = isOn;

        bool chargeOn = false;
        bool.TryParse(module.Fields.GetValue("Charging").ToString(), out chargeOn);

        spacer.SetActive(true);
        chargeElement.SetActive(true);
        if (chargeOn)
        {
          chargeToggle.isOn = chargeOn;
        }
      }
      if (m.moduleName == "FusionReactor")
      {
        iconRoot.gameObject.SetActive(true);
        iconRoot.FindDeepChild("FusionReactorIcon").gameObject.SetActive(true);
        AddDataWidget("heatGenerated", Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_HeatGeneratedTitle"));
        AddDataWidget("powerGenerated", Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_PowerGeneratedTitle"));
        AddDataWidget("lifetime", Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_CoreLifeTitle"));
        AddDataWidget("temperature", Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_CoreTemperatureTitle"));
        Utils.Log($"x");
        Utils.Log($"{spacer}");
        bool.TryParse(module.Fields.GetValue("Enabled").ToString(), out bool isOn);
        onToggle.isOn = isOn;

        bool.TryParse(module.Fields.GetValue("Charging").ToString(), out bool chargeOn);
        Utils.Log($"{chargeElement}");
        Utils.Log($"{chargeToggle}");
        spacer.SetActive(true);
        chargeElement.SetActive(true);
        if (chargeOn)
        {
          chargeToggle.isOn = chargeOn;
        }
      }
    }

    protected void AddDataWidget(string name, string displayName)
    {
      GameObject newWidget = (GameObject)Instantiate(SystemHeatUILoader.ReactorDataFieldPrefab, Vector3.zero, Quaternion.identity);
      newWidget.transform.SetParent(dataRoot);
      newWidget.transform.localPosition = Vector3.zero;
      ReactorDataField field = newWidget.AddComponent<ReactorDataField>();
      field.Initialize(displayName);
      datafields.Add(name, field);
    }
    public void ToggleReactor()
    {
      if (onToggle.isOn)
      {
        if (module.moduleName == "FusionReactor" || module.moduleName == "ModuleFusionEngine")
        {
          module.Invoke("EnableReactor", 0f);
        }
        if (module.moduleName == "ModuleSystemHeatFissionReactor" || module.moduleName == "ModuleSystemHeatFissionEngine")
        {
          module.Invoke("EnableReactor", 0f);
        }
      }
      else
      {
        if (module.moduleName == "FusionReactor" || module.moduleName == "ModuleFusionEngine")
        {
          module.Invoke("DisableReactor", 0f);
        }
        if (module.moduleName == "ModuleSystemHeatFissionReactor" || module.moduleName == "ModuleSystemHeatFissionEngine")
        {
          module.Invoke("DisableReactor", 0f);
        }
      }
      
    }

    public void ToggleCharge()
    {
      if (chargeToggle.isOn)
      {
        if (module.moduleName == "FusionReactor" || module.moduleName == "ModuleFusionEngine")
        {
          module.Invoke("EnableCharging", 0f);
        }
      }
      else
      {
        if (module.moduleName == "FusionReactor" || module.moduleName == "ModuleFusionEngine")
        {
          module.Invoke("DisableCharging", 0f);
        }
      }

    }
    protected void FixedUpdate()
    {
      float nominalTemp = 0f;
      if (module.moduleName == "FusionReactor" || module.moduleName == "ModuleFusionEngine")
      {
        nominalTemp = float.Parse(module.Fields.GetValue("SystemOutletTemperature").ToString());
        datafields["heatGenerated"].SetValue(Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_HeatGenerated", float.Parse(module.Fields.GetValue("SystemPower").ToString()).ToString("F0") ) );
        datafields["powerGenerated"].SetValue(Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_PowerGenerated", float.Parse(module.Fields.GetValue("CurrentPowerProduced").ToString() ).ToString("F0")));
        datafields["lifetime"].SetValue(Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_CoreLife", module.Fields.GetValue("FuelInput")));
        datafields["temperature"].SetValue(Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_CoreTemperature", heatModule.LoopTemperature.ToString("F0")));


        if (heatModule.LoopTemperature > nominalTemp)
          datafields["temperature"].SetValue(Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_CoreTemperatureAlert", heatModule.LoopTemperature.ToString("F0")), Color.red);
        else
          datafields["temperature"].SetValue(Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_CoreTemperature", heatModule.LoopTemperature.ToString("F0")), new Color(0.705f, 0.83f, 0.33f));


        bool isOn = false;
        bool.TryParse(module.Fields.GetValue("Enabled").ToString(), out isOn);
        if (isOn != onToggle.isOn)
          onToggle.SetIsOnWithoutNotify(isOn);

        //onToggle.isOn = isOn;


        bool isCharge = false;
        bool.TryParse(module.Fields.GetValue("Charging").ToString(), out isCharge);
        chargeToggle.SetIsOnWithoutNotify(isCharge);
        //chargeToggle.isOn = isCharge;
        float chargeVal = (float)module.Fields.GetValue("CurrentCharge") / (float)module.Fields.GetValue("ChargeGoal");
        chargeSlider.value = chargeVal;
        Color32 fillColor;
        if (chargeVal < 1.0f)
           HexColorField.HexToColor("#F67A28", out fillColor);
        else
          HexColorField.HexToColor("#B4D455", out fillColor);

        sliderFill.color = fillColor;

      }
      if (module.moduleName == "ModuleSystemHeatFissionReactor" || module.moduleName == "ModuleSystemHeatFissionEngine")
      {
        nominalTemp = float.Parse(module.Fields.GetValue("NominalTemperature").ToString());
        datafields["heatGenerated"].SetValue(Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_HeatGenerated", float.Parse(module.Fields.GetValue("CurrentHeatGeneration").ToString()).ToString("F0") ) );
        datafields["powerGenerated"].SetValue(Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_PowerGenerated", float.Parse(module.Fields.GetValue("CurrentElectricalGeneration").ToString()).ToString("F0") ));
        datafields["lifetime"].SetValue(Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_CoreLife", module.Fields.GetValue("FuelStatus")));

        if (heatModule.LoopTemperature > nominalTemp)
          datafields["temperature"].SetValue(Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_CoreTemperatureAlert", heatModule.LoopTemperature.ToString("F0")), Color.red);
        else
          datafields["temperature"].SetValue(Localizer.Format("#LOC_SystemHeat_ReactorPanel_Field_CoreTemperature", heatModule.LoopTemperature.ToString("F0")), new Color(0.705f, 0.83f, 0.33f));

        bool isOn = false;
        bool.TryParse(module.Fields.GetValue("Enabled").ToString(), out isOn);
        onToggle.isOn = isOn;
      }


      if (heatModule.LoopTemperature > nominalTemp)
      {
        warningIcon.enabled = true;
      } else
      {
        warningIcon.enabled = false;
      }
    }
    public void ToggleData()
    {
      
      infoRoot.gameObject.SetActive(!infoRoot.gameObject.activeSelf);
    }
    public void SetVisible(bool state) { }
  }
}
