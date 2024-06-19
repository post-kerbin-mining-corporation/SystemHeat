using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SystemHeat.UI
{
  public class ToolbarLoops
  {

    public GameObject loopPanel;
    public GameObject loopPanelScrollRoot;
    public RectTransform loopPanelScrollBackground;
    public RectTransform loopPanelScrollCarat;
    public RectTransform loopPanelScrollRootRect;
    public RectTransform loopPanelScrollViewportRect;

    protected RectTransform loopPanelRootRect;
    protected RectTransform basePanelRootRect;
    protected List<ToolbarPanelLoopWidget> loopPanelWidgets;
    protected bool shown = false;

    public void Initialize(Transform root)
    {

      loopPanelWidgets = new List<ToolbarPanelLoopWidget>();

      //loopsTitle = root.FindDeepChild("LoopsHeaderText").GetComponent<Text>();
      //noLoopsText = root.FindDeepChild("NoLoopText").GetComponent<Text>();
      loopPanel = root.FindDeepChild("PanelLoops").gameObject;
      loopPanelRootRect = loopPanel.GetComponent<RectTransform>();
      basePanelRootRect = root.FindDeepChild("PanelBase").GetComponent<RectTransform>();

      loopPanelScrollRoot = root.FindDeepChild("Scrolly").gameObject;
      loopPanelScrollRootRect = root.FindDeepChild("Scrolly").GetComponent<RectTransform>();
      loopPanelScrollViewportRect = root.FindDeepChild("ScrollViewPort").GetComponent<RectTransform>();

      loopPanelScrollCarat = root.FindDeepChild("LoopCarat").GetComponent<RectTransform>();
      loopPanelScrollBackground = root.FindDeepChild("LoopBackground").GetComponent<RectTransform>();
      SetVisible(shown);
    }

    public void Update(SystemHeatSimulator simulator)
    {
      // Turn on the no loops text if there are no loops
      if (simulator.HeatLoops.Count == 0)
      {
        if (loopPanelWidgets.Count > 0)
        {
          DestroyLoopWidgets();
        }

        //if (!noLoopsText.gameObject.activeSelf)
        //  noLoopsText.gameObject.SetActive(true);

      }
      else
      {

        PollLoopWidgets(simulator);
        //if (noLoopsText.gameObject.activeSelf)
        //  noLoopsText.gameObject.SetActive(false);


      }
    }
    void DestroyLoopWidgets()
    {
      for (int i = loopPanelWidgets.Count - 1; i >= 0; i--)
      {
        GameObject.Destroy(loopPanelWidgets[i].gameObject);
      }
      loopPanelWidgets.Clear();
    }

    void PollLoopWidgets(SystemHeatSimulator simulator)
    {

      loopPanelScrollBackground.gameObject.SetActive(!simulator.HeatLoops.Count == 0);
      loopPanelScrollCarat.gameObject.SetActive(!simulator.HeatLoops.Count == 0);

      for (int i = loopPanelWidgets.Count - 1; i >= 0; i--)
      {
        if (!simulator.HasLoop(loopPanelWidgets[i].TrackedLoopID))
        {
          GameObject.Destroy(loopPanelWidgets[i].gameObject);
          loopPanelWidgets.RemoveAt(i);
          RecalculatePanelPositionData();
        }
      }
      foreach (HeatLoop loop in simulator.HeatLoops)
      {
        bool generateWidget = true;
        for (int i = loopPanelWidgets.Count - 1; i >= 0; i--)
        {
          if (loopPanelWidgets[i].TrackedLoopID == loop.ID)
          {
            generateWidget = false;
          }
        }

        if (generateWidget)
        {
          Utils.Log("[UI]: Generating a new loop widget", LogType.UI);
          GameObject newObj = (GameObject)GameObject.Instantiate(SystemHeatUILoader.ToolbarPanelLoopPrefab, Vector3.zero, Quaternion.identity);
          newObj.transform.SetParent(loopPanelScrollRoot.transform, false);
          //newWidget.transform.localPosition = Vector3.zero;
          ToolbarPanelLoopWidget newWidget = newObj.AddComponent<ToolbarPanelLoopWidget>();
          newWidget.AssignSimulator(simulator);
          newWidget.SetLoop(loop.ID);
          newWidget.SetVisible(true);
          loopPanelWidgets.Add(newWidget);
          RecalculatePanelPositionData();
        }
      }
    }
    protected void RecalculatePanelPositionData()
    {
      LayoutRebuilder.ForceRebuildLayoutImmediate((loopPanelRootRect));
      Utils.Log("Recalculating Loop Area position data", LogType.UI);
      VerticalLayoutGroup vlg = loopPanelScrollRootRect.GetComponent<VerticalLayoutGroup>();

      // TODO: don't hardcode
      float buttonYOffsetFromTop = -120;
      float loopPanelMaxHeight = basePanelRootRect.sizeDelta.y;
      float widgetTotalHeight = vlg.preferredHeight;

      Utils.Log($"Source data: buttonYOffsetFromTop={buttonYOffsetFromTop }, loopPanelMaxHeight={loopPanelMaxHeight} widgetTotalHeight={widgetTotalHeight}", LogType.UI);
      //loopPanelWidgets.Count * 68f + vlg.padding.top+vlg.padding.bottom+ 3f+7.5f*(loopPanelWidgets.Count-1);
      loopPanelRootRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, loopPanelMaxHeight);
      loopPanelScrollViewportRect.anchorMin = loopPanelScrollBackground.anchorMin = loopPanelScrollCarat.anchorMin = new Vector2(0, 0);
      loopPanelScrollViewportRect.anchorMax = loopPanelScrollBackground.anchorMax = loopPanelScrollCarat.anchorMax = new Vector2(1, 1);
     
      loopPanelScrollCarat.anchoredPosition = new Vector2(-11, buttonYOffsetFromTop);

      if (widgetTotalHeight < loopPanelMaxHeight)
      {
        Utils.Log($"Setting scroll viewport rect position anchors to {new Vector2(0, buttonYOffsetFromTop + widgetTotalHeight / 2f) }");
        /// there will be no scrolling, set the position of the scroll rect to the top and have fun
        loopPanelScrollViewportRect.anchoredPosition = new Vector2(0, buttonYOffsetFromTop + widgetTotalHeight / 2f);
        loopPanelScrollViewportRect.sizeDelta = loopPanelScrollBackground.sizeDelta = new Vector2(195, widgetTotalHeight);
        loopPanelScrollBackground.anchoredPosition = Vector2.zero;
        loopPanelScrollBackground.offsetMin = new Vector2(loopPanelScrollBackground.offsetMin.x,
          loopPanelMaxHeight - widgetTotalHeight);
      }
      else
      {
        /// set height to max
        loopPanelScrollViewportRect.anchoredPosition = loopPanelScrollBackground.anchoredPosition = new Vector2(0, 0);
        loopPanelScrollViewportRect.sizeDelta = loopPanelScrollBackground.sizeDelta = new Vector2(195, loopPanelMaxHeight);
      }
    }

    public void SetVisible(bool visibility)
    {
      loopPanel.SetActive(visibility);
    }
    public void ToggleLoopPanel()
    {
      shown = !shown;
      SetVisible(shown);
    }
    public void SetOverlayVisible()
    {
      foreach (ToolbarPanelLoopWidget widget in loopPanelWidgets)
      {
        SystemHeatOverlay.Instance.SetVisible(widget.OverlayState, widget.TrackedLoopID);
      }
    }
    public bool GetOverlayLoopVisibility(int id)
    {
      foreach (ToolbarPanelLoopWidget widget in loopPanelWidgets)
      {
        if (widget.TrackedLoopID == id)
          return widget.OverlayState;
      }
      return false;
    }
  }
}
