using Assets.Src.Components.Menus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Src
{
    public static class StartGameManager
    {

        static StartGameLoadingScreen loadingScreen;

        static bool multiplayer;

        public static IEnumerator StartGameInitial(StartGameLoadingScreen loadingScreen, bool multiplayer)
        {
            Utils.Log($"Starting game (initial)... Multiplayer: {multiplayer}");

            StartGameManager.loadingScreen = loadingScreen;
            loadingScreen.Text.text = "Loading...";

            StartGameManager.multiplayer = multiplayer;

            yield return new WaitForEndOfFrame();
            
            loadingScreen.Text.text = "Changing scenes...";
            yield return new WaitForEndOfFrame();

            SceneManager.LoadScene("Game");
        }

        public static IEnumerator StartGameSecondary(StartGameLoadingScreen loadingScreen)
        {
            Utils.Log($"Starting game (secondary)... Multiplayer: {multiplayer}");

            StartGameManager.loadingScreen = loadingScreen;
            loadingScreen.Text.text = "Loading...";

            yield return new WaitForEndOfFrame();
        }

    }
}