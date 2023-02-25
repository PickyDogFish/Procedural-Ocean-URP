using UnityEngine;

namespace PlantGeneration.Kelp {
    [CreateAssetMenu(fileName = "New Kelp Settings", menuName = "Flora/Kelp settings")]
    public class KelpSettings : PlantGenSettings {
        [Header("Generation settings")]
        public float maxHeight = 3f;
        public float segmentLength = 0.02f;
        public float minAngle, maxAngle = 135;
        public float branchAngle = 0.3f;


        [Header("Mesh settings")]
        public int radialSubdivs = 3;
        public float diameter = 0.02f;
        public Mesh leaf;
        public float leafScale = 1;


        [Header("Debug")]
        public bool showGizmos;

        public override PlantGenerator GetGenerator()
        {
            return FindObjectOfType<GiantKelpGen>();
        }
    }
}