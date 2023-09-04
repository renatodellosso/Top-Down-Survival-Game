using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Src.Components.Menus
{
    public class NewGameMenu : ExitableMenu
    {

        public bool multiplayer = false;

        public StartGameLoadingScreen startGameLoadingScreen;

        //NOTE: TMP_InputField, not InputField
        TMP_InputField nameInput, worldSizeInput, chunkSizeInput;
        TMP_Dropdown worldSizeDropdown, chunkSizeDropdown;
        Button confirmButton, playButton;

        TMP_Text worldGenerationLog;

        Image worldGenerationDisplay;

        const int MIN_WORLD_SIZE = 20, MAX_WORLD_SIZE = 1000;

        readonly Dictionary<string, int> WORLD_SIZE_PRESETS = new()
        {
            { "Small", 100 },
            { "Medium", 200 },
            { "Large", 300 },
            { "Custom", 200 }
        };

        int WorldSize
        {
            get
            {
                try
                {
                    return int.Parse(worldSizeInput.text);
                }
                catch
                {
                    return WORLD_SIZE_PRESETS["Medium"];
                }
            }
            set
            {
                print($"Setting world size to {value}...");

                worldSizeInput.SetTextWithoutNotify(value.ToString());

                //If the value is not a preset, set the dropdown to custom
                if (!WORLD_SIZE_PRESETS.ContainsValue(WorldSize))
                {
                    worldSizeDropdown.SetValueWithoutNotify(WORLD_SIZE_PRESETS.Count - 1);
                    worldSizeDropdown.RefreshShownValue();
                }
                else
                {
                    worldSizeDropdown.SetValueWithoutNotify(WORLD_SIZE_PRESETS.Values.ToList().IndexOf(WorldSize));
                    worldSizeDropdown.RefreshShownValue();
                }
            }
        }

        const int MIN_CHUNK_SIZE = 5, MAX_CHUNK_SIZE = 50;

        readonly Dictionary<string, int> CHUNK_SIZE_PRESETS = new()
        {
            { "Small", 10 },
            { "Medium", 20 },
            { "Large", 30 },
            { "Custom", 20 }
        };

        int ChunkSize
        {
            get
            {
                return int.Parse(chunkSizeInput.text);
            }
            set
            {
                print($"Setting chunk size to {value}...");

                chunkSizeInput.SetTextWithoutNotify(value.ToString());

                //If the value is not a preset, set the dropdown to custom
                if (!CHUNK_SIZE_PRESETS.ContainsValue(ChunkSize))
                {
                    chunkSizeDropdown.SetValueWithoutNotify(CHUNK_SIZE_PRESETS.Count - 1);
                    chunkSizeDropdown.RefreshShownValue();
                }
                else
                {
                    chunkSizeDropdown.SetValueWithoutNotify(CHUNK_SIZE_PRESETS.Values.ToList().IndexOf(ChunkSize));
                    chunkSizeDropdown.RefreshShownValue();
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            //Get inputs
            IEnumerable<TMP_InputField> inputs = GetComponentsInChildren<TMP_InputField>();
            nameInput = inputs.Where(i => i.contentType == TMP_InputField.ContentType.Alphanumeric).First();
            inputs = inputs.Where(i => i.contentType == TMP_InputField.ContentType.IntegerNumber); //Switch to only alphanumeric inputs
            worldSizeInput = inputs.First();
            chunkSizeInput = inputs.Last();

            //Get dropdowns
            IEnumerable<TMP_Dropdown> dropdowns = GetComponentsInChildren<TMP_Dropdown>();
            worldSizeDropdown = dropdowns.First();
            chunkSizeDropdown = dropdowns.Last();

            //Get buttons
            IEnumerable<Button> buttons = GetComponentsInChildren<Button>();
            confirmButton = buttons.Where(c => c.name == "Confirm").First();
            playButton = buttons.Where(c => c.name == "Play").First();

            //Get text
            worldGenerationLog = GetComponentsInChildren<TMP_Text>().Where(t => t.name == "Log").First();

            //Set up world size dropdown
            worldSizeDropdown.ClearOptions();
            worldSizeDropdown.AddOptions(new List<string>(WORLD_SIZE_PRESETS.Keys));
            worldSizeDropdown.SetValueWithoutNotify(1);
            worldSizeDropdown.RefreshShownValue();
            worldSizeDropdown.onValueChanged.AddListener(OnWorldSizeDropdownChanged);

            //Set up world size input
            worldSizeInput.onValueChanged.AddListener(OnWorldSizeInputChanged);

            //Init world size
            WorldSize = WORLD_SIZE_PRESETS.Values.ElementAt(worldSizeDropdown.value);
            worldSizeInput.SetTextWithoutNotify(WorldSize.ToString());

            //Set up chunk size dropdown
            chunkSizeDropdown.ClearOptions();
            chunkSizeDropdown.AddOptions(new List<string>(CHUNK_SIZE_PRESETS.Keys));
            chunkSizeDropdown.SetValueWithoutNotify(1);
            chunkSizeDropdown.RefreshShownValue();
            chunkSizeDropdown.onValueChanged.AddListener(OnChunkSizeDropdownChanged);

            //Set up chunk size input
            chunkSizeInput.onValueChanged.AddListener(OnChunkSizeInputChanged);

            //Init chunk size
            ChunkSize = CHUNK_SIZE_PRESETS.Values.ElementAt(chunkSizeDropdown.value);
            chunkSizeInput.SetTextWithoutNotify(ChunkSize.ToString());

            //Set up button
            confirmButton.onClick.AddListener(StartGeneration);

            //Set up world generation display
            worldGenerationDisplay = GetComponentsInChildren<Image>().Where(i => i.name == "World Map Display").First();

            //Set up play button
            playButton.onClick.AddListener(PlayGame);
            playButton.interactable = false;

            //Reset world generation log
            worldGenerationLog.text = "";
        }

        void OnWorldSizeDropdownChanged(int value)
        {
            WorldSize = WORLD_SIZE_PRESETS.Values.ElementAt(value);
        }

        void OnWorldSizeInputChanged(string value)
        {
            try
            {
                WorldSize = Math.Clamp(int.Parse(value), MIN_WORLD_SIZE, MAX_WORLD_SIZE);
            }
            catch
            {
                WorldSize = MIN_WORLD_SIZE;
                return;
            }
        }

        void OnChunkSizeDropdownChanged(int value)
        {
            ChunkSize = CHUNK_SIZE_PRESETS.Values.ElementAt(value);
        }

        void OnChunkSizeInputChanged(string value)
        {
            try
            {
                ChunkSize = Math.Clamp(int.Parse(value), MIN_CHUNK_SIZE, MAX_CHUNK_SIZE);
            }
            catch
            {
                ChunkSize = MIN_CHUNK_SIZE;
                return;
            }
        }

        void StartGeneration()
        {
            if (nameInput.text == "")
                nameInput.text = "Unnamed World";

            print("Starting world generation...");
            worldGenerationLog.text = "Starting world generation...\n";

            //Set settings to not interactable
            SetInteractables(false);

            WorldGeneration.WorldGeneration.StartGenerationAsync(WorldSize);

            //Start coroutine to update the display
            StartCoroutine(WhileWorldGenerating());
        }

        void SetInteractables(bool useable)
        {
            confirmButton.interactable = useable;

            nameInput.interactable = useable;

            worldSizeDropdown.interactable = useable;
            worldSizeInput.interactable = useable;

            chunkSizeDropdown.interactable = useable;
            chunkSizeInput.interactable = useable;

            playButton.interactable = useable;
        }

        IEnumerator FlashWorldNameInput()
        {
            for (int i = 0; i < 3; i++)
            {
                nameInput.interactable = false;
                yield return new WaitForSeconds(0.1f);
                nameInput.interactable = true;
                yield return new WaitForSeconds(0.1f);
            }
        }

        IEnumerator WhileWorldGenerating()
        {
            do
            {
                UpdateWorldMapDisplay();
                Utils.Log($"World Generation Task Complete: {WorldGeneration.WorldGeneration.task.IsCompleted}, Successful: {WorldGeneration.WorldGeneration.task.IsCompletedSuccessfully}");
                yield return new WaitForSeconds(0.5f);
            }
            while (!WorldGeneration.WorldGeneration.task.IsCompleted);

            UpdateWorldMapDisplay();
            OnWorldGenerationComplete();
        }

        void UpdateWorldMapDisplay()
        {
            //Convert the world map to an array of colors
            Color32[,] worldMapDisplayColors = WorldGeneration.WorldGeneration.GetDisplayColors();
            Color32[] mapColors = new Color32[worldMapDisplayColors.GetLength(0) * worldMapDisplayColors.GetLength(1)];

            for (int x = 0; x < worldMapDisplayColors.GetLength(0); x++)
                for (int y = 0; y < worldMapDisplayColors.GetLength(1); y++)
                    //Set the color at the index to the color at the x and y position
                    // x * worldMapDisplayColors.GetLength(0) adds the x offset for that row
                    mapColors[(x * worldMapDisplayColors.GetLength(0)) + y] = worldMapDisplayColors[x, y];

            //Create a texture from the colors
            Texture2D texture = new(worldMapDisplayColors.GetLength(0), worldMapDisplayColors.GetLength(1), TextureFormat.ARGB32, false);
            texture.SetPixels32(mapColors, 0);
            texture.filterMode = FilterMode.Point;

            //worldGenerationDisplay.material.mainTexture = texture;

            //We have to make a new sprite for the texture
            worldGenerationDisplay.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));

            texture.Apply(); //Upload the changes to the GPU
        }

        void OnWorldGenerationComplete()
        {
            Utils.Log($"World Generation Complete: {WorldGeneration.WorldGeneration.task.IsCompleted}, Successful: {WorldGeneration.WorldGeneration.task.IsCompletedSuccessfully}");
            worldGenerationLog.text += "World generation complete!\n";

            //Save the world
            SaveWorld();

            //Set settings to interactable
            SetInteractables(true);
            playButton.interactable = false; //Don't let the user play until the world is saved
        }

        void SaveWorld()
        {
            worldGenerationLog.text += "Saving world...\n";

            //Set the save name
            string saveName = nameInput.text;
            if (SaveManager.SaveExists(saveName))
            {
                int counter = 0;
                while (SaveManager.SaveExists(saveName))
                {
                    counter++;
                    saveName = $"{nameInput.text} ({counter})";
                }
            }

            worldGenerationLog.text += $"Save name: {saveName}\n";

            SaveManager.saveName = saveName;

            worldGenerationLog.text += "Saving world data...\n";

            //Save the world
            CancellableTask saveTask = SaveManager.SaveWorld();
            StartCoroutine(WaitForSavingToComplete(saveTask));
        }

        IEnumerator WaitForSavingToComplete(Task saveTask)
        {
            while(!saveTask.IsCompleted)
                yield return new WaitForSeconds(0.5f);

            worldGenerationLog.text += "World saved!\n";

            //Set play button to interactable
            playButton.interactable = true;
        }

        void PlayGame()
        {
            print("Starting game...");
            StartGameLoadingScreen.instance = startGameLoadingScreen;
            FadeOut(onFadeComplete: () => StartGameLoadingScreen.StartGame(multiplayer));
        }

    }
}