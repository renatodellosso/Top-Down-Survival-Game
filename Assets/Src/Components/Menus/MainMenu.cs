using Assets.Src.Components.Misc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Src.Components.Menus
{
    public class MainMenu : Fadeable
    {

        public SaveSelectionMenu saveSelectionMenu;

        // Start is called before the first frame update
        void Start()
        {
            saveSelectionMenu = transform.parent.GetComponentInChildren<SaveSelectionMenu>(includeInactive: true);
            saveSelectionMenu.previousMenu = this;
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
            saveSelectionMenu.multiplayer = false;
            TransitionToSaveSelectionMenu();
        }

        public void HostGame()
        {
            saveSelectionMenu.multiplayer = true;
            TransitionToSaveSelectionMenu();
        }

        public void JoinGame()
        {
            FadeOut();
        }

        public void Quit()
        {
            FadeOut(onFadeComplete: () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
        }

        void TransitionToSaveSelectionMenu()
        {
            FadeOut(onFadeComplete: () => saveSelectionMenu.FadeIn());
        }
    }
}