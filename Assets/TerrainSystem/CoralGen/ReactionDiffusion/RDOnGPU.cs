using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ReactionDiffusion
{
    public class RDOnGPU : MonoBehaviour
    {
        /* #region  In-editor settings */
        public RDSettings settings;
        public Material previewMaterial;
        public bool preview = false;
        [SerializeField] private ComputeShader computeShader;
        /* #endregion */

        /* #region Values from settings */
        private Texture2D initialTexture;
        private Texture2D feedTexture;
        private int resolution { get { return settings.resolution; } }
        /* #endregion */

        [HideInInspector] public float extraKill = 0;

        private RenderTexture[] _textures = new RenderTexture[2];

        private RenderTexture readBuffer
        {
            get { return _textures[0]; }
            set { _textures[0] = value; }
        }

        private RenderTexture writeBuffer
        {
            get { return _textures[1]; }
            set { _textures[1] = value; }
        }

        void Awake()
        {
            readBuffer = CreateRenderTexture();
            writeBuffer = CreateRenderTexture();

            InitializeComputeShader();
            previewMaterial.SetTexture("_BaseMap", readBuffer);
        }

        void InitializeComputeShader()
        {
            int kernel = computeShader.FindKernel("Init");
            computeShader.SetTexture(kernel, "Write", readBuffer);
            initialTexture = TextureScaler.Scaled(settings.initialTexture, resolution, resolution);
            feedTexture = TextureScaler.Scaled(settings.feedTexture, resolution, resolution);
            computeShader.SetTexture(kernel, "Read", initialTexture);
            computeShader.Dispatch(kernel, resolution / 8, resolution / 8, 1);
        }

        void Update()
        {
            if (preview)
            {
                Iterate();
            }
        }


        private RenderTexture CreateRenderTexture()
        {
            RenderTexture renderTexture = new RenderTexture(resolution, resolution, 16, RenderTextureFormat.RG32);

            renderTexture.name = "Output";
            renderTexture.enableRandomWrite = true;
            renderTexture.dimension = TextureDimension.Tex2D;
            renderTexture.volumeDepth = resolution;
            renderTexture.filterMode = FilterMode.Bilinear;
            renderTexture.wrapMode = TextureWrapMode.Repeat;
            renderTexture.Create();
            return renderTexture;
        }

        public void Iterate(int n)
        {
            for (int i = 0; i < n; i++)
            {
                Iterate();
            }
        }

        public void Iterate()
        {
            int kernel = computeShader.FindKernel("Update");
            computeShader.SetInt("resolution", resolution);
            computeShader.SetFloat("speed", settings.speed);
            computeShader.SetVector("diffusion", new Vector2(settings.du, settings.dv));
            computeShader.SetFloat("kill", settings.kill + extraKill);

            //Setting feed parameters
            computeShader.SetFloat("feed", settings.feed);
            computeShader.SetFloat("feedTexStrength", settings.feedTexStrength);
            computeShader.SetBool("useFeedTex", settings.useFeedTexture);
            computeShader.SetTexture(kernel, "FeedTex", feedTexture);

            //setting up the textures we are operation on
            computeShader.SetTexture(kernel, "Read", readBuffer);
            computeShader.SetTexture(kernel, "Write", writeBuffer);

            computeShader.Dispatch(kernel, resolution / 8, resolution / 8, 1);

            Swap();
        }

        private void Swap()
        {
            RenderTexture tmp = _textures[0];
            _textures[0] = _textures[1];
            _textures[1] = tmp;
        }

        public Texture2D GetTexture()
        {
            Texture2D tex = ToTexture2D(readBuffer);
            return tex;
        }

        Texture2D ToTexture2D(RenderTexture rTex)
        {
            Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGB24, true);
            // ReadPixels looks at the active RenderTexture.
            RenderTexture.active = rTex;
            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();
            return tex;
        }
    }
}