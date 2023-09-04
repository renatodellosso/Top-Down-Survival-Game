using Assets.Src.Components.Misc;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Src.Components.Menus
{
    public class StartGameLoadingScreen : Fadeable
    {

        public static StartGameLoadingScreen instance;

        public TMP_Text Text { protected set; get; }

        // Start is called before the first frame update
        void Start()
        {
            instance = this;

            Text = GetComponentInChildren<TMP_Text>();

            if (SceneManager.GetActiveScene().name == "Game")
                StartCoroutine(StartGameManager.StartGameSecondary(this));
        }

        // Update is called once per frame
        void Update()
        {

        }

        public static void StartGame(bool multiplayer)
        {
            print($"Starting game... Multiplayer: {multiplayer}");

            instance.FadeIn(onFadeComplete: () => {
                instance.StartCoroutine(StartGameManager.StartGameInitial(instance, multiplayer));
            });
        }
    }
}