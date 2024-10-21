using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ImageDisplay : MonoBehaviour
{
    public Renderer screenRenderer; 

    void Start()
    {
        StartCoroutine(ReceiveProcessedFrames());
    }

    IEnumerator ReceiveProcessedFrames()
    {
        while (true)
        {
            UnityWebRequest www = UnityWebRequest.Get("http://localhost:5000/get-processed-image");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error receiving image: " + www.error);
            }
            else
            {
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(www.downloadHandler.data);
                screenRenderer.material.mainTexture = texture;
            }

            yield return new WaitForSeconds(1f); 
        }
    }
}
