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
    private static GameObject overlayPanelPrefab;
    private static GameObject toolbarPanelPrefab;

    public static GameObject OverlayPanelPrefab
    {
      get { return overlayPanelPrefab; }
    }
    public static GameObject ToolbarPanelPrefab
    {
      get { return toolbarPanelPrefab; }
    }

    private void Awake()
    {
      Utils.Log("[SystemHeatUILoader]: Loading UI Prefabs");
      AssetBundle prefabs = AssetBundle.LoadFromFile(Path.Combine(KSPUtil.ApplicationRootPath, "GameData/SystemHeat/UI/systemheatui.dat"));
      overlayPanelPrefab = prefabs.LoadAsset("SystemInfo") as GameObject;
      toolbarPanelPrefab = prefabs.LoadAsset("ToolbarWindow") as GameObject;
      Utils.Log("[SystemHeatUILoader]: Loaded UI Prefabs");
    }
  }
}
