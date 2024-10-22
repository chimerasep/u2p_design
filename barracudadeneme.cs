using Unity.Barracuda;
using UnityEngine;

public class ImageProcessor : MonoBehaviour
{
    public NNModel modelAsset;
    private IWorker worker;
    private Texture2D inputTexture;
    private RenderTexture outputTexture;

    void Start()
    {
        Model model = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Compute, model);

       
        inputTexture = new Texture2D(256, 256); 
        outputTexture = new RenderTexture(256, 256, 0);
    }

    public void ProcessImage(Texture2D inputImage)
    {
        // convert to tensor
        Tensor inputTensor = new Tensor(inputImage, 3); 
        worker.Execute(inputTensor);

        
        Tensor outputTensor = worker.PeekOutput();

        // back to texture
        RenderTexture.active = outputTexture;
        outputTensor.ToRenderTexture(outputTexture);
        RenderTexture.active = null;

        inputTensor.Dispose();
        outputTensor.Dispose();
    }

    void OnDestroy()
    {
        worker.Dispose();
    }
}

