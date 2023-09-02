using Assets.Src.Components.Menus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Src
{
    public static class StartGameManager
    {

        static StartGameLoadingScreen loadingScreen;

        public static IEnumerator StartGame(StartGameLoadingScreen loadingScreen, bool multiplayer)
        {
            StartGameManager.loadingScreen = loadingScreen;
            loadingScreen.Text.text = "Loading...";

            yield return new WaitForEndOfFrame();
        }

    }
}