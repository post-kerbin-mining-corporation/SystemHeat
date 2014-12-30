/// ModuleSystemHeat.cs
/// Basic module for the System Heat plugin
/// Should always be on any part that wants to be part of the heat system
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SystemHeat
{
    public class ModuleSystemHeat: PartModule
    {
        // KSPFields
        // --------

	// RADIATION
	
	// Whether to use passive radiation
	[KSPField(isPersistant = false)]
	public bool passiveRadiation = false;
	
	// Emissivity (S-B law). Emissivity scales how well a part radiates heat
        [KSPField(isPersistant = false)]
	public float emissivity = 0.5f;
	// Emissive temperature (S-B law). Temperature scales how well a part radiates heat
        [KSPField(isPersistant = false)]
	public float emissiveTemp = 290f;
	// Albedo. Percent of incoming solar radiation that is reflected when a part is exposed to sunlight. 
	// High albedo = low absorption of solar energy
        [KSPField(isPersistant = false)]
        public float albedo = 0.5f;
        // Scaling factor that represents how a part's mass relates to its radiative surface area
        // Effective radiating area = radiative mass scalar*mass
        [KSPField(isPersistant = false)]
	public float radiativeMassScalar = 4f;
 	 // How much of the part's surface area is exposed to the sun at a time. 
        [KSPField(isPersistant = false)]
	public float radiativeExposure = 0.3f;

	// CONVECTION
	
	// Whether to use passive convection 
        [KSPField(isPersistant = false)]
	public bool passiveConvection = false;
	
	// Scaling factor that represents how a part's mass relates to its convective surface area
	// Effective convection area = convective mass scalar*mass
        [KSPField(isPersistant = false)]
	public float convectiveMassScalar = 4f;
	// Temperature of the part. Should generally be the same as emissive temp
        [KSPField(isPersistant = false)]
	public float convectiveTemp = 300f;

	// Frequency (physics frames) of passive updates
        [KSPField(isPersistant = false)]
	public int updateFrequency = 3; 
	
	// Change in Part Heat 
        [KSPField(isPersistant = false, guiActive = true, guiName = "Part Heat Delta")]
	public float partHeatDelta = 0f;
	
	// Change in Vessel Heat
        [KSPField(isPersistant = false, guiActive = true, guiName = "Vessel Heat Delta")]
	public float vesselHeatDelta = 0f;
	
	// Private variables
	
        // Counter
	private int frameCounter = 0;
	
	private bool partHeatStorage = false;

        // Last frame's heat totals
	private float lastFramePartHeat = 0f;
	private float lastFrameVesselHeat = 0f;

        // Actions and UI
        // --------------
        
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

        // Accessors
        // --------------

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
        	
            get {
                float maxAmount = 0f;
                float curAmount = 0f;
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

        // Public Methods
        // --------------
        // Adds heat to the part
        // Returns the amount of extra 
        public double GenerateHeat(double amt)
        {
        	return this.GenerateHeat(amt,ResourceFlowMode.ALL_VESSEL )
        }
        public double GenerateHeat(double amt, ResourceFlowMode mode)
        {
        	// returns actual amount generated
        	double actual = part.RequestResource(Utils.HeatResourceName, -amt, mode);
        	return (amt - actual);
        }
        // Consumes heat from the part
        // Returns the shortfall 
        public double ConsumeHeat(double amt)
        {
        	return this.ConsumeHeat(amt,ResourceFlowMode.ALL_VESSEL )
        }
        public double ConsumeHeat(double amt, ResourceFlowMode mode)
        {
        	double actual = part.RequestResource(Utils.HeatResourceName, -amt, mode);
        	return actual;
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

		// Update debug variables
                WindSpeed = Utils.GetAirSpeed(part.vessel);
                AirTemp = Utils.GetAirTemperature(part.vessel);
                Insolation = Utils.CalculateSolarInput(part.vessel);
                //PathLen = Utils.AtmosphericPathLength(part.vessel);
                //Zenith = Utils.ZenithAngle(part.vessel,part.vessel.mainBody);
                
                // Update heat changes
partHeatDelta = (lastFramePartHeat - PartHeatStored)/TimeWarp.fixedDeltaTime;
			vesselHeatDelta = (lastFrameVesselHeat - VesselHeatStored)/TimeWarp.fixedDeltaTime;

                    
			    if ( frameCounter > updateFrequency)
			    {
				    
				    // do convection
			        if (passiveConvection)
			        {
				        float amtConvected = CalculatePassiveConvection();
				        if (amtConvected > 0f)
				        	this.GenerateHeat((float)amtConvected*TimeWarp.fixedDeltaTime)
				        else 
				        	this.ConsumeHeat((float)amtConvected*TimeWarp.fixedDeltaTime)
			        }
				        // do radiation
                    if (passiveRadiation)
                    {
                    	float amtRadiated = CalculatePassiveRadiation();
                    	if (amtRadiated > 0f)
                    		this.GenerateHeat((float)amtRadiated*TimeWarp.fixedDeltaTime)
			 else 
				this.ConsumeHeat((float)amtRadiated*TimeWarp.fixedDeltaTime)
                    	
                        
                      
                    }
                    
			
			        frameCounter = 0;
			    }
			    frameCounter = frameCounter+1;
			    lastFramePartHeat = PartHeatStored;
                    lastFrameVesselHeat = VesselHeatStored;
		    }

	    }

	    // Calculates net convection balance in kW
	    // Positive means the part is gaining heat
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

	    // Calculates net radiation balance in kW
	    // positive means the part is gaining heat
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
            	// ray origin is 2000 units in the direciton of the sun
            	Vector3 castDirection = FlightGlobals.Bodies[0].position - part.transform.position;
            	Vector3 castOrigin = (part.transform.position-FlightGlobals.Bodies[0].position).normalized*2000f+part.transform.position
            	
                RaycastHit[] cast = Physics.RaycastAll(castOrigin, castDirection, 2500f);

                // If no hits, do nothing
                if (cast.Length == 0)
                {
                    heatInput = 0f;
                }
                else
                {
                	bool hitSelfOnly = true
                	foreach (RaycastHit hit in cast)
                	{
                		if (hit.collider.attachedRigidbody != part.Rigidbody)
                			hitSelfOnly = false;
                	}
                	if (hitSelfOnly)
                	{
                		heatInput = RadiativeHeatInput();
                	}
                }
            }
            else
            {
                heatInput = -2f;
            }

            HeatInputRadiation = heatInput;
            HeatOutputRadiation = heatOutput;

		    return heatInput-heatOutput;
	    }

        protected float RadiativeHeatInput()
        {
            return part.mass * radiativeMassScalar * radiativeExposure * albedo * Utils.CalculateSolarInput(vessel);
        }

    }
}
