using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PlantGeneration.ReactionDiffusion
{
    public static class RDSimulator
    {

/*         void Awake()
        {
            readBuffer = CreateRenderTexture();
            writeBuffer = CreateRenderTexture();

            InitializeComputeShader();
            previewMaterial.SetTexture("_BaseMap", readBuffer);
        } */

        public static void InitializeComputeShader(ref ComputeShader RDShader, RDSettings settings, ref RenderTexture readBuffer)
        {
            int kernel = RDShader.FindKernel("Init");
            RDShader.SetTexture(kernel, "Write", readBuffer);

            Texture2D initialTexture = TextureScaler.Scaled(settings.initialTexture, settings.resolution, settings.resolution);
            RDShader.SetTexture(kernel, "Read", initialTexture);

            Texture2D flowTexture = TextureScaler.Scaled(settings.flowTexture, settings.resolution, settings.resolution);
            RDShader.SetTexture(kernel, "FlowTex", flowTexture);
            RDShader.Dispatch(kernel, settings.resolution / 8, settings.resolution / 8, 1);
        }


        /// <summary>
        /// Creates a 2D RenderTexture to be used as a read/write buffer for Reaction Diffusion.
        /// </summary>
        /// <param name="res">Square texture resolution.</param>
        /// <returns>Returns RenderTexture of given size</returns>
        public static RenderTexture CreateRenderTexture(int res)
        {
            RenderTexture renderTexture = new RenderTexture(res, res, 16, RenderTextureFormat.RG32);

            renderTexture.name = "Output";
            renderTexture.enableRandomWrite = true;
            renderTexture.dimension = TextureDimension.Tex2D;
            renderTexture.volumeDepth = res;
            renderTexture.filterMode = FilterMode.Bilinear;
            renderTexture.wrapMode = TextureWrapMode.Repeat;
            renderTexture.Create();
            return renderTexture;
        }

        public static void Iterate(ref RenderTexture readBuffer, ref RenderTexture writeBuffer, ref ComputeShader RDShader, RDSettings settings, float extraKill, int layer, int iterations)
        {
            int kernel = RDShader.FindKernel("Update");
            Texture2D flowTexture = TextureScaler.Scaled(settings.flowTexture, settings.resolution, settings.resolution);
            RDShader.SetTexture(kernel, "FlowTex", flowTexture);

            for (int i = 0; i < iterations; i++)
            {
                RDShader.SetInt("resolution", settings.resolution);
                RDShader.SetFloat("speed", settings.speed);
                RDShader.SetVector("diffusion", new Vector2(settings.du, settings.dv));
                RDShader.SetFloat("kill", settings.kill + extraKill);

                RDShader.SetFloat("flowIntensity", settings.flowIntensity);

                int[] offset = new int[2]{(int)Mathf.Cos(layer + i) * 10, (int)Mathf.Sin(layer + i) * 10};
                RDShader.SetInts("flowOffset", offset);

                //Setting feed parameters
                RDShader.SetTexture(kernel, "FeedTex", settings.feedTexture);
                RDShader.SetFloat("feed", settings.feed);
                RDShader.SetFloat("feedTexStrength", settings.feedTexStrength);
                RDShader.SetBool("useFeedTex", settings.useFeedTexture);

                //setting up the textures we are operation on
                RDShader.SetTexture(kernel, "Read", readBuffer);
                RDShader.SetTexture(kernel, "Write", writeBuffer);

                RDShader.Dispatch(kernel, settings.resolution / 8, settings.resolution / 8, 1);
                Swap(ref readBuffer, ref writeBuffer);
            }
        }

        private static void Swap(ref RenderTexture readBuffer, ref RenderTexture writeBuffer)
        {
            RenderTexture tmp = writeBuffer;
            writeBuffer = readBuffer;
            readBuffer = tmp;
        }

        public static Texture2D ToTexture2D(RenderTexture rTex, int resolution)
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