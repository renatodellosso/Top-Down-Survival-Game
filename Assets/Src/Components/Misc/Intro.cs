using Assets.Src.Components.Menus;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Multiplayer.Playmode;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;

namespace Assets.Src.Components.Misc
{
    public class Intro : MonoBehaviour
    {
        static bool introPlayed = false;

        float speed = 1f;
        const float INITIAL_WAIT = 0.5f, FADE_IN_DURATION = 1.25f, TEXT_WAIT = 0.75f, FADE_OUT_DURATION = 1f;

        public MainMenu menu;

        string playerTag;

        void Awake()
        {
            playerTag = CurrentPlayer.ReadOnlyTag();
            print("Player Tag: " + playerTag);

            if (playerTag == "Untagged")
                StandardIntro();
            else if (playerTag == "SkipIntro")
                SkipIntro();
            else if (playerTag == "StartHostImmediately")
                StartHostImmediately();
            else
            {
                Debug.LogError("Unknown player tag: " + playerTag);
                StandardIntro();
            }
        }

        void StandardIntro()
        {
            print("Running standard intro...");
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
        }

        void SkipIntro()
    {
            print("Skipping main menu...");
            
            menu.gameObject.SetActive(true);
            menu.Enter();
            Destroy(gameObject);
        }

        void StartHostImmediately()
        {
            print("Starting host immediately...");

            Destroy(gameObject);

            //Load most recent save
            string[] saves = SaveManager.GetSaveFileNames();
            if(saves.Length > 0)
            {
                SaveManager.SaveName = saves.First();
                StartGameLoadingScreen.StartGame(true, true);
            }
            else
            {
                Debug.LogError("No save files found");
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            if (introPlayed)
            {
                menu.FadeIn();
                Destroy(gameObject);
            }
            else
            {
                print("Starting intro...");

                menu.gameObject.SetActive(false);

                //Set alpha to 0
                Graphic[] graphics = GetComponentsInChildren<Graphic>();
                foreach (Graphic graphic in graphics)
                {
                    graphic.canvasRenderer.SetAlpha(0);
                }

                Invoke(nameof(FadeIn), INITIAL_WAIT / speed);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.anyKeyDown)
            {
                speed *= 2;
                FadeOut();
            }
        }

        void FadeIn()
        {
            Graphic[] graphics = GetComponentsInChildren<Graphic>();
            foreach (Graphic graphic in graphics)
            {
                graphic.CrossFadeAlpha(1, FADE_IN_DURATION / speed, true);
            }

            Invoke(nameof(FadeOut), (FADE_IN_DURATION + TEXT_WAIT) / speed);
        }

        void FadeOut()
        {
            Graphic[] graphics = GetComponentsInChildren<Graphic>();
            foreach (Graphic graphic in graphics)
            {
                graphic.CrossFadeAlpha(0, FADE_OUT_DURATION / speed, true);
            }

            Invoke(nameof(FinishIntro), FADE_OUT_DURATION / speed);
        }

        void FinishIntro()
        {
            print("Intro complete");

            menu.gameObject.SetActive(true);
            menu.Enter();

            introPlayed = true;
            Destroy(gameObject);
        }
    }
}