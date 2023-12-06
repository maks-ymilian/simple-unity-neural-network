using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public partial class NeuralNetwork
{
    public NeuralNetwork(Settings settings)
    {
        this.settings = settings;

        switch (settings.activationFunction)
        {
            case Settings.ActivationFunction.Sigmoid:
                activationFunction = Utils.Sigmoid;
                activationDerivative = Utils.SigmoidDerivative;
                break;

            case Settings.ActivationFunction.ReLU:
                activationFunction = Utils.ReLU;
                activationDerivative = Utils.ReLUDerivative;
                break;

            case Settings.ActivationFunction.LeakyReLU:
                activationFunction = Utils.LeakyReLU;
                activationDerivative = Utils.LeakyReLUDerivative;
                break;
        }

        int parameterCount = 0;

        int[] sizes = settings.layers;
        layers = new Layer[sizes.Length];

        for (int layerIndex = 0; layerIndex < sizes.Length; layerIndex++)
        {
            Layer layer = new Layer();
            layers[layerIndex] = layer;
            layer.neurons = new Neuron[sizes[layerIndex]];

            for (int neuronIndex = 0; neuronIndex < layer.neurons.Length; neuronIndex++)
            {
                Neuron currentNeuron = new Neuron();
                layer.neurons[neuronIndex] = currentNeuron;

                currentNeuron.index = new Vector2Int(layerIndex, neuronIndex);

                if (layerIndex != sizes.Length - 1)
                    currentNeuron.edgesRight = new Edge[sizes[layerIndex + 1]];

                if (layerIndex == 0)
                    continue;

                currentNeuron.layer = layer;

                int neuronEdgeCount = sizes[layerIndex - 1];
                currentNeuron.edgesLeft = new Edge[neuronEdgeCount];
                for (int edgeIndex = 0; edgeIndex < neuronEdgeCount; edgeIndex++)
                {
                    Edge currentEdge = new Edge();
                    currentEdge.parentNeuron = currentNeuron;
                    currentEdge.previousNeuron = layers[layerIndex - 1].neurons[edgeIndex];

                    currentEdge.previousNeuron.edgesRight[neuronIndex] = currentEdge;

                    currentNeuron.edgesLeft[edgeIndex] = currentEdge;

                    parameterCount++;
                }

                parameterCount++;
            }
        }

        gradient = new float[parameterCount];
    }

    public void SetInputs(float[] inputs)
    {
        if (inputs.Length != layers[0].neurons.Length)
        {
            Debug.LogError("inputs length must be the same as number of neurons in input layer");
            return;
        }

        for (int i = 0; i < inputs.Length; i++)
        {
            layers[0].neurons[i].activation = inputs[i];
        }
    }

    /// <summary>
    /// sets all parameters for all neurons with weights first and bias second
    /// </summary>
    void SetParameters(float[] parameters)
    {
        int param = 0;
        for (int i = 1; i < layers.Length; i++)
        {
            foreach (var neuron in layers[i].neurons)
            {
                foreach (var edge in neuron.edgesLeft)
                {
                    edge.weight = parameters[param];
                    param++;
                }

                neuron.bias = parameters[param];
                param++;
            }
        }
    }

    /// <summary>
    /// adds to all parameters for all neurons with weights first and bias second
    /// </summary>
    void AddToParameters(float[] parameters)
    {
        int param = 0;
        for (int i = 1; i < layers.Length; i++)
        {
            foreach (var neuron in layers[i].neurons)
            {
                foreach (var edge in neuron.edgesLeft)
                {
                    edge.weight += parameters[param];
                    param++;
                }

                neuron.bias += parameters[param];
                param++;
            }
        }
    }

    public void ForwardPropagate()
    {
        for (int i = 1; i < layers.Length; i++)
        {
            foreach (var neuron in layers[i].neurons)
            {
                neuron.EvaluateActivation(activationFunction);
            }
        }
    }

    void ForwardPropagate(Neuron n)
    {
        for (int i = 1; i < layers.Length; i++)
        {
            foreach (var neuron in layers[i].neurons)
            {
                if (neuron == n)
                    continue;

                neuron.EvaluateActivation(activationFunction);
            }
        }
    }

    public void InitializeRandomParameters()
    {
        foreach (var layer in layers)
        {
            foreach (var neuron in layer.neurons)
            {
                if (System.Array.IndexOf(layers, layer) == 0)
                    continue;

                neuron.bias = Random.Range(settings.biasInitRange.x, settings.biasInitRange.y);

                foreach (var edge in neuron.edgesLeft)
                {
                    edge.weight = Random.Range(settings.weightInitRange.x, settings.weightInitRange.y);//Mathf2.RandomGaussian(0, 0.17f);
                }
            }
        }
    }

    static float Cost(float[] output, float[] targetOutputs)
    {
        if (output.Length != targetOutputs.Length)
        {
            Debug.LogError("targetOuputs length must be the same as number of neurons in output layer");
            return -1;
        }

        float cost = 0;
        for (int i = 0; i < output.Length; i++)
        {
            float difference = targetOutputs[i] - output[i];
            cost += difference * difference;
        }

        return cost;
    }

    public float[] outputs
    {
        get
        {
            Neuron[] neurons = layers[layers.Length - 1].neurons;
            float[] ret = new float[neurons.Length];

            for (int i = 0; i < neurons.Length; i++)
            {
                ret[i] = neurons[i].activation;
            }

            return ret;
        }
    }

    public Layer[] layers;

    Func<float, float> activationFunction;
    Func<float, float> activationDerivative;

    Settings settings;

    [Serializable]
    public class Settings
    {
        public int[] layers = new int[] { 1, 2, 1 };

        public Vector2 weightInitRange = new Vector2(-0.3f, 0.3f);
        public Vector2 biasInitRange = new Vector2(0, 0);

        public float learningRate = 0.01f;
        public int batchSize = 10;

        public enum ActivationFunction { Sigmoid, ReLU, LeakyReLU }
        public ActivationFunction activationFunction;
    }

    public class Layer
    {
        public Neuron[] neurons;
    }

    public class Neuron
    {
        public float activation;
        public float bias;
        public float z;

        public Edge[] edgesLeft;
        public Edge[] edgesRight;

        public Layer layer;

        public Vector2Int index; // layer index, neuron index

        public void EvaluateActivation(Func<float, float> activationFunction)
        {
            float weightedSum = 0;
            foreach (var edge in edgesLeft)
            {
                weightedSum += edge.weight * edge.previousNeuron.activation;
            }
            weightedSum += bias;

            z = weightedSum;
            activation = activationFunction(weightedSum);
        }

        public override int GetHashCode()
        {
            return index.x * 1000 + index.y;
        }
    }

    public class Edge
    {
        public float weight;

        public Neuron parentNeuron;
        public Neuron previousNeuron;
    }
}