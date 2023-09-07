using Assets.Src.Components.Menus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

#nullable enable
namespace Assets.Src
{
    public static class StartGameManager
    {

        static StartGameLoadingScreen? loadingScreen;

        static bool multiplayer;

        static NetworkManager? networkManager;
        static UnityTransport? unityTransport, relayTransport;

        static string LoadingMessage { set { loadingScreen.Text.text = value; } }

        static bool serverStarted = false;

        static string? joinCode;

        public static IEnumerator StartGameInitial(StartGameLoadingScreen loadingScreen, bool loadSaveFile, bool multiplayer, string? joinCode)
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

            int iterator = 1;
            while(!serverStarted)
            {
                iterator++;
                iterator %= 4;

                LoadingMessage = loadingScreen.Text.text.Replace(".", "") + new string('.', iterator);

                yield return new WaitForSeconds(.5f);
            }

            //Set up the game world

            loadingScreen.FadeOut();
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

            networkManager.OnServerStarted += OnServerStarted;
        }

        static void StartHost()
        {
            if(multiplayer) StartRelayTransportAsync();
            else StartUnityTransport();
        }

        static void StartUnityTransport()
        {
            Utils.Log("Starting UnityTransport...");
            LoadingMessage = "Starting UnityTransport...";

            //Get rid of the relay transport
            GameObject.Destroy(relayTransport);

            //Set the network transport to the unity transport
            networkManager!.NetworkConfig.NetworkTransport = unityTransport;

            networkManager!.StartHost();
        }

        static async void StartRelayTransportAsync()
        {
            Utils.Log("Starting RelayTransport...");
            LoadingMessage = "Starting RelayTransport...";


            //Get rid of the unity transport
            GameObject.Destroy(unityTransport);

            //Set the network transport to the relay transport
            networkManager!.NetworkConfig.NetworkTransport = relayTransport;

            //Initialize Unity Services
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();

            //Authenticate user anonymously
            if(!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            //Create allocation
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(Config.Network.MAX_PLAYERS_WHILE_HOSTING);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            networkManager!.StartHost();
        }

        static void OnServerStarted()
        {
            Utils.Log($"Server started!{(multiplayer ? " Join Code: " + joinCode : "")}");
            LoadingMessage = "Server started!";

            serverStarted = true;
        }

    }
}