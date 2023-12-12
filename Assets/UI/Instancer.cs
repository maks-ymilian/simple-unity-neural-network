using UnityEngine;
using UnityEngine.UIElements;

public class Instancer : MonoBehaviour
{
    [SerializeField] Mesh originalMesh;

    GraphicsBuffer commandBuffer;
    GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;
    const int commandCount = 1;

    ComputeBuffer posBuffer;
    ComputeBuffer angleBuffer;
    ComputeBuffer lengthBuffer;
    ComputeBuffer colorBuffer;

    Mesh mesh;
    Material material;
    RenderParams renderParams;

    bool initialized;

    public void Init(Vector3 scale, Vector2[] points, float[] angles = null, float[] lengths = null)
    {
        mesh = new Mesh();
        mesh.vertices = originalMesh.vertices;
        mesh.triangles = originalMesh.triangles;
        mesh = ScaleMesh(mesh, scale);

        material = GetComponent<Renderer>().material;
        renderParams.material = material;

        commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[commandCount];
        commandData[0].indexCountPerInstance = mesh.GetIndexCount(0);
        commandData[0].instanceCount = (uint)points.Length;
        commandBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        commandBuffer.SetData(commandData);

        posBuffer = new ComputeBuffer(points.Length, sizeof(float) * 2);
        posBuffer.SetData(points);
        material.SetBuffer("_Points", posBuffer);

        colorBuffer = new ComputeBuffer(points.Length, sizeof(float));
        material.SetBuffer("_Colors", colorBuffer);

        if (angles != null)
        {
            angleBuffer = new ComputeBuffer(angles.Length, sizeof(float));
            angleBuffer.SetData(angles);
            material.SetBuffer("_Angles", angleBuffer);
        }
        if (lengths != null)
        {
            lengthBuffer = new ComputeBuffer(lengths.Length, sizeof(float));
            lengthBuffer.SetData(lengths);
            material.SetBuffer("_Lengths", lengthBuffer);
        }

        initialized = true;
    }

    public void UpdateColors(ref float[] activations)
    {
        colorBuffer.SetData(activations);
    }

    void Update()
    {
        if (!initialized)
            return;

        Graphics.RenderMeshIndirect(renderParams, mesh, commandBuffer, commandCount);
    }

    void OnDisable()
    {
        if (!initialized)
            return;

        commandBuffer.Release();
        posBuffer.Release();
        colorBuffer.Release();
        if (angleBuffer != null)
            angleBuffer.Release();
        if (lengthBuffer != null)
            lengthBuffer.Release();
    }

    Mesh ScaleMesh(Mesh mesh, Vector3 scale)
    {
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= scale.x;
            vertices[i].y *= scale.y;
            vertices[i].z *= scale.z;
        }
        mesh.vertices = vertices;
        return mesh;
    }
}