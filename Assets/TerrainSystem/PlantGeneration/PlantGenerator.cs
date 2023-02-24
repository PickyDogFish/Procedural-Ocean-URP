using UnityEngine;

namespace PlantGeneration {
    public abstract class PlantGenerator : MonoBehaviour {
        public abstract Mesh Generate(PlantGenSettings settings, int seed = 0);
    }
}