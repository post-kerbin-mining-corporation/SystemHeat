using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;

namespace SystemHeat
{
  /// <summary>
  /// The connection between a stock ModuleResourceConverter and the SystemHeat system
  /// </summary>
  public class ModuleSystemHeatConverter: ModuleResourceConverter
  {
    
    // This should be unique on the part
    [KSPField(isPersistant = false)]
    public string moduleID = "converter";

    // This should correspond to the related ModuleSystemHeat
    [KSPField(isPersistant = false)]
    public string systemHeatModuleID ;

    // Map loop temperature to system efficiency (0-1.0)
    [KSPField(isPersistant = false)]
    public FloatCurve systemEfficiency = new FloatCurve();

    // Map system outlet temperature (K) to heat generation (kW)
    [KSPField(isPersistant = false)]
    public FloatCurve systemPower = new FloatCurve();


    // 
    [KSPField(isPersistant = false)]
    public float shutdownTemperature = 1000f;

    // The temperature the system contributes to loops
    [KSPField(isPersistant = false)]
    public float systemOutletTemperature = 1000f;

    // If on, shows in editor thermal sims
    [KSPField(isPersistant = true)]
    public bool editorThermalSim = false;

    [KSPEvent(guiActive = false, guiName = "Toggle", guiActiveEditor =true, active = true)]
    public void ToggleEditorThermalSim()
    {
      editorThermalSim = !editorThermalSim;
    }

    // Current efficiency GUI string
    [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Converter Efficiency")]
    public string ConverterEfficiency = "-1%";

    // base paramters
    private List<ResourceBaseRatio> inputs;
    private List<ResourceBaseRatio> outputs;
    protected ModuleSystemHeat heatModule;

    public void Start()
    {
      heatModule = ModuleUtils.FindNamedComponent<ModuleSystemHeat>(this.part, systemHeatModuleID);

      if (HighLogic.LoadedSceneIsFlight)
      {
        SetupResourceRatios();

      }
      else
      {

        SetupResourceRatios();

        //this.CurrentSafetyOverride = this.NominalTemperature;
      }
      if (SystemHeatSettings.DebugModules)
      {
        Utils.Log("[ModuleSystemHeatConverter] Setup completed");
      }

      Events["ToggleEditorThermalSim"].guiName = String.Format("Toggle Simulate {0}", base.ConverterName);
      Fields["ConverterEfficiency"].guiName = String.Format("{0} Converter Efficiency", base.ConverterName);
    }
    public override void FixedUpdate() {
      base.FixedUpdate();
      if (HighLogic.LoadedSceneIsFlight)
      {
        GenerateHeatFlight();
        UpdateSystemHeatFlight();
      }
      if (HighLogic.LoadedSceneIsEditor)
      {
        GenerateHeatEditor();
        
        Fields["ConverterEfficiency"].guiActiveEditor = editorThermalSim;
        
      }
      ConverterEfficiency = String.Format("{0}%", (systemEfficiency.Evaluate(heatModule.currentLoopTemperature)*100f).ToString("F1"));
    }
    
    protected void GenerateHeatEditor()
    {
      if (editorThermalSim)
        heatModule.AddFlux(moduleID, systemOutletTemperature, systemPower.Evaluate(systemOutletTemperature));
      else
        heatModule.AddFlux(moduleID, 0f, 0f);
    }

    protected void GenerateHeatFlight()
    {
      if (base.ModuleIsActive())
      {
        heatModule.AddFlux(moduleID, systemOutletTemperature, systemPower.Evaluate(systemOutletTemperature));
      }
      else
      {
        heatModule.AddFlux(moduleID, 0f, 0f);
      }
    }
    protected void UpdateSystemHeatFlight()
    {
      if (base.ModuleIsActive())
      {
        if (heatModule.currentLoopTemperature > shutdownTemperature)
        {
          ScreenMessages.PostScreenMessage(
            new ScreenMessage(
              String.Format("Converter overhead on {0}! Emergency shutdown!",
                                                             part.partInfo.title),
                                                             3.0f,
                                                             ScreenMessageStyle.UPPER_CENTER));
          ToggleResourceConverterAction(new KSPActionParam(0, KSPActionType.Activate));
          if (SystemHeatSettings.DebugModules)
          {
            Utils.Log("[ModuleSystemHeatConverter]: Overheated, shutdown fired");
          }
        }
        base._recipe = ModuleUtils.RecalculateRatios(systemEfficiency.Evaluate(heatModule.currentLoopTemperature), inputs, outputs, inputList, outputList, base._recipe);
      }
    }

    private void SetupResourceRatios()
    {

      inputs = new List<ResourceBaseRatio>();
      outputs = new List<ResourceBaseRatio>();

      for (int i = 0; i < inputList.Count; i++)
      {
        inputs.Add(new ResourceBaseRatio(inputList[i].ResourceName, inputList[i].Ratio));
      }
      for (int i = 0; i < outputList.Count; i++)
      {
        outputs.Add(new ResourceBaseRatio(outputList[i].ResourceName, outputList[i].Ratio));
      }
    }

  }



}
