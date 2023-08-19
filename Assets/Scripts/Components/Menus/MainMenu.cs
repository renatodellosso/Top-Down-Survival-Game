using Assets.Scripts.Components.Misc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Components.Menus
{
    public class MainMenu : Fadeable
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// Runs after the intro finishes
        /// </summary>
        public void Enter()
        {
            FadeIn();
        }

        public void Singleplayer()
        {
            FadeOut();
        }

        public void HostGame()
        {
            FadeOut();
        }

        public void JoinGame()
        {
            FadeOut();
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}