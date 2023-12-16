using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NumberExampleMain : MonoBehaviour
{
    [SerializeField] NeuralNetwork.Settings neuralNetworkSettings;
    [SerializeField] NNDiagram diagram;
    [SerializeField] DrawingArea drawingArea;
    [SerializeField] Text infoText;

    MNISTDataset mnist;
    NeuralNetwork neuralNetwork;
    bool training = true;

    private void Awake()
    {
        neuralNetwork = new NeuralNetwork(neuralNetworkSettings);
        neuralNetwork.InitializeRandomParameters();

        diagram.Create(neuralNetwork);

        mnist = new MNISTDataset();
        neuralNetwork.StartTraining(mnist, this);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            drawingArea.ClearTexture();

            if (training)
            {
                training = false;
                neuralNetwork.StopTraining();
            }
            else
            {
                training = true;
                neuralNetwork.ResumeTraining();
            }
        }

        string info = "Cost: " + neuralNetwork.currentCost + "\n" + "Epoch: " + mnist.epoch;

        if (training)
        {
            diagram.UpdateWeightColors();
        }
        else
        {
            byte[] inputs = drawingArea.textureData;
            neuralNetwork.SetInputs(inputs);
            neuralNetwork.ForwardPropagate();
            diagram.UpdateActivationColors();

            info += "\n" + "Predicted number: " + GetPredictedNumber();
        }

        infoText.text = info;
    }

    public void SetRandomTestItem()
    {
        drawingArea.SetRawTextureDataFloat(mnist.RandomTestItem().input);
    }

    int GetPredictedNumber()
    {
        float[] outputs = neuralNetwork.outputs;
        float highestNumber = Mathf.NegativeInfinity;
        int highestNumberIndex = 0;
        for (int i = 0; i < outputs.Length; i++)
        {
            if (outputs[i] > highestNumber)
            {
                highestNumber = outputs[i];
                highestNumberIndex = i;
            }
        }

        return highestNumberIndex;
    }
}