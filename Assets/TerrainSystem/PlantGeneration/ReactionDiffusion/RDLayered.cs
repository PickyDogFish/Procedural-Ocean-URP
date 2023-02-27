using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarchingCubes;

namespace PlantGeneration.ReactionDiffusion
{
    public class RDLayered : PlantGenerator
    {
        ComputeBuffer _voxelBuffer;
        MeshBuilder _builder;
        [SerializeField] ComputeShader _builderCompute = null;
        [SerializeField] ComputeShader simulationCompute = null;



        private RenderTexture simulationRead;
        private RenderTexture simulationWrite;

        float[] values;


        public override void Initialize(PlantGenSettings settings){
            RDLayerSettings layerSettings = (RDLayerSettings) settings;
            values = new float[layerSettings.size * layerSettings.size * layerSettings.size];
            _voxelBuffer = new ComputeBuffer(layerSettings.size * layerSettings.size * layerSettings.size, sizeof(float));
            _builder = new MeshBuilder(new Vector3Int(layerSettings.size, layerSettings.size, layerSettings.size), layerSettings.builderTriangleBudget, _builderCompute);
            Debug.Log(layerSettings);
            Debug.Log(layerSettings.simulationSettings);
            simulationRead = RDSimulator.CreateRenderTexture(layerSettings.simulationSettings.resolution);
            simulationWrite = RDSimulator.CreateRenderTexture(layerSettings.simulationSettings.resolution);
        }

        public override Mesh Generate(PlantGenSettings settings, int seed){
            RDLayerSettings layerSettings = (RDLayerSettings)settings;
            RDSimulator.InitializeComputeShader(ref simulationCompute, layerSettings.simulationSettings, ref simulationRead);

            //building the values array from layers from RDOnGPU
            for (int layerIndex = 0; layerIndex < layerSettings.size; layerIndex++)
            {
                AddNextLayerToValues(layerSettings, layerIndex);
            }
            _voxelBuffer.SetData(values);
            _builder.BuildIsosurface(_voxelBuffer, layerSettings.builderTargetValue, layerSettings.builderGridScale);
            Debug.Log("Finished generating coral");
            return _builder.Mesh;
        }

        void AddNextLayerToValues(RDLayerSettings layerSettings, int layerIndex)
        {
            float extraKill = layerSettings.killIncrease.Evaluate((float)layerIndex/layerSettings.size);
            RDSimulator.Iterate(ref simulationRead, ref simulationWrite, ref simulationCompute, layerSettings.simulationSettings, extraKill, layerSettings.step);
            Texture2D tex = RDSimulator.ToTexture2D(simulationWrite, layerSettings.simulationSettings.resolution);
            TextureScaler.Scale(tex, layerSettings.size, layerSettings.size);

            Color[] colors = tex.GetPixels();
            for (int i = 0; i < layerSettings.size * layerSettings.size; i++)
            {
                values[layerIndex * layerSettings.size * layerSettings.size + i] = colors[i].g;
            }
        }

        void OnDestroy()
        {
            CleanUp();
        }

        public void CleanUp(){
            if (_voxelBuffer != null) _voxelBuffer.Dispose();
            if (_builder != null) _builder.Dispose();
        }


    }
}