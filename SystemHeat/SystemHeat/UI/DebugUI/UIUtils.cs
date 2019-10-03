using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DynamicBatteryStorage.UI
{
  public static class UIUtils
  {
    public static void IconDataField(Rect uiRect, AtlasIcon icon, string value, GUIStyle dataStyle)
    {
      Color color = GUI.color;
      IconDataField(uiRect, icon, value, dataStyle, color);
      GUI.color = color;
    }
    public static void IconDataField(Rect uiRect, AtlasIcon icon, string value, GUIStyle dataStyle, Color color)
    {
      Color oldColor = GUI.color;
      GUI.color = color;
      GUI.BeginGroup(uiRect);
      Rect iconRect = new Rect(0, 0, uiRect.height, uiRect.height);
      Rect dataRect = new Rect(0, 0, uiRect.width, uiRect.height);

      GUI.DrawTextureWithTexCoords(iconRect, icon.iconAtlas, icon.iconRect);
      GUI.Label(dataRect, value, dataStyle);
      GUI.EndGroup();
      GUI.color = oldColor;
    }


  }
}
