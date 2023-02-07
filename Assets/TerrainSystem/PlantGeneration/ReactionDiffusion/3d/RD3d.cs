using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using MarchingCubes;

namespace ReactionDiffusion
{

    public class RD3d : MonoBehaviour
    {
        public RDSettings settings;
        private int resolution { get { return settings.resolution; } }
        [SerializeField] private ComputeShader computeShader;
        [SerializeField] private ComputeShader meshBuilderShader;
        private Texture2D feedTexture;

        public RenderTexture[] computeTextures = new RenderTexture[2];
        private MeshBuilder meshBuilder;
        private RenderTexture readBuffer
        {
            get { return computeTextures[0]; }
            set { computeTextures[0] = value; }
        }

        private RenderTexture writeBuffer
        {
            get { return computeTextures[1]; }
            set { computeTextures[1] = value; }
        }

        void Awake()
        {
            readBuffer = CreateRenderTexture("Read");
            writeBuffer = CreateRenderTexture("Write");
            InitializeComputeShader();
            meshBuilder = new MeshBuilder(resolution, resolution, resolution, 1048576, meshBuilderShader);
        }

        void Start()
        {
        }
        void Update()
        {
            Iterate();
            BuildMesh();
        }

        public void BuildMesh()
        {
            meshBuilder.BuildIsoSurface(readBuffer, 0.02f, 0.0625f);
            GetComponent<MeshFilter>().sharedMesh = meshBuilder.Mesh;
        }

        void InitializeComputeShader()
        {
            int kernel = computeShader.FindKernel("Init");
            feedTexture = TextureScaler.Scaled(settings.feedTexture, resolution, resolution);
            computeShader.SetTexture(kernel, "Write", readBuffer);
            computeShader.Dispatch(kernel, resolution / 8, resolution / 8, resolution / 8);
        }


        private RenderTexture CreateRenderTexture(string name)
        {
            RenderTexture renderTexture = new RenderTexture(resolution, resolution, 0);

            renderTexture.name = name;
            renderTexture.enableRandomWrite = true;
            renderTexture.dimension = TextureDimension.Tex3D;
            renderTexture.volumeDepth = resolution;
            renderTexture.filterMode = FilterMode.Bilinear;
            renderTexture.wrapMode = TextureWrapMode.Repeat;
            renderTexture.Create();
            return renderTexture;
        }

        public void Iterate()
        {
            int kernel = computeShader.FindKernel("Update");
            computeShader.SetInt("resolution", resolution);
            computeShader.SetFloat("speed", settings.speed);
            computeShader.SetVector("diffusion", settings.diffusion);
            computeShader.SetFloat("kill", settings.kill);

            //Setting feed parameters
            computeShader.SetFloat("feed", settings.feed);
            computeShader.SetFloat("feedTexStrength", settings.feedTexStrength);
            //computeShader.SetBool("useFeedTex", settings.useFeedTexture);
            computeShader.SetTexture(kernel, "FeedTex", feedTexture);

            //setting up the textures we are operation on
            computeShader.SetTexture(kernel, "Read", readBuffer);
            computeShader.SetTexture(kernel, "Write", writeBuffer);

            computeShader.Dispatch(kernel, resolution / 8, resolution / 8, resolution / 8);

            Swap();
        }

        private void Swap()
        {
            RenderTexture tmp = computeTextures[0];
            computeTextures[0] = computeTextures[1];
            computeTextures[1] = tmp;
        }

        void OnDestroy()
        {
            readBuffer.Release();
            writeBuffer.Release();
            meshBuilder.Dispose();
        }

    }
}