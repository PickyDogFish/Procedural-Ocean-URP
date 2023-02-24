using UnityEngine;


namespace PlantGeneration {
    [RequireComponent(typeof(MeshFilter))]
    public class GeneratorPreview : MonoBehaviour {
        public PlantGenSettings settings;
        public PlantGenerator generator;
        // Start is called before the first frame update
        public void Generate() {
            GetComponent<MeshFilter>().mesh = generator.Generate(settings);
        }
    }
}