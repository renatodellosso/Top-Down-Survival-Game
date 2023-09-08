using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

#nullable enable
namespace Assets.Src.Components.Menus
{
    public class SaveSelectionMenu : ExitableMenu
    {

        public NewGameMenu newGameMenu;

        public StartGameLoadingScreen startGameLoadingScreen;

        public bool multiplayer = false;

        public Transform saveFileButtonParent;
        [SerializeField] AssetReferenceGameObject buttonReference;

        Task<GameObject>? buttonLoadTask;

        // Start is called before the first frame update
        void Start()
        {
            newGameMenu = transform.parent.GetComponentInChildren<NewGameMenu>(includeInactive: true);
            newGameMenu.previousMenu = this;


            buttonLoadTask = buttonReference.LoadAssetAsync().Task;
        }

        public void NewGame()
        {
            newGameMenu.multiplayer = multiplayer;
            FadeOut(onFadeComplete: () => newGameMenu.FadeIn());
        }

        void FixedUpdate()
        {
            if(buttonLoadTask != null && buttonLoadTask.IsCompleted)
            {
                buttonLoadTask = null;
                PopulateSaveFiles(buttonLoadTask!);
            }
        }

        void PopulateSaveFiles(Task<GameObject> task)
        {
            Utils.Log("Populating save files...");

            GameObject button = (GameObject)buttonReference.Asset;

            //Load save files
            string[] saveFiles = SaveManager.GetSaveFileNames();

            //Create buttons for each save file
            foreach (string saveFile in saveFiles)
            {
                GameObject newButton = Instantiate(button, saveFileButtonParent);
                newButton.GetComponentInChildren<TMP_Text>().text = saveFile;
                newButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => LoadGame(saveFile));
            }
        }

        public void LoadGame(string saveFile)
        {
            Utils.Log($"Loading save file {saveFile}...");

            SaveManager.SaveName = saveFile;

            StartGameLoadingScreen.instance = startGameLoadingScreen;

            FadeOut(onFadeComplete: () => StartGameLoadingScreen.StartGame(loadSaveFile: true, multiplayer));
        }
    }
}