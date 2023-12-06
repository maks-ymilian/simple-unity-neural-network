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

    public void StartTraining(DatasetLoader datasetLoader, MonoBehaviour monoBehaviour)
    {
        loader = datasetLoader;
        monoBehaviour.StartCoroutine(TrainLoop());
    }

    IEnumerator TrainLoop()
    {
        while (true)
        {
            TrainBatchSingleThread();

            yield return new WaitForEndOfFrame();
        }
    }

    void TrainBatchSingleThread()
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

    void f()
    {
        int threadCount = 8;
        int itemsLeft = settings.batchSize;
        int itemsPerThread = settings.batchSize / threadCount;

        DatasetItem[] batch = NextBatch();

        Thread[] threads = new Thread[threadCount];
        float[][] threadGradients = new float[threadCount][];
        for (int i = 0; i < threadCount; i++)
        {
            int numItems = itemsPerThread;
            int startIndex = itemsPerThread * i;

            int threadIndex = i;
            threads[i] = new Thread(() => threadGradients[threadIndex] = TrainBatchMultithread(numItems, startIndex, batch));
            threads[i].Start();
        }

        for (int i = 0; i < threadCount; i++)
        {
            threads[i].Join();
        }

        for (int i = 0; i < threadCount; i++)
        {
            gradient = AddArrays(threadGradients[i], gradient);
        }

        AddToParameters(gradient);
    }

    float[] TrainBatchMultithread(int numItems, int startIndex, DatasetItem[] batch)
    {
        float[] gradient = new float[this.gradient.Length];

        for (int i = 0; i < settings.batchSize; i++)
        {
            float[] currentGradient = TrainOneExample(batch[i]);
            gradient = AddArrays(currentGradient, gradient);
        }

        return gradient;
    }

    float[] TrainOneExample(DatasetItem item)
    {
        SetInputs(item.input);
        ForwardPropagate();

        return BackPropagate(item.output);
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