using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SystemHeat
{
    public class ModuleGenericHeatStorage: PartModule
    {
        // Animation that plays with increasing heat
        [KSPField(isPersistant = false)]
        public string HeatAnimation;
        
        // Transform for animation mixing
        [KSPField(isPersistant = false)]
        public string HeatTransformName;

         // Private variables
        private AnimationState[] heatStates;
        private Transform heatTransform;
        private ModuleSystemHeat heatModule;
        
        
        public override void OnStart(PartModule.StartState state)
        {
            heatModule = part.gameObject.GetComponent<ModuleSystemHeat>();

            if (heatModule == null)
            {
                Utils.LogError("No SystemHeat Module on part!");
                return;
            }

            // Set up animation
            if (HeatAnimation != "")
            {
                heatStates = Utils.SetUpAnimation(HeatAnimation, part);


                if (heatTransform != null)
                {
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
            }
        }
        
        public void Update()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                if (HeatAnimation != "" && heatModule != null)
                {
                    foreach (AnimationState state in heatStates)
                    {
                        state.normalizedTime = heatModule.PartHeatStoredFraction;
                    }
                }
            }
        }
            
    }
}
