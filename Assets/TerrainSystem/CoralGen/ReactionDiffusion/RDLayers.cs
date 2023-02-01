using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarchingCubes;

namespace ReactionDiffusion
{
    public class RDLayers : MonoBehaviour
    {
        public RDLayerSettings layerSettings;
        private RDOnGPU rd;
        ComputeBuffer _voxelBuffer;
        MeshBuilder _builder;
        [SerializeField] ComputeShader _builderCompute = null;

        int size { get { return layerSettings.size; } }

        float[] values;

        void Awake()
        {
            rd = FindObjectOfType<RDOnGPU>();
            rd.settings = layerSettings.simulationSettings;
            values = new float[size * size * size];
            _voxelBuffer = new ComputeBuffer(size * size * size, sizeof(float));
            _builder = new MeshBuilder(new Vector3Int(size, size, size), layerSettings.builderTriangleBudget, _builderCompute);
        }
        void Start()
        {
            Debug.Log(layerSettings.killIncrease.Evaluate(0.1f));

            //building the values array from layers from RDOnGPU
            for (int layerIndex = 0; layerIndex < size; layerIndex++)
            {
                AddNextLayerToValues(layerIndex);
                rd.extraKill += layerSettings.killIncrease.Evaluate((float)layerIndex/size);
            }
            _voxelBuffer.SetData(values);
            _builder.BuildIsosurface(_voxelBuffer, layerSettings.builderTargetValue, layerSettings.builderGridScale);
            GetComponent<MeshFilter>().sharedMesh = _builder.Mesh;
        }

        // Update is called once per frame
        void Update()
        {

        }

        void AddNextLayerToValues(int layerIndex)
        {
            rd.Iterate(layerSettings.step);
            Texture2D tex = rd.GetTexture();
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