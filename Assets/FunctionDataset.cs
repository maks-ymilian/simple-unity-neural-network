using System;
using System.Collections.Generic;
using UnityEngine;
using DatasetItem = DatasetLoader.DatasetItem;

public class FunctionDataset : DatasetLoader
{
    Func<float[], float[]> function;

    DatasetItem[] dataset;

    List<int> datasetShuffled;
    int currentDatasetIndex;

    public int epoch { get; private set; }

    public FunctionDataset(int size, Func<float[], float[]> function, Vector2 inputRange)
    {
        this.function = function;

        GenerateDataset(size, inputRange);
    }

    void GenerateDataset(int itemCount, Vector2 inputRange)
    {
        dataset = new DatasetItem[itemCount];
        for (int i = 0; i < itemCount; i++)
        {
            float x = UnityEngine.Random.Range(inputRange.x, inputRange.y);
            float y = UnityEngine.Random.Range(inputRange.x, inputRange.y);
            float[] input = new float[] { x, y };

            var item = new DatasetItem();
            item.input = input;
            item.output = function(input);
            dataset[i] = item;
        }

        datasetShuffled = new List<int>(itemCount);
        for (int i = 0; i < itemCount; i++)
        {
            datasetShuffled.Add(i);
        }
        datasetShuffled.Shuffle();
    }

    public DatasetItem NextItem()
    {
        if (currentDatasetIndex >= dataset.Length)
        {
            datasetShuffled.Shuffle();
            currentDatasetIndex = 0;
            epoch++;
        }

        DatasetItem item;
        item = dataset[datasetShuffled[currentDatasetIndex]];
        currentDatasetIndex++;

        return item;
    }

    public DatasetItem[] GetDataset()
    {
        return dataset;
    }
}