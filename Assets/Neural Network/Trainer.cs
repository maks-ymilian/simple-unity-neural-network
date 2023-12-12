using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using DatasetItem = DatasetLoader.DatasetItem;

public partial class NeuralNetwork
{
    public float currentCost { get; private set; }

    DatasetLoader loader;
    float[] gradient;
    Coroutine trainLoop;
    MonoBehaviour monoBehaviour;

    public void StartTraining(DatasetLoader datasetLoader, MonoBehaviour monoBehaviour)
    {
        if (trainLoop != null)
            return;

        this.monoBehaviour = monoBehaviour;
        loader = datasetLoader;
        trainLoop = monoBehaviour.StartCoroutine(TrainLoop());
    }

    public void StopTraining()
    {
        if (trainLoop == null)
            return;

        monoBehaviour.StopCoroutine(trainLoop);
        trainLoop = null;
    }

    public void ResumeTraining()
    {
        if (trainLoop != null)
            return;

        trainLoop = monoBehaviour.StartCoroutine(TrainLoop());
    }

    IEnumerator TrainLoop()
    {
        while (true)
        {
            TrainBatch();

            yield return new WaitForEndOfFrame();
        }
    }

    void TrainBatch()
    {
        DatasetItem[] batch = NextBatch();
        Array.Clear(gradient, 0, gradient.Length);

        float cost = 0;
        for (int i = 0; i < settings.batchSize; i++)
        {
            SetInputs(batch[i].input);
            ForwardPropagate();
            float[] targetOutput = batch[i].output;
            cost += Cost(outputs, targetOutput);

            float[] currentGradient = BackPropagate(targetOutput);
            gradient = AddArrays(currentGradient, gradient);
        }
        cost /= settings.batchSize;
        currentCost = cost;

        AddToParameters(gradient);
    }

    DatasetItem[] NextBatch()
    {
        DatasetItem[] batch = new DatasetItem[settings.batchSize];

        for (int i = 0; i < settings.batchSize; i++)
        {
            batch[i] = loader.NextItem();
        }

        return batch;
    }

    float[] AddArrays(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            return null;

        for (int i = 0; i < a.Length; i++)
        {
            a[i] += b[i];
        }

        return a;
    }
}