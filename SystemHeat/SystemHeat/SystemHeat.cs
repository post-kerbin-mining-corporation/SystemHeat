using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SystemHeat
{
    // This class loads and stores data mostly
    [KSPAddon(KSPAddon.Startup.EveryScene, true)]
    public class SystemHeat : MonoBehaviour
    {
        public void Awake()
        {
            // Load resources up
            Utils.LoadPluginData();
        }
    }
}
