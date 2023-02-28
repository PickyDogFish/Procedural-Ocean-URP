using UnityEngine;

namespace PlantGeneration.ReactionDiffusion
{
    [CreateAssetMenu(fileName = "New RD Settings", menuName = "Flora/RD Settings")]
    public class RDSettings : ScriptableObject
    {
        [Header("Feed settings")]
        [Range(0,0.25f)] public float feed = 0.01f;
        public bool useFeedTexture = false;
        public Texture2D feedTexture;
        [Range(0,1f)] public float feedTexStrength = 0;


        [Header("Flow settings")]
        public Texture2D flowTexture;
        public float flowIntensity = 1;
        
        [Header("Other settings")]
        [Range(0,0.25f)] public float kill = 0.01f;

        [Range(0,1)] public float du = 0.05f;
        [Range(0,1)] public float dv = 0.05f;

        [Range(0.00001f, 1.5f)] public float speed = 1;
        public Texture2D initialTexture;
        public int resolution = 512;

        public Vector2 diffusion { get {return new Vector2(du,dv);}}
    }
}
