using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Vectrosity;
using KSP.UI;

namespace SystemHeat.UI
{
  public class OverlayLine
  {
    Color lineColor;
    VectorLine line;

    public OverlayLine(Transform parent, HeatLoop loop)
    {
      Utils.Log($"[OverlayLine]: building line for loop ID {loop.ID}");
      

      line = new VectorLine($"SystemHeat_Loop{loop.ID}_VectorLine", new List<Vector3>(), SystemHeatSettings.OverlayBaseLineWidth, LineType.Continuous, Joins.Weld);

      VectorLine.canvas.renderMode = RenderMode.ScreenSpaceCamera;
      VectorLine.canvas.sortingLayerName = UIMasterController.Instance.appCanvas.sortingLayerName;
      VectorLine.canvas.sortingLayerID = UIMasterController.Instance.appCanvas.sortingLayerID;
      VectorLine.canvas.planeDistance = 200f;
      VectorLine.canvas.worldCamera = UIMasterController.Instance.uiCamera;
      VectorLine.canvas.gameObject.SetLayerRecursive(5);

      lineColor = SystemHeatSettings.GetLoopColor(loop.ID);
    }
    public void UpdatePositions(List<Vector3> positions)
    {
      Utils.Log(positions[0].ToString());
      Utils.Log(positions[1].ToString());
      Utils.Log(positions[2].ToString());
      Utils.Log(positions[3].ToString());
      line.points3 = positions;
      line.SetColor(lineColor);
    }

    public void Draw()
    {
      line.Draw3D();
    }
    public void UpdateGlow(float currentTemp, float nominalTemp)
    {
      if (currentTemp <= nominalTemp)
      {

      }
      else
      {
      }
    }

    public void SetVisible(bool visible)
    {
      line.active = visible;
    }
    public void Destroy()
    {
      VectorLine.Destroy(ref line);
    }


  }
}
