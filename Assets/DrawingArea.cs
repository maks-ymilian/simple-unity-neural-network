using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingArea : MonoBehaviour
{
    [SerializeField] int radius;

    [SerializeField] Vector2Int size;
    [SerializeField] float pixelSize;

    [SerializeField] GameObject pixelPrefab;

    [SerializeField] Dictionary<Vector2Int, SpriteRenderer> pixels;

    Bounds screenBounds;

    Vector2Int lastPos;

    public float[] GetTexture()
    {
        float[] ret = new float[pixels.Count];

        int i = 0;
        foreach (var pixel in pixels.Values)
        {
            ret[i] = pixel.color.r;
            i++;
        }

        return ret;
    }

    public void SetTexture(float[] texture)
    {
        int i = 0;
        foreach (var pixel in pixels.Values)
        {
            Color color = new Color(texture[i], texture[i], texture[i]);
            pixel.color = color;
            i++;
        }
    }

    private void Awake()
    {
        pixels = new Dictionary<Vector2Int, SpriteRenderer>();

        Vector2 boundsCentre = (Vector2)Camera.main.WorldToScreenPoint(transform.position);
        Vector2 boundsSize = Camera.main.WorldToScreenPoint(new Vector2(size.x * pixelSize, size.y * pixelSize)) - (Vector3)boundsCentre;
        screenBounds = new Bounds(boundsCentre, boundsSize);

        for (int y = 0; y < size.x; y++)
        {
            for (int x = 0; x < size.y; x++)
            {
                GameObject pixel = Instantiate(pixelPrefab, new Vector3((x - (float)size.x / 2) * pixelSize + pixelSize / 2, (-y + (float)size.y / 2) * pixelSize - pixelSize / 2, 0), Quaternion.identity, transform);
                pixel.transform.localScale = new Vector3(pixelSize, pixelSize, 1);

                pixels.Add(new Vector2Int(x, size.y-y), pixel.GetComponent<SpriteRenderer>());
            }
        }
    }

    //private void Update()
    //{
    //    if (screenBounds.Contains(Input.mousePosition) && Input.GetMouseButton(0) || Input.GetMouseButton(1))
    //    {
    //        Vector2 _pixelPos = new Vector2((Input.mousePosition.x - screenBounds.min.x) / screenBounds.size.x * size.x, (Input.mousePosition.y - screenBounds.min.y) / screenBounds.size.y * size.y);
    //        Vector2Int pixelPos = new Vector2Int((int)(_pixelPos.x), (int)(_pixelPos.y + 1));

    //        bool erase = Input.GetMouseButton(0);

    //        if (lastPos != pixelPos)
    //            DrawCircle(pixelPos, erase);

    //        lastPos = pixelPos;
    //    }
    //}

    void DrawCircle(Vector2Int pos, bool erase)
    {
        for (int x = pos.x - radius; x < pos.x + radius; x++)
        {
            for (int y = pos.y - radius; y < pos.y + radius; y++)
            {
                Vector2Int currentPos = new Vector2Int(x, y);

                float dist = Mathf.Clamp01(1 - (Vector2.Distance(pos, currentPos) / (float)radius));

                if (!pixels.ContainsKey(currentPos))
                    continue;

                Color color = pixels[currentPos].color;
                color += new Color(dist, dist, dist) * (erase ? 1 : -1);
                color.a = 1;
                color.r = Mathf.Clamp01(color.r);
                color.g = Mathf.Clamp01(color.g);
                color.b = Mathf.Clamp01(color.b);
                pixels[currentPos].color = color;
            }
        }
    }
}