using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DynamicBatteryStorage;
using KSP.Localization;

namespace DynamicBatteryStorage.UI
{
  public class UIElectricalView: UIView
  {

    public float SolarSimulationScalar
    {
      get { return solarSimulationScalar; }
      set { solarSimulationScalar = value; }
    }

    // Positive power flow flag
    bool charging = false;
    float solarSimulationScalar;
    UISolarPanelManager solarManager;
    UIDBSManager dbsManager;

    #region GUI Strings
    string batteryStatusHeader = "";
    string batteryChargeDepleted = "";
    string batteryChargeStable = "";
    string batteryChargeFullCharge = "";
    string batteryChargeCharging = "";

    string flowHeader = "";
    string netPowerFlux = "0";
    string availableBattery = "100";
    string chargeTime = "";

    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="uiHost">The instance of the main UI class to link to </param>
    public UIElectricalView(DynamicBatteryStorageUI uiHost):base(uiHost)
    {

      if (HighLogic.LoadedSceneIsEditor)
        solarManager = new UISolarPanelManager(uiHost, this);
      if (HighLogic.LoadedSceneIsFlight)
        dbsManager = new UIDBSManager(uiHost, this);

      foreach (KeyValuePair<string, List<ModuleDataHandler>> entry in producerCats)
      {
        // Currently always generated with Show = false
        producerCategoryUIItems.Add(entry.Key, new UIExpandableItem(entry.Key, entry.Value, dataHost, false, (col_width - 10f), powerFlowUnits));
      }
      foreach (KeyValuePair<string, List<ModuleDataHandler>> entry in consumerCats)
      {
        // Currently always generated with Show = false
        consumerCategoryUIItems.Add(entry.Key, new UIExpandableItem(entry.Key, entry.Value, dataHost, false, (col_width - 10f), powerFlowUnits));
      }

      if (Settings.DebugUIMode)
        Utils.Log("[UI]: [ElectricalView]: New instance created");
    }

    /// <summary>
    /// Triggers on creation, localizes relevant strings
    /// </summary>
    protected override void Localize()
    {
      base.Localize();
      flowHeader = Localizer.Format("#LOC_DynamicBatteryStorage_UI_FlowPanelTitle");
      totalConsumptionHeader = Localizer.Format("#LOC_DynamicBatteryStorage_UI_TotalPowerConsumptionTitle");
      totalProductionHeader = Localizer.Format("#LOC_DynamicBatteryStorage_UI_TotalPowerGenerationTitle");
      batteryStatusHeader = Localizer.Format("#LOC_DynamicBatteryStorage_UI_BatteryManagerTitle");
      batteryChargeDepleted = Localizer.Format("#LOC_DynamicBatteryStorage_UI_BatteryTimeDepletion");
      batteryChargeStable = Localizer.Format("#LOC_DynamicBatteryStorage_UI_BatteryTimeStable");
      batteryChargeCharging = Localizer.Format("#LOC_DynamicBatteryStorage_UI_BatteryTimeCharging");
      batteryChargeFullCharge = Localizer.Format("#LOC_DynamicBatteryStorage_UI_BatteryTimeFullCharge");
    }

    /// <summary>
    /// Draws the upper panel with flxues and battery states
    /// </summary>
    protected override void DrawUpperPanel()
    {
      GUILayout.BeginHorizontal(GUILayout.Height(180f));

      GUILayout.BeginVertical(GUILayout.MaxWidth(150f));
      GUILayout.FlexibleSpace();
      GUILayout.BeginVertical(UIHost.GUIResources.GetStyle("block_background"));
      GUILayout.Label(flowHeader, UIHost.GUIResources.GetStyle("panel_header_centered"));
      Rect flowRect = GUILayoutUtility.GetRect(80f, 48f);
      UIUtils.IconDataField(flowRect, UIHost.GUIResources.GetIcon("lightning"), netPowerFlux, UIHost.GUIResources.GetStyle("data_field_large"));
      GUILayout.EndVertical();
      GUILayout.FlexibleSpace();
      GUILayout.EndVertical();

      GUILayout.BeginVertical(GUILayout.MaxWidth(200f));
      GUILayout.FlexibleSpace();
      GUILayout.BeginVertical(UIHost.GUIResources.GetStyle("block_background"));
      GUILayout.Label(batteryStatusHeader, UIHost.GUIResources.GetStyle("panel_header_centered"));
      Rect batteryRect = GUILayoutUtility.GetRect(80f, 32f);
      UIUtils.IconDataField(batteryRect, UIHost.GUIResources.GetIcon("battery"), availableBattery, UIHost.GUIResources.GetStyle("data_field"));
      Rect chargeRect = GUILayoutUtility.GetRect(80f, 32f);
      UIUtils.IconDataField(chargeRect, UIHost.GUIResources.GetIcon("timer"), chargeTime, UIHost.GUIResources.GetStyle("data_field"));
      GUILayout.EndVertical();
      GUILayout.FlexibleSpace();

      GUILayout.EndVertical();

      if (HighLogic.LoadedSceneIsEditor)
        solarManager.Draw();
      if (HighLogic.LoadedSceneIsFlight)
        dbsManager.Draw();

      GUILayout.EndHorizontal();
    }

