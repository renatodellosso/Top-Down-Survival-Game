using Assets.Scripts.Components.Misc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Components.Menus
{
    public class SaveSelectionMenu : Fadeable
    {

        public MainMenu mainMenu;

        public bool multiplayer = false;

        // Start is called before the first frame update
        void Start()
        {
            FadeOut(0);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Back()
        {
            FadeOut(onFadeComplete: () => mainMenu.FadeIn());
        }

        public void NewGame()
        {

        }
    }
}