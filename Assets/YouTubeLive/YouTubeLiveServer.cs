using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System;
using UnityEngine;

namespace YouTubeLive
{
    // Main Thread Dispatcher
    public class YouTubeLiveServer : MonoBehaviour
    {
        HttpListener listener;
        bool isRunning;

        Queue<string> queue = new Queue<string>();

        public event Action<string> OnReceiveCode;

        public void Listen()
        {
            ThreadStart start = () => {
                try
                {
                    listener = new HttpListener();
                    listener.Prefixes.Add("http://*:8080/");
                    listener.Start();

                    isRunning = true;
                    while (isRunning)
                    {
                        var context = listener.GetContext();
                        var req = context.Request;
                        var res = context.Response;

                        var code = ExtractCode(req.RawUrl);
                        if (code != "")
                        {
                            queue.Enqueue(code);
                        }

                        var content = Encoding.UTF8.GetBytes(req.RawUrl);
                        res.StatusCode = 200;
                        res.OutputStream.Write(content, 0, content.Length);
                        res.Close();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            };

            var thread = new Thread(start);
            thread.Start();
        }

        public void Stop()
        {
            if (isRunning)
            {
                listener.Close();
                isRunning = false;
            }
        }

        void Update()
        {
            if (queue.Count > 0)
            {
                var item = queue.Dequeue();

                if (OnReceiveCode != null)
                {
                    OnReceiveCode(item);
                }
            }
        }

        void OnDestory()
        {
            Stop();
        }

        public static string ExtractCode(string rawUrl)
        {
            var re = new Regex(@"/\?code=(?<c>.*)");
            return re.Match(rawUrl).Groups["c"].ToString();
        }

    }
}