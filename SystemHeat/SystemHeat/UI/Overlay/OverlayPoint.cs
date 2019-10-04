using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SystemHeat;
using KSP.Localization;

namespace SystemHeat.UI
{
  /// <summary>
  /// Represents a single point on the loop overlay
  /// </summary>
  public class OverlayPoint
  {
    public Vector3 coords { get; set; }
    public Vector3 coordsProjected { get; set; }
    public float sortingAngle  { get; set; }

    public OverlayPoint(Vector3 coordinates, float depth)
    {
      coords = coordinates;
      coordsProjected = new Vector3(coordinates[0], coordinates[1], depth);
    }

    /// <summary>
    /// Gets the drawing coordinates for the point with a padding
    /// </summary>
    public virtual Vector3[] GetDrawingCoords(float padding)
    {
      Vector3[] coordsList = new Vector3[1] ;
      coordsList[0] = coords;
      return coordsList;
    }
  }

  /// <summary>
  /// Represents a single influx point on the loop overlay
  /// </summary>
  public class OverlaySystemPoint : OverlayPoint
  {

    protected Vector3[] bounds;
    protected int closestBounds;
    protected float projectedDepth;

    public OverlaySystemPoint(Vector3 coordinates, float depth, float[] bounds):base(coordinates, depth)
    {
      coords = coordinates;
      closestBounds = FindNearestBounds(bounds, coords);
      projectedDepth = depth;

      switch (closestBounds)
      {
        case 0:
          coordsProjected = new Vector3(bounds[0], coords[1], projectedDepth);
          break;
        case 1:
          coordsProjected = new Vector3(bounds[1], coords[1], projectedDepth);
          break;
        case 2:
          coordsProjected = new Vector3(coords[0], bounds[2], projectedDepth);
          break;
        case 3:
          coordsProjected = new Vector3(coords[0], bounds[3], projectedDepth);
          break;
      }
    }
    /// <summary>
    /// Gets the drawing coordinates for the point with a padding
    /// </summary>
    public override Vector3[] GetDrawingCoords(float padding)
    {
      Vector3[] coordsList = new Vector3[4];

      if (closestBounds == 0 || closestBounds == 3)
        padding = -padding;

      if (closestBounds < 2)
      {
        coordsList[0] = coordsProjected + new Vector3(0, padding, 0);
        coordsList[3] = coordsProjected + new Vector3(0, -padding, 0);

        coordsList[1] = coords + new Vector3(0, padding, 0);
        coordsList[2] = coords + new Vector3(0, -padding, 0);
      } else
      {
        coordsList[0] = coordsProjected + new Vector3(padding, 0, 0);
        coordsList[3] = coordsProjected + new Vector3(-padding, 0, 0);

        coordsList[1] = coords + new Vector3(padding, 0, 0);
        coordsList[2] = coords +  new Vector3(-padding, 0, 0);
      }
      return coordsList;
    }

    /// <summary>
    /// Finds an integer representing the nearest boundaries to the point
    /// </summary>
    protected int FindNearestBounds(float[] bounds, Vector3 point)
    {
      float[] boundsDelta = {Mathf.Abs(point[0] - bounds[0]), Mathf.Abs(point[0] - bounds[1]), Mathf.Abs(point[1] - bounds[2]), Mathf.Abs(point[1] - bounds[3])};
      int idx = 0;
      float counter = 999999f;
      for (int i =0 ; i < boundsDelta.Length; i++)
      {
        if (boundsDelta[i] < counter)
        {
          idx = i;
          counter = boundsDelta[i];
        }
      }
      return idx;
    }
  }


}
