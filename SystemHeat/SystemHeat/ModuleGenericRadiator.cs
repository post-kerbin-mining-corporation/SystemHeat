using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SystemHeat
{
    
    // A simplistic heat radiator
    // Can represent a simple radiator or a deplyed radiator
    // Hijacks ModuleDeplyableSolarPanel to do sun-parallel rotation
    
    public class ModuleGenericRadiator: ModuleDeployableSolarPanel
    {
        // KSPFields
        // --------
        
        // GAMEPLAY
        
        // Heat radiated when open (kW)
        [KSPField(isPersistant = false)]
        public float HeatRadiatedExtended;

        // Heat radiated when closed (kW)
        [KSPField(isPersistant = false)]
        public float HeatRadiated;
     
        // Resource use when extended
        [KSPField(isPersistant = false)]
        public float ResourceUseExtended = 0f;
        
        // Resource use when closed
        [KSPField(isPersistant = false)]
        public float ResourceUse = 0f;
        
        // Name of the resource to use
        [KSPField(isPersistant = false)]
        public string ResourceName = "";
        
        // ANIMATION
      
        // Start deployed or closed
        [KSPField(guiName = "Start", guiActiveEditor = true, isPersistant = true)]
        [UI_Toggle(disabledText = "Closed", enabledText = "Open")]
        public bool StartDeployed = false;
        
        // Allow or disallow sun tracking (cosmetic only for now)
        [KSPField(guiName = "Tracking", guiActiveEditor = true, isPersistant = true)]
        [UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
        public bool TrackSun = true;
        
        

        // Animation that plays with increasing heat
        [KSPField(isPersistant = false)]
        public string HeatAnimation = "";
        // Transform for animation mixing
        [KSPField(isPersistant = false)]
        public string HeatTransformName = "";

         // Private variables
        private AnimationState[] deployStates;
        private AnimationState[] heatStates;
        private Transform heatTransform;
        private ModuleSystemHeat heatModule;

        // Actions and UI
        // --------------

        // Heat Rejection UI note
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

        // UI shown in VAB/SPH
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
                return;
            }

            // get the animation state for panel deployment
            deployStates = Utils.SetUpAnimation(base.animationName, part);
            
            // Set up heat animation
            if (heatTransform != null && HeatAnimation != "")
            {
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
            }

            if (!TrackSun)
                base.trackingSpeed = 0f;

            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                part.force_activate();
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            // Hide all the solar panel fields
            if  (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
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
                   
            // If in the editor
            if (HighLogic.LoadedSceneIsEditor)
            {
                // If we have a panel animation...
                if (deployStates != null)
                {
                    if (StartDeployed)
                    {
                        // Play the animation
                        foreach (AnimationState a in deployStates)
                        {
                            a.speed = 2f;
                            a.normalizedTime = 1f;
                        }
                        base.panelState = ModuleDeployableSolarPanel.panelStates.EXTENDED;
                    } else 
                    {
                        // Reverse the animation
                        foreach (AnimationState a in deployStates)
                        {
                            a.speed = -2f;
                            a.normalizedTime = 0f;
                        }
                        base.panelState = ModuleDeployableSolarPanel.panelStates.RETRACTED;
                    }
                    //Unbreak the persistance
                    base.stateString = Enum.GetName(typeof(ModuleDeployableSolarPanel.panelStates),base.panelState);
                }
                
            }
        }
        
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();  
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                if (heatModule == null)
                {
                    Utils.LogError("No SystemHeat Module on part!");
                    return;
                }
                else
                {
                    // Heat rejection from panels
                    float availableHeatRejection = 0f;

                    
                    // If an animation name is present, assume deployable
                    if (base.animationName != "")
                    {
                        if (base.panelState != ModuleDeployableSolarPanel.panelStates.EXTENDED && base.panelState != ModuleDeployableSolarPanel.panelStates.BROKEN)
                        {
                            // Utils.Log("Closed! " + HeatRadiated.ToString());
                            availableHeatRejection += HeatRadiated;
                        }
                        else if (base.panelState == ModuleDeployableSolarPanel.panelStates.BROKEN)
                        {
                            // Utils.Log("Broken!! " + 0.ToString());
                            availableHeatRejection = 0f;
                        }
                        else
                        {
                            // Utils.Log("Open! " + HeatRadiatedExtended.ToString());
                            availableHeatRejection += HeatRadiatedExtended;
                        }
                    }
                    // always radiate
                    else
                    {
                        availableHeatRejection += HeatRadiated;
                    }
                    
                    // Add the heat via the HeatModule
                    float actualHeat = (float)heatModule.ConsumeHeat(availableHeatRejection*TimeWarp.fixedDeltaTime);
                    
                    // Update the UI widget
                    HeatRejectionGUI = String.Format("{0:F1} kW", availableHeatRejection);
                    
                    if (HeatAnimation != "" && heatStates != null)
                    {
                        foreach (AnimationState state in heatStates)
                        {
                            state.normalizedTime = Mathf.MoveTowards(state.normalizedTime, Mathf.Clamp01(actualHeat / availableHeatRejection), 0.1f * TimeWarp.fixedDeltaTime);
                        }
                    }
                    
                }
            }
          
        }

    }
    
}
