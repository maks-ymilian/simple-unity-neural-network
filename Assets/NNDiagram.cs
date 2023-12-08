using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static NeuralNetwork;
using UnityEngine.UI;

public class NNDiagram : MonoBehaviour
{
    [SerializeField] float layerSpacing;
    [SerializeField] float neuronSpacing;
    [SerializeField] float scale;
    [SerializeField] float edgeThickness;
    [SerializeField] int maxNeuronsFirstLayer;

    [SerializeField] Instancer neuronInstancer;
    [SerializeField] Instancer edgeInstancer;

    List<NeuralNetwork.Edge> edges = new List<NeuralNetwork.Edge>();
    List<NeuralNetwork.Neuron> neurons = new List<NeuralNetwork.Neuron>();

    float[] weightColors;
    float[] activationColors;

    public void Create(NeuralNetwork nn)
    {
        int[] sizes = new int[nn.layers.Length];
        for (int i = 0; i < sizes.Length; i++)
        {
            sizes[i] = nn.layers[i].neurons.Length;

            if (i == 0)
                sizes[i] = Mathf.Clamp(sizes[i], 0, maxNeuronsFirstLayer);
        }

        List<Vector2> neuronPositions = new List<Vector2>();
        for (int layer = 0; layer < nn.layers.Length; layer++)
        {
            int layerNeuronCount = sizes[layer];
            for (int neuron = 0; neuron < layerNeuronCount; neuron++)
            {
                if (layer == 0 && neuron > maxNeuronsFirstLayer - 1)
                    break;

                Vector2 pos;
                pos.x = layer * layerSpacing * scale - (float)nn.layers.Length / 2;
                pos.y = (-neuron + (float)layerNeuronCount / 2) * neuronSpacing * scale;
                pos += (Vector2)transform.position;

                neuronPositions.Add(pos);

                neurons.Add(nn.layers[layer].neurons[neuron]);
            }
        }
        neuronInstancer.Init(new Vector3(scale, scale, 1), neuronPositions.ToArray());

        List<Vector2> edgePositions = new List<Vector2>();
        List<float> edgeAngles = new List<float>();
        List<float> edgeLengths = new List<float>();
        for (int layer = 1; layer < nn.layers.Length; layer++)
        {
            int layerNeuronCount = sizes[layer];
            for (int neuron = 0; neuron < layerNeuronCount; neuron++)
            {
                int flatNeuronIndex = GetFlatNeuronIndex(layer, neuron, sizes);
                Vector2 neuronPos = neuronPositions[flatNeuronIndex];

                int neuronEdgeCount = sizes[layer - 1];
                for (int edge = 0; edge < neuronEdgeCount; edge++)
                {
                    int prevNeuronIndex = flatNeuronIndex - neuron - sizes[layer - 1] + edge;
                    Vector2 prevNeuronPos = neuronPositions[prevNeuronIndex];

                    Vector2 edgePos = Midpoint(prevNeuronPos, neuronPos);
                    float edgeAngle = Mathf.Atan(Slope(prevNeuronPos, neuronPos));
                    float edgeLength = Vector2.Distance(prevNeuronPos, neuronPos) * 1 / scale;

                    edgePositions.Add(edgePos);
                    edgeAngles.Add(edgeAngle);
                    edgeLengths.Add(edgeLength);

                    edges.Add(nn.layers[layer].neurons[neuron].edgesLeft[edge]);
                }
            }
        }
        edgeInstancer.Init(new Vector3(scale, scale * edgeThickness, 1), edgePositions.ToArray(), edgeAngles.ToArray(), edgeLengths.ToArray());

        weightColors = new float[edgePositions.Count];
        activationColors = new float[neuronPositions.Count];

        UpdateWeightColors();
    }

    public void UpdateWeightColors()
    {
        for (int i = 0; i < weightColors.Length; i++)
        {
            weightColors[i] = edges[i].weight;
        }
        edgeInstancer.UpdateColors(ref weightColors);
    }

    public void UpdateActivationColors()
    {
        for (int i = 0; i < activationColors.Length; i++)
        {
            activationColors[i] = neurons[i].activation;
        }
        neuronInstancer.UpdateColors(ref activationColors);
    }

    int GetFlatNeuronIndex(int layerIndex, int neuronIndex, int[] sizes)
    {
        int index = 0;
        for (int i = 0; i < sizes.Length; i++)
        {
            if (i == layerIndex)
                break;

            index += sizes[i];
        }
        index += neuronIndex;
        return index;
    }

    public static Vector2 Midpoint(Vector2 a, Vector2 b)
    {
        Vector2 ret;
        ret.x = (a.x + b.x) / 2;
        ret.y = (a.y + b.y) / 2;
        return ret;
    }

    public static float Slope(Vector2 a, Vector2 b)
    {
        return (b.y - a.y) / (b.x - a.x);
    }
}