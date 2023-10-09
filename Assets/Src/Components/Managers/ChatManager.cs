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

        private static ChatManager? instance;

        private readonly List<ChatMessage> messages = new();

        [SerializeField] private TMP_Text? chatLog;

        [SerializeField] private float lastMessageTime = 0;
        private const float LOG_FADE_CHECK_INTERVAL = 2f, LOG_FADE_TIMEOUT = 6f, LOG_FADE_IN_TIME = .5f, LOG_FADE_OUT_TIME = 1f;

        private static readonly LocalChatUser LocalChatUser = new();

        public static ChatMessage? SendMsg(string message)
        {
            Utils.Log($"Attempting to send chat message: {message}");
            try
            {
                if (instance == null) instance = FindAnyObjectByType<ChatManager>();

                if (!instance.IsServer)
                    return null;

                ChatMessage chatMessage = new(message);

                //instance.messages.Add(chatMessage);
                instance.UpdateChatLogClientRpc(chatMessage);

                return chatMessage;
            }
            catch (System.Exception e)
            {
                Utils.Log(e, "Error sending chat message");
                return null;
            }
        }

        public static ChatMessage? SendMsgLocally(string message)
        {
            Utils.Log($"Attempting to send chat message locally: {message}");

            try
            {
                if (instance == null) instance = FindAnyObjectByType<ChatManager>();

                ChatMessage chatMessage = new(LocalChatUser, message);

                instance.messages.Add(chatMessage);
                instance.UpdateChatLog(string.Join("\n", instance.messages));

                return chatMessage;
            }
            catch (System.Exception e)
            {
                Utils.Log(e, "Error sending chat message locally");
                return null;
            }
        }

        // Start is called before the first frame update
        private void Start()
        {
            InvokeRepeating(nameof(CheckIfShouldFadeLog), LOG_FADE_CHECK_INTERVAL, LOG_FADE_CHECK_INTERVAL);
        }

        //ClientRpcs will run on the host (plus any clients)
        [ClientRpc]
        private void UpdateChatLogClientRpc(ChatMessage msg, ClientRpcParams clientRpcParams = default)
        {
            if (instance == null) instance = FindAnyObjectByType<ChatManager>();

            instance.messages.Add(msg);
            instance.UpdateChatLog(string.Join("\n", instance.messages));
        }

        private void CheckIfShouldFadeLog()
        {
            if (Time.time - lastMessageTime > LOG_FADE_TIMEOUT && chatLog!.canvasRenderer.GetAlpha() == 1)
            {
                print("Fading out chat log...");
                chatLog.CrossFadeAlpha(0, LOG_FADE_OUT_TIME, false);
            }
        }

        private void UpdateChatLog(string text)
        {
            print("Updating chat log... Msgs: " + messages.Count);

            if (instance == null) instance = FindAnyObjectByType<ChatManager>();

            chatLog!.text = text;

            chatLog.CrossFadeAlpha(1, LOG_FADE_IN_TIME, false);

            lastMessageTime = Time.time;
        }

    }
}