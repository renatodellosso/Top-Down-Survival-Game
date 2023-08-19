﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

#nullable enable //We need this for ?
namespace Assets.Scripts.Components.Misc
{
    public class Fadeable : MonoBehaviour
    {

        const float DEFAULT_FADE_DURATION = 0.3f;

        Action? onFadeComplete;

        event Action? OnFadeComplete;

        public void FadeIn(float duration = DEFAULT_FADE_DURATION, Action? onFadeComplete = null)
        {
            //Enable children
            foreach (Transform t in transform.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                if (t != transform)
                    t.gameObject.SetActive(true);
            }

            //Fade in UI
            foreach (Graphic graphic in GetComponentsInChildren<Graphic>())
            {
                graphic.canvasRenderer.SetAlpha(0);
                graphic.CrossFadeAlpha(1, duration, true);
            }

            OnFadeComplete += onFadeComplete;
            Invoke(nameof(FadeComplete), duration);
        }

        public void FadeOut(float duration = DEFAULT_FADE_DURATION, Action? onFadeComplete = null, bool disableAfterwards = true)
        {
            OnFadeComplete += onFadeComplete;

            if (disableAfterwards)
            {
                //Disable children when complete
                OnFadeComplete += () =>
                {
                    foreach (Transform t in transform.GetComponentsInChildren<Transform>())
                    {
                        if (t != transform)
                            t.gameObject.SetActive(false);
                    }
                };
            }
            
            //Fade in UI
            foreach (Graphic graphic in GetComponentsInChildren<Graphic>())
            {
                graphic.CrossFadeAlpha(0, duration, true);
            }

            this.onFadeComplete = onFadeComplete;
            Invoke(nameof(FadeComplete), duration);
        }

        private void FadeComplete()
        {
            OnFadeComplete?.Invoke();
            OnFadeComplete = null;
        }

    }
}
