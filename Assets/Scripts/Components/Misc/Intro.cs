using Components.Menus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Components.Misc
{
    public class Intro : MonoBehaviour
    {
        static bool introPlayed = false;

        float speed = 1f;
        const float INITIAL_WAIT = 0.5f, FADE_IN_DURATION = 1.25f, TEXT_WAIT = 0.75f, FADE_OUT_DURATION = 1f;

        public MainMenu menu;

        void Awake()
        {
            print("Intro awake");
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            if(introPlayed)
                Destroy(gameObject);
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
                speed *= 2;
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