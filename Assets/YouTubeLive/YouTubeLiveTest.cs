using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouTubeLive
{
    public enum CommentType
    {
        Normal, SuperChat
    }

    [System.Serializable]
    public class Comment
    {
        public CommentType type;
        public Texture icon;
        public string name; 
        public string comment;
    }

    [RequireComponent(typeof(YouTubeLiveController))]
    public class YouTubeLiveTest : MonoBehaviour
    {

        public List<Comment> comments = new List<Comment>();

        ImageLoader loader;

        void Start()
        {
            loader = new ImageLoader();

            var ctrl = GetComponent<YouTubeLiveController>();

            ctrl.OnMessage += async msg => {

                Debug.Log(msg.name + ": " + msg.text);

                Texture tex = await loader.LoadTextureAsync(msg.img);

                comments.Add(new Comment() { type = CommentType.Normal, icon = tex, name = msg.name, comment = msg.text });
            };
        }
    }
}