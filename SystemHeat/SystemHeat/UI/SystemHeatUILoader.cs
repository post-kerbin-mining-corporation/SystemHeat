using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;


namespace SystemHeat.UI
{
  [KSPAddon(KSPAddon.Startup.Instantly, true)]
  public class SystemHeatUILoader : MonoBehaviour
  {
    public static GameObject OverlayPanelPrefab { get; private set; }
    public static GameObject ToolbarPanelPrefab { get; private set; }

    public static GameObject ReactorWidgetPrefab { get; private set; }
    public static GameObject ReactorToolbarPanelPrefab { get; private set; }

    private void Awake()
    {
      Utils.Log("[SystemHeatUILoader]: Loading UI Prefabs");
      AssetBundle prefabs = AssetBundle.LoadFromFile(Path.Combine(KSPUtil.ApplicationRootPath, "GameData/SystemHeat/UI/systemheatui.dat"));
      OverlayPanelPrefab = prefabs.LoadAsset("SystemInfo") as GameObject;
      OverlayPanelPrefab = prefabs.LoadAsset("ToolbarWindow") as GameObject;
      ReactorToolbarPanelPrefab = prefabs.LoadAsset("ReactorToolbarWindow") as GameObject;
      ReactorWidgetPrefab = prefabs.LoadAsset("ReactorWidget") as GameObject;
      Utils.Log("[SystemHeatUILoader]: Loaded UI Prefabs");
    }
  }
}
