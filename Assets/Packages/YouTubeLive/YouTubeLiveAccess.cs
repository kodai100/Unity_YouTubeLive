using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouTubeLive
{

    [System.Serializable]
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

}