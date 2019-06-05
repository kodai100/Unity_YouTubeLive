using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UniRx.Async;
using UnityEngine.Events;

namespace YouTubeLive
{
    [System.Serializable]
    public class OnMessageEvent : UnityEvent<Chat.Comment> { }

    public class YouTubeLiveController : MonoBehaviour
    {

        [SerializeField]
        YouTubeLiveAccess access;

        [SerializeField]
        float interval = 3f;
        
        public OnMessageEvent OnMessage;

        async void Start()
        {

            var client = new YouTubeLiveClient(access);

            using (var server = new YouTubeLiveServer(code => { access.code = code; }))
            {
                // OAuth access, and get authorization code
                if (access.code == "")
                {
                    server.Start();
                    Application.OpenURL(client.AuthUrl);
                    await UniTask.WaitUntil(() => access.code != "");
                }
            }

            // get access token
            if (access.token == "")
            {
                (access.token, _) = await client.GetToken();
            }


            // get chat id from YoutubeLive ID
            var chatId = "";
            chatId = await client.GetLiveChatId(access.id);


            // get chat message
            var pageToken = "";

            while (true)
            {
                var chat = await client.GetChatMessages(chatId, pageToken);

                foreach (var msg in chat.msgs)
                {
                    OnMessage.Invoke(msg);
                }
                pageToken = chat.pageToken;

                await UniTask.Delay((int)(interval * 1000));
            }
        }
        

    }

}