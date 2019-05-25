using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

using UniRx.Async;

namespace YouTubeLive
{
    public class Chat
    {
        [System.Serializable]
        public class SuperChatDetails
        {
            public int amount;
            public string currency;
            public string comment;
            public string amountDispStr;
            public int tier;
        }

        [System.Serializable]
        public class Comment
        {
            public enum CommentType { Normal, SuperChat }

            public CommentType type;
            public string name;
            public Texture img;
            public string comment;
            public SuperChatDetails superChatDetails;
        }

        public List<Comment> msgs = new List<Comment>();
        public string pageToken;
    }

    [Serializable]
    public class YouTubeLiveAccess
    {
        [Header("YouTube")]
        public string id = "_video_uri";

        [Header("OAuth")]
        public string clientId = "_client_id";
        public string clientSecret = "_client_secret";

        [Header("Options")]
        public string redirectUri = "http://localhost:8080";
        public string grantType = "authorization_code";
        public string accessType = "offline";
        public string scope = "https://www.googleapis.com/auth/youtube.readonly";
        public string code = "";
        public string token = "";
    }

    public class YouTubeLiveClient
    {

        ImageLoader imageLoader;

        public YouTubeLiveClient()
        {
            imageLoader = new ImageLoader();
        }

        public YouTubeLiveAccess access { set; get; }

        public string AuthUrl()
        {
            return "https://accounts.google.com/o/oauth2/v2/auth?response_type=code"
              + "&client_id=" + access.clientId
              + "&redirect_uri=" + access.redirectUri
              + "&scope=" + access.scope
              + "&access_type=" + access.accessType;
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


                var commentType = superChatDetails["amountMicros"].AsLong > 0 ? Chat.Comment.CommentType.SuperChat : Chat.Comment.CommentType.Normal;

                Chat.SuperChatDetails scd = null;
                if (commentType == Chat.Comment.CommentType.SuperChat)
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
                    comment = scd != null ? scd.comment : snip["displayMessage"].RawString(),
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

    }

    public static class SimpleJsonUtility
    {
        public static string RawString(this JSONNode node)
        {
            var len = node.ToString().Length - 2;
            return node.ToString().Substring(1, len);
        }
    }
}