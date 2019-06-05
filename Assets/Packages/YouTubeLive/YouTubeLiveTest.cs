using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExchangeRate;
using System;

namespace YouTubeLive
{

    [RequireComponent(typeof(YouTubeLiveController))]
    public class YouTubeLiveTest : MonoBehaviour
    {

        public List<Chat.Comment> comments = new List<Chat.Comment>();

        void Start()
        {

            var ctrl = GetComponent<YouTubeLiveController>();

            ctrl.OnMessage += async msg => {

                if (msg.type == CommentType.SuperChat)
                {

                    Currency currency = (Currency)Enum.Parse(typeof(Currency), msg.superChatDetails.currency);

                    float yen = await ExchangeRateAPI.Exchange(msg.superChatDetails.amount, currency, Currency.JPY);

                    Debug.Log($"<color=yellow>{msg.name} : {msg.comment} - {yen} [JPY] <- {msg.superChatDetails.amount} [{currency.ToString()}]</color>");
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