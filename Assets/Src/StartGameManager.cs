using Assets.Src.Components.Menus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Src
{
    public static class StartGameManager
    {

        static StartGameLoadingScreen loadingScreen;

        static bool multiplayer;

        static NetworkManager networkManager;
        static UnityTransport unityTransport, relayTransport;

        static string LoadingMessage { set { loadingScreen.Text.text = value; } }

        public static IEnumerator StartGameInitial(StartGameLoadingScreen loadingScreen, bool loadSaveFile, bool multiplayer)
        {
            Utils.Log($"Starting game (initial)... Load Save File: {loadSaveFile}, Multiplayer: {multiplayer}");

            StartGameManager.loadingScreen = loadingScreen;
            loadingScreen.Text.text = "Loading...";

            StartGameManager.multiplayer = multiplayer;

            yield return new WaitForEndOfFrame();

            if(loadSaveFile)
            {
                LoadingMessage = "Loading save file...";

                Task loadTask = SaveManager.LoadGameAsync();

                int iterator = 0;
                while (!loadTask.IsCompleted)
                {
                    iterator++;
                    iterator %= 4;

                    loadingScreen.Text.text = "Loading save file" + new string('.', iterator);

                    yield return new WaitForSeconds(1f);
                }
            }

            LoadingMessage = "Changing scenes...";
            yield return new WaitForEndOfFrame();

            SceneManager.LoadScene("Game");
        }

        public static IEnumerator StartGameSecondary(StartGameLoadingScreen loadingScreen)
        {
            Utils.Log($"Starting game (secondary)... Multiplayer: {multiplayer}");

            StartGameManager.loadingScreen = loadingScreen;
            LoadingMessage = "Loading...";

            yield return new WaitForEndOfFrame();

            InitNetworkManager();

            yield return new WaitForEndOfFrame();

            StartHost();
        }
        
        static void InitNetworkManager()
        {
            Utils.Log("Initializing network manager...");
            LoadingMessage = "Initializing network manager...";

            networkManager = GameObject.FindAnyObjectByType<NetworkManager>();

            //Find transports
            IEnumerable<UnityTransport> transports = networkManager.GetComponents<UnityTransport>();
            unityTransport = transports.Where(t => t.Protocol == UnityTransport.ProtocolType.UnityTransport).First();
            relayTransport = transports.Where(t => t.Protocol == UnityTransport.ProtocolType.RelayUnityTransport).First();
        }

        static void StartHost()
        {
            if(multiplayer) StartRelayTransport();
            else StartUnityTransport();
        }

        static void StartUnityTransport()
        {
            Utils.Log("Starting UnityTransport...");
            LoadingMessage = "Starting UnityTransport...";
        }

        static void StartRelayTransport()
        {
            Utils.Log("Starting RelayTransport...");
            LoadingMessage = "Starting RelayTransport...";
        }

    }
}