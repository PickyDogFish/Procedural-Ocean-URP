using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarchingCubes;

namespace ReactionDiffusion
{
    public class RDLayered : MonoBehaviour
    {
        public RDLayerSettings layerSettings;
        ComputeBuffer _voxelBuffer;
        MeshBuilder _builder;
        [SerializeField] ComputeShader _builderCompute = null;
        [SerializeField] ComputeShader simulationCompute = null;



        private RenderTexture simulationRead;
        private RenderTexture simulationWrite;



        int size { get { return layerSettings.size; } }

        float[] values;

        void Awake()
        {
            values = new float[size * size * size];
            _voxelBuffer = new ComputeBuffer(size * size * size, sizeof(float));
            _builder = new MeshBuilder(new Vector3Int(size, size, size), layerSettings.builderTriangleBudget, _builderCompute);
            simulationRead = RDSimulator.CreateRenderTexture(layerSettings.simulationSettings.resolution);
            simulationWrite = RDSimulator.CreateRenderTexture(layerSettings.simulationSettings.resolution);
        }

        void Start()
        {
            GetComponent<MeshFilter>().sharedMesh = GenerateCoral();
        }

        public Mesh GenerateCoral(){
            RDSimulator.InitializeComputeShader(ref simulationCompute, layerSettings.simulationSettings, ref simulationRead);

            //building the values array from layers from RDOnGPU
            for (int layerIndex = 0; layerIndex < size; layerIndex++)
            {
                AddNextLayerToValues(layerIndex);
            }
            Debug.Log(values);
            _voxelBuffer.SetData(values);
            _builder.BuildIsosurface(_voxelBuffer, layerSettings.builderTargetValue, layerSettings.builderGridScale);
            return _builder.Mesh;
        }

        // Update is called once per frame
        void Update()
        {

        }

        void AddNextLayerToValues(int layerIndex)
        {
            RDSimulator.Iterate(ref simulationRead, ref simulationWrite, ref simulationCompute, layerSettings.simulationSettings, layerSettings.killIncrease.Evaluate(layerIndex/size), layerSettings.step);
            Texture2D tex = RDSimulator.ToTexture2D(simulationWrite, layerSettings.simulationSettings.resolution);
            TextureScaler.Scale(tex, size, size);

            Color[] colors = tex.GetPixels();
            for (int i = 0; i < size * size; i++)
            {
                values[layerIndex * size * size + i] = colors[i].g;
            }
        }

        void OnDestroy()
        {
            _voxelBuffer.Dispose();
            _builder.Dispose();
        }


    }
}