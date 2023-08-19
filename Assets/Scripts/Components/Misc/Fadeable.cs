using System;
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

        const float DEFAULT_FADE_DURATION = 1;

        Action? onFadeComplete;

        public void FadeIn(float duration = DEFAULT_FADE_DURATION, Action? onFadeComplete = null)
        {
            //Enable children
            foreach (Transform t in transform.GetComponentsInChildren<Transform>())
            {
                if (t != transform)
                    t.gameObject.SetActive(true);
            }

            //Fade in UI
            foreach (Graphic graphic in GetComponentsInChildren<Graphic>())
            {
                graphic.canvasRenderer.SetAlpha(0);
                graphic.CrossFadeAlpha(1, 1, true);
            }

            this.onFadeComplete = onFadeComplete;
            Invoke(nameof(FadeComplete), duration);
        }

        public void FadeOut(float duration = DEFAULT_FADE_DURATION, Action? onFadeComplete = null)
        {
            //Disable children when complete
            onFadeComplete ??= () =>
            {
                foreach(Transform t in transform.GetComponentsInChildren<Transform>())
                {
                    if(t != transform)
                        t.gameObject.SetActive(false);
                }
            };
            
            //Fade in UI
            foreach (Graphic graphic in GetComponentsInChildren<Graphic>())
            {
                graphic.CrossFadeAlpha(0, 1, true);
            }

            this.onFadeComplete = onFadeComplete;
            Invoke(nameof(FadeComplete), duration);
        }

        private void FadeComplete()
        {
            onFadeComplete?.Invoke();
        }

    }
}
