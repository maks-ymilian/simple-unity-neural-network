using UnityEngine;

public class Instancer : MonoBehaviour
{
    [SerializeField] int instanceCount;
    [SerializeField] Mesh mesh;
    [SerializeField] Bounds bounds;

    ComputeBuffer argsBuffer;
    ComputeBuffer pointBuffer;

    Material material;

    void Start()
    {
        material = GetComponent<Material>();

        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        args[0] = mesh.GetIndexCount(0);
        args[1] = (uint)instanceCount;
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        pointBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 3 + sizeof(float) * 3);
        material.SetBuffer("_Points", pointBuffer);
    }

    void Update()
    {
        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);
    }

    void OnDisable()
    {
        argsBuffer.Release();
        pointBuffer.Release();
    }
}