using UnityEngine;


namespace PlantGeneration {
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class GeneratorPreview : MonoBehaviour {
        public PlantGenSettings settings;
        // Start is called before the first frame update
        public void Generate() {
            settings.GetGenerator().Initialize(settings);
            GetComponent<MeshFilter>().sharedMesh = settings.GetGenerator().Generate(settings);
            GetComponent<MeshRenderer>().material = settings.material;
        }
        public void Clear() {
            DestroyImmediate(GetComponent<MeshFilter>().sharedMesh);
            GetComponent<MeshFilter>().sharedMesh = null;
        }
    }
}