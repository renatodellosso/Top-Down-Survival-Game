using Assets.Src.Components.Base;
using UnityEngine;

namespace Assets.Src.Components.Menus
{
    public class MainMenu : Fadeable
    {

        SaveSelectionMenu saveSelectionMenu;
        JoinGameMenu joinGameMenu;

        // Start is called before the first frame update
        void Start()
        {
            saveSelectionMenu = transform.parent.GetComponentInChildren<SaveSelectionMenu>(includeInactive: true);
            saveSelectionMenu.previousMenu = this;

            joinGameMenu = transform.parent.GetComponentInChildren<JoinGameMenu>(includeInactive: true);
            joinGameMenu.previousMenu = this;
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
            FadeOut(onFadeComplete: () => joinGameMenu.FadeIn());
        }

        public void Quit()
        {
            FadeOut(onFadeComplete: () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                UnityEngine.Application.Quit();
#endif
            });
        }

        void TransitionToSaveSelectionMenu()
        {
            FadeOut(onFadeComplete: () => saveSelectionMenu.FadeIn());
        }
    }
}