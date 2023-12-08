using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Main : MonoBehaviour
{
    [SerializeField] NeuralNetwork.Settings neuralNetworkSettings;

    [SerializeField] int datasetSize;

    [SerializeField] bool showDataset;
    [SerializeField] bool showFunction;

    [SerializeField] NNDiagram diagram  ;
    [SerializeField] DrawingArea drawingArea;

    NeuralNetwork neuralNetwork;

    private void Awake()
    {
        neuralNetwork = new NeuralNetwork(neuralNetworkSettings);
        neuralNetwork.InitializeRandomParameters();

        diagram.Create(neuralNetwork);

        //var loader = new FunctionDataset(datasetSize, Sine, new Vector2(0, 1));
        //var loader = new MNISTDataset();

        //neuralNetwork.StartTraining(loader, this);
    }

    private void Update()
    {
        //ui.UpdateText(neuralNetwork.currentCost, 1);
        //ui.UpdateColors();

        //if (Input.GetKey(KeyCode.Space))
        //{
        //    neuralNetwork.StopTraining();
        //    drawingArea.Clear();
        //}
        //else if (Input.GetKey(KeyCode.N))
        //{
        //    neuralNetwork.ResumeTraining();
        //    drawingArea.Clear();
        //}

        //neuralNetwork.SetInputs(drawingArea.GetTexture());
        //neuralNetwork.ForwardPropagate();

        //float[] texture = new float[28 * 28];
        //int i = 0;
        //for (int x = 0; x < 28; x++)
        //{
        //    for (int y = 0; y < 28; y++)
        //    {
        //        float x_ = (float)x / 28;
        //        float y_ = (float)y / 28;

        //        neuralNetwork.SetInputs(new float[] { x_, y_ });
        //        neuralNetwork.ForwardPropagate();
        //        texture[i] = neuralNetwork.outputs[0];
        //        i++;
        //    }
        //}
        //drawingArea.SetTexture(texture);

        //if (showDataset)
        //{
        //    var dataset = loader.GetDataset();
        //    for (i = 0; i < dataset.Length; i++)
        //    {
        //        int index = (int)(dataset[i].input[0] * 28) * 28 + (int)(dataset[i].input[1] * 28);
        //        texture[index] = dataset[i].output[0];
        //    }
        //    drawingArea.SetTexture(texture);
        //}
        //if (showFunction)
        //{
        //    texture = new float[28 * 28];
        //    i = 0;
        //    for (int x = 0; x < 28; x++)
        //    {
        //        for (int y = 0; y < 28; y++)
        //        {
        //            float x_ = (float)x / 28;
        //            float y_ = (float)y / 28;

        //            texture[i] = Sine(new float[] { x_, y_ })[0];
        //            i++;
        //        }
        //    }
        //    drawingArea.SetTexture(texture);
        //}
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