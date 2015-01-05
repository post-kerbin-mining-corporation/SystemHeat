using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SystemHeat
{
    public static class Utils
    {

        // Loads all plugin data
        public static void LoadPluginData()
        {

            //key = 206000000000 0 0 0
			//key = 13599840256 1 0 0
			//key = 68773560320 0.5 0 0
			//key = 0 10 0 0

            solarCurve = new FloatCurve();
            solarCurve.Add(0f, 13660f);
            solarCurve.Add(68773560320f, 683f);
            solarCurve.Add(13599840256f, 1366f);
            solarCurve.Add(206000000000f, 0f);
        }

        public static FloatCurve solarCurve;
        public static float aerosolOpticalDepth = 0.2f;
        public static float sigma = 0.0000000567f;

        // This function loads up some animationstates
        public static AnimationState[] SetUpAnimation(string animationName, Part part)
        {
            var states = new List<AnimationState>();
            foreach (var animation in part.FindModelAnimators(animationName))
            {
                var animationState = animation[animationName];
                animationState.speed = 0;
                animationState.enabled = true;
                // Clamp this or else weird things happen
                animationState.wrapMode = WrapMode.ClampForever;
                animation.Blend(animationName);
                states.Add(animationState);
            }
            // Convert 
            return states.ToArray();
        }

        // Name of the 'heat' resource
        public static string HeatResourceName = "SystemHeat";

        // Convenience for resource ID
        public static int HeatResourceID
        {
            get { return PartResourceLibrary.Instance.GetDefinition(Utils.HeatResourceName).id; }
        }

        public static int ResourceIDFromName(string nm)
        {
            return PartResourceLibrary.Instance.GetDefinition(nm).id;
        }

        

        //public static FloatCurve 



        // Finds if a vessel is in an atmosphere
	    public static bool InAtmosphere(Vessel vessel)
	    {
		    if (vessel.staticPressure > 0d)
			    return true;
		    else 
			    return false;
	    }
        public static  float MaxAtmosphereHeight(CelestialBody body)
        {
            return (float)body.atmosphereScaleHeight * 1000f * Mathf.Log(1000000);
        }

	    // Radiative calculations
	    public static float CalculateSolarInput(Vessel vessel)
	    {
		    // Divide by 1000 for kW
		    float baseIrradiance = solarCurve.Evaluate(GetSolarAltitude(vessel))/1000f;

		    if (InAtmosphere(vessel))
		    {
			    return baseIrradiance*Mathf.Exp(-AtmosphericPathLength(vessel)*aerosolOpticalDepth);
		    } else 
		    {
			    return baseIrradiance;
		    }
	
	    }

        // Get height above the sun
        public static float GetSolarAltitude(Vessel vessel)
        {
            double altAboveSun = FlightGlobals.getAltitudeAtPos(vessel.GetWorldPos3D(), FlightGlobals.Bodies[0]);
            return (float)altAboveSun;
        }

	    // Calculates if the sun is visible from a part
	    public static bool SolarExposure(Part part)
	    {
            Transform refXForm = part.partTransform;

		    // use method from NF solar
            bool sunVisible = true;
            float angle = 0f;
            string obscuringBody = "nil";

            CelestialBody sun = FlightGlobals.Bodies[0];
            CelestialBody currentBody = FlightGlobals.currentMainBody;

            angle = Vector3.Angle(refXForm.forward, sun.transform.position-refXForm.position);

            if (currentBody != sun)
            {

                Vector3d vT = sun.position - part.vessel.GetWorldPos3D();
                Vector3d vC = currentBody.position - part.vessel.GetWorldPos3D();
                // if true, behind horizon plane
                if (Vector3d.Dot(vT, vC) > (vC.sqrMagnitude - currentBody.Radius * currentBody.Radius))
                {
                    // if true, obscured
                    if ((Mathf.Pow(Vector3.Dot(vT, vC), 2) / vT.sqrMagnitude) > (vC.sqrMagnitude - currentBody.Radius * currentBody.Radius))
                    {
                        sunVisible = false;
                        obscuringBody = currentBody.name;
                    }
                }
            }

            return sunVisible;
	    }

	    // Find the path length of a ray through an atmosphere to the vessel
	    public static float AtmosphericPathLength(Vessel vessel)
	    {
            CelestialBody body = vessel.mainBody;
		    float zenithAngle = ZenithAngle(vessel,body);
		    // Geometric calculation
		    return (float)Mathf.Sqrt(Mathf.Pow(
                ((float)body.Radius + (float)vessel.altitude) + (MaxAtmosphereHeight(body) - (float)vessel.altitude)
                , 2 ) -
                Mathf.Pow((float)body.Radius+(float)vessel.altitude,2)*Mathf.Pow(Mathf.Sin(zenithAngle),2))-
                (float)(body.Radius+vessel.altitude)*Mathf.Cos(zenithAngle);
	    }

	    // Get zenith angle of sun
	    public static float ZenithAngle(Vessel vessel, CelestialBody body)
	    {
		    // Oh I'm so clever. no need for EoT madness
		    return Mathf.Deg2Rad*(float)Vector3d.Angle(body.position-vessel.GetWorldPos3D(),vessel.GetWorldPos3D()-FlightGlobals.Bodies[0].position);
	    }

	    //public static float Get
	    public static float GetAirTemperature(Vessel vessel)
	    {
		    // altitude
		    FloatCurve planetTempAltitude = GetPlanetTemperatureCurve(vessel.mainBody);
		    float airTemp = planetTempAltitude.Evaluate((float)vessel.altitude);
		    // latitude
		    airTemp = airTemp*Mathf.Cos(0.75f*(float)vessel.latitude*Mathf.Deg2Rad);
		    // time of day, clamp zenith angle to between 0 and 90 (90=horizon)
		    float zen = Mathf.Clamp(ZenithAngle(vessel,vessel.mainBody),0f,90f);
		    // scale air temp by a fudge factor of 1/pressure
		    airTemp = airTemp* Mathf.Cos(   (1f/(float)vessel.mainBody.staticPressureASL*zen*Mathf.Deg2Rad ));
		    return airTemp;
	    }

	    public static float GetAirSpeed(Vessel vessel)
	    {
		    FloatCurve windCurve = GetPlanetWindCurve(vessel.mainBody);
		    return windCurve.Evaluate((float)vessel.altitude)+(float)vessel.srf_velocity.magnitude;
	    }

	    public static FloatCurve GetPlanetWindCurve(CelestialBody body)
	    {
		    FloatCurve planetCurve = new FloatCurve();
		    // aloft wind speed = 0
            planetCurve.Add(MaxAtmosphereHeight(body), 0f);
		    // WRONG
            
            float srfSpeedRot =( Mathf.PI*2f* (float)(body.Radius))/(float)body.rotationPeriod;
            planetCurve.Add(MaxAtmosphereHeight(body) / 2f, 0.5f * srfSpeedRot);
		    // surface wind speed = 1x of rotation speed
		    planetCurve.Add(0f,0.05f*srfSpeedRot);

            return planetCurve;
	    }

	    // auto-generates a planet temperature curve from a messy thing
	    public static FloatCurve GetPlanetTemperatureCurve(CelestialBody body)
	    {
		    FloatCurve planetCurve = new FloatCurve();
		    // it is zero at the top 
            planetCurve.Add(MaxAtmosphereHeight(body), 0f);
		    // defined in terms of incoming radiation, atmospheric pressure at sea level
		    // earth or kerbin is 1366 w/m2
		    float baseIrradiance = solarCurve.Evaluate((float)FlightGlobals.getAltitudeAtPos(body.position, FlightGlobals.Bodies[0]));
            planetCurve.Add(0f, Mathf.Pow((float)body.staticPressureASL, 0.01f) * Mathf.Pow((float)body.atmosphereScaleHeight, 0.5f) * (baseIrradiance / 20f));

		    return planetCurve;
		
	    }



        // LOGGING
        // -------
        public static void Log(string message)
        {
            Debug.Log("HeatSystem: " + message);
        }

        public static void LogWarn(string message)
        {
            Debug.LogWarning("HeatSystem: " + message);
        }

        public static void LogError(string message)
        {
            Debug.LogError("HeatSystem: " + message);
        }
    }
}
