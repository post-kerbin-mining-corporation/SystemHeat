using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SystemHeat
{
    public class ModuleSystemHeat: PartModule
    {
        
	    // Radiative properties
	    public float emissivity = 0.5f;
	    public float emissiveTemp = 290f;
        public float albedo = 0.5f;
	    public float radiativeMassScalar = 4f;

 	    // How much of the part is exposed to the sun at a time (usually half)
	    public float radiativeExposure = 0.5f;

	    // Convective properties
	    public float convectiveMassScalar = 4f;
	    public float convectiveTemp = 300f;

	    // whether to use passive convection and radiation
	    public bool passiveConvection = false;
	    public bool passiveRadiation = false;

	    // frequency (physics frames) of passive updates
	    public int updateFrequency = 3; 
        // counter
	    private int frameCounter = 0;

	    private bool partHeatStorage = false;

        // last frame's variables
	    private float lastFramePartHeat = 0f;
	    private float lastFrameVesselHeat = 0f;

        // last frame's deltas
	    private float partHeatDelta = 0f;
	    private float vesselHeatDelta = 0f;


        // ACCESSORS

        // PART: Get heat stored
        public float PartHeatStored
        {
            get
            {
                if (partHeatStorage)
                    return (float)part.Resources.Get(Utils.HeatResourceID).amount;
                else
                    return 0f;
            }
        }

        // PART: Get heat fraction stored
        public float PartHeatStoredFraction
        {
            get
            {
                if (partHeatStorage)
                {
                    return (float) (part.Resources.Get(Utils.HeatResourceID).amount/part.Resources.Get(Utils.HeatResourceID).maxAmount);
                }
                else
                {
                    return 0f;
                }
            }
        }

        // PART: Get heat delta 
        public float PartHeatBalance
        {
            get
            {
                if (partHeatStorage)
                {
                    return partHeatDelta;
                }
                {
                    return 0f;
                }
            }
        }

        // VESSEL: Get heat stored
        public float VeselHeatStored
        {
            get {
                return 0f;
            }
        }

        // VESSEL: get heat balance
        public float VesselHeatBalance
        {
            get
            {
                return vesselHeatDelta;
            }
        }

        // METHODS
        // -------
        // Add or subtract heat from the vessel
	    public float AddHeat(float amt)
	    {
		    return (float)AddHeat((double)amt);
	    }
	    public double AddHeat(double amt)
	    {
		    double remainder = part.RequestResource(Utils.HeatResourceName, amt, ResourceFlowMode.ALL_VESSEL);
		    return  remainder;
	    }
	
	    

	    public override void OnStart()
	    {
		    // Find if the part has heat storage!
            if (part.Resources.Get(Utils.HeatResourceID) != null)
            {
                Utils.Log("Part has heat storage");
                partHeatStorage = true;
            }
            else
            {
                partHeatStorage = false;
            }
	    }

    }
}
