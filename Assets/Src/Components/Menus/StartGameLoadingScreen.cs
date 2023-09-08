using Assets.Src.Components.Misc;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

#nullable enable
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
                if (StartGameManager.Instance != null)
                    StartCoroutine(StartGameManager.Instance?.StartGameSecondary(this));
                else StartGameFromGameScene();
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// Starts the game (will transition to the game scene)
        /// </summary>
        /// <param name="loadSaveFile">Whether to load the save file that SaveManager is currently set to</param>
        /// <param name="multiplayer">Whether the game is starting in multiplayer mode</param>
        /// <param name="joinCode">If a value is passed, will use that join code to attempt to join a relay allocation. If not passed, will host the session</param>
        public static void StartGame(bool loadSaveFile, bool multiplayer, string? joinCode = null)
        {
            print($"Starting game... Load Save File: {loadSaveFile}, Multiplayer: {multiplayer}, Join Code: {joinCode}");

            if(instance == null) instance = FindAnyObjectByType<StartGameLoadingScreen>(FindObjectsInactive.Include);

            instance.FadeIn(onFadeComplete: () => {
                StartGameManager startGameManager = new();
                instance.StartCoroutine(startGameManager.StartGameInitial(instance, loadSaveFile, multiplayer, joinCode));
            });
        }

        static void StartGameFromGameScene()
        {
            print("Starting game from game scene...");
            
            string[] saveFiles = SaveManager.GetSaveFileNames();

            if(saveFiles.Length == 0)
            {
                Debug.LogError("No save files found!");
                return;
            }

            SaveManager.SaveName = saveFiles[0];

            StartGame(true, true);
        }
    }
}