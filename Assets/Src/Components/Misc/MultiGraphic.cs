using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Src.Components.Misc
{
    public class MultiGraphic : Graphic
    {
        public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha)
        {
            if (targetColor.Equals(Color.white) && !color.Equals(targetColor)) return;

            color = new Color(targetColor.r, targetColor.g, targetColor.b, 0);

            //print("Fading multiple graphics to " + targetColor + ", duration: " + duration);


            IEnumerable<Graphic> graphics = transform.GetComponentsInChildren<Graphic>().Where(g => g.CompareTag("Transition") ||
                (g.transform.parent != null && g.transform.parent.CompareTag("Transition")));
            foreach (var graphic in graphics)
            {
                graphic.CrossFadeColor(targetColor, duration, true, true);
            }
        }

        protected override void OnEnable()
        {
            //print("Enabling multiple graphics");
            CrossFadeColor(new Color(color.r, color.g, color.b, 1), 0, true, true);
        }
    }
}