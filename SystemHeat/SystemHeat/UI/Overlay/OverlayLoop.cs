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
    protected HeatLoop heatLoop;

    public OverlayLoop(HeatLoop loop, Transform overlayRoot, bool startVisible)
    {
      root = overlayRoot;
      heatLoop = loop;
      overlayLine = new OverlayLine(root, loop);
      SetVisible(startVisible);
    }

    public void Update()
    {
      GenerateLoopOverlayPoints();
      UpdatePositions();
      UpdateLoopProperties();
    }
    protected void UpdateLoopProperties()
    {
      overlayLine.UpdateGlow(heatLoop.Temperature, heatLoop.NominalTemperature);
    }
    protected void UpdatePositions()
    {
      List<Vector3[]> positions = overlayPoints.Select(x => x.GetDrawingCoords(SystemHeatSettings.OverlayPadding)).ToList();
      overlayLine.UpdatePositions(positions.SelectMany(i => i).ToArray());
    }

    public void GenerateLoopOverlayPoints()
    {
      // Collect positions of all heat modules
      Vector3[] systemCoords = new Vector3[heatLoop.LoopModules.Count];
      for (int i=0; i < heatLoop.LoopModules.Count; i++)
      {
        systemCoords[i] = heatLoop.LoopModules[i].part.transform.position;
      }

      float[] x0 = null;
      float[] y0 = null;
      float[] z0 = null;

      ArrayUtils.SplitVector3Array(systemCoords, ref x0, ref y0, ref z0);
      float origin = z0.Average();

      // Set up the bounds
      float padding = SystemHeatSettings.OverlayPadding;
      float padding_bounds = SystemHeatSettings.OverlayBoundsPadding;

      float[] bounds = {x0.Min()-padding_bounds, x0.Max()+padding_bounds, y0.Min()-padding_bounds, y0.Max()+padding_bounds};
      Vector3[] boundsCoords = {new Vector3(bounds[0], bounds[2], origin), new Vector3(bounds[0], bounds[3], origin),new Vector3(bounds[1], bounds[2], origin), new Vector3(bounds[1], bounds[3], origin)};

      List<OverlayPoint> systemPoints = new List<OverlayPoint>();
      for (int i=0; i < systemCoords.Length; i++)
      {
        systemPoints.Add(new OverlaySystemPoint(systemCoords[i], origin, bounds));
      }
      for (int i=0; i < boundsCoords.Length; i++)
      {
        systemPoints.Add(new OverlayPoint(boundsCoords[i], origin));
      }

      Vector3 projectedMean = new Vector3(systemPoints.Average(x => x.coordsProjected[0]),
        systemPoints.Average(x => x.coordsProjected[1]), origin);

      // Sort by angles
      for (int i=0; i < systemPoints.Count; i++)
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

  public class OverlayLine
  {

    protected GameObject lineObject;
    protected GameObject glowObject;

    protected LineRenderer glowRenderer;
    protected LineRenderer lineRenderer;

    public OverlayLine(Transform parent, HeatLoop loop)
    {
      lineObject = new GameObject(String.Format("SHOverlay_LineRenderer_{0}", loop.ID));
      lineObject.transform.SetParent(parent, true);
      lineRenderer = lineObject.AddComponent<LineRenderer>();

      // Set up the material
      lineRenderer.material = new Material(Shader.Find(SystemHeatSettings.OverlayLineShader));
      lineRenderer.material.color = Color.white;
      lineRenderer.material.renderQueue = 3000;
      lineObject.layer = 0;

      lineRenderer.positionCount = 2;
      lineRenderer.startWidth = SystemHeatSettings.OverlayBaseLineWidth;
      lineRenderer.endWidth = SystemHeatSettings.OverlayBaseLineWidth;

      glowRenderer = glowObject.AddComponent<LineRenderer>();
      glowObject = new GameObject(String.Format("SHOverlay_GlowRenderer_{0}", loop.ID));
      glowObject.transform.SetParent(parent, true);

      // Set up the material
      glowRenderer.material = new Material(Shader.Find(SystemHeatSettings.OverlayGlowShader));
      glowRenderer.material.color = Color.white;
      glowRenderer.material.renderQueue = 3000;
      glowObject.layer = 0;
      
      glowRenderer.positionCount = 2;
      glowRenderer.startWidth = SystemHeatSettings.OverlayBaseGlowWidth;
      glowRenderer.endWidth = SystemHeatSettings.OverlayBaseGlowWidth;

    }
    public void UpdatePositions(Vector3[] positions)
    {
      glowRenderer.positionCount = positions.Length;
      lineRenderer.positionCount = positions.Length;
    }
    public void UpdateGlow(float currentTemp, float nominalTemp)
    {
      if (currentTemp <= nominalTemp)
      {
        glowRenderer.startColor = Color.green;
        glowRenderer.endColor = Color.green;
      }
      else
      {
        glowRenderer.startColor  = Color.Lerp(Color.green, Color.red, (currentTemp - nominalTemp) / 1000f);
        glowRenderer.endColor = Color.Lerp(Color.green, Color.red, (currentTemp - nominalTemp) / 1000f);
      }
    }

    public void  SetVisible(bool visible)
    {
      glowRenderer.enabled = visible;
      lineRenderer.enabled = visible;
    }
    public void Destroy()
    {
      GameObject.Destroy(glowObject);
      GameObject.Destroy(lineObject);
    }


  }
}
