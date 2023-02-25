using UnityEngine;

namespace PlantGeneration.ReactionDiffusion
{
    [CreateAssetMenu(fileName = "New RD Layer Settings", menuName = "Flora/RD Layer Settings")]
    public class RDLayerSettings : PlantGenSettings
    {
        [Header("Layer settings")]
        [SerializeField] public int startOffset = 100;
        [SerializeField] public int step = 100;
        [SerializeField] public int size = 512;
        [SerializeField] public AnimationCurve killIncrease = null;
        [SerializeField] public RDSettings simulationSettings = null;
        [Header("Marching cubes settings")]
        [SerializeField] public int builderTriangleBudget = 524288;
        [SerializeField] public float builderTargetValue = 0.3f;
        [SerializeField] public float builderGridScale = 4.0f / 64;

        public override PlantGenerator GetGenerator()
        {
            return FindObjectOfType<RDLayered>();
        }
    }
}
