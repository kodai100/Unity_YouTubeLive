using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using UniRx.Async;
using System;
using System.Collections.Generic;

public class ImageLoader
{

    public ImageLoader()
    {

    }
    
    public async UniTask<Texture> LoadTextureAsync(string url)
    {
        Debug.Log($"<color=yellow>Load start : {url}</color>");

        var request = UnityWebRequestTexture.GetTexture($"{url}");
        await request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError(request.error);
            return null;
        }
        else
        {
            Texture tex = DownloadHandlerTexture.GetContent(request);
            tex.name = Path.GetFileNameWithoutExtension(url);

            return tex;
        }

    }


}
