using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System;
using UnityEngine;
using UniRx.Async;

namespace YouTubeLive
{
    // Main Thread Dispatcher
    public class YouTubeLiveServer : IDisposable
    {
        HttpListener listener;
        bool isRunning;

        event Action<string> OnReceiveCode;

        public YouTubeLiveServer(Action<string> OnReceiveCode)
        {
            this.OnReceiveCode = OnReceiveCode;
        }

        public async UniTask Start()
        {
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add("http://*:8080/");
                listener.Start();

                isRunning = true;

                while (isRunning)
                {
                    var context = await listener.GetContextAsync();

                    var req = context.Request;
                    var res = context.Response;

                    var code = ExtractCode(req.RawUrl);
                    if (code != "")
                    {
                        OnReceiveCode(code);

                        var content = Encoding.UTF8.GetBytes($"<h1>Response code: </h1><br>{code}");
                        res.StatusCode = 200;
                        res.OutputStream.Write(content, 0, content.Length);
                        res.Close();

                        Stop();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void Stop()
        {
            if (isRunning)
            {
                isRunning = false;

                listener.Stop();
                listener.Close();
            }
        }

        // Extract OAuth code from requested url
        public static string ExtractCode(string rawUrl)
        {
            var re = new Regex(@"/\?code=(?<c>.*)");
            return re.Match(rawUrl).Groups["c"].ToString();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}