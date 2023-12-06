using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NeuralNetDiagram : MonoBehaviour
{
    [SerializeField] float lineThickness;
    [SerializeField] float neuronSize;
    [SerializeField] int numberOfInputsToShow;
    [SerializeField] Gradient weightGradient;

    [SerializeField] Transform diagramParent;
    [SerializeField] Transform edgeParent;
    [SerializeField] GraphicRaycaster graphicRaycaster;
    [SerializeField] TextMeshProUGUI text;

    [SerializeField] GameObject layerLayoutPrefab;
    [SerializeField] GameObject layerPrefab;
    [SerializeField] GameObject neuronPrefab;
    [SerializeField] GameObject linePrefab;

    Transform layerParent;
    NeuralNetwork neuralNetwork;

    Dictionary<GameObject, NeuralNetwork.Neuron> neurons = new Dictionary<GameObject, NeuralNetwork.Neuron>();
    Dictionary<GameObject, NeuralNetwork.Edge> edges = new Dictionary<GameObject, NeuralNetwork.Edge>();

    private void Update()
    {
        PointerEventData pointerEventData = new PointerEventData(null);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, results);

        if (results.Count != 0)
        {
            GameObject obj = results[0].gameObject;

            if (obj.CompareTag("Neuron"))
            {
                Debug.Log("Activation: " + neurons[obj].activation + "\nBias: " + neurons[obj].bias);
            }
            else if (obj.CompareTag("Edge"))
            {
                Debug.Log(edges[obj].weight);
            }
        }
    }

    public void CreateDiagram(NeuralNetwork neuralNetwork)
    {
        this.neuralNetwork = neuralNetwork;

        layerParent = Instantiate(layerLayoutPrefab, diagramParent).GetComponent<RectTransform>();

        for (int layerIndex = 0; layerIndex < neuralNetwork.layers.Length; layerIndex++)
        {
            Transform layerTransform = Instantiate(layerPrefab, layerParent).transform;

            var currentLayer = neuralNetwork.layers[layerIndex];
            for (int neuronIndex = 0; neuronIndex < currentLayer.neurons.Length; neuronIndex++)
            {
                if (neuronIndex >= numberOfInputsToShow && layerIndex == 0)
                    break;

                GameObject n = Instantiate(neuronPrefab, layerTransform);
                n.GetComponent<RectTransform>().sizeDelta = Vector2.one * neuronSize;
                neurons.Add(n, neuralNetwork.layers[layerIndex].neurons[neuronIndex]);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(layerParent.GetComponent<RectTransform>());

        for (int layerIndex = 0; layerIndex < neuralNetwork.layers.Length; layerIndex++)
        {
            var currentLayer = neuralNetwork.layers[layerIndex];
            for (int neuronIndex = 0; neuronIndex < currentLayer.neurons.Length; neuronIndex++)
            {
                if (layerIndex == 0)
                    continue;

                RectTransform neuronTransform = layerParent.GetChild(layerIndex).GetChild(neuronIndex).GetComponent<RectTransform>();

                Transform prevLayerTransform = layerParent.GetChild(layerIndex - 1);
                for (int prevNeuronIndex = 0; prevNeuronIndex < prevLayerTransform.childCount; prevNeuronIndex++)
                {
                    RectTransform prevNeuronTransform = layerParent.GetChild(layerIndex - 1).GetChild(prevNeuronIndex).GetComponent<RectTransform>();
                    GameObject l = InstantiateLine(prevNeuronTransform.position, neuronTransform.position, lineThickness, edgeParent);
                    edges.Add(l, neuralNetwork.layers[layerIndex].neurons[neuronIndex].edgesLeft[prevNeuronIndex]);
                }
            }
        }

        UpdateColors();
    }

    public void UpdateColors()
    {
        foreach (var neuron in neurons)
        {
            float color = neuron.Value.activation;
            neuron.Key.GetComponent<RawImage>().color = new Color(color, color, color);
        }

        foreach (var edge in edges)
        {
            Color color = weightGradient.Evaluate((edge.Value.weight / 2) + 0.5f);
            edge.Key.GetComponent<RawImage>().color = color;
        }
    }

    public void UpdateText(float cost, int epoch)
    {
        text.text = "Cost: " + cost + "\nEpoch: " + epoch;
    }

    GameObject InstantiateLine(Vector2 from, Vector2 to, float thickness, Transform parent)
    {
        RectTransform line = Instantiate(linePrefab).GetComponent<RectTransform>();

        line.position = Midpoint(from, to);
        line.sizeDelta = new Vector2(Vector2.Distance(from, to), thickness);
        line.rotation = Quaternion.AngleAxis(Mathf.Atan(Slope(from, to)) * Mathf.Rad2Deg, Vector3.forward);

        line.SetParent(parent, true);

        return line.gameObject;
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