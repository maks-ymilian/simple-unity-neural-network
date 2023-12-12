using System;
using UnityEngine;
using UnityEngine.UI;

public class FunctionExampleMain : MonoBehaviour
{
    [SerializeField] NeuralNetwork.Settings neuralNetworkSettings;
    [SerializeField] NNDiagram diagram;
    [SerializeField] DrawingArea functionTexture;
    [SerializeField] DrawingArea trainingTexture;
    [SerializeField] Text infoText;

    [SerializeField] int functionDatasetSize;
    [SerializeField] Function functionEnum;

    byte[] currentTexture;
    FunctionDataset functionDataset;
    NeuralNetwork neuralNetwork;
    Func<float[], float[]> function;

    enum Function { Circle, Sine }

    private void Awake()
    {
        neuralNetwork = new NeuralNetwork(neuralNetworkSettings);
        neuralNetwork.InitializeRandomParameters();

        diagram.Create(neuralNetwork);

        if (functionEnum == Function.Sine)
            function = Sine;
        else if (functionEnum == Function.Circle)
            function = Circle;

        functionDataset = new FunctionDataset(functionDatasetSize, function, new Vector2(0, 1));
        neuralNetwork.StartTraining(functionDataset, this);

        currentTexture = new byte[trainingTexture.textureSize * trainingTexture.textureSize];

        InitFunctionTexture();
    }

    private void Update()
    {
        for (int y = 0; y < trainingTexture.textureSize ; y++)
        {
            for (int x = 0; x < trainingTexture.textureSize; x++)
            {
                float _x = (float)x / trainingTexture.textureSize;
                float _y = (float)y / trainingTexture.textureSize;
                neuralNetwork.SetInputs(new float[] { _x, _y });
                neuralNetwork.ForwardPropagate();

                currentTexture[y * trainingTexture.textureSize + x] = (byte)(Mathf.Clamp01(neuralNetwork.outputs[0]) * 255);
            }
        }
        trainingTexture.SetRawTextureData(currentTexture);

        infoText.text = "Cost: " + neuralNetwork.currentCost + "\n" + "Epoch: " + functionDataset.epoch;
    }

    void InitFunctionTexture()
    {
        int size = functionTexture.textureSize;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float _x = (float)x / size;
                float _y = (float)y / size;

                currentTexture[y * size + x] = (byte)(Mathf.Clamp01(function(new float[] { _x, _y })[0]) * 255);
            }
        }
        functionTexture.SetRawTextureData(currentTexture);
    }

    float[] Sine(float[] input)
    {
        float x = input[0];
        float y = input[1];

        Vector2 pos = new Vector2(x, y) * 28;
        float sin = Mathf.Sin((pos.y + 1) / 2) * 10;
        return new float[] { (sin - (pos.x - 14)) / 10 };
    }

    float[] Circle(float[] input)
    {
        Vector2 pos = new Vector2(input[0], input[1]) * 28;
        Vector2 centre = new Vector2(14, 14);

        return new float[] { Vector2.Distance(centre, pos) / 7 - 0.8f };
    }
}