    /// <summary>
    /// Draws the Detail area with info about detailed production and consumption per module
    /// </summary>
    protected override void DrawDetailPanel()
    {
      if (showDetails)
        UIHost.windowPos.height = 310f + scrollHeight;
      else
        UIHost.windowPos.height = 310f;

      base.DrawDetailPanel();
    }

    /// <summary>
    /// Updates the data for drawing - strings and handler data caches
    /// </summary>
    public override void Update()
    {
      base.Update();
      if (dataHost.ElectricalData != null)
      {
        UpdateHeaderPanelData();
        UpdateDetailPanelData();
      }
      if (dataHost.ElectricalData != null && HighLogic.LoadedSceneIsEditor)
        solarManager.Update();

      if (dataHost.ElectricalData != null && HighLogic.LoadedSceneIsFlight)
        dbsManager.Update();
    }

    /// <summary>
    /// Updates the header string data
    /// </summary>
    protected override void UpdateHeaderPanelData()
    {
      double EC = 0d;
      double maxEC = 0d;
      double netPower = dataHost.ElectricalData.CurrentConsumption + dataHost.ElectricalData.GetSimulatedElectricalProdution();

      dataHost.ElectricalData.GetElectricalChargeLevels(out EC, out maxEC);

      if (netPower == 0d)
      {
        charging = false;
        netPowerFlux = String.Format("{0:F2} {1}", Math.Abs(netPower), powerFlowUnits);
        chargeTime = String.Format("{0}", batteryChargeStable);
      }
      else if (netPower > 0d)
      {

        charging = true;
        netPowerFlux = String.Format("▲ {0:F2} {1}", Math.Abs(netPower), powerFlowUnits);
        if (HighLogic.LoadedSceneIsFlight)
        {
          if ((maxEC - EC) < 0.001d)
            chargeTime = "0 s";
          else
            chargeTime = String.Format("{0} {1}", batteryChargeCharging, FormatUtils.FormatTimeString((maxEC - EC) / netPower));
        }
        else
        {
          chargeTime = String.Format("{0} {1}", batteryChargeFullCharge, FormatUtils.FormatTimeString(maxEC / netPower));
        }
      }
      else
      {
        charging = false;
        netPowerFlux = String.Format("<color=#fd6868> ▼ {0:F2} {1}</color>", Math.Abs(netPower), powerFlowUnits);
        if (EC < 0.1d)
          chargeTime = "0 s";
        else
          chargeTime = String.Format("{0} in {1}", batteryChargeDepleted, FormatUtils.FormatTimeString(EC / -netPower));
      }

      totalConsumption = String.Format("▼ {0:F2} {1}",
        Math.Abs(dataHost.ElectricalData.CurrentConsumption),
        powerFlowUnits);
      totalProduction = String.Format("▲ {0:F2} {1}",
        Math.Abs(dataHost.ElectricalData.GetSimulatedElectricalProdution()),
        powerFlowUnits);
      availableBattery = String.Format("{0:F0} / {1:F0} ({2:F1}%)", EC, maxEC, EC/maxEC * 100d);
    }


    /// <summary>
    /// Updates the detail panel data - this is mostly rebuilding the handler list
    /// </summary>
    protected override void UpdateDetailPanelData()
    {
      // If no cached list, rebuild it from scratch
      if (cachedHandlers == null)
        RebuildCachedList(dataHost.ElectricalData.AllHandlers);

      // If the list changed, rebuild it from components
      var firstNotSecond = dataHost.ElectricalData.AllHandlers.Except(cachedHandlers).ToList();
      var secondNotFirst = cachedHandlers.Except(dataHost.ElectricalData.AllHandlers).ToList();
      if ( firstNotSecond.Any() || secondNotFirst.Any())
      {
        if (Settings.DebugUIMode)
        {
          Utils.Log("[UI]: [ElectricalView]: Cached handler list does not appear to match the current handler list");
        }
        RebuildCachedList(dataHost.ElectricalData.AllHandlers);
      }
      else
      {
        // Just update if no changes
        for (int i = 0 ; i < categoryNames.Count() ; i++)
        {
          producerCategoryUIItems[categoryNames[i]].Update(solarSimulationScalar);
          consumerCategoryUIItems[categoryNames[i]].Update(solarSimulationScalar);
        }
      }
    }

  }
}
