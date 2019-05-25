using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

using UniRx.Async;

namespace YouTubeLive
{
    
    public class YouTubeLiveClient
    {

        ImageLoader imageLoader;

        public YouTubeLiveAccess access { private set; get; }

        // OAuth URL
        public string AuthUrl => "https://accounts.google.com/o/oauth2/v2/auth?response_type=code"
              + "&client_id=" + access.clientId
              + "&redirect_uri=" + access.redirectUri
              + "&scope=" + access.scope
              + "&access_type=" + access.accessType;


        public YouTubeLiveClient(YouTubeLiveAccess access)
        {

            this.access = access;
            this.imageLoader = new ImageLoader();
        }
        
        public async UniTask<(string access, string reflesh)> GetToken()
        {
            var url = "https://www.googleapis.com/oauth2/v4/token";

            var content = new Dictionary<string, string>() {
                { "code", access.code },
                { "client_id", access.clientId },
                { "client_secret", access.clientSecret },
                { "redirect_uri", access.redirectUri },
                { "grant_type", access.grantType },
                { "access_type", access.accessType }
            };

            var text = await Post(url, content);
            var json = JSON.Parse(text);
            var accessToken = json["access_token"].RawString();
            var refreshToken = json["refresh_token"].RawString();

            return (accessToken, refreshToken);

        }

        public async UniTask<string> GetLiveChatId(string id)
        {
            var url = "https://www.googleapis.com/youtube/v3/liveBroadcasts?part=snippet";
            url += "&id=" + id;

            var text = await Get(url);

            var json = JSON.Parse(text);
            var chatId = json["items"][0]["snippet"]["liveChatId"].RawString();

            return chatId;
        }

        public async UniTask<Chat> GetChatMessages(string chatId, string pageToken)
        {
            var url = "https://www.googleapis.com/youtube/v3/liveChat/messages?part=snippet,authorDetails";
            url += "&liveChatId=" + chatId;
            url += pageToken == "" ? "" : "&pageToken=" + pageToken;

            var text = await Get(url);

            var chat = new Chat();
            
            // Live Streaming API : Live Chat Messages
            // https://developers.google.com/youtube/v3/live/docs/liveChatMessages#snippet.superChatDetails.tier
            var json = JSON.Parse(text);

            var items = json["items"];

            foreach (var item in items)
            {

                var snip = item.Value["snippet"];
                var author = item.Value["authorDetails"];
                var superChatDetails = snip["superChatDetails"];


                var commentType = superChatDetails["amountMicros"].AsLong > 0 ? CommentType.SuperChat : CommentType.Normal;

                Chat.SuperChatDetails scd = null;
                if (commentType == CommentType.SuperChat)
                {
                    scd = new Chat.SuperChatDetails()
                    {
                        amount = (int)superChatDetails["amountMicros"].AsLong / 1000000,
                        currency = superChatDetails["currency"].RawString(),
                        comment = superChatDetails["userComment"].RawString(),
                        amountDispStr = superChatDetails["amountDisplayString"].RawString(),
                        tier = superChatDetails["tier"].AsInt
                    };
                }

                var msg = new Chat.Comment()
                {
                    type = commentType,
                    comment = commentType == CommentType.SuperChat ? scd.comment : snip["displayMessage"].RawString(),
                    name = author["displayName"].RawString(),
                    img = await imageLoader.LoadTextureAsync(author["profileImageUrl"].RawString()),
                    superChatDetails = scd
                };

                chat.msgs.Add(msg);
            }

            var next = json["nextPageToken"];
            chat.pageToken = next;

            return chat;
        }

        #region WebUtility
        async UniTask<string> Get(string uri)
        {
            var content = new Dictionary<string, string>() {
                { "Authorization", "Bearer " + access.token }
            };

            return await Get(uri, content);
        }

        async UniTask<string> Get(string uri, Dictionary<string, string> content)
        {
            UnityWebRequest request = UnityWebRequest.Get(uri);
            foreach (var d in content)
            {
                request.SetRequestHeader(d.Key, d.Value);
            }

            await request.SendWebRequest();

            if (request.isNetworkError)
            {
                Debug.LogError("Get: [Network Error]");
                return null;
            }
            else if (request.isHttpError)
            {
                Debug.LogError($"get: [Http Error] {request.downloadHandler.text}");
                return null;
            }
            else
            {
                return request.downloadHandler.text;
            }
        }

        async UniTask<string> Post(string uri, Dictionary<string, string> content)
        {
            UnityWebRequest request = UnityWebRequest.Post(uri, content);
            await request.SendWebRequest();

            if (request.isNetworkError)
            {
                Debug.LogError("Post: [Network Error]");
                return null;
            }
            else if (request.isHttpError)
            {
                Debug.LogError($"Post: [Http Error] {request.downloadHandler.text}");
                return null;
            }
            else
            {
                return request.downloadHandler.text;
            }
        }
        #endregion
    }
}