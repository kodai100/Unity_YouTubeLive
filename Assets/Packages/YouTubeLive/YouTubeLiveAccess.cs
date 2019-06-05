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
        
        [HideInInspector]
        public string redirectUri = "http://localhost:8080";

        [HideInInspector]
        public string grantType = "authorization_code";

        [HideInInspector]
        public string accessType = "offline";

        [HideInInspector]
        public string scope = "https://www.googleapis.com/auth/youtube.readonly";

        [HideInInspector]
        public string code = "";

        [HideInInspector]
        public string token = "";
    }

}