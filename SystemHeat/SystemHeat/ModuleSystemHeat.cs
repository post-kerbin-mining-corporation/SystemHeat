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
        [KSPField(isPersistant = false)]
	    public float emissivity = 0.5f;
        [KSPField(isPersistant = false)]
	    public float emissiveTemp = 290f;
        [KSPField(isPersistant = false)]
        public float albedo = 0.5f;
        [KSPField(isPersistant = false)]
	    public float radiativeMassScalar = 4f;

 	    // How much of the part is exposed to the sun at a time (usually half)
        [KSPField(isPersistant = false)]
	    public float radiativeExposure = 0.5f;

	    // Convective properties
        [KSPField(isPersistant = false)]
	    public float convectiveMassScalar = 4f;
        [KSPField(isPersistant = false)]
	    public float convectiveTemp = 300f;

	    // whether to use passive convection and radiation
        [KSPField(isPersistant = false)]
	    public bool passiveConvection = false;
        [KSPField(isPersistant = false)]
	    public bool passiveRadiation = false;

	    // frequency (physics frames) of passive updates
        [KSPField(isPersistant = false)]
	    public int updateFrequency = 3; 
        // counter
	    private int frameCounter = 0;

	    private bool partHeatStorage = false;

        // last frame's variables
	    private float lastFramePartHeat = 0f;
	    private float lastFrameVesselHeat = 0f;

        // last frame's deltas
        [KSPField(isPersistant = false, guiActive = true, guiName = "Part Heat Delta")]
	    public float partHeatDelta = 0f;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Vessel Heat Delta")]
	    public float vesselHeatDelta = 0f;

        // GUI
        // debug mostly

        [KSPField(isPersistant = false, guiActive = true, guiName = "Rad Heat In")]
        public float HeatInputRadiation;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Rad Heat Out")]
        public float HeatOutputRadiation;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Conv Net Heat ")]
        public float HeatChangeConvec;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Solar Insolation ")]
        public float Insolation;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Air Temperature ")]
        public float AirTemp;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Wind Speed")]
        public float WindSpeed;

        //[KSPField(isPersistant = false, guiActive = true, guiName = "Zenith Angle")]
        //public float Zenith;

       // [KSPField(isPersistant = false, guiActive = true, guiName = "Path Len")]
        //public float PathLen;

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
        public float VesselHeatStored
        {
        	float maxAmount = 0f;
        	float curAmount = 0f;
            get {
            	foreach (Part p in this.vessel.parts)
            	{
            		foreach (PartResource resource in p.Resources)
            		{
            			if (resource.resourceName == Utils.HeatResourceName)
            			{
            				maxAmount += (float)resource.maxAmount;
            				curAmount += (float)resource.amount;
            			}
            		}
            	
            	}
                return curAmount;
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
            //Utils.Log(amt.ToString());

		    return (float)AddHeat((double)amt);
	    }
	    public double AddHeat(double amt)
	    {
            
		    double remainder = part.RequestResource(Utils.HeatResourceName, -amt, ResourceFlowMode.ALL_VESSEL);
		    return  remainder;
	    }



        public override void OnStart(PartModule.StartState state)
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

        protected void FixedUpdate()
	    {
		    if (HighLogic.LoadedScene == GameScenes.FLIGHT)
		    {

                WindSpeed = Utils.GetAirSpeed(part.vessel);
                AirTemp = Utils.GetAirTemperature(part.vessel);
                Insolation = Utils.CalculateSolarInput(part.vessel);
                //PathLen = Utils.AtmosphericPathLength(part.vessel);
                //Zenith = Utils.ZenithAngle(part.vessel,part.vessel.mainBody);
partHeatDelta = (lastFramePartHeat - PartHeatStored)/TimeWarp.fixedDeltaTime;
			vesselHeatDelta = (lastFrameVesselHeat - VesselHeatStored)/TimeWarp.fixedDeltaTime;

                    
			    if ( frameCounter > updateFrequency)
			    {
				    // Update heat deltas
				    
				    

				    // do convection
			        if (passiveConvection)
			        {
				        float amtConvected = CalculatePassiveConvection();
				        this.AddHeat(amtConvected*TimeWarp.fixedDeltaTime);
			        }
				        // do radiation
                    if (passiveRadiation)
                    {
                        float amtRadiated = CalculatePassiveRadiation();
                        this.AddHeat(amtRadiated * TimeWarp.fixedDeltaTime);
                    }
                    
			
			        frameCounter = 0;
			    }
			    frameCounter = frameCounter+1;
			    lastFramePartHeat = PartHeatStored;
                    lastFrameVesselHeat = VesselHeatStored;
		    }

	    }

	    // calculates net convection balance
	    protected float CalculatePassiveConvection()
	    {
		    float heatChange  = 0f;


		    if (Utils.InAtmosphere(vessel))
		    {
			    float atmoTemp = Utils.GetAirTemperature(part.vessel);
			    float atmoSpeed = Utils.GetAirSpeed(part.vessel);
			    float atmoPressure = (float)vessel.staticPressure;
			    // My profs would scream at this, but... hacky newtonian convection
                heatChange = (atmoSpeed / 2f) * (atmoPressure / 2f) * part.mass * convectiveMassScalar * (atmoTemp-convectiveTemp);
		    }

            HeatChangeConvec = heatChange / 1000f;


            return heatChange / 1000f;

	    }

	    // Calculates net radiation balance
	    protected float CalculatePassiveRadiation()
	    {
		    float heatInput = 0f;
		    float heatOutput = 0f;
		    // Wow, this doesn't take into account anything!
		    // This temperature value should eventually mean something
		    // result is in kW
		    heatOutput = part.mass*radiativeMassScalar*Utils.sigma*emissivity*Mathf.Pow(emissiveTemp,4)/1000f;

		    // Calculate obscurance by terrain and ships
            if (Utils.SolarExposure(part))
            {
                RaycastHit[] cast = Physics.RaycastAll(part.transform.position, part.transform.position - FlightGlobals.Bodies[0].position, 2500f);

                // If not hits, do solar input
                if (cast.Length == 0)
                {
                    heatInput = RadiativeHeatInput();
                }
                else
                {
                    int hitCounter = 0;
                    foreach (RaycastHit hit in cast)
                    {
                        if (!hit.collider.attachedRigidbody == part.Rigidbody)
                        {
                            hitCounter = hitCounter + 1;
                        }
                    }
                    // Just ourself was in the way!
                    if (hitCounter == 0)
                    {
                        heatInput = RadiativeHeatInput();
                    }
                    else
                    {
                        heatInput = -1f;
                    }
                }

            }
            else
            {
                heatInput = -2f;
            }

            HeatInputRadiation = heatInput;
            HeatOutputRadiation = heatOutput;

		    return heatOutput-heatInput;
	    }

        protected float RadiativeHeatInput()
        {
            return part.mass * radiativeMassScalar * radiativeExposure * albedo * Utils.CalculateSolarInput(vessel);
        }

    }
}
