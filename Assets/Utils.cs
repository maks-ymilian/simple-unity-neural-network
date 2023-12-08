using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static float RandomGaussian(float mean, float stdDev)
    {
        float u1 = 1.0f - UnityEngine.Random.value; //uniform(0,1] random doubles
        float u2 = 1.0f - UnityEngine.Random.value;
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2); //random normal(0,1)
        float randNormal = mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)

        return randNormal;
    }

    static System.Random rng = new System.Random();
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static Vector2 GetNormalizedVector(this Bounds bounds, Vector2 point)
    {
        Vector2 relativePosition = point - (Vector2)bounds.min;
        Vector2 boundsSize = bounds.size;

        float normalizedX = relativePosition.x / boundsSize.x;
        float normalizedY = relativePosition.y / boundsSize.y;

        return new Vector2(normalizedX, normalizedY);
    }

    public static float Sigmoid(float x)
    {
        float k = Mathf.Exp(x);
        return k / (1.0f + k);
    }

    public static float SigmoidDerivative(float x)
    {
        float f = Sigmoid(x);
        float df = f * (1 - f);
        return df;
    }

    public static float ReLU(float x)
    {
        return x < 0 ? 0 : x;
    }

    public static float ReLUDerivative(float x)
    {
        return x < 0 ? 0 : 1;
    }

    public static float LeakyReLU(float x)
    {
        return x < 0 ? x * 0.01f : x;
    }

    public static float LeakyReLUDerivative(float x)
    {
        return x < 0 ? 0.01f : 1;
    }
}