using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Components.Menus
{
    public class MainMenu : MonoBehaviour
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
            //Fade in UI
            foreach (Graphic graphic in GetComponentsInChildren<Graphic>())
            {
                graphic.canvasRenderer.SetAlpha(0);
                graphic.CrossFadeAlpha(1, 1, true);
            }
        }

        public void Singleplayer()
        {

        }

        public void HostGame()
        {

        }

        public void JoinGame()
        {

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