using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Src.Components.Misc
{
    [RequireComponent(typeof(TMP_Text))]
    [DisallowMultipleComponent]
    public class PerformanceDisplay : MonoBehaviour
    {

        private TMP_Text text;
        private NetworkManager networkManager;

        const float UPDATE_INTERVAL = 0.5f;

        // Start is called before the first frame update
        private void Start()
        {
            text = GetComponent<TMP_Text>();
            networkManager = NetworkManager.Singleton;

            InvokeRepeating(nameof(UpdateStats), 0, UPDATE_INTERVAL);
        }

        private void UpdateStats()
        {

            int framerate = (int)(1f / Time.unscaledDeltaTime);

            text.text = $"{framerate} FPS";

            if (networkManager == null)
                return;
            
            if(networkManager.IsHost)
            {
                text.text += $"\nClients: {networkManager.ConnectedClientsList.Count}";
            }
            else if (networkManager.IsClient && !networkManager.IsHost)
            {
                ulong pingTime = 0;
                try
                {
                    //GetCurrentRtt gives the ping time in milliseconds to whatever client id is passed in
                    pingTime = networkManager.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.ServerClientId);
                }
                catch { }

                text.text += $"\n{pingTime}ms";
            }
        }

    }
}