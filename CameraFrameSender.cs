using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CameraFrameSender : MonoBehaviour
{
    public Camera mainCamera;      // Camera to capture frames from
    public Camera secondCamera;    // Camera to display processed frames
    public GameObject displayObject;  // 3D object (e.g., plane) to display the processed frame
    private RenderTexture secondCameraRenderTexture;

    private string serverUrl = "http://127.0.0.1:5000/process-frame"; // Python Flask server URL

    private void Start()
    {
        // Initialize the RenderTexture for the second camera
        secondCameraRenderTexture = secondCamera.targetTexture;

        // Start sending frames continuously
        StartCoroutine(SendFrameCoroutine());
    }

    // Coroutine to capture and send frames every frame (approximately 30 FPS)
    IEnumerator SendFrameCoroutine()
    {
        while (true)
        {
            // Capture a frame from the main camera
            Texture2D capturedFrame = CaptureFrame();

            // Encode the captured frame to a PNG byte array
            byte[] frameBytes = capturedFrame.EncodeToPNG();  // You can choose JPG or other formats

            // Send the frame to the Python server
            yield return StartCoroutine(SendFrameToServer(frameBytes));

            // Wait for the next frame (30 FPS)
            yield return new WaitForSeconds(1f / 30f);
        }
    }

    // Capture the current frame from the main camera
    Texture2D CaptureFrame()
    {
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        mainCamera.targetTexture = rt;
        mainCamera.Render();

        // Create a Texture2D to store the captured frame
        Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        RenderTexture.active = rt;
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        texture.Apply();

        // Reset camera's target texture and active RenderTexture
        mainCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        return texture;
    }

    // Send the captured frame to the Python server
    IEnumerator SendFrameToServer(byte[] frameBytes)
    {
        UnityWebRequest www = UnityWebRequest.Put(serverUrl, frameBytes);
        www.method = UnityWebRequest.kHttpVerbPOST;
        www.SetRequestHeader("Content-Type", "application/octet-stream");

        // Wait for the response from the server
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Frame sent successfully");

            // Handle the server response, which contains the processed frame (e.g., grayscale)
            byte[] processedFrame = www.downloadHandler.data;
            OnReceiveProcessedFrame(processedFrame);
        }
        else
        {
            Debug.LogError("Error sending frame: " + www.error);
        }
    }

    // Receive the processed frame from the server and display it on the second camera
    public void OnReceiveProcessedFrame(byte[] processedBytes)
    {
        // Convert the byte array back into a Texture2D
        Texture2D processedTexture = new Texture2D(2, 2);
        processedTexture.LoadImage(processedBytes);  // This loads the image from the byte array

        // Display the processed texture either on the second camera's RenderTexture or a 3D object
        if (secondCameraRenderTexture != null)
        {
            TransferTexture2DToRenderTexture(processedTexture, secondCameraRenderTexture);
        }

        if (displayObject != null)
        {
            ApplyTextureToMaterial(processedTexture);
        }
    }

    // Transfer the Texture2D to the second camera's RenderTexture
    void TransferTexture2DToRenderTexture(Texture2D texture2D, RenderTexture renderTexture)
    {
        // Set the RenderTexture as active
        RenderTexture.active = renderTexture;

        // Copy the Texture2D to the RenderTexture
        Graphics.Blit(texture2D, renderTexture);

        // Reset the active RenderTexture
        RenderTexture.active = null;
    }

    // Apply the processed Texture2D to a material of a 3D object (for example, a plane)
    void ApplyTextureToMaterial(Texture2D texture)
    {
        if (displayObject != null)
        {
            // Get the renderer of the display object
            Renderer renderer = displayObject.GetComponent<Renderer>();

            // Assign the texture to the object's material
            renderer.material.mainTexture = texture;
        }
    }
}
