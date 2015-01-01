// A simplistic heat convector
// Transports heat away via convection when the vessel is in an atmosphere

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SystemHeat
{
    
    public class ModuleGenericConvector: PartModule
    {
        // Enabled or not
        [KSPField(isPersistant = true)]
        public bool Enabled = false;
        
        // Amount of heat to remove under optimum conditions (kerbin ASL)
        [KSPField(isPersistant = false)]
        public float HeatConvected = 100f;
        
        // Resource use when on
        [KSPField(isPersistant = false)]
        public float ResourceUse = 0f;
        
        // Name of the resource to use
        [KSPField(isPersistant = false)]
        public string ResourceName = "";
        
        // Animation that plays with increasing heat
        [KSPField(isPersistant = false)]
        public string HeatAnimation;
        
        // Transform for animation mixing
        [KSPField(isPersistant = false)]
        public string HeatTransformName;

        /// UI
        /// ----
        
        // Heating status
        [KSPField(isPersistant = false, guiActive = true, guiName = "Heat Convected")]
        public string HeatStatus;
        
        // Info for ui
        public override string GetInfo()
        {
            return "";
        }
        
        /// ACTIONS AND EVENTS
        // -------
        [KSPEvent(guiActive = true, guiName = "Start Convector", active = true)]
        public void EnableConvector()
        {
            Enabled = true;
        }
        [KSPEvent(guiActive = true, guiName = "Shutdown Convector", active = false)]
        public void DisableConvector()
        {
            Enabled = false;
        }
        [KSPEvent(guiActive = true, guiName = "Toggle Convector", active = true)]
        public void ToggleConvector()
        {
            Enabled = !Enabled;
        }

        [KSPAction("Start Convector")]
        public void StartConvectorAction(KSPActionParam param)
        {
            EnableConvector();
        }

        [KSPAction("Shutdown Convector")]
        public void ShutdownConvectorAction(KSPActionParam param)
        {
            DisableConvector();
        }
        
        [KSPAction("Toggle Convector")]
        public void ToggleConvectorAction(KSPActionParam param)
        {
            ToggleConvector();
        }
        
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
        }
        protected void FixedUpdate()
	    {
		    if (HighLogic.LoadedScene == GameScenes.FLIGHT)
		    {
		        if (heatModule == null)
                {
                    Utils.LogError("No SystemHeat Module on part!");
                    return;
                }
		        if (Enabled)
		        {
		            bool success = true;;
		            float heatChange = 0f;
		            string errorReason = "";
		            
		            if (ResourceUse > 0f)
		            {
		                double req = part.RequestResource(ResourceName,(double)(ResourceUse*TimeWarp.fixedDeltaTime));
		                if (req >= ResourceUse*TimeWarp.fixedDeltaTime)
		                {
		                    success = CalculateConvection(out errorReason, out heatChange);
		                } else 
		                {
                            errorReason = ResourceName + " deprived!";
		                }
		            }
		            else 
		            {
		                success = CalculateConvection(out errorReason, out heatChange);
		            }
		            
		            if (success)
		            {
    	                HeatStatus = String.Format("{0:F0}", heatChange);
    	                heatModule.ConsumeHeat((double)heatChange);
		            } else 
		            {
		                HeatStatus = errorReason;
		                DisableConvector();
		            }
		        } 
		        
		        
		        
		    }
	    }
	    
	    // returns whether convection was successful
	    //convection amount, in kW
	    protected bool CalculateConvection(out string err, out float heatChange)
	    {
            heatChange = 0f;
		    // Convector really only works in atmo
		    if (Utils.InAtmosphere(vessel))
		    {
			    float atmoTemp = Utils.GetAirTemperature(part.vessel);
			    float atmoSpeed = Utils.GetAirSpeed(part.vessel);
			    
			    //float atmoPressure = (float)vessel.staticPressure;
			    // My profs would scream at this, but... hacky newtonian convection
                //heatChange = (atmoSpeed / 2f) * (atmoPressure / 2f) * part.mass * convectiveMassScalar * (atmoTemp-convectiveTemp);
		    } else 
		    {
		        heatChange = 0f;
		        err = "No atmosphere!";
		        return false;
		    }
		    
            heatChange = heatChange / 1000f;
            
            // if there is actual heat removed...
            if (heatChange <= 0)
            {
                err = "";   
                return true;
            } else
            {
                err = "No convection!";
                return false;
            }
            
      

	    }
    }
}
