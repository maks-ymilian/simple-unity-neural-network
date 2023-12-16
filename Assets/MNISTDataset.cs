using System;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UnityEngine;
using DatasetItem = DatasetLoader.DatasetItem;

public class MNISTDataset : DatasetLoader
{
    DatasetItem[] trainSet;
    string[] trainSetString;
    string[] testSetString;

    const string trainPath = "MNIST_CSV/mnist_train.csv";
    const string testPath = "MNIST_CSV/mnist_test.csv";

    public int epoch { get; set; }
    int totalItemsDrawn;

    const int parseThreadCount = 5;

    public MNISTDataset()
    {
        trainSetString = File.ReadAllLines(Application.streamingAssetsPath + "/" + trainPath);
        testSetString = File.ReadAllLines(Application.streamingAssetsPath + "/" + testPath);
        ParseDatasetString();
    }

    void ParseDatasetString()
    {
        trainSet = new DatasetItem[trainSetString.Length];

        Task[] tasks = new Task[parseThreadCount];
        for (int i = 0; i < parseThreadCount; i++)
        {
            int threadId = i;

            int length = trainSetString.Length / parseThreadCount;
            int startIndex = threadId * length;
            if (threadId == parseThreadCount - 1)
            {
                int remainder = trainSetString.Length - (length * parseThreadCount);
                length += remainder;
            }

            Action action = () => ParseDatasetStringPortion(startIndex, length);
            tasks[i] = new Task(action);
            tasks[i].Start();
        }

        for (int i = 0; i < parseThreadCount; i++)
        {
            tasks[i].Wait();
        }
    }

    void ParseDatasetStringPortion(int startIndex, int length)
    {
        for (int i = startIndex; i < startIndex + length; i++)
        {
            trainSet[i] = ParseMNISTLine(trainSetString[i]);
        }
    }

    DatasetItem ParseMNISTLine(string line)
    {
        string[] stringValues = line.Split(',');

        byte label = byte.Parse(stringValues[0]);

        float[] values = new float[stringValues.Length - 1];
        for (int j = 1; j < values.Length; j++)
        {
            int x = j % 28;
            int y = Mathf.FloorToInt(j / 28);
            y = -y + 27; // flip y axis
            int index = y * 28 + x;

            values[index] = (float)byte.Parse(stringValues[j]) / 255;
        }

        var item = new DatasetItem();
        item.output = LabelToTargetOutput(label);
        item.input = values;
        return item;
    }

    public DatasetItem RandomItem()
    {
        totalItemsDrawn++;
        if (totalItemsDrawn >= trainSet.Length)
        {
            epoch++;
            totalItemsDrawn = 0;
        }

        int index = UnityEngine.Random.Range(0, trainSet.Length);
        return trainSet[index];
    }

    public DatasetItem RandomTestItem()
    {
        int index = UnityEngine.Random.Range(0, testSetString.Length);
        return ParseMNISTLine(testSetString[index]);
    }

    float[] LabelToTargetOutput(int label)
    {
        float[] outputs = new float[10];
        outputs[label] = 1;
        return outputs;
    }

    public DatasetItem NextItem()
    {
        return RandomItem();
    }

    public DatasetItem[] GetDataset()
    {
        return null;
    }
}