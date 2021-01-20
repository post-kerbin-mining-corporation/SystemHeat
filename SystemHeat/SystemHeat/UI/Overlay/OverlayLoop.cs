using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SystemHeat;
using KSP.Localization;

namespace SystemHeat.UI
{
  /// <summary>
  /// The controller for a loop renderer
  /// </summary>
  public class OverlayLoop
  {

    public bool Drawn { get; set; }

    protected Transform root;
    protected OverlayLine overlayLine;
    protected List<OverlayPoint> overlayPoints;
    protected List<OverlayPanel> overlaPanels;
    public HeatLoop heatLoop;

    public OverlayLoop(HeatLoop loop, Transform overlayRoot, bool startVisible)
    {
      root = overlayRoot;
      heatLoop = loop;
      overlayLine = new OverlayLine(root, loop.ID);

      SetVisible(startVisible);
    }

    public void Update(HeatLoop loop)
    {
      heatLoop = loop;

      GenerateLinePoints();
      UpdateLinePositions();
      UpdateLoopProperties();
    }


    protected void UpdateLoopProperties()
    {
      overlayLine.UpdateColor(heatLoop.ID);
      overlayLine.UpdateGlow(heatLoop.Temperature, heatLoop.NominalTemperature);
    }


    protected void UpdateLinePositions()
    {
      List<Vector3[]> positions = overlayPoints.Select(x => x.GetDrawingCoords(SystemHeatSettings.OverlayPadding)).ToList();
      List<Vector3> pos3d = positions.SelectMany(i => i).ToList();
      pos3d.Add(pos3d[0]);
      for (int i=0;i < pos3d.Count  ; i++)
      {
        if (HighLogic.LoadedSceneIsEditor)
          pos3d[i] = pos3d[i];
        if (HighLogic.LoadedSceneIsFlight)
          pos3d[i] = heatLoop.LoopModules[0].part.vessel.vesselTransform.TransformPoint(pos3d[i]);
        
      }
      overlayLine.UpdatePositions(pos3d);
    }



    public void GenerateLinePoints()
    {
      // Collect positions of all heat modules
      Vector3[] systemCoords = new Vector3[heatLoop.LoopModules.Count];
      for (int i = 0; i < heatLoop.LoopModules.Count; i++)
      {
        if (HighLogic.LoadedSceneIsEditor)
        {
          systemCoords[i] = heatLoop.LoopModules[i].part.transform.position;
        }

        if (HighLogic.LoadedSceneIsFlight)
        {
          systemCoords[i] = heatLoop.LoopModules[i].part.vessel.vesselTransform.InverseTransformPoint(heatLoop.LoopModules[i].part.transform.position);
        }
      }
      
      float[] x0 = null;
      float[] y0 = null;
      float[] z0 = null;

      ArrayUtils.SplitVector3Array(systemCoords, ref x0, ref y0, ref z0);
      float origin = z0.Average();

      // Set up the bounds
      float padding = SystemHeatSettings.OverlayPadding;
      float padding_bounds = SystemHeatSettings.OverlayBoundsPadding;

      float[] bounds = { x0.Min() - padding_bounds, x0.Max() + padding_bounds, y0.Min() - padding_bounds*1.05f, y0.Max() + padding_bounds*1.05f };
      Vector3[] boundsCoords = { new Vector3(bounds[0], bounds[2], origin), new Vector3(bounds[0], bounds[3], origin), new Vector3(bounds[1], bounds[2], origin), new Vector3(bounds[1], bounds[3], origin) };

      List<OverlayPoint> systemPoints = new List<OverlayPoint>();
      for (int i = 0; i < systemCoords.Length; i++)
      {
        systemPoints.Add(new OverlaySystemPoint(systemCoords[i], origin, bounds));
      }
      for (int i = 0; i < boundsCoords.Length; i++)
      {
        systemPoints.Add(new OverlayPoint(boundsCoords[i], origin));
      }

      Vector3 projectedMean = new Vector3(systemPoints.Average(x => x.coordsProjected[0]),
        systemPoints.Average(x => x.coordsProjected[1]), origin);

      // Sort by angles
      for (int i = 0; i < systemPoints.Count; i++)
      {
        Vector2 upVector = new Vector2(0, 1);
        Vector2 pointVector = new Vector2(systemPoints[i].coordsProjected.x - projectedMean.x, systemPoints[i].coordsProjected.y - projectedMean.y);
        systemPoints[i].sortingAngle = AngleBetween(upVector, pointVector);
      }
      overlayPoints = systemPoints.OrderBy(p => p.sortingAngle).ToList();
    }

    protected float AngleBetween(Vector2 up, Vector2 fwd)
    {
      float ang1 = Mathf.Atan2(up.x, up.y);
      float ang2 = Mathf.Atan2(fwd.x, fwd.y);
      return (ang1 - ang2) % (2f * Mathf.PI);
    }

    public void  SetVisible(bool visible)
    {
      Drawn = visible;
      overlayLine.SetVisible(visible);
    }
    public void Destroy()
    {
      overlayLine.Destroy();
    }
  }

  
}
