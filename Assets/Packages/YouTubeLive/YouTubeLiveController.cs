﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UniRx.Async;

namespace YouTubeLive
{
    [RequireComponent(typeof(YouTubeLiveServer))]
    public class YouTubeLiveController : MonoBehaviour
    {

        [SerializeField]
        YouTubeLiveAccess access;

        [SerializeField]
        float interval = 3f;

        public event Action<Chat.Comment> OnMessage;

        async void Start()
        {

            OnMessage += _ => { };

            var client = new YouTubeLiveClient(access);

            // OAuth access, and get authorization code
            if (access.code == "")
            {
                var server = GetComponent<YouTubeLiveServer>();
                
                // 認証待ち
                server.Listen();
                server.OnReceiveCode += code => {
                    access.code = code;
                };

                // Webで認証
                Application.OpenURL(client.AuthUrl);
                
                await UniTask.WaitUntil(() => access.code != "");

                server.Stop();
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
                    OnMessage(msg);
                }
                pageToken = chat.pageToken;

                await UniTask.Delay((int)(interval * 1000));
            }
        }

    }

}