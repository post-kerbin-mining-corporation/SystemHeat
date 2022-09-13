using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemHeat
{

  public class SystemHeatGameSettings_ReactorDamage : GameParameters.CustomParameterNode
  {

    [GameParameters.CustomParameterUI("Allow Reactor Damage",
      toolTip = "If enabled, overheated reactors will take damage",
      autoPersistance = true)]
    public bool reactorDamage = true;

    [GameParameters.CustomParameterUI("Allow Reactor Repairs",
      toolTip = "If enabled, damaged fission reactors can be repaired",
      autoPersistance = true)]
    public bool reactorRepairs = true;

    [GameParameters.CustomIntParameterUI("Engineer Level Required",
      maxValue = 5, minValue = 1, stepSize = 1,
      toolTip = "Engineer level needed to repair a reactor",
      autoPersistance = true)]
    public int engineerLevel = 5;


    [GameParameters.CustomFloatParameterUI("Amount Repaired per Repair Kit",
      maxValue = 100f, minValue = 5f, asPercentage = true, stepCount = 20,
      toolTip = "The amount of repairs a single kit makes", autoPersistance = true)]
    public float repairPerKit = 25f;

    [GameParameters.CustomFloatParameterUI("Minimum Repairable level",
      maxValue = 100f, minValue = 5f, asPercentage = true, stepCount = 20,
      toolTip = "If a reactor is below this level it can't be repaired", autoPersistance = true)]
    public float repairThreshold = 10f;


    [GameParameters.CustomFloatParameterUI("Maximum Allowed Repair Amount",
      maxValue = 100f, minValue = 5f, asPercentage = true, stepCount = 20,
      toolTip = "The maximum amount you are allowed to repair a reactor", autoPersistance = true)]
    public float repairMax = 75f;

    public override string DisplaySection
    {
      get
      {
        return Section;
      }
    }

    public override string Section
    {
      get
      {
        return "System Heat";
      }
    }

    public override string Title
    {
      get
      {
        return "Fission Reactor Damage";
      }
    }

    public override int SectionOrder
    {
      get
      {
        return 3;
      }
    }

    public override GameParameters.GameMode GameMode
    {
      get
      {
        return GameParameters.GameMode.ANY;
      }
    }

    public override bool HasPresets
    {
      get
      {
        return false;
      }
    }

    public override bool Enabled(System.Reflection.MemberInfo member, GameParameters parameters)
    {
      if (reactorDamage || member.Name == "reactorDamage")
        return true;
      else
        return false;
    }

    public static bool ReactorDamage
    {
      get
      {
        if (HighLogic.LoadedScene == GameScenes.MAINMENU)
          return true;
        SystemHeatGameSettings_ReactorDamage settings = HighLogic.CurrentGame.Parameters.CustomParams<SystemHeatGameSettings_ReactorDamage>();
        return settings.reactorDamage;
      }

      set
      {
        SystemHeatGameSettings_ReactorDamage settings = HighLogic.CurrentGame.Parameters.CustomParams<SystemHeatGameSettings_ReactorDamage>();
        settings.reactorDamage = value;
      }
    }

    public static int EngineerLevel
    {
      get
      {
        SystemHeatGameSettings_ReactorDamage settings = HighLogic.CurrentGame.Parameters.CustomParams<SystemHeatGameSettings_ReactorDamage>();
        return settings.engineerLevel;
      }

      set
      {
        SystemHeatGameSettings_ReactorDamage settings = HighLogic.CurrentGame.Parameters.CustomParams<SystemHeatGameSettings_ReactorDamage>();
        settings.engineerLevel = value;
      }
    }

    public static float RepairPerKit
    {
      get
      {
        SystemHeatGameSettings_ReactorDamage settings = HighLogic.CurrentGame.Parameters.CustomParams<SystemHeatGameSettings_ReactorDamage>();
        return settings.repairPerKit;
      }

      set
      {
        SystemHeatGameSettings_ReactorDamage settings = HighLogic.CurrentGame.Parameters.CustomParams<SystemHeatGameSettings_ReactorDamage>();
        settings.repairPerKit = value;
      }
    }
    public static float RepairThreshold
    {
      get
      {
        SystemHeatGameSettings_ReactorDamage settings = HighLogic.CurrentGame.Parameters.CustomParams<SystemHeatGameSettings_ReactorDamage>();
        return settings.repairThreshold;
      }

      set
      {
        SystemHeatGameSettings_ReactorDamage settings = HighLogic.CurrentGame.Parameters.CustomParams<SystemHeatGameSettings_ReactorDamage>();
        settings.repairThreshold = value;
      }
    }


    public static float RepairMax
    {
      get
      {
        SystemHeatGameSettings_ReactorDamage settings = HighLogic.CurrentGame.Parameters.CustomParams<SystemHeatGameSettings_ReactorDamage>();
        return settings.repairMax;
      }

      set
      {
        SystemHeatGameSettings_ReactorDamage settings = HighLogic.CurrentGame.Parameters.CustomParams<SystemHeatGameSettings_ReactorDamage>();
        settings.repairMax = value;
      }
    }

  }
}

