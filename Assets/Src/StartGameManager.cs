using Assets.Src.Components.Managers;
using Assets.Src.Components.Menus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

#nullable enable
namespace Assets.Src
{
    public sealed class StartGameManager
    {

        public static StartGameManager? Instance { get; private set; }

        StartGameLoadingScreen? loadingScreen;

        bool multiplayer;

        NetworkManager? networkManager;
        UnityTransport? unityTransport, relayTransport;

        string LoadingMessage {
            set {
                if (loadingScreen != null)
                    loadingScreen.Text.text = value;
            }
        }

        string? joinCode;

        /// <summary>
        /// How long to wait between each dot in loading messages
        /// </summary>
        const float DOT_INTERVAL = .5f;

        public StartGameManager()
        {
            Instance = this;
        }

        /// <summary>
        /// Frees up the resources used by the StartGameManager. Deletes the instance.
        /// </summary>
        public static void CleanUp()
        {
            Instance = null;
        }

        public IEnumerator StartGameInitial(StartGameLoadingScreen loadingScreen, bool loadSaveFile, bool multiplayer, string? joinCode)
        {
            Utils.Log($"Starting game (initial)... Load Save File: {loadSaveFile}, Multiplayer: {multiplayer}");

            this.loadingScreen = loadingScreen;
            loadingScreen.Text.text = "Loading...";

            this.multiplayer = multiplayer;

            this.joinCode = joinCode;

            yield return new WaitForEndOfFrame();

            if (loadSaveFile)
            {
                LoadingMessage = "Loading save file...";

                Task loadTask = SaveManager.LoadGameAsync();

                int iterator = 0;
                while (!loadTask.IsCompleted)
                {
                    iterator++;
                    iterator %= 4;

                    loadingScreen.Text.text = "Loading save file" + new string('.', iterator);

                    yield return new WaitForSeconds(DOT_INTERVAL);
                }
            }

            LoadingMessage = "Changing scenes...";
            yield return new WaitForEndOfFrame();

            SceneManager.LoadScene("Game");
        }

        public IEnumerator StartGameSecondary(StartGameLoadingScreen loadingScreen)
        {
            Utils.Log($"Starting game (secondary)... Multiplayer: {multiplayer}");

            this.loadingScreen = loadingScreen;
            LoadingMessage = "Loading...";

            yield return new WaitForEndOfFrame();

            InitNetworkManager();

            yield return new WaitForEndOfFrame();

            Task<bool> networkStartTask;
            if (joinCode == null) networkStartTask = StartHost();
            else networkStartTask = StartClient();

            int iterator = 1;
            while (!networkManager!.IsClient && !networkManager.IsHost)
            {
                iterator++;
                iterator %= 4;

                LoadingMessage = loadingScreen.Text.text.Replace(".", "") + new string('.', iterator);

                yield return new WaitForSeconds(DOT_INTERVAL);
            }

            if(!networkStartTask.Result)
            {
                LoadingMessage = "Failed to start network";
                yield break;
            }

            //Set up the game world
            ChatManager.Send("Game loaded!");
            loadingScreen.FadeOut(onFadeComplete: CleanUp);
        }
        
        void InitNetworkManager()
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

        async Task<bool> StartHost()
        {
            if(multiplayer) return await StartRelayHostAsync();
            else return StartUnityTransport();
        }

        bool StartUnityTransport()
        {
            Utils.Log("Starting UnityTransport...");
            LoadingMessage = "Starting UnityTransport...";

            //Get rid of the relay transport
            GameObject.Destroy(relayTransport);

            //Set the network transport to the unity transport
            networkManager!.NetworkConfig.NetworkTransport = unityTransport;

            networkManager!.StartHost();

            return true;
        }

        async Task InitRelayTransportAsync()
        {
            Utils.Log("Initting RelayTransport...");
            LoadingMessage = "Initting RelayTransport...";

            //Get rid of the unity transport
            GameObject.Destroy(unityTransport);

            //Set the network transport to the relay transport
            networkManager!.NetworkConfig.NetworkTransport = relayTransport;

            //Initialize Unity Services
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();

            //Authenticate user anonymously
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        async Task<bool> StartRelayHostAsync()
        {
            await InitRelayTransportAsync();

            Utils.Log("Starting relay host...");
            LoadingMessage = "Starting relay host";

            //Create allocation
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(Config.Network.MAX_PLAYERS_WHILE_HOSTING);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            //Set relay server data
            RelayServerData relayServerData = new(allocation, "dtls");
            relayTransport!.SetRelayServerData(relayServerData);

            networkManager!.StartHost();

            LoadingMessage = "Started relay host";
            return true;
        }

        async Task<bool> StartClient()
        {
            await InitRelayTransportAsync();

            Utils.Log("Starting client...");
            LoadingMessage = "Joining allocation";

            try
            {
                //Join allocation
                JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode!);

                //Set relay server data
                RelayServerData relayServerData = new(allocation, "dtls");
                relayTransport!.SetRelayServerData(relayServerData);

                //Start client
                LoadingMessage = "Starting client";
                networkManager!.StartClient();
            }
            catch (Exception e)
            {
                Utils.Log(e, $"Failed to join allocation");
                LoadingMessage = $"Failed to join allocation: {e.Message}";
                return false;
            }

            Utils.Log("Joined allocation");
            LoadingMessage = "Joined allocation";
            return true;
        }

        void OnServerStarted()
        {
            Utils.Log($"Server started!{(multiplayer ? " Join Code: " + joinCode : "")}");
            LoadingMessage = "Server started!";

            ChatManager.Send("Server started!");
            if (joinCode != null)
                ChatManager.Send($"Your session join code is {Utils.FormatText(joinCode, "yellow")}");
        }

    }
}