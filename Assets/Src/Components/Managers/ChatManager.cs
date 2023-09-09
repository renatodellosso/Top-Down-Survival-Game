using Assets.Src.Chat;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

#nullable enable
namespace Assets.Src.Components.Managers
{
    public class ChatManager : NetworkBehaviour
    {

        static ChatManager? instance;

        readonly List<ChatMessage> messages = new();

        [SerializeField] TMP_Text chatLog;

        [SerializeField] float lastMessageTime = 0;
        const float LOG_FADE_CHECK_INTERVAL = 2f, LOG_FADE_TIMEOUT = 3f, LOG_FADE_IN_TIME = .5f, LOG_FADE_OUT_TIME = 1f;

        public static ChatMessage? Send(string message)
        {
            Utils.Log($"Attempting to send chat message: {message}");

            if (instance == null) instance = FindAnyObjectByType<ChatManager>();

            if (!instance.IsServer)
                return null;

            ChatMessage chatMessage = new(message);

            instance.messages.Add(chatMessage);
            instance.UpdateChatLogClientRpc(string.Join("\n", instance.messages));

            return chatMessage;
        }

        // Start is called before the first frame update
        void Start()
        {
            InvokeRepeating(nameof(CheckIfShouldFadeLog), LOG_FADE_CHECK_INTERVAL, LOG_FADE_CHECK_INTERVAL);
        }
        
        //ClientRpcs will run on the host (plus any clients)
        [ClientRpc]
        void UpdateChatLogClientRpc(string text)
        {
            print("Updating chat log...");
            
            if (instance == null) instance = FindAnyObjectByType<ChatManager>();

            chatLog.text = text;

            chatLog.CrossFadeAlpha(1, LOG_FADE_IN_TIME, false);

            lastMessageTime = Time.time;
            print(lastMessageTime);
        }

        void CheckIfShouldFadeLog()
        {
            if(Time.time - lastMessageTime > LOG_FADE_TIMEOUT && chatLog.canvasRenderer.GetAlpha() == 1)
            {
                print("Fading out chat log...");
                chatLog.CrossFadeAlpha(0, LOG_FADE_OUT_TIME, false);
            }
        }
    }
}