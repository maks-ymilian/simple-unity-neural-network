using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;

public class DrawingArea : MonoBehaviour
{
    const int numThreads = 8;
    [SerializeField] ComputeShader lineShader;
    [SerializeField] ComputeShader clearShader;
    [SerializeField] int textureSize;

    List<Vector2> lastPath = new List<Vector2>();

    bool mouseHeld;

    Bounds screenBounds;
    RenderTexture renderTexture;

    public byte[] textureData
    {
        get
        {
            return GetRenderTexturePixels(renderTexture);
        }
    }

    private void Awake()
    {
        Vector2 boundsCentre = (Vector2)Camera.main.WorldToScreenPoint(transform.position);
        Vector2 boundsMax = Camera.main.WorldToScreenPoint((Vector2)(transform.position + transform.localScale / 2));
        Vector2 boundsMin = Camera.main.WorldToScreenPoint((Vector2)(transform.position - transform.localScale / 2));
        Vector2 boundsSize = boundsMax - boundsMin;
        screenBounds = new Bounds(boundsCentre, boundsSize);

        renderTexture = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.R8);
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        GetComponent<Renderer>().material.SetTexture("_MainTex", renderTexture);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && screenBounds.Contains(Input.mousePosition))
            mouseHeld = true;
        else if (Input.GetMouseButtonUp(0))
        {
            mouseHeld = false;
            lastPath.Add(new Vector2(-1, 0));
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ClearTexture();
            PreProcessPath();
            DrawLastPath();
            lastPath.Clear();
        }

        if (mouseHeld)
        {
            lastPath.Add(screenBounds.GetNormalizedVector(Input.mousePosition));

            int pointCount = lastPath.Count;
            if (pointCount >= 2)
                DrawLine(lastPath[pointCount - 2], lastPath[pointCount - 1]);
        }
    }

    void PreProcessPath()
    {
        if (lastPath.Count == 0)
            return;

        Bounds bounds = new Bounds(lastPath[0], Vector2.zero);
        for (int i = 1; i < lastPath.Count; i++)
        {
            Vector2 point = lastPath[i];

            if (point.x == -1)
                continue;

            bounds.Encapsulate(point);
        }

        Vector2 offset = new Vector2(0.5f, 0.5f) - (Vector2)bounds.center;
        Vector2 size = bounds.size;
        float scaleFactor = 1 / Mathf.Max(size.x, size.y) * 0.6f;

        for (int i = 0; i < lastPath.Count; i++)
        {
            Vector2 point = lastPath[i];

            if (point.x == -1)
                continue;

            lastPath[i] = new Vector2(
                ((point.x - 0.5f + offset.x) * scaleFactor) + 0.5f,
                ((point.y - 0.5f + offset.y) * scaleFactor) + 0.5f);
        }
    }

    void DrawLastPath()
    {
        for (int i = 1; i < lastPath.Count; i++)
        {
            DrawLine(lastPath[i - 1], lastPath[i]);
        }
    }

    void DrawLine(Vector2 from, Vector2 to)
    {
        if (from.x == -1 || to.x == -1)
            return;

        lineShader.SetVector("from", from);
        lineShader.SetVector("to", to);
        lineShader.SetInt("textureSize", textureSize);
        lineShader.SetTexture(0, "outTexture", renderTexture);

        int threadGroups = (int)Mathf.Ceil((float)textureSize / numThreads);
        lineShader.Dispatch(0, threadGroups, threadGroups, 1);
    }

    public byte[] GetRenderTexturePixels(RenderTexture renderTexture)
    {
        RenderTexture currentRT = RenderTexture.active;

        var texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.R8, false);

        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        RenderTexture.active = currentRT;

        return texture.GetRawTextureData();
    }

    void ClearTexture()
    {
        clearShader.SetTexture(0, "outTexture", renderTexture);

        int threadGroups = (int)Mathf.Ceil((float)textureSize / numThreads);
        clearShader.Dispatch(0, threadGroups, threadGroups, 1);
    }
}