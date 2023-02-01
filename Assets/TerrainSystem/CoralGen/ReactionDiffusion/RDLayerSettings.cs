using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ReactionDiffusion
{
    [CreateAssetMenu()]
    public class RDLayerSettings : ScriptableObject
    {
        [field: SerializeField] public int startOffset {get; private set;} = 100;
        [field: SerializeField] public int step {get; private set;} = 100;
        [field: SerializeField] public int size {get; private set;} = 512;
        [field: SerializeField] public float killIncrease {get; private set;} = 0.002f;
        [field: SerializeField] public int builderTriangleBudget {get; private set;} = 65536;
        [field: SerializeField] public float builderTargetValue {get; private set;} = 0.3f;
        [field: SerializeField] public float builderGridScale {get; private set;} = 4.0f / 64;
        [field: SerializeField] public RDSettings simulationSettings {get; private set;} = null;
    }
}
