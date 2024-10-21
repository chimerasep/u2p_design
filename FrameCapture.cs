using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class FrameCapture : MonoBehaviour
{
    public Camera vrCamera;
    public string pythonServerUrl = "http://localhost:5000/process-image"; 

    void Start()
    {
        StartCoroutine(CaptureAndSendFrames());
    }

    IEnumerator CaptureAndSendFrames()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            texture.Apply();

            byte[] bytes = texture.EncodeToPNG();
            Destroy(texture);

            StartCoroutine(SendImageToPython(bytes));
        }
    }

    IEnumerator SendImageToPython(byte[] imageBytes)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imageBytes, "frame.png", "image/png");

        UnityWebRequest www = UnityWebRequest.Post(pythonServerUrl, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error sending image: " + www.error);
        }
        else
        {
            Debug.Log("Image successfully sent to Python");
        }
    }
}
