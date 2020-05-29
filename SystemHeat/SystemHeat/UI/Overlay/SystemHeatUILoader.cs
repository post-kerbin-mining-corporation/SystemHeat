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

    public static GameObject OverlayPanelPrefab
    {
      get { return overlayPanelPrefab; }
    }

    private void Awake()
    {
      Utils.Log("[SystemHeatUILoader]: Loading UI Prefabs");
      AssetBundle prefabs = AssetBundle.LoadFromFile(Path.Combine(KSPUtil.ApplicationRootPath, "GameData/SystemHeat/UI/systemheatui.dat"));
      overlayPanelPrefab = prefabs.LoadAsset("SystemInfo") as GameObject;
      Utils.Log("[SystemHeatUILoader]: Loaded UI Prefabs");
    }
  }
}
