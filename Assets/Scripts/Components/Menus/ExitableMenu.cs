using Assets.Scripts.Components.Menus;
using Assets.Scripts.Components.Misc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Components.Menus
{
    public class ExitableMenu : Fadeable
    {

        public Fadeable previousMenu;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        /// <summary>
        /// Remember to call base.Update() in derived classes
        /// </summary>
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                Back();
            }
        }

        public void Back()
        {
            FadeOut(onFadeComplete: () => previousMenu.FadeIn());
        }
    }
}