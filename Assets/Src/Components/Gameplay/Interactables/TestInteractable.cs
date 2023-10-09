using Assets.Src.Components.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Src.Components.Gameplay
{
    public class TestInteractable : Interactable
    {

        protected override string GetTooltipVerb() => "test the interactable";

        protected override void OnInteract()
        {
            print($"Interacted with {name}");
        }
    }
}