public class SystemHeatGameSettings_NuclearFuel : GameParameters.CustomParameterNode
{
  [GameParameters.CustomParameterUI("Nuclear Fuel Transfer Needs Engineers",
    toolTip = "If enabled, nuclear fuel transfer needs experienced engineers",
    autoPersistance = true)]
  public bool requireEngineersForTransfer = true;

  [GameParameters.CustomIntParameterUI("Engineer Level Required",
    maxValue = 5, minValue = 1, stepSize = 1,
    toolTip = "Engineer level needed to transfer",
    autoPersistance = true)]
  public int engineerLevel = 3;

  public override string DisplaySection
  {
    get
    {
      return Section;
    }
  }

  public override string Section
  {
    get
    {
      return "System Heat";
    }
  }

  public override string Title
  {
    get
    {
      return "Nuclear Fuel";
    }
  }

  public override int SectionOrder
  {
    get
    {
      return 1;
    }
  }

  public override GameParameters.GameMode GameMode
  {
    get
    {
      return GameParameters.GameMode.ANY;
    }
  }

  public override bool HasPresets
  {
    get
    {
      return false;
    }
  }

  public override bool Enabled(System.Reflection.MemberInfo member, GameParameters parameters)
  {
    if (requireEngineersForTransfer || member.Name == "requireEngineersForTransfer")
      return true;
    else
      return false;
  }

  public static bool RequireEngineersForTransfer
  {
    get
    {
      if (HighLogic.LoadedScene == GameScenes.MAINMENU)
        return true;
      SystemHeatGameSettings_NuclearFuel settings = HighLogic.CurrentGame.Parameters.CustomParams<SystemHeatGameSettings_NuclearFuel>();
      return settings.requireEngineersForTransfer;
    }

    set
    {
      SystemHeatGameSettings_NuclearFuel settings = HighLogic.CurrentGame.Parameters.CustomParams<SystemHeatGameSettings_NuclearFuel>();
      settings.requireEngineersForTransfer = value;
    }
  }

  public static int EngineerLevel
  {
    get
    {
      SystemHeatGameSettings_NuclearFuel settings = HighLogic.CurrentGame.Parameters.CustomParams<SystemHeatGameSettings_NuclearFuel>();
      return settings.engineerLevel;
    }

    set
    {
      SystemHeatGameSettings_NuclearFuel settings = HighLogic.CurrentGame.Parameters.CustomParams<SystemHeatGameSettings_NuclearFuel>();
      settings.engineerLevel = value;
    }
  }

}

public class SystemHeatGameSettings_Boiloff : GameParameters.CustomParameterNode
{
  [GameParameters.CustomParameterUI("Fuel Boiloff",
    toolTip = "If enabled, configured fuels that need coolin", autoPersistance = true)]
  public bool boiloffEnabled = true;

  [GameParameters.CustomFloatParameterUI("Fuel Boiloff Rate",
    maxValue = 5.0f, minValue = 0.01f, asPercentage = true, stepCount = 100,
    toolTip = "Modifies base boiloff rate", autoPersistance = true)]
  public float boiloffRate = 1.0f;

  public override string DisplaySection
  {
    get
    {
      return Section;
    }
  }

  public override string Section
  {
    get
    {
      return "System Heat";
    }
  }

  public override string Title
  {
    get
    {
      return "Boiloff";
    }
  }

  public override int SectionOrder
  {
    get
    {
      return 1;
    }
  }

  public override GameParameters.GameMode GameMode
  {
    get
    {
      return GameParameters.GameMode.ANY;
    }
  }

  public override bool HasPresets
  {
    get
    {
      return false;
    }
  }

  public override bool Enabled(System.Reflection.MemberInfo member, GameParameters parameters)
  {
    if (boiloffEnabled || member.Name == "boiloffEnabled")
      return true;
    else
      return false;
  }

  public static bool BoiloffEnabled
  {
    get
    {
      if (HighLogic.LoadedScene == GameScenes.MAINMENU)
        return true;
      SystemHeatGameSettings_Boiloff settings = HighLogic.CurrentGame.Parameters.CustomParams<SystemHeatGameSettings_Boiloff>();
      return settings.boiloffEnabled;
    }

    set
    {
      SystemHeatGameSettings_Boiloff settings = HighLogic.CurrentGame.Parameters.CustomParams<SystemHeatGameSettings_Boiloff>();
      settings.boiloffEnabled = value;
    }
  }

  public static float BoiloffScale
  {
    get
    {
      SystemHeatGameSettings_Boiloff settings = HighLogic.CurrentGame.Parameters.CustomParams<SystemHeatGameSettings_Boiloff>();
      return settings.boiloffRate;
    }

    set
    {
      SystemHeatGameSettings_Boiloff settings = HighLogic.CurrentGame.Parameters.CustomParams<SystemHeatGameSettings_Boiloff>();
      settings.boiloffRate = value;
    }
  }


}
