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
    public static GameObject OverlayPanelPrefab { get { return overlayPanelPrefab; } }
    public static GameObject ToolbarPanelPrefab { get { return toolbarPanelPrefab; } }
    public static GameObject ToolbarPanelLoopPrefab { get { return toolbarPanelLoopPrefab; } }

    public static GameObject ReactorDataFieldPrefab { get { return reactorDataFieldPrefab; } }
    public static GameObject ReactorWidgetPrefab { get { return reactorWidgetPrefab; } }
    public static GameObject ReactorToolbarPanelPrefab { get { return reactorToolbarPanelPrefab; } }

    private static GameObject overlayPanelPrefab;
    private static GameObject toolbarPanelPrefab;
    private static GameObject toolbarPanelLoopPrefab;

    private static GameObject reactorDataFieldPrefab;
    private static GameObject reactorWidgetPrefab;
    private static GameObject reactorToolbarPanelPrefab;

    private void Awake()
    {
      Utils.Log("[SystemHeatUILoader]: Loading UI Prefabs");
      AssetBundle prefabs = AssetBundle.LoadFromFile(Path.Combine(KSPUtil.ApplicationRootPath, "GameData/SystemHeat/UI/systemheatui.dat"));
      overlayPanelPrefab = prefabs.LoadAsset("SystemInfo") as GameObject;
      toolbarPanelPrefab = prefabs.LoadAsset("SystemHeatToolbar") as GameObject;
      toolbarPanelLoopPrefab = prefabs.LoadAsset("SystemHeatLoopData") as GameObject;
      reactorToolbarPanelPrefab = prefabs.LoadAsset("ReactorWindow") as GameObject;
      reactorWidgetPrefab = prefabs.LoadAsset("ReactorWidget") as GameObject;
      reactorDataFieldPrefab = prefabs.LoadAsset("ReactorDataField") as GameObject;
      Utils.Log("[SystemHeatUILoader]: Loaded UI Prefabs");
    }
  }
}
