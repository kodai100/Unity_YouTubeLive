using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouTubeLive
{

    [RequireComponent(typeof(YouTubeLiveController))]
    public class YouTubeLiveTest : MonoBehaviour
    {

        public List<Chat.Comment> comments = new List<Chat.Comment>();

        ImageLoader loader;

        void Start()
        {
            loader = new ImageLoader();

            var ctrl = GetComponent<YouTubeLiveController>();

            ctrl.OnMessage += msg => {

                if (msg.superChatDetails != null)
                {
                    Debug.Log($"<color=yellow>{msg.name} : {msg.comment} - {msg.superChatDetails.amount}</color>");
                }
                else
                {
                    Debug.Log($"{msg.name} : {msg.comment}");
                }
                

                comments.Add(msg);
            };
        }
    }
}