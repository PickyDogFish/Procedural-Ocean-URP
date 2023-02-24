using UnityEngine;

namespace PlantGeneration.ProceduralGrass {

    public class ProceduralGrass : MonoBehaviour {
        public GrassSettings settings;

        private GraphicsBuffer terrainTriangleBuffer;
        private GraphicsBuffer terrainVertexBuffer;

        private GraphicsBuffer transformMatrixBuffer;

        private GraphicsBuffer grassTriangleBuffer;
        private GraphicsBuffer grassVertexBuffer;
        private GraphicsBuffer grassUVBuffer;

        private Bounds bounds;
        private MaterialPropertyBlock properties;

        private int kernel;
        private uint threadGroupSize;
        private int terrainTriangleCount = 0;

        public bool drawGrass = false;
        public bool dataGenerated { get; private set; }

        public void GenerateData(Mesh terrainMesh) {
            kernel = settings.computeShader.FindKernel("CalculateBladePositions");

            // Terrain data for the compute shader.
            Vector3[] terrainVertices = terrainMesh.vertices;
            terrainVertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, terrainVertices.Length, sizeof(float) * 3);
            terrainVertexBuffer.SetData(terrainVertices);

            int[] terrainTriangles = terrainMesh.triangles;
            terrainTriangleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, terrainTriangles.Length, sizeof(int));
            terrainTriangleBuffer.SetData(terrainTriangles);

            terrainTriangleCount = terrainTriangles.Length / 3;

            settings.computeShader.SetVector("_ObjectPos", gameObject.transform.position);
            settings.computeShader.SetBuffer(kernel, "_TerrainPositions", terrainVertexBuffer);
            settings.computeShader.SetBuffer(kernel, "_TerrainTriangles", terrainTriangleBuffer);

            // Grass data for RenderPrimitives.
            Vector3[] grassVertices = settings.grassMesh.vertices;
            grassVertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, grassVertices.Length, sizeof(float) * 3);
            grassVertexBuffer.SetData(grassVertices);

            int[] grassTriangles = settings.grassMesh.triangles;
            grassTriangleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, grassTriangles.Length, sizeof(int));
            grassTriangleBuffer.SetData(grassTriangles);

            Vector2[] grassUVs = settings.grassMesh.uv;
            grassUVBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, grassUVs.Length, sizeof(float) * 2);
            grassUVBuffer.SetData(grassUVs);

            // Set up buffer for the grass blade transformation matrices.
            transformMatrixBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, terrainTriangleCount, sizeof(float) * 16);
            settings.computeShader.SetBuffer(kernel, "_TransformMatrices", transformMatrixBuffer);

            // Set bounds.
            bounds = terrainMesh.bounds;
            bounds.center += transform.position;
            bounds.Expand(settings.maxBladeHeight);

            // Bind buffers to a MaterialPropertyBlock which will get used for the draw call.
            properties = new MaterialPropertyBlock();
            properties.SetBuffer("_TransformMatrices", transformMatrixBuffer);
            properties.SetBuffer("_Positions", grassVertexBuffer);
            properties.SetBuffer("_UVs", grassUVBuffer);
            properties.SetFloat("_SwayScale", settings.swayScale);

            RunComputeShader();
            dataGenerated = true;
        }

        private void RunComputeShader() {
            // Bind variables to the compute shader.
            settings.computeShader.SetMatrix("_TerrainObjectToWorld", transform.localToWorldMatrix);
            settings.computeShader.SetInt("_TerrainTriangleCount", terrainTriangleCount);
            settings.computeShader.SetFloat("_MinBladeHeight", settings.minBladeHeight);
            settings.computeShader.SetFloat("_MaxBladeHeight", settings.maxBladeHeight);
            settings.computeShader.SetFloat("_MinOffset", settings.minOffset);
            settings.computeShader.SetFloat("_MaxOffset", settings.maxOffset);
            settings.computeShader.SetFloat("_Scale", settings.scale);

            // Run the compute shader's kernel function.
            settings.computeShader.GetKernelThreadGroupSizes(kernel, out threadGroupSize, out _, out _);
            int threadGroups = Mathf.CeilToInt(terrainTriangleCount / threadGroupSize);
            settings.computeShader.Dispatch(kernel, threadGroups, 1, 1);
        }

        // Run a single draw call to render all the grass blade meshes each frame.
        private void Update() {
            if (dataGenerated && drawGrass) {
                Graphics.DrawProcedural(settings.material, bounds, MeshTopology.Triangles, grassTriangleBuffer, grassTriangleBuffer.count,
                    instanceCount: terrainTriangleCount,
                    properties: properties,
                    castShadows: settings.castShadows,
                    receiveShadows: settings.receiveShadows);
            }
        }

        private void OnDestroy() {
            CleanUp();
        }

        public void CleanUp() {
            if (terrainTriangleBuffer != null) {
                terrainTriangleBuffer.Dispose();
            }
            if (terrainVertexBuffer != null) {
                terrainVertexBuffer.Dispose();
            }
            if (transformMatrixBuffer != null) {
                transformMatrixBuffer.Dispose();
            }
            if (grassTriangleBuffer != null) {
                grassTriangleBuffer.Dispose();
            }
            if (grassVertexBuffer != null) {
                grassVertexBuffer.Dispose();
            }
            if (grassUVBuffer != null) {
                grassUVBuffer.Dispose();
            }
        }
    }
}