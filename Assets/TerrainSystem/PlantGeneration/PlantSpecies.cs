using UnityEngine;
using PlantGeneration.SpaceColonisation;

namespace PlantGeneration {

    /// <summary>
    /// Holds coralCount amount of corals generated with the same settings but with random seeds.
    /// </summary>
    public class PlantSpecies : MonoBehaviour{
        public PlantGenSettings settings;
        private GameObject[] corals;
        private int coralCount;

        public void  Initialize(int coralCount, PlantGenSettings settings, Transform parent){
            transform.parent = parent;
            this.coralCount = coralCount;
            this.settings = settings;
            corals = new GameObject[coralCount];
            GenerateCorals(settings.speciesName);
            gameObject.name = settings.speciesName;
            gameObject.SetActive(false);
        }

        private void GenerateCorals(string name) {
            for (int i = 0; i < coralCount; i++) {
                corals[i] = CreateGameObject();
                corals[i].name = name + i.ToString();
            }
        }

        private GameObject CreateGameObject(){
            GameObject newGO = new GameObject();
            settings.GetGenerator().Initialize(settings);
            newGO.AddComponent<MeshFilter>();
            newGO.GetComponent<MeshFilter>().sharedMesh = settings.GetGenerator().Generate(settings, Random.Range(-10000, 10000));
            
            newGO.AddComponent<MeshRenderer>();
            newGO.GetComponent<MeshRenderer>().material = settings.material;
            newGO.transform.localScale = Vector3.one * Random.Range(settings.minSize, settings.maxSize);
            
            newGO.transform.parent = transform;
            return newGO;
        }

        public GameObject GetCoralInstance(int index) {
            if (index > coralCount) {
                return null;
            } else {
                return GameObject.Instantiate(corals[index]);
            }
        }

        public GameObject GetRandomCoralInstance() {
            return GetCoralInstance(Random.Range(0, coralCount));
        }
    }
}