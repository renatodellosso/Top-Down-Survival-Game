using Assets.Src.Components.Misc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Src.Components.Menus
{
    public class SaveSelectionMenu : ExitableMenu
    {

        public NewGameMenu newGameMenu;

        public bool multiplayer = false;

        // Start is called before the first frame update
        void Start()
        {
            newGameMenu = transform.parent.GetComponentInChildren<NewGameMenu>(includeInactive: true);
            newGameMenu.previousMenu = this;
        }

        public void NewGame()
        {
            FadeOut(onFadeComplete: () => newGameMenu.FadeIn());
        }
    }
}