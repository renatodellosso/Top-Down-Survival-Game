using Assets.Src.Components.Base;
using Assets.Src.Components.Managers;
using Assets.Src.Components.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Src.Components.Gameplay.Interactables
{
    public class MenuOpener : Interactable
    {

        [SerializeField] private string tooltipVerb;
        [SerializeField] private ExitableMenu menu;

        protected override string GetTooltipVerb() => tooltipVerb;

        protected override void OnInteract()
        {
            print($"Opening menu: {menu.name}...");
            GameObject newMenu = Instantiate(menu.gameObject, Utils.GetMainCanvas().transform);
            Fadeable fadeable = newMenu.GetComponent<Fadeable>();

            newMenu.SetActive(false);
            fadeable.FadeIn();
        }

    }
}
