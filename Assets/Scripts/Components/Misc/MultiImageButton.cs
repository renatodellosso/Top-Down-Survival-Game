using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Components.Misc
{
    public class MultiImageButton : Button
    {
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            Color targetColor =
                state == SelectionState.Disabled ? colors.disabledColor :
                state == SelectionState.Highlighted ? colors.highlightedColor :
                state == SelectionState.Normal ? colors.normalColor :
                state == SelectionState.Pressed ? colors.pressedColor :
                state == SelectionState.Selected ? colors.selectedColor : Color.white;

            IEnumerable<Graphic> graphics = transform.GetComponentsInChildren<Graphic>().Where(g => g.tag == "Transition" || g.transform.parent.tag == "Transition");
            foreach (var graphic in graphics)
            {
                graphic.CrossFadeColor(targetColor, instant ? 0f : colors.fadeDuration, true, true);
            }
        }
    }
}