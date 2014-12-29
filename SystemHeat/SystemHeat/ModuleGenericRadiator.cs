using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SystemHeat
{
    // Defines a simplistic radiator
    // Can be deployed or undeployed
    public class ModuleGenericRadiator: ModuleDeployableSolarPanel
    {
        // kW radiated when open
        [KSPField(isPersistant = false)]
        public float HeatRadiatedExtended;

        // kW radiated when closed
        [KSPField(isPersistant = false)]
        public float HeatRadiated;
     
        // Tweakable to allow or disallow sun tracking
        [KSPField(guiName = "Tracking", guiActiveEditor = true, isPersistant = true)]
        [UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
        public bool TrackSun = true;

        // animation for radiator heat
        [KSPField(isPersistant = false)]
        public string HeatAnimation;

        [KSPField(isPersistant = false)]
        public string HeatTransformName;

         // Private
        private AnimationState[] heatStates;
        private Transform heatTransform;
        private ModuleSystemHeat heatModule;

        // Actions and UI
        // --------------

        // Heat Rejection UI
        [KSPField(isPersistant = false, guiActive = true, guiName = "Current Heat Rejection")]
        public string HeatRejectionGUI = "0 kW";


        // Toggle radiator
        public void Toggle()
        {
            if (base.panelState == ModuleDeployableSolarPanel.panelStates.EXTENDED)
                base.Retract();
            if (base.panelState == ModuleDeployableSolarPanel.panelStates.RETRACTED)
                base.Extend();
        }

       


        public override string GetInfo()
        {
            string info = "";

            if (base.animationName == "")
            {
               info += String.Format("Heat Radiated: {0:F1} kW", HeatRadiated);
            } else 
            {
                info += String.Format("Heat Radiated (Retracted): {0:F1} kW", HeatRadiated) + "\n" +
                    String.Format("Heat Radiated (Deployed): {0:F1} kW", HeatRadiatedExtended);
            }

            return info;
             
        }
        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            heatModule = part.gameObject.GetComponent<ModuleSystemHeat>();

            if (heatModule == null)
            {
                Utils.LogError("No SystemHeat Module on part!");
            }

            heatStates = Utils.SetUpAnimation(HeatAnimation, part);
            heatTransform = part.FindModelTransform(HeatTransformName);

            foreach (AnimationState heatState in heatStates)
            {
                heatState.AddMixingTransform(heatTransform);
                heatState.blendMode = AnimationBlendMode.Blend;
                heatState.layer = 15;
                heatState.weight = 1.0f;
                heatState.enabled = true;
            }

            if (!TrackSun)
                base.trackingSpeed = 0f;

            part.force_activate();
        }

        public override void OnUpdate()
        {
 
            base.OnUpdate();
            foreach (BaseField fld in base.Fields)
            {
                if (fld.guiName == "Sun Exposure")
                    fld.guiActive = false;
                if (fld.guiName == "Energy Flow")
                    fld.guiActive = false;
                if (fld.guiName == "Status")
                    fld.guiActive = false;
               
            }
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

            if (heatModule != null)
            {

                float availableHeatRejection = 0f;

                // Heat rejection from panels
                if (base.animationName != "")
                {
                    if (base.panelState != ModuleDeployableSolarPanel.panelStates.EXTENDED && base.panelState != ModuleDeployableSolarPanel.panelStates.BROKEN)
                    {
                        // Debug.Log("Closed! " + HeatRadiatedClosed.ToString());
                        availableHeatRejection += HeatRadiated;
                    }
                    else if (base.panelState == ModuleDeployableSolarPanel.panelStates.BROKEN)
                    {
                        // Debug.Log("Broken!! " + 0.ToString());
                        availableHeatRejection = 0f;
                    }
                    else
                    {
                        // Debug.Log("Open! " + HeatRadiated.ToString());
                        availableHeatRejection += HeatRadiatedExtended;
                    }
                }
                else
                {
                    availableHeatRejection += HeatRadiated;
                }

                // Add the heat
                heatModule.AddHeat(-availableHeatRejection*TimeWarp.fixedDeltaTime);

                foreach (AnimationState state in heatStates)
                {

                    //state.normalizedTime = Mathf.MoveTowards(state.normalizedTime, Mathf.Clamp01(requestedHeatRejection / availableHeatRejection), 0.1f * TimeWarp.fixedDeltaTime);
                }
                HeatRejectionGUI = String.Format("{0:F1} kW", availableHeatRejection);
            }
        }

    }
    
}
