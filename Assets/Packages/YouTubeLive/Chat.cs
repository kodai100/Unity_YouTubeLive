using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouTubeLive
{
    
    public enum CommentType { Normal, SuperChat }

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
            public CommentType type;
            public string name;
            public Texture img;
            public string comment;
            public SuperChatDetails superChatDetails;
        }

        public List<Comment> msgs = new List<Comment>();
        public string pageToken;
    }

}