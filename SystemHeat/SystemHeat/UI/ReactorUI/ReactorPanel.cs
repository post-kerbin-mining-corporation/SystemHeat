using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using KSP.Localization;
using KSP.UI;

namespace SystemHeat.UI
{
  public class ReactorPanel : MonoBehaviour
  {
    public RectTransform rect;
    public Transform scrollRoot;
    public Text windowTitle;
    public Text noReactorsObject;


    private List<PartModule> reactorModules;
    private List<ReactorWidget> reactorWidgets;

    public void Awake()
    {

      FindComponents();
    }
    private void FindComponents()
    {
      reactorWidgets = new List<ReactorWidget>();
      reactorModules = new List<PartModule>();
      rect = this.transform as RectTransform;
      scrollRoot = transform.FindDeepChild("PanelScroll");
      windowTitle = transform.FindDeepChild("PanelTitleText").GetComponent<Text>();
      noReactorsObject = transform.FindDeepChild("NoReactorsObject").GetComponent<Text>();

      Localize();
    }

    void Localize()
    {
      windowTitle.text = Localizer.Format("#LOC_SystemHeat_ReactorPanel_Title");
      noReactorsObject.text = Localizer.Format("#LOC_SystemHeat_ReactorPanel_NoReactors");
    }
    public void SetVisible(bool state)
    {
      rect.gameObject.SetActive(state);
    }
    public void ClearReactors()
    {
      if (reactorModules != null)
        reactorModules.Clear();
      if (reactorWidgets != null)
        for (int i = reactorWidgets.Count - 1; i >= 0; i--)
        {
          if (reactorWidgets[i] != null)
            Destroy(reactorWidgets[i].gameObject);
        }
    }
    public void AddReactor(PartModule pm)
    {
      if (rect == null)
        FindComponents();

      noReactorsObject.gameObject.SetActive(false);
      if (!reactorModules.Contains(pm))
      {
        reactorModules.Add(pm);

        GameObject newWidget = (GameObject)Instantiate(SystemHeatUILoader.ReactorWidgetPrefab, Vector3.zero, Quaternion.identity);
        newWidget.transform.SetParent(scrollRoot);
        newWidget.transform.localPosition = Vector3.zero;
        ReactorWidget w = newWidget.AddComponent<ReactorWidget>();
        w.SetReactor(pm);
        reactorWidgets.Add(w);
      }

    }

    public void FixedUpdate()
    {

    }
  }


}
