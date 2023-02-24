using UnityEngine;


namespace PlantGeneration.Kelp {
    [RequireComponent(typeof(MeshFilter))]
    public class KelpPreview : MonoBehaviour {
        public KelpSettings settings;
        public GiantKelpGen generator;
        // Start is called before the first frame update
        public void Generate() {
            GetComponent<MeshFilter>().mesh = generator.Generate(settings);
        }
    }
}