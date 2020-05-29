using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SystemHeat.UI
{
  public class OverlayPanel:MonoBehaviour
  {
    public bool active = true;
    public bool panelOpen = false;
    public RectTransform rect;
    public GameObject infoPanel;
    public GameObject icon;

    public Image colorRing;
    public Image colorRingAnimated;
    public Image systemIcon;
    public Image systemIconBackground;
    public Image heatIcon;
    public Image heatIconBackground;
    public Image heatIconGlow;

    public Button iconButton;

    public Text infoPanelTitle;
    public Text infoPanelUpperText;
    public Text infoPanelLowerText;

    public ModuleSystemHeat heatModule;
    public Canvas parentCanvas;
    protected HeatLoop loop;

    public void Awake()
    {
      // Find all the components
      rect = this.GetComponent<RectTransform>();
      icon = transform.FindDeepChild("Icon").gameObject;
      infoPanel = transform.FindDeepChild("InfoPanel").gameObject;

      colorRing = transform.FindDeepChild("IconRing").GetComponent<Image>();
      colorRingAnimated = transform.FindDeepChild("IconRingSpin").GetComponent<Image>();

      colorRingAnimated.gameObject.AddComponent<ImageRotateAnimator>();

      systemIcon = transform.FindDeepChild("IconForeground").GetComponent<Image>();
      heatIconBackground = transform.FindDeepChild("FireButton").GetComponent<Image>();
      heatIcon = transform.FindDeepChild("ThermoIcon").GetComponent<Image>();
      heatIconGlow = transform.FindDeepChild("FireGlow").GetComponent<Image>();
      heatIconGlow.gameObject.AddComponent<ImageFadeAnimator>();

      systemIconBackground = transform.FindDeepChild("InfoIconBackground").GetComponent<Image>();
      iconButton = transform.FindDeepChild("InfoIconBackground").GetComponent<Button>();

      infoPanelTitle = transform.FindDeepChild("InfoPanelTitleText").GetComponent<Text>();
      infoPanelUpperText = transform.FindDeepChild("InfoPanelTopText").GetComponent<Text>();
      infoPanelLowerText = transform.FindDeepChild("InfoPanelBottomText").GetComponent<Text>();

      iconButton.onClick.AddListener(delegate { OnButtonClick(); });
      SetPanel(panelOpen);

      heatIcon.enabled = false;
    }

    public void OnButtonClick()
    {
      SetPanel(!panelOpen);
    }

    public void Update()
    {
      if (active && loop != null && heatModule != null)
      {
        if (panelOpen)
        {
          infoPanelUpperText.text = $"Temperature Output {heatModule.systemNominalTemperature} K \nHeat Output {heatModule.totalSystemFlux} kW";
          infoPanelLowerText.text = $"<b>Loop Status</b>\n Nominal Temperature {loop.NominalTemperature} K \n Net Flux {loop.NetFlux} kW";
        }

        if (loop.NominalTemperature > heatModule.systemNominalTemperature && !heatIcon.enabled)
        {
          heatIcon.enabled = true;
          heatIconGlow.enabled = true;
        }
        if (loop.NominalTemperature <= heatModule.systemNominalTemperature && heatIcon.enabled)
        {
          heatIcon.enabled = false;
          heatIconGlow.enabled = false;
        }

        Vector2 screenPoint = Camera.main.WorldToScreenPoint(heatModule.part.transform.position);
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.GetComponent<RectTransform>(), screenPoint, parentCanvas.worldCamera, out localPoint);
        transform.localPosition = localPoint;
      }
      
    }

    public Vector3 worldToUISpace(Canvas parentCanvas, Vector3 worldPos)
    {
      //Convert the world for screen point so that it can be used with ScreenPointToLocalPointInRectangle function
      Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
      Vector2 movePos;

      //Convert the screenpoint to ui rectangle local point
      RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, screenPos, parentCanvas.worldCamera, out movePos);
      //Convert the local point to world point
      return parentCanvas.transform.TransformPoint(movePos);
    }

    public void SetupLoop(HeatLoop lp, ModuleSystemHeat sh)
    {
      loop = lp;
      heatModule = sh;

      infoPanelTitle.text = heatModule.part.partInfo.title;
      colorRing.color = SystemHeatSettings.GetLoopColor(loop.ID);
      Transform xform = transform.FindDeepChild(heatModule.iconName);
      if (xform != null)
      {
        systemIcon.sprite = xform.GetComponent<Image>().sprite;
      }
      SetVisibility(true);
    }

    public void SetPanel(bool state)
    {
      infoPanel.SetActive(state);
      panelOpen = state;
    }
    public void SetVisibility(bool state)
    {
      active = state;
      icon.SetActive(state);
      if (!state)
        SetPanel(state);
    }
    

  }
}
