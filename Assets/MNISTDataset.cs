using System.IO;
using UnityEngine;
using DatasetItem = DatasetLoader.DatasetItem;

public class MNISTDataset : DatasetLoader
{
    string[] datasetString;
    const string path = "D:\\Unity Projects\\N\\Assets\\MNIST_CSV\\mnist_train.csv";

    public int epoch { get; set; }
    int totalItemsDrawn;

    public MNISTDataset()
    {
        datasetString = File.ReadAllLines(path);
    }

    public DatasetItem[] RandomBatch(int batchSize)
    {
        DatasetItem[] batch = new DatasetItem[batchSize];

        for (int i = 0; i < batchSize; i++)
        {
            batch[i] = RandomItem();
        }

        return batch;
    }

    public DatasetItem RandomItem()
    {
        totalItemsDrawn++;
        if (totalItemsDrawn >= datasetString.Length)
        {
            epoch++;
            totalItemsDrawn = 0;
        }

        int index = Random.Range(0, datasetString.Length);
        string itemString = datasetString[index];
        string[] stringValues = itemString.Split(',');
        
        int label = int.Parse(stringValues[0]);
        
        float[] values = new float[stringValues.Length - 1];
        for (int i = 1; i < values.Length; i++)
        {
            values[i] = (float)int.Parse(stringValues[i]) / 255;
        }
        
        DatasetItem item;
        item.output = LabelToTargetOutput(label);
        item.input = values;

        return item;
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