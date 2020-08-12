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

      headerButton.onClick.RemoveAllListeners();
      onToggle.onValueChanged.RemoveAllListeners();

      headerButton.onClick.AddListener(delegate { ToggleData(); });
      onToggle.onValueChanged.AddListener(delegate { ToggleReactor(); });

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
        AddDataWidget("heatGenerated", "Heat Generated");
        AddDataWidget("powerGenerated", "Power Generated");
        AddDataWidget("lifetime", "Core Life");
        AddDataWidget("temperature", "Core Temperature");
        bool isOn = false;
        bool.TryParse(module.Fields.GetValue("Enabled").ToString(), out isOn);
        onToggle.isOn = isOn;

      }
      if (m.moduleName == "ModuleSystemHeatFissionEngine")
      {
        iconRoot.gameObject.SetActive(true);
        iconRoot.FindDeepChild("FissionEngineIcon").gameObject.SetActive(true);
        AddDataWidget("heatGenerated", "Heat Generated");
        AddDataWidget("powerGenerated", "Power Generated");
        AddDataWidget("lifetime", "Core Life");
        AddDataWidget("temperature", "Core Temperature");
        bool isOn = false;
        bool.TryParse(module.Fields.GetValue("Enabled").ToString(), out isOn);
        onToggle.isOn = isOn;

      }
      if (m.moduleName == "ModuleFusionEngine")
      {
        iconRoot.gameObject.SetActive(true);
        iconRoot.FindDeepChild("FusionEngineIcon").gameObject.SetActive(true);
        AddDataWidget("heatGenerated", "Heat Generated");
        AddDataWidget("powerGenerated", "Power Generated");
        AddDataWidget("lifetime", "Core Life");
        AddDataWidget("temperature", "Core Temperature");
        bool isOn = false;
        bool.TryParse(module.Fields.GetValue("Enabled").ToString(), out isOn);
        onToggle.isOn = isOn;
      }
      if (m.moduleName == "FusionReactor")
      {
        iconRoot.gameObject.SetActive(true);
        iconRoot.FindDeepChild("FusionReactorIcon").gameObject.SetActive(true);
        AddDataWidget("heatGenerated", "Heat Generated");
        AddDataWidget("powerGenerated", "Power Generated");
        AddDataWidget("lifetime", "Core Life");
        AddDataWidget("temperature", "Core Temperature");
        bool isOn = false;
        bool.TryParse(module.Fields.GetValue("Enabled").ToString(), out isOn);
        onToggle.isOn = isOn;
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
    protected void FixedUpdate()
    {
      float nominalTemp = 0f;
      if (module.moduleName == "FusionReactor" || module.moduleName == "ModuleFusionEngine")
      {
        nominalTemp = float.Parse(module.Fields.GetValue("SystemOutletTemperature").ToString());
        datafields["heatGenerated"].SetValue(String.Format("{0:F0} kW", float.Parse(module.Fields.GetValue("SystemPower").ToString())));
        datafields["powerGenerated"].SetValue(String.Format("{0:F0} kW", float.Parse(module.Fields.GetValue("CurrentPowerProduced").ToString() )));
        datafields["lifetime"].SetValue(String.Format("{0}", module.Fields.GetValue("FuelInput")));
        datafields["temperature"].SetValue(String.Format("{0:F0} K", heatModule.LoopTemperature.ToString("F0")));


        if (heatModule.LoopTemperature > nominalTemp)
          datafields["temperature"].SetValue(String.Format("{0:F0} K!", heatModule.LoopTemperature.ToString("F0")), Color.red);
        else
          datafields["temperature"].SetValue(String.Format("{0:F0} K", heatModule.LoopTemperature.ToString("F0")), new Color(0.705f, 0.83f, 0.33f));


        bool isOn = false;
        bool.TryParse(module.Fields.GetValue("Enabled").ToString(), out isOn);
        onToggle.isOn = isOn;
      }
      if (module.moduleName == "ModuleSystemHeatFissionReactor" || module.moduleName == "ModuleSystemHeatFissionEngine")
      {
        nominalTemp = float.Parse(module.Fields.GetValue("NominalTemperature").ToString());
        datafields["heatGenerated"].SetValue(String.Format("{0:F0} kW", float.Parse(module.Fields.GetValue("CurrentHeatGeneration").ToString())));
        datafields["powerGenerated"].SetValue(String.Format("{0:F0} kW", float.Parse(module.Fields.GetValue("CurrentElectricalGeneration").ToString())));
        datafields["lifetime"].SetValue(String.Format("{0}", module.Fields.GetValue("FuelStatus")));

        if (heatModule.LoopTemperature > nominalTemp)
          datafields["temperature"].SetValue(String.Format("{0:F0} K!", heatModule.LoopTemperature.ToString("F0")), Color.red);
        else
          datafields["temperature"].SetValue(String.Format("{0:F0} K", heatModule.LoopTemperature.ToString("F0")), new Color(0.705f, 0.83f, 0.33f));

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
      Utils.Log($"{infoRoot.gameObject.activeSelf}");
      infoRoot.gameObject.SetActive(!infoRoot.gameObject.activeSelf);
    }
    public void SetVisible(bool state) { }
  }
}
