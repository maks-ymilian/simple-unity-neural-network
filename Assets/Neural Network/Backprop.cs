using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public partial class NeuralNetwork
{
    Dictionary<Neuron, float> activationDerivativeCache = new Dictionary<Neuron, float>();
    float[] targetOutputs;

    /// <summary>
    /// returns negative gradient vector for one training example
    /// </summary>
    float[] BackPropagate(float[] targetOutputs)
    {
        if (targetOutputs.Length != layers[layers.Length - 1].neurons.Length)
        {
            Debug.LogError("targetOuputs length must be the same as number of neurons in output layer");
            return null;
        }

        float[] gradient = new float[parameterCount];

        activationDerivativeCache.Clear();
        this.targetOutputs = targetOutputs;

        int gradientIndex = 0;
        for (int i = 1; i < layers.Length; i++)
        {
            foreach (var neuron in layers[i].neurons)
            {
                foreach (var edge in neuron.edgesLeft)
                {
                    gradient[gradientIndex++] = -CostDerivativeWeight(edge) * settings.learningRate;
                }

                gradient[gradientIndex++] = -CostDerivativeBias(neuron) * settings.learningRate;
            }
        }

        return gradient;
    }

    /// <summary>
    /// finds the cost gradient component for one specific weight
    /// </summary>
    float CostDerivativeWeight(Edge edge)
    {
        float zw = edge.previousNeuron.activation;
        float az = activationDerivative(edge.parentNeuron.z);
        return zw * az * TryGetCachedCostDerivativeActivation(edge.parentNeuron);
    }

    /// <summary>
    /// finds the cost gradient component for one specific bias
    /// </summary>
    float CostDerivativeBias(Neuron neuron)
    {
        float az = activationDerivative(neuron.z);
        return az * TryGetCachedCostDerivativeActivation(neuron);
    }

    float TryGetCachedCostDerivativeActivation(Neuron neuron)
    {
        if (activationDerivativeCache.TryGetValue(neuron, out float derivative))
            return derivative;
        else
        {
            derivative = CostDerivativeActivation(neuron);
            activationDerivativeCache.Add(neuron, derivative);
            return derivative;
        }
    }

    /// <summary>
    /// finds the cost gradient component for one specific activation
    /// </summary>
    float CostDerivativeActivation(Neuron neuron)
    {
        bool lastLayer = neuron.layerIndex == layers.Length - 1;
        if (lastLayer)
        {
            int outputIndex = neuron.neuronIndex;

            float ca = 2 * (neuron.activation - targetOutputs[outputIndex]);
            return ca;
        }

        float layerSum = 0;
        bool secondLastLayer = neuron.layerIndex == layers.Length - 2;

        for (int i = 0; i < neuron.edgesRight.Length; i++)
        {
            Edge edge = neuron.edgesRight[i];

            float z1a0 = edge.weight;
            float a1z1 = activationDerivative(edge.parentNeuron.z);

            float chain = z1a0 * a1z1;

            if (secondLastLayer)
            {
                int outputIndex = edge.parentNeuron.neuronIndex;

                float ca = 2 * (edge.parentNeuron.activation - targetOutputs[outputIndex]);
                chain *= ca;
            }
            else
            {
                chain *= TryGetCachedCostDerivativeActivation(edge.parentNeuron);
            }

            layerSum += chain;
        }

        return layerSum;
    }

    float[] NumericalGradient(float[] targetOutputs)
    {
        if (targetOutputs.Length != layers[layers.Length - 1].neurons.Length)
        {
            Debug.LogError("targetOuputs length must be the same as number of neurons in output layer");
            return null;
        }

        List<float> gradient = new List<float>();

        for (int i = 1; i < layers.Length; i++)
        {
            foreach (var neuron in layers[i].neurons)
            {
                foreach (var edge in neuron.edgesLeft)
                {
                    gradient.Add(-CostGradientWeightTest(edge, targetOutputs) * settings.learningRate);
                }

                gradient.Add(-CostGradientBiasTest(neuron, targetOutputs) * settings.learningRate);
            }
        }

        return gradient.ToArray();
    }

    float CostGradientWeightTest(Edge edge, float[] targetOutputs)
    {
        float h = 0.000001f;
        float originalValue = edge.weight;

        edge.weight = originalValue + h;
        ForwardPropagate();
        float costplus = Cost(outputs, targetOutputs);

        edge.weight = originalValue - h;
        ForwardPropagate();
        float costminus = Cost(outputs, targetOutputs);

        edge.weight = originalValue;

        return (costplus - costminus) / (2 * h);
    }

    float CostGradientBiasTest(Neuron neuron, float[] targetOutputs)
    {
        float h = 0.000001f;
        float originalValue = neuron.bias;

        neuron.bias = originalValue + h;
        ForwardPropagate(neuron);
        float costplus = Cost(outputs, targetOutputs);

        neuron.bias = originalValue - h;
        ForwardPropagate(neuron);
        float costminus = Cost(outputs, targetOutputs);

        neuron.bias = originalValue;

        return (costplus - costminus) / (2 * h);
    }

    float CostGradientActivationTest(Neuron neuron, float[] targetOutputs)
    {
        float h = 0.000001f;
        float originalValue = neuron.activation;

        neuron.activation = originalValue + h;
        ForwardPropagate(neuron);
        float costplus = Cost(outputs, targetOutputs);

        neuron.activation = originalValue - h;
        ForwardPropagate(neuron);
        float costminus = Cost(outputs, targetOutputs);

        neuron.activation = originalValue;

        return (costplus - costminus) / (2 * h);
    }
}