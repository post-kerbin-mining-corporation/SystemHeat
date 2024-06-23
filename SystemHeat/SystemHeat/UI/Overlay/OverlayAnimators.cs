using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SystemHeat.UI
{
  public class ImageRotateAnimator : MonoBehaviour
  {
    public bool Animate { get; set; }
    public float SpinRate = 100f;

    protected RectTransform xform;
    // Use this for initialization
    void Start()
    {
      xform = this.transform as RectTransform;
    }


    // Update is called once per frame
    void Update()
    {
      if (xform != null)
      {
        if (Animate)
        {
          xform.Rotate(Vector3.forward, Time.deltaTime * SpinRate, Space.Self);
        }
      }
    }
  }
  public class ImageFadeAnimator : MonoBehaviour
  {
    public float rate = 2f;
    protected Image image;
    protected Color c;
    int direction = 1;
    // Use this for initialization
    void Start()
    {
      image = this.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
      if (image)
      {

        c = image.color;

        c.a += rate * direction * Time.deltaTime;


        if (c.a <= 0.0f)
          direction = 1;
        if (c.a >= 1.0f)
          direction = -1;




        image.color = c;
      }
    }
  }
}